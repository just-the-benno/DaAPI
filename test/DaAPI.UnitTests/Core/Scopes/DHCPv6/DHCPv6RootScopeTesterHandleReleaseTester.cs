using DaAPI.Core.Common;
using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Notifications;
using DaAPI.Core.Notifications.Triggers;
using DaAPI.Core.Packets.DHCPv6;
using DaAPI.Core.Scopes;
using DaAPI.Core.Scopes.DHCPv6;
using DaAPI.TestHelper;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using static DaAPI.Core.Scopes.DHCPv6.DHCPv6LeaseEvents;
using static DaAPI.Core.Scopes.DHCPv6.DHCPv6PacketHandledEvents;

namespace DaAPI.UnitTests.Core.Scopes.DHCPv6
{
    public class DHCPv6RootScopeTesterHandleReleaseTester : DHCPv6RootSoopeTesterBase
    {
        private void CheckHandeledEvent(Int32 index, DHCPv6Packet requestPacket, DHCPv6Packet result, DHCPv6RootScope rootScope, Guid? scopeId, DHCPv6ReleaseHandledEvent.ReleaseError error)
        {
            IEnumerable<DomainEvent> changes = rootScope.GetChanges();

            Assert.IsAssignableFrom<DHCPv6ReleaseHandledEvent>(changes.ElementAt(index));

            DHCPv6ReleaseHandledEvent handeledEvent = (DHCPv6ReleaseHandledEvent)changes.ElementAt(index);
            Assert.Equal(requestPacket, handeledEvent.Request);
            Assert.Equal(result, handeledEvent.Response);
            Assert.Equal(scopeId, handeledEvent.ScopeId);
            Assert.Equal(error == DHCPv6ReleaseHandledEvent.ReleaseError.NoError, handeledEvent.WasSuccessfullHandled);
            Assert.Equal(error, handeledEvent.Error);
        }

        protected static void CheckLeaseReleasedEvent(
         Guid scopeId,
         DHCPv6RootScope rootScope, Guid leaseId, Boolean onlyPrefix)
        {
            IEnumerable<DomainEvent> changes = rootScope.GetChanges();
            Assert.NotNull(changes);

            Assert.IsAssignableFrom<DHCPv6LeaseReleasedEvent>(changes.First());

            DHCPv6LeaseReleasedEvent castedEvent = (DHCPv6LeaseReleasedEvent)changes.First();
            Assert.NotNull(castedEvent);

            Assert.Equal(scopeId, castedEvent.ScopeId);
            Assert.Equal(leaseId, castedEvent.EntityId);
            Assert.Equal(onlyPrefix, castedEvent.OnlyPrefix);
        }

        private DHCPv6Packet GetRelayedReleasePacket(
          Random random, out IPv6Address usedAddress, out DUID clientDuid, out UInt32 iaId, Boolean withIdentity, params DHCPv6PacketOption[] options)
        {
            IPv6HeaderInformation headerInformation =
              new IPv6HeaderInformation(IPv6Address.FromString("fe80::1"), IPv6Address.FromString("fe80::2"));

            DHCPv6Packet innerPacket = GetReleasePacket(random, out usedAddress, out clientDuid, out iaId, withIdentity, options);

            DHCPv6Packet packet = DHCPv6RelayPacket.AsOuterRelay(headerInformation, true, 1, random.GetIPv6Address(), random.GetIPv6Address(),
                new DHCPv6PacketOption[] { new DHCPv6PacketByteArrayOption(DHCPv6PacketOptionTypes.InterfaceId, random.NextBytes(10)) }, innerPacket);

            return packet;
        }

