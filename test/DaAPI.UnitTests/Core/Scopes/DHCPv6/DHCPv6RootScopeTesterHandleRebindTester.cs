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
    public class DHCPv6RootScopeTesterHandleRebindTester : DHCPv6RootSoopeTesterBase
    {
        private void CheckHandeledEvent(Int32 index, DHCPv6Packet requestPacket, DHCPv6Packet result, DHCPv6RootScope rootScope, Guid? scopeId, DHCPv6RebindHandledEvent.RebindErrors error)
        {
            IEnumerable<DomainEvent> changes = rootScope.GetChanges();

            Assert.IsAssignableFrom<DHCPv6RebindHandledEvent>(changes.ElementAt(index));

            DHCPv6RebindHandledEvent handeledEvent = (DHCPv6RebindHandledEvent)changes.ElementAt(index);
            Assert.Equal(requestPacket, handeledEvent.Request);
            Assert.Equal(result, handeledEvent.Response);
            Assert.Equal(scopeId, handeledEvent.ScopeId);
            Assert.Equal(error == DHCPv6RebindHandledEvent.RebindErrors.NoError, handeledEvent.WasSuccessfullHandled);
            Assert.Equal(error, handeledEvent.Error);
        }

        private DHCPv6Packet GetRelayedRebindPacket(
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

            DHCPv6Packet innerPacket = DHCPv6Packet.AsInner(random.NextUInt16(),
                DHCPv6PacketTypes.REBIND, packetOptions);

            DHCPv6Packet packet = DHCPv6RelayPacket.AsOuterRelay(headerInformation, true, 1, random.GetIPv6Address(), random.GetIPv6Address(),
                new DHCPv6PacketOption[] { new DHCPv6PacketByteArrayOption(DHCPv6PacketOptionTypes.InterfaceId, random.NextBytes(10)) }, innerPacket);

            return packet;
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void HandleRebind_LeaseFound_ReuseIsAllowed(Boolean withPrefixDelegation)
        {
            Random random = new Random();

            UInt32 prefixId = random.NextBoolean() == false ? random.NextUInt32() : 0;
            Byte prefixLength = (Byte)random.Next(20, 60);
            IPv6Address prefixNetworkAddress = IPv6Address.FromString("fd01::0");

            DHCPv6PacketOption[] options = Array.Empty<DHCPv6PacketOption>();
            if (withPrefixDelegation == true)
            {
                options = new DHCPv6PacketOption[] {
                new DHCPv6PacketIdentityAssociationPrefixDelegationOption(prefixId,TimeSpan.FromSeconds(random.Next()),TimeSpan.FromSeconds(random.Next()),Array.Empty<DHCPv6PacketSuboption>())
                };
            }

            var packet = GetRelayedRebindPacket(random, out IPv6Address _, out DUID clientDuid, out UInt32 iaId, true, options);

            var resolverInformations = GetMockupResolver(packet, out Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>> scopeResolverMock);

            Guid scopeId = random.NextGuid();
            Guid leaseId = random.NextGuid();
            IPv6Address leasedAddress = IPv6Address.FromString("fe80::2");
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

            DHCPv6Packet result = rootScope.HandleRebind(packet, GetServerPropertiesResolver());
            CheckPacket(packet, leasedAddress, iaId, result, DHCPv6PacketTypes.REPLY, withPrefixDelegation == false ? DHCPv6PrefixDelegation.None : DHCPv6PrefixDelegation.FromValues(prefixNetworkAddress, new IPv6SubnetMask(new IPv6SubnetMaskIdentifier(prefixLength)), prefixId));

            DHCPv6Lease lease = CheckLease(0, 1, leasedAddress, scopeId, rootScope, leaseCreatedAt, clientDuid, iaId, false, withPrefixDelegation);

            CheckEventAmount(2, rootScope);
            CheckLeaseRenewdEvent(scopeId, rootScope, lease, false, false);
            CheckHandeledEvent(1, packet, result, rootScope, scopeId, DHCPv6RebindHandledEvent.RebindErrors.NoError);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void HandleRebind_LeaseFound_ReuseAllowed_ButLeaseNotExtendable(Boolean withPrefixDelegation)
        {
            Random random = new Random();

            UInt32 prefixId = random.NextBoolean() == false ? random.NextUInt32() : 0;
            Byte prefixLength = (Byte)random.Next(34, 64);
            IPv6Address prefixNetworkAddress = IPv6Address.FromString("fd01::0");

            DHCPv6PacketOption[] options = Array.Empty<DHCPv6PacketOption>();
            if (withPrefixDelegation == true)
            {
                options = new DHCPv6PacketOption[] {
                new DHCPv6PacketIdentityAssociationPrefixDelegationOption(prefixId,TimeSpan.FromSeconds(random.Next()),TimeSpan.FromSeconds(random.Next()),Array.Empty<DHCPv6PacketSuboption>())
                };
            }

            var packet = GetRelayedRebindPacket(random, out IPv6Address _, out DUID clientDuid, out UInt32 iaId, true, options);

            var resolverInformations = GetMockupResolver(packet, out Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>> scopeResolverMock);

            Guid scopeId = random.NextGuid();
            Guid leaseId = random.NextGuid();
            IPv6Address leasedAddress = IPv6Address.FromString("fe80::01");
            IPv6Address expectedAddress = IPv6Address.FromString("fe80::03");

            DateTime leaseCreatedAt = DateTime.UtcNow.AddMinutes(-2);

            DHCPv6RootScope rootScope = GetRootScope(scopeResolverMock);
            rootScope.Load(new List<DomainEvent>{ new DHCPv6ScopeEvents.DHCPv6ScopeAddedEvent(
                new DHCPv6ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv6ScopeAddressProperties(
                        IPv6Address.FromString("fe80::01"),
                        IPv6Address.FromString("fe80::ff"),
                        new List<IPv6Address>{IPv6Address.FromString("fe80::02") },
                        t1: DHCPv6TimeScale.FromDouble(0.5),
                        t2: DHCPv6TimeScale.FromDouble(0.75),
                        preferredLifeTime: TimeSpan.FromDays(0.5),
                        validLifeTime: TimeSpan.FromDays(1),
                        reuseAddressIfPossible: true,
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
                },
                new DHCPv6LeaseCanceledEvent
                {
                    EntityId = leaseId,
                    ScopeId = scopeId
                }
            });

            DHCPv6Packet result = rootScope.HandleRebind(packet, GetServerPropertiesResolver());

            DHCPv6Lease lease = CheckLease(1, 2, expectedAddress, scopeId, rootScope, DateTime.UtcNow, clientDuid, iaId, false, withPrefixDelegation);
            CheckPacket(packet, expectedAddress, iaId, result, DHCPv6PacketTypes.REPLY, withPrefixDelegation == false ? DHCPv6PrefixDelegation.None : DHCPv6PrefixDelegation.FromValues(lease.PrefixDelegation.NetworkAddress, new IPv6SubnetMask(new IPv6SubnetMaskIdentifier(prefixLength)), prefixId));

            var canceledLease = rootScope.GetScopeById(scopeId).Leases.GetLeaseById(leaseId);
            Assert.Equal(LeaseStates.Canceled, canceledLease.State);

            CheckEventAmount(3, rootScope);
            CheckLeaseCreatedEvent(0, clientDuid, iaId, scopeId, rootScope, expectedAddress, lease);
            CheckLeaseActivatedEvent(1, scopeId, lease.Id, rootScope);
            CheckHandeledEvent(2, packet, result, rootScope, scopeId, DHCPv6RebindHandledEvent.RebindErrors.NoError);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void HandleRebind_LeaseFound_ReuseIsNotAllowed(Boolean withPrefixDelegation)
        {
            Random random = new Random();

            UInt32 prefixId = random.NextBoolean() == false ? random.NextUInt32() : 0;
            Byte prefixLength = (Byte)random.Next(34, 64);
            IPv6Address prefixNetworkAddress = IPv6Address.FromString("fd01::0");

            DHCPv6PacketOption[] options = Array.Empty<DHCPv6PacketOption>();
            if (withPrefixDelegation == true)
            {
                options = new DHCPv6PacketOption[] {
                new DHCPv6PacketIdentityAssociationPrefixDelegationOption(prefixId,TimeSpan.FromSeconds(random.Next()),TimeSpan.FromSeconds(random.Next()),Array.Empty<DHCPv6PacketSuboption>())
                };
            }

            var packet = GetRelayedRebindPacket(random, out IPv6Address _, out DUID clientDuid, out UInt32 iaId, true, options);

            var resolverInformations = GetMockupResolver(packet, out Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>> scopeResolverMock);

            Guid scopeId = random.NextGuid();
            Guid leaseId = random.NextGuid();
            IPv6Address leasedAddress = IPv6Address.FromString("fe80::01");
            IPv6Address expectedAddress = IPv6Address.FromString("fe80::03");

            DateTime leaseCreatedAt = DateTime.UtcNow.AddMinutes(-2);

            DHCPv6RootScope rootScope = GetRootScope(scopeResolverMock);
            rootScope.Load(new List<DomainEvent>{ new DHCPv6ScopeEvents.DHCPv6ScopeAddedEvent(
                new DHCPv6ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv6ScopeAddressProperties(
                        IPv6Address.FromString("fe80::01"),
                        IPv6Address.FromString("fe80::ff"),
                        new List<IPv6Address>{IPv6Address.FromString("fe80::02") },
                        t1: DHCPv6TimeScale.FromDouble(0.5),
                        t2: DHCPv6TimeScale.FromDouble(0.75),
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

            DHCPv6Packet result = rootScope.HandleRebind(packet, GetServerPropertiesResolver());

            DHCPv6Lease lease = CheckLease(1, 2, expectedAddress, scopeId, rootScope, DateTime.UtcNow, clientDuid, iaId, false, withPrefixDelegation);
            CheckPacket(packet, expectedAddress, iaId, result, DHCPv6PacketTypes.REPLY, withPrefixDelegation == false ? DHCPv6PrefixDelegation.None : DHCPv6PrefixDelegation.FromValues(lease.PrefixDelegation.NetworkAddress, new IPv6SubnetMask(new IPv6SubnetMaskIdentifier(prefixLength)), prefixId));

            var revokedLease = rootScope.GetScopeById(scopeId).Leases.GetLeaseById(leaseId);
            Assert.False(revokedLease.IsActive());
            Assert.Equal(LeaseStates.Revoked, revokedLease.State);

            CheckEventAmount(4, rootScope);
            CheckRevokedEvent(0, scopeId, leaseId, rootScope);
            CheckLeaseCreatedEvent(1, clientDuid, iaId, scopeId, rootScope, expectedAddress, lease);
            CheckLeaseActivatedEvent(2, scopeId, lease.Id, rootScope);

            CheckHandeledEvent(3, packet, result, rootScope, scopeId, DHCPv6RebindHandledEvent.RebindErrors.NoError);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task HandleRebind_LeaseFound_ReuseIsNotAllowed_TwoPackets(Boolean withPrefixDelegation)
        {
            Random random = new Random();

            UInt32 prefixId = random.NextBoolean() == false ? random.NextUInt32() : 0;
            Byte prefixLength = (Byte)random.Next(34, 64);
            IPv6Address prefixNetworkAddress = IPv6Address.FromString("fd01::0");

            DHCPv6PacketOption[] options = Array.Empty<DHCPv6PacketOption>();
            if (withPrefixDelegation == true)
            {
                options = new DHCPv6PacketOption[] {
                new DHCPv6PacketIdentityAssociationPrefixDelegationOption(prefixId,TimeSpan.FromSeconds(random.Next()),TimeSpan.FromSeconds(random.Next()),Array.Empty<DHCPv6PacketSuboption>())
                };
            }

            var packet = GetRelayedRebindPacket(random, out IPv6Address _, out DUID clientDuid, out UInt32 iaId, true, options);

            var resolverInformations = GetMockupResolver(packet, out Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>> scopeResolverMock);

            Guid scopeId = random.NextGuid();
            Guid leaseId = random.NextGuid();
            IPv6Address leasedAddress = IPv6Address.FromString("fe80::01");
            IPv6Address expectedAddress = IPv6Address.FromString("fe80::03");

            DateTime leaseCreatedAt = DateTime.UtcNow.AddHours(-2);

            DHCPv6RootScope rootScope = GetRootScope(scopeResolverMock);
            rootScope.Load(new List<DomainEvent>{ new DHCPv6ScopeEvents.DHCPv6ScopeAddedEvent(
                new DHCPv6ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv6ScopeAddressProperties(
                        IPv6Address.FromString("fe80::01"),
                        IPv6Address.FromString("fe80::ff"),
                        new List<IPv6Address>{IPv6Address.FromString("fe80::02") },
                        t1: DHCPv6TimeScale.FromDouble(0.5),
                        t2: DHCPv6TimeScale.FromDouble(0.75),
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

            Guid serverId = random.NextGuid();

            DHCPv6Packet result = rootScope.HandleRebind(packet, GetServerPropertiesResolver(serverId));

            await Task.Delay(3000);
            DHCPv6Packet secondResult = rootScope.HandleRebind(packet, GetServerPropertiesResolver(serverId));
            Assert.Equal(result, secondResult);

            DHCPv6Lease lease = CheckLease(1, 2, expectedAddress, scopeId, rootScope, DateTime.UtcNow, clientDuid, iaId, false, withPrefixDelegation);
            CheckPacket(packet, expectedAddress, iaId, result, DHCPv6PacketTypes.REPLY, withPrefixDelegation == false ? DHCPv6PrefixDelegation.None : DHCPv6PrefixDelegation.FromValues(lease.PrefixDelegation.NetworkAddress, new IPv6SubnetMask(new IPv6SubnetMaskIdentifier(prefixLength)), prefixId));

            var revokedLease = rootScope.GetScopeById(scopeId).Leases.GetLeaseById(leaseId);
            Assert.False(revokedLease.IsActive());
            Assert.Equal(LeaseStates.Revoked, revokedLease.State);

            CheckEventAmount(5, rootScope);
            CheckRevokedEvent(0, scopeId, leaseId, rootScope);
            CheckLeaseCreatedEvent(1, clientDuid, iaId, scopeId, rootScope, expectedAddress, lease);
            CheckLeaseActivatedEvent(2, scopeId, lease.Id, rootScope);

            CheckHandeledEvent(3, packet, result, rootScope, scopeId, DHCPv6RebindHandledEvent.RebindErrors.NoError);
            CheckHandeledEvent(4, packet, result, rootScope, scopeId, DHCPv6RebindHandledEvent.RebindErrors.NoError);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void HandleRebind_NoLeaseFound(Boolean withPrefixDelegation)
        {
            Random random = new Random();

            UInt32 prefixId = random.NextBoolean() == false ? random.NextUInt32() : 0;
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

            var packet = GetRelayedRebindPacket(random, out IPv6Address leasedAddress, out _, out UInt32 iaId, true, options);

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

            DHCPv6Packet result = rootScope.HandleRebind(packet, GetServerPropertiesResolver());

            CheckErrorPacket(packet, leasedAddress, iaId, result, withPrefixDelegation == false ? DHCPv6PrefixDelegation.None : DHCPv6PrefixDelegation.FromValues(prefixNetworkAddress, new IPv6SubnetMask(new IPv6SubnetMaskIdentifier(prefixLength)), prefixId), DHCPv6StatusCodes.NoBinding);
            CheckEventAmount(1, rootScope);

            CheckHandeledEvent(0, packet, result, rootScope, scopeId, DHCPv6RebindHandledEvent.RebindErrors.LeaseNotFound);
        }



        [Fact]
        public void HandleRebind_OnlyPrefix()
        {
            Random random = new Random();

            UInt32 prefixId = random.NextBoolean() == false ? random.NextUInt32() : 0;
            Byte prefixLength = (Byte)random.Next(34, 64);
            IPv6Address prefixNetworkAddress = IPv6Address.FromString("fd01::0");

            DHCPv6PacketOption[] options = new DHCPv6PacketOption[] {
                new DHCPv6PacketIdentityAssociationPrefixDelegationOption(prefixId,TimeSpan.FromSeconds(random.Next()),TimeSpan.FromSeconds(random.Next()),new DHCPv6PacketSuboption[]{
                new DHCPv6PacketIdentityAssociationPrefixDelegationSuboption(TimeSpan.FromSeconds(random.Next()),TimeSpan.FromSeconds(random.Next()),prefixLength,prefixNetworkAddress,Array.Empty<DHCPv6PacketSuboption>())
                })
                };

            var packet = GetRelayedRebindPacket(random, out IPv6Address _, out DUID _, out UInt32 _, false, options);

            var resolverInformations = GetMockupResolver(packet, out Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>> scopeResolverMock);

            Guid scopeId = random.NextGuid();

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
            });

            DHCPv6Packet result = rootScope.HandleRebind(packet, GetServerPropertiesResolver());

            CheckErrorPacket(packet, IPv6Address.Empty, 0, result, DHCPv6PrefixDelegation.FromValues(prefixNetworkAddress, new IPv6SubnetMask(new IPv6SubnetMaskIdentifier(prefixLength)), prefixId), DHCPv6StatusCodes.NoBinding);
            CheckEventAmount(1, rootScope);

            CheckHandeledEvent(0, packet, result, rootScope, scopeId, DHCPv6RebindHandledEvent.RebindErrors.OnlyPrefixIsNotAllowed);
        }

        [Fact]
        public void HandleRebind_ScopeNotFound_ByResolver()
        {
            Random random = new Random();

            DHCPv6PacketOption[] options = Array.Empty<DHCPv6PacketOption>();

            var packet = GetRelayedRebindPacket(random, out IPv6Address _, out DUID _, out UInt32 _, true, options);
            GetMockupResolver(DHCPv6Packet.Empty, out Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>> scopeResolverMock);

            DHCPv6RootScope rootScope = GetRootScope(scopeResolverMock);

            DHCPv6Packet result = rootScope.HandleRebind(packet, GetServerPropertiesResolver());
            Assert.Equal(DHCPv6Packet.Empty, result);

            CheckEventAmount(1, rootScope);
            CheckHandeledEvent(0, packet, result, rootScope, null, DHCPv6RebindHandledEvent.RebindErrors.ScopeNotFound);
        }

        [Fact]
        public void HandleRebind_ScopeFound_ButPseudoResolverUsed()
        {
            Random random = new Random();

            var packet = GetRelayedRebindPacket(random, out IPv6Address _, out _, out _, true);
            var resolverInformations = GetMockupPseudoResolver(out Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>> scopeResolverMock);

            Guid scopeId = random.NextGuid();


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
                        addressAllocationStrategy: DHCPv6ScopeAddressProperties.AddressAllocationStrategies.Next),
                    ResolverInformation = resolverInformations,
                    Name = "Testscope",
                    Id = scopeId,
                }),
            });

            DHCPv6Packet result = rootScope.HandleRebind(packet, GetServerPropertiesResolver());
            Assert.Equal(DHCPv6Packet.Empty, result);

            CheckEventAmount(1, rootScope);
            CheckHandeledEvent(0, packet, result, rootScope, null, DHCPv6RebindHandledEvent.RebindErrors.ScopeNotFound);
        }
    }
}
