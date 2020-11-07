using DaAPI.Core.Common;
using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Packets.DHCPv6;
using DaAPI.Core.Scopes;
using DaAPI.Core.Scopes.DHCPv6;
using DaAPI.TestHelper;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static DaAPI.Core.Scopes.DHCPv6.DHCPv6LeaseEvents;
using static DaAPI.Core.Scopes.DHCPv6.DHCPv6PacketHandledEvents;

namespace DaAPI.UnitTests.Core.Scopes.DHCPv6
{
    public class DHCPv6RootScopeTesterHandleConfirmTester : DHCPv6RootSoopeTesterBase
    {
        private void CheckHandeledEvent(Int32 index, DHCPv6Packet requestPacket, DHCPv6Packet result, DHCPv6RootScope rootScope, Guid? scopeId, DHCPv6ConfirmHandledEvent.ConfirmErrors error)
        {
            IEnumerable<DomainEvent> changes = rootScope.GetChanges();

            Assert.IsAssignableFrom<DHCPv6ConfirmHandledEvent>(changes.ElementAt(index));

            DHCPv6ConfirmHandledEvent handeledEvent = (DHCPv6ConfirmHandledEvent)changes.ElementAt(index);
            Assert.Equal(requestPacket, handeledEvent.Request);
            Assert.Equal(result, handeledEvent.Response);
            Assert.Equal(scopeId, handeledEvent.ScopeId);
            Assert.Equal(error == DHCPv6ConfirmHandledEvent.ConfirmErrors.NoError, handeledEvent.WasSuccessfullHandled);
            Assert.Equal(error, handeledEvent.Error);
        }

        private DHCPv6Packet GetRelayedConfirmPacket(
          Random random, out IPv6Address usedAddress, out DUID clientDuid, out UInt32 iaId, out DHCPv6PrefixDelegation prefixDelegation, Boolean withAddress, Boolean withPrefix)
        {
            IPv6HeaderInformation headerInformation =
              new IPv6HeaderInformation(IPv6Address.FromString("fe80::1"), IPv6Address.FromString("fe80::2"));

            clientDuid = new UUIDDUID(random.NextGuid());

            usedAddress = IPv6Address.Empty;
            prefixDelegation = DHCPv6PrefixDelegation.None;
            iaId = 0;

            var packetOptions = new List<DHCPv6PacketOption>()
                {
                    new DHCPv6PacketIdentifierOption(DHCPv6PacketOptionTypes.ClientIdentifier,clientDuid),
                };

            if (withAddress == true)
            {
                iaId = random.NextUInt32();
                usedAddress = random.GetIPv6Address();
                packetOptions.Add(new DHCPv6PacketIdentityAssociationNonTemporaryAddressesOption(iaId, TimeSpan.FromSeconds(random.Next()), TimeSpan.FromSeconds(random.Next()), new DHCPv6PacketSuboption[]
                {
                     new DHCPv6PacketIdentityAssociationAddressSuboption(usedAddress,TimeSpan.FromSeconds(random.Next()),TimeSpan.FromSeconds(random.Next()),Array.Empty<DHCPv6PacketSuboption>())
                }));
            }

            if (withPrefix == true)
            {
                IPv6Address prefix = IPv6Address.FromString("2acd:adce::0");
                UInt32 prefixId = random.NextBoolean() == false ? random.NextUInt32() : 0;
                Byte mask = (Byte)random.Next(64, 80);

                prefixDelegation = DHCPv6PrefixDelegation.FromValues(prefix, new IPv6SubnetMask(new IPv6SubnetMaskIdentifier(mask)), prefixId);

                packetOptions.Add(new DHCPv6PacketIdentityAssociationPrefixDelegationOption(prefixId, TimeSpan.FromSeconds(random.Next()), TimeSpan.FromSeconds(random.Next()), new DHCPv6PacketSuboption[]
                {
                     new DHCPv6PacketIdentityAssociationPrefixDelegationSuboption(TimeSpan.Zero,TimeSpan.Zero,mask,prefix,Array.Empty<DHCPv6PacketSuboption>())
                }));
            }

            DHCPv6Packet innerPacket = DHCPv6Packet.AsInner(random.NextUInt16(),
                DHCPv6PacketTypes.CONFIRM, packetOptions);

            DHCPv6Packet packet = DHCPv6RelayPacket.AsOuterRelay(headerInformation, true, 1, random.GetIPv6Address(), random.GetIPv6Address(),
                new DHCPv6PacketOption[] { new DHCPv6PacketByteArrayOption(DHCPv6PacketOptionTypes.InterfaceId, random.NextBytes(10)) }, innerPacket);

            return packet;
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        public void HandleConfirm_LeaseFound_LeaseIsActive_AdderessAreEqaul(Boolean withAddress, Boolean withPrefix)
        {
            Random random = new Random();

            var packet = GetRelayedConfirmPacket(random, out IPv6Address leasedAddress, out DUID clientDuid, out UInt32 iaId, out DHCPv6PrefixDelegation prefix, withAddress, withPrefix);

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
                        t1: DHCPv6TimeScale.FromDouble(0.5),
                        t2: DHCPv6TimeScale.FromDouble(0.75),
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
                    HasPrefixDelegation = withPrefix,
                    PrefixLength = withPrefix == false ? (Byte)0 : prefix.Mask.Identifier,
                    DelegatedNetworkAddress = withPrefix == false ? IPv6Address.Empty :  prefix.NetworkAddress,
                    IdentityAssocationIdForPrefix = withPrefix == false ? 0 : prefix.IdentityAssociation,
                },
                new DHCPv6LeaseActivatedEvent
                {
                    EntityId = leaseId,
                    ScopeId = scopeId,
                }
            });