        private DHCPv6Packet GetReleasePacket(
       Random random, out IPv6Address usedAddress, out DUID clientDuid, out UInt32 iaId, Boolean withIdentity, params DHCPv6PacketOption[] options)
        {
            IPv6HeaderInformation headerInformation =
               new IPv6HeaderInformation(IPv6Address.FromString("fe80::1"), IPv6Address.FromString("fe80::2"));

            clientDuid = new UUIDDUID(random.NextGuid());
            iaId = random.NextBoolean() == false ? random.NextUInt32() : 0;
            usedAddress = random.GetIPv6Address();

            var packetOptions = new List<DHCPv6PacketOption>(options)
                {
                    new DHCPv6PacketIdentifierOption(DHCPv6PacketOptionTypes.ClientIdentifier,clientDuid),
                };

            if (withIdentity == true)
            {
                packetOptions.Add(new DHCPv6PacketIdentityAssociationNonTemporaryAddressesOption(iaId, TimeSpan.FromSeconds(random.Next()), TimeSpan.FromSeconds(random.Next()), new DHCPv6PacketSuboption[]
                    {
                     new DHCPv6PacketIdentityAssociationAddressSuboption(usedAddress,TimeSpan.FromSeconds(random.Next()),TimeSpan.FromSeconds(random.Next()),Array.Empty<DHCPv6PacketSuboption>())
                    }));
            }

            DHCPv6Packet packet = DHCPv6Packet.AsOuter(headerInformation, random.NextUInt16(),
                DHCPv6PacketTypes.RELEASE, packetOptions);

            return packet;
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void HandleRelease_LeaseFound(Boolean withPrefixDelegation, Boolean isUnicast)
        {
            Random random = new Random();

            UInt32 prefixId = random.NextUInt32();
            Byte prefixLength = (Byte)random.Next(40, 60);
            IPv6Address prefixNetworkAddress = IPv6Address.FromString("fd01::0");

            DHCPv6PacketOption[] options = Array.Empty<DHCPv6PacketOption>();
            if (withPrefixDelegation == true)
            {
                options = new DHCPv6PacketOption[] {
                new DHCPv6PacketIdentityAssociationPrefixDelegationOption(prefixId,TimeSpan.FromSeconds(random.Next()),TimeSpan.FromSeconds(random.Next()),new DHCPv6PacketSuboption[]
                {
                    new DHCPv6PacketIdentityAssociationPrefixDelegationSuboption(TimeSpan.FromSeconds(random.Next()),TimeSpan.FromSeconds(random.Next()),prefixLength,prefixNetworkAddress,Array.Empty<DHCPv6PacketSuboption>())
                })
                };
            }

            var packet = isUnicast == true ?
                    GetReleasePacket(random, out IPv6Address leasedAddress, out DUID clientDuid, out uint iaId, true, options) :
                    GetRelayedReleasePacket(random, out leasedAddress, out clientDuid, out iaId, true, options);

            var resolverInformations = GetMockupResolver(packet, out Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>> scopeResolverMock);

            Guid scopeId = random.NextGuid();
            Guid leaseId = random.NextGuid();
            DateTime leaseCreatedAt = DateTime.UtcNow;

            DHCPv6RootScope rootScope = GetRootScope(scopeResolverMock);
            rootScope.Load(new List<DomainEvent>{ new DHCPv6ScopeEvents.DHCPv6ScopeAddedEvent(
                new DHCPv6ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv6ScopeAddressProperties(
                        IPv6Address.FromString("fe80::0"),
                        IPv6Address.FromString("fe80::ff"),
                        new List<IPv6Address>{IPv6Address.FromString("fe80::1") },
                        preferredLifeTime: TimeSpan.FromDays(0.5),
                        validLifeTime: TimeSpan.FromDays(1),
                        reuseAddressIfPossible: true),
                    ResolverInformation = resolverInformations,
                    Name = "Testscope",
                    Id = scopeId,
                }),
                new DHCPv6LeaseCreatedEvent
                {
                    EntityId = leaseId,
                    Address = leasedAddress,
                    ClientIdentifier = clientDuid,
                    IdentityAssocationId = iaId,
                    ScopeId = scopeId,
                    StartedAt = leaseCreatedAt,
                    ValidUntil = DateTime.UtcNow.AddDays(1),
                    HasPrefixDelegation = withPrefixDelegation,
                    PrefixLength = prefixLength,
                    IdentityAssocationIdForPrefix = prefixId,
                    DelegatedNetworkAddress = prefixNetworkAddress,
                },
                new DHCPv6LeaseActivatedEvent
                {
                    EntityId = leaseId,
                    ScopeId = scopeId,
                }
            });

            DHCPv6Packet result = rootScope.HandleRelease(packet, GetServerPropertiesResolver());
            CheckErrorPacket(packet, leasedAddress, iaId, result, withPrefixDelegation == false ? DHCPv6PrefixDelegation.None : DHCPv6PrefixDelegation.FromValues(prefixNetworkAddress, new IPv6SubnetMask(new IPv6SubnetMaskIdentifier(prefixLength)), prefixId), DHCPv6StatusCodes.Success);

            DHCPv6Lease lease = rootScope.GetScopeById(scopeId).Leases.GetLeaseById(leaseId);
            Assert.Equal(LeaseStates.Released, lease.State);

            CheckEventAmount(2, rootScope);
            CheckLeaseReleasedEvent(scopeId, rootScope, leaseId, false);
            CheckHandeledEvent(1, packet, result, rootScope, scopeId, DHCPv6ReleaseHandledEvent.ReleaseError.NoError);

            if (withPrefixDelegation == true)
            {
                var trigger = CheckTrigger<PrefixEdgeRouterBindingUpdatedTrigger>(rootScope);
                Assert.Equal(scopeId, trigger.ScopeId);
                Assert.Null(trigger.NewBinding);
                Assert.NotNull(trigger.OldBinding);

                Assert.Equal(leasedAddress, trigger.OldBinding.Host);
                Assert.Equal(prefixLength, trigger.OldBinding.Mask.Identifier);
                Assert.Equal(prefixNetworkAddress, trigger.OldBinding.Prefix);
            }
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void HandleRelease_NoLeaseFound(Boolean withPrefixDelegation, Boolean isUnicast)
        {
            Random random = new Random();

            UInt32 prefixId = random.NextUInt32();
            Byte prefixLength = (Byte)random.Next(34, 64);
            IPv6Address prefixNetworkAddress = IPv6Address.FromString("fd01::0");

            DHCPv6PacketOption[] options = Array.Empty<DHCPv6PacketOption>();
            if (withPrefixDelegation == true)
            {
                options = new DHCPv6PacketOption[] {
                new DHCPv6PacketIdentityAssociationPrefixDelegationOption(prefixId,TimeSpan.FromSeconds(random.Next()),TimeSpan.FromSeconds(random.Next()),new DHCPv6PacketSuboption[]{
                new DHCPv6PacketIdentityAssociationPrefixDelegationSuboption(TimeSpan.FromSeconds(random.Next()),TimeSpan.FromSeconds(random.Next()),prefixLength,prefixNetworkAddress,Array.Empty<DHCPv6PacketSuboption>())
                })
                };
            }

            var packet = isUnicast == true ?
                    GetReleasePacket(random, out IPv6Address leasedAddress, out _, out UInt32 iaId, true, options) :
                    GetRelayedReleasePacket(random, out leasedAddress, out _, out iaId, true, options);

            var resolverInformations = GetMockupResolver(packet, out Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>> scopeResolverMock);

            Guid scopeId = random.NextGuid();
            Guid leaseId = random.NextGuid();

            DateTime leaseCreatedAt = DateTime.UtcNow;

            DHCPv6RootScope rootScope = GetRootScope(scopeResolverMock);
            rootScope.Load(new List<DomainEvent>{ new DHCPv6ScopeEvents.DHCPv6ScopeAddedEvent(
                new DHCPv6ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv6ScopeAddressProperties(
                        IPv6Address.FromString("fe80::01"),
                        IPv6Address.FromString("fe80::ff"),
                        new List<IPv6Address>{IPv6Address.FromString("fe80::02") },
                        preferredLifeTime: TimeSpan.FromDays(0.5),
                        validLifeTime: TimeSpan.FromDays(1),
                        reuseAddressIfPossible: false,
                        prefixDelgationInfo: DHCPv6PrefixDelgationInfo.FromValues(IPv6Address.FromString("2140:1::0"),new IPv6SubnetMaskIdentifier(32),new IPv6SubnetMaskIdentifier(prefixLength)),
                        addressAllocationStrategy: DHCPv6ScopeAddressProperties.AddressAllocationStrategies.Next),
                    ResolverInformation = resolverInformations,
                    Name = "Testscope",
                    Id = scopeId,
                }),
                new DHCPv6LeaseCreatedEvent
                {
                    EntityId = leaseId,
                    Address = leasedAddress,
                    ClientIdentifier = new UUIDDUID(random.NextGuid()),
                    IdentityAssocationId = iaId,
                    ScopeId = scopeId,
                    StartedAt = leaseCreatedAt,
                    ValidUntil = DateTime.UtcNow.AddDays(1),
                    HasPrefixDelegation = withPrefixDelegation,
                    PrefixLength = prefixLength,
                    IdentityAssocationIdForPrefix = prefixId,
                    DelegatedNetworkAddress = prefixNetworkAddress,
                },
                new DHCPv6LeaseActivatedEvent
                {
                    EntityId = leaseId,
                    ScopeId = scopeId,
                }
            });

            DHCPv6Packet result = rootScope.HandleRelease(packet, GetServerPropertiesResolver());
            CheckErrorPacket(packet, leasedAddress, iaId, result, withPrefixDelegation == false ? DHCPv6PrefixDelegation.None : DHCPv6PrefixDelegation.FromValues(prefixNetworkAddress, new IPv6SubnetMask(new IPv6SubnetMaskIdentifier(prefixLength)), prefixId), DHCPv6StatusCodes.NoBinding);

            DHCPv6Lease lease = rootScope.GetScopeById(scopeId).Leases.GetLeaseById(leaseId);
            Assert.Equal(LeaseStates.Active, lease.State);

            CheckEventAmount(1, rootScope);
            CheckHandeledEvent(0, packet, result, rootScope, scopeId, DHCPv6ReleaseHandledEvent.ReleaseError.NoLeaseFound);
        }

        [Fact]
        public void HandleRelease_ScopeNotFound_ByResolver()
        {
            Random random = new Random();

            DHCPv6PacketOption[] options = Array.Empty<DHCPv6PacketOption>();

            var packet = GetRelayedReleasePacket(random, out IPv6Address _, out DUID _, out UInt32 _, true, options);
            GetMockupResolver(DHCPv6Packet.Empty, out Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>> scopeResolverMock);

            DHCPv6RootScope rootScope = GetRootScope(scopeResolverMock);

            DHCPv6Packet result = rootScope.HandleRelease(packet, GetServerPropertiesResolver());
            Assert.Equal(DHCPv6Packet.Empty, result);

            CheckEventAmount(1, rootScope);
            CheckHandeledEvent(0, packet, result, rootScope, null, DHCPv6ReleaseHandledEvent.ReleaseError.ScopeNotFound);
        }

        [Fact]
        public void HandleRelease_ScopeNotFound_ByAddress()
        {
            Random random = new Random();

            var packet = GetReleasePacket(random, out _, out _, out _, true);

            var resolverInformations = GetMockupResolver(packet, out Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>> scopeResolverMock);

            Guid scopeId = random.NextGuid();

            DHCPv6RootScope rootScope = GetRootScope(scopeResolverMock);
            rootScope.Load(new List<DomainEvent>{ new DHCPv6ScopeEvents.DHCPv6ScopeAddedEvent(
                new DHCPv6ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv6ScopeAddressProperties(
                        IPv6Address.FromString("fe80::10"),
                        IPv6Address.FromString("fe80::ff"),
                        new List<IPv6Address>{IPv6Address.FromString("fe80::12") },
                        preferredLifeTime: TimeSpan.FromDays(0.5),
                        validLifeTime: TimeSpan.FromDays(1),
                        reuseAddressIfPossible: false,
                        addressAllocationStrategy: DHCPv6ScopeAddressProperties.AddressAllocationStrategies.Next),
                    ResolverInformation = resolverInformations,
                    Name = "Testscope",
                    Id = scopeId,
                }),
            });

            DHCPv6Packet result = rootScope.HandleRelease(packet, GetServerPropertiesResolver());
            Assert.Equal(DHCPv6Packet.Empty, result);

            //CheckErrorPacket(leasedAddress, iaId, result, DHCPv6PrefixDelegation.None,DHCPv6StatusCodes.NoBinding);
            CheckEventAmount(1, rootScope);
            CheckHandeledEvent(0, packet, result, rootScope, null, DHCPv6ReleaseHandledEvent.ReleaseError.ScopeNotFound);
        }
    }
}