            DHCPv6Packet result = rootScope.HandleConfirm(packet, GetServerPropertiesResolver());
            CheckPacket(packet, leasedAddress, iaId, result, DHCPv6PacketTypes.REPLY, prefix);

            CheckEventAmount(1, rootScope);
            CheckHandeledEvent(0, packet, result, rootScope, scopeId, DHCPv6ConfirmHandledEvent.ConfirmErrors.NoError);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        public void HandleConfirm_LeaseFound_LeaseIsNotActive(Boolean withAddress, Boolean withPrefix)
        {
            Random random = new Random();

            var packet = GetRelayedConfirmPacket(random, out IPv6Address leasedAddress, out DUID clientDuid, out UInt32 iaId, out DHCPv6PrefixDelegation prefix, withAddress, withPrefix);

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
                        t1: DHCPv6TimeScale.FromDouble(0.5),
                        t2: DHCPv6TimeScale.FromDouble(0.75),
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
                    HasPrefixDelegation = withPrefix,
                    PrefixLength = withPrefix == false ? (Byte)0 : prefix.Mask.Identifier,
                    DelegatedNetworkAddress = withPrefix == false ? IPv6Address.Empty :  prefix.NetworkAddress,
                    IdentityAssocationIdForPrefix = withPrefix == false ? 0 : prefix.IdentityAssociation,
                },
            });

            DHCPv6Packet result = rootScope.HandleConfirm(packet, GetServerPropertiesResolver());
            CheckErrorPacket(packet, leasedAddress, iaId, result, prefix, DHCPv6StatusCodes.NotOnLink);

            CheckEventAmount(1, rootScope);
            CheckHandeledEvent(0, packet, result, rootScope, scopeId, DHCPv6ConfirmHandledEvent.ConfirmErrors.LeaseNotActive);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        public void HandleConfirm_LeaseNotFound(Boolean withAddress, Boolean withPrefix)
        {
            Random random = new Random();

            var packet = GetRelayedConfirmPacket(random, out IPv6Address leasedAddress, out DUID _, out UInt32 iaId, out DHCPv6PrefixDelegation prefix, withAddress, withPrefix);

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
                        t1: DHCPv6TimeScale.FromDouble(0.5),
                        t2: DHCPv6TimeScale.FromDouble(0.75),
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
                    ClientIdentifier = new UUIDDUID(random.NextGuid()),
                    IdentityAssocationId = iaId,
                    ScopeId = scopeId,
                    StartedAt = leaseCreatedAt,
                    ValidUntil = DateTime.UtcNow.AddDays(1),
                    HasPrefixDelegation = withPrefix,
                    PrefixLength = withPrefix == false ? (Byte)0 : prefix.Mask.Identifier,
                    DelegatedNetworkAddress = withPrefix == false ? IPv6Address.Empty :  prefix.NetworkAddress,
                    IdentityAssocationIdForPrefix = withPrefix == false ? 0 : prefix.IdentityAssociation,
                },
            });

            DHCPv6Packet result = rootScope.HandleConfirm(packet, GetServerPropertiesResolver());
            Assert.Equal(DHCPv6Packet.Empty, result);
            
            CheckEventAmount(1, rootScope);
            CheckHandeledEvent(0, packet, result, rootScope, scopeId, DHCPv6ConfirmHandledEvent.ConfirmErrors.LeaseNotFound);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        public void HandleConfirm_LeaseFound_LeaseIsActive_AdderessAreNotEqaul(Boolean withAddress, Boolean withPrefix)
        {
            Random random = new Random();

            var packet = GetRelayedConfirmPacket(random, out IPv6Address leasedAddress, out DUID clientDuid, out UInt32 iaId, out DHCPv6PrefixDelegation prefix, withAddress, withPrefix);

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
                        t1: DHCPv6TimeScale.FromDouble(0.5),
                        t2: DHCPv6TimeScale.FromDouble(0.75),
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
                    Address = random.GetIPv6Address(),
                    ClientIdentifier = clientDuid,
                    IdentityAssocationId = iaId,
                    ScopeId = scopeId,
                    StartedAt = leaseCreatedAt,
                    ValidUntil = DateTime.UtcNow.AddDays(1),
                    HasPrefixDelegation = withPrefix,
                    PrefixLength = withPrefix == false ? (Byte)0 : prefix.Mask.Identifier,
                    DelegatedNetworkAddress = withPrefix == false ? IPv6Address.Empty :  IPv6Address.FromString("aaaa:aaaa::"),
                    IdentityAssocationIdForPrefix = withPrefix == false ? 0 : prefix.IdentityAssociation,
                },
                new DHCPv6LeaseActivatedEvent
                {
                    EntityId = leaseId,
                    ScopeId = scopeId,
                }
            });

            DHCPv6Packet result = rootScope.HandleConfirm(packet, GetServerPropertiesResolver());
            CheckErrorPacket(packet, leasedAddress, iaId, result, prefix, DHCPv6StatusCodes.NotOnLink);

            CheckEventAmount(1, rootScope);
            CheckHandeledEvent(0, packet, result, rootScope, scopeId, DHCPv6ConfirmHandledEvent.ConfirmErrors.AddressMismtach);
        }
    }
}
