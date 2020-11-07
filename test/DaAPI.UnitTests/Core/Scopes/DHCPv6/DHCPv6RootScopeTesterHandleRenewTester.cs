using DaAPI.Core.Common;
using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Notifications.Triggers;
using DaAPI.Core.Packets.DHCPv6;
using DaAPI.Core.Scopes;
using DaAPI.Core.Scopes.DHCPv6;
using DaAPI.TestHelper;
using Microsoft.Extensions.Logging;
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
    public class DHCPv6RootScopeTesterHandleRenewTester : DHCPv6RootSoopeTesterBase
    {
        private void CheckHandeledEvent(Int32 index, DHCPv6Packet requestPacket, DHCPv6Packet result, DHCPv6RootScope rootScope, Guid? scopeId, DHCPv6RenewHandledEvent.RenewErrors error)
        {
            IEnumerable<DomainEvent> changes = rootScope.GetChanges();

            Assert.IsAssignableFrom<DHCPv6RenewHandledEvent>(changes.ElementAt(index));

            DHCPv6RenewHandledEvent handeledEvent = (DHCPv6RenewHandledEvent)changes.ElementAt(index);
            Assert.Equal(requestPacket, handeledEvent.Request);
            Assert.Equal(result, handeledEvent.Response);
            Assert.Equal(scopeId, handeledEvent.ScopeId);
            Assert.Equal(error == DHCPv6RenewHandledEvent.RenewErrors.NoError, handeledEvent.WasSuccessfullHandled);
            Assert.Equal(error, handeledEvent.Error);
        }

        private DHCPv6Packet GetRelayedRenewPacket(
          Random random, out IPv6Address usedAddress, out DUID clientDuid, out UInt32 iaId, Boolean withIdentity, params DHCPv6PacketOption[] options)
        {
            IPv6HeaderInformation headerInformation =
              new IPv6HeaderInformation(IPv6Address.FromString("fe80::1"), IPv6Address.FromString("fe80::1"));

            DHCPv6Packet innerPacket = GetRenewPacket(random, out usedAddress, out clientDuid, out iaId, withIdentity, options);

            DHCPv6Packet packet = DHCPv6RelayPacket.AsOuterRelay(headerInformation, true, 1, random.GetIPv6Address(), random.GetIPv6Address(),
                Array.Empty<DHCPv6PacketOption>(), innerPacket);

            return packet;
        }

        private DHCPv6Packet GetRenewPacket(
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
                DHCPv6PacketTypes.RENEW, packetOptions);

            return packet;
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void HandleRenew_LeaseFound_ReuseIsAllowed(Boolean withPrefixDelegation, Boolean isUnicast)
        {
            Random random = new Random();

            UInt32 prefixId = random.NextUInt32();
            Byte prefixLength = (Byte)random.Next(20, 60);
            IPv6Address prefixNetworkAddress = IPv6Address.FromString("fd01::0");

            DHCPv6PacketOption[] options = Array.Empty<DHCPv6PacketOption>();
            if (withPrefixDelegation == true)
            {
                options = new DHCPv6PacketOption[] {
                new DHCPv6PacketIdentityAssociationPrefixDelegationOption(prefixId,TimeSpan.FromSeconds(random.Next()),TimeSpan.FromSeconds(random.Next()),Array.Empty<DHCPv6PacketSuboption>())
                };
            }

            var packet = isUnicast == true ?
                    GetRenewPacket(random, out IPv6Address _, out DUID clientDuid, out UInt32 iaId, true, options) :
                    GetRelayedRenewPacket(random, out IPv6Address _, out  clientDuid, out iaId, true, options);

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

            DHCPv6Packet result = rootScope.HandleRenew(packet, GetServerPropertiesResolver());
            CheckPacket(packet, leasedAddress, iaId, result, DHCPv6PacketTypes.REPLY, withPrefixDelegation == false ? DHCPv6PrefixDelegation.None : DHCPv6PrefixDelegation.FromValues(prefixNetworkAddress, new IPv6SubnetMask(new IPv6SubnetMaskIdentifier(prefixLength)), prefixId));

            DHCPv6Lease lease = CheckLease(0, 1, leasedAddress, scopeId, rootScope, leaseCreatedAt, clientDuid, iaId, false, withPrefixDelegation);

            CheckEventAmount(2, rootScope);
            CheckLeaseRenewdEvent(scopeId, rootScope, lease, false, false);
            CheckHandeledEvent(1, packet, result, rootScope, scopeId, DHCPv6RenewHandledEvent.RenewErrors.NoError);
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void HandleRenew_LeaseFound_ReuseAllowed_ButLeaseNotExtentable(Boolean withPrefixDelegation, Boolean isUnicast)
        {
            Random random = new Random();

            UInt32 prefixId = random.NextUInt32();
            Byte prefixLength = (Byte)random.Next(34, 64);
            IPv6Address prefixNetworkAddress = IPv6Address.FromString("fd01::0");

            DHCPv6PacketOption[] options = Array.Empty<DHCPv6PacketOption>();
            if (withPrefixDelegation == true)
            {
                options = new DHCPv6PacketOption[] {
                new DHCPv6PacketIdentityAssociationPrefixDelegationOption(prefixId,TimeSpan.FromSeconds(random.Next()),TimeSpan.FromSeconds(random.Next()),Array.Empty<DHCPv6PacketSuboption>())
                };
            }

            var packet = isUnicast == true ?
                    GetRenewPacket(random, out IPv6Address _, out DUID clientDuid, out UInt32 iaId, true, options) :
                    GetRelayedRenewPacket(random, out IPv6Address _, out clientDuid, out iaId, true, options);

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
                    ScopeId = scopeId,
                }
            });

            DHCPv6Packet result = rootScope.HandleRenew(packet, GetServerPropertiesResolver());

            DHCPv6Lease lease = CheckLease(1, 2, expectedAddress, scopeId, rootScope, DateTime.UtcNow, clientDuid, iaId, false, withPrefixDelegation);
            CheckPacket(packet, expectedAddress, iaId, result, DHCPv6PacketTypes.REPLY, withPrefixDelegation == false ? DHCPv6PrefixDelegation.None : DHCPv6PrefixDelegation.FromValues(lease.PrefixDelegation.NetworkAddress, new IPv6SubnetMask(new IPv6SubnetMaskIdentifier(prefixLength)), prefixId));

            var canceledLease = rootScope.GetScopeById(scopeId).Leases.GetLeaseById(leaseId);
            Assert.Equal(LeaseStates.Canceled, canceledLease.State);

            CheckEventAmount(3, rootScope);
            CheckLeaseCreatedEvent(0, clientDuid, iaId, scopeId, rootScope, expectedAddress, lease);
            CheckLeaseActivatedEvent(1, scopeId, lease.Id, rootScope);
            CheckHandeledEvent(2, packet, result, rootScope, scopeId, DHCPv6RenewHandledEvent.RenewErrors.NoError);
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void HandleRenew_LeaseFound_ReuseIsNotAllowed(Boolean withPrefixDelegation, Boolean isUnicast)
        {
            Random random = new Random();

            UInt32 prefixId = random.NextUInt32();
            Byte prefixLength = (Byte)random.Next(34, 64);
            IPv6Address prefixNetworkAddress = IPv6Address.FromString("fd01::0");

            DHCPv6PacketOption[] options = Array.Empty<DHCPv6PacketOption>();
            if (withPrefixDelegation == true)
            {
                options = new DHCPv6PacketOption[] {
                new DHCPv6PacketIdentityAssociationPrefixDelegationOption(prefixId,TimeSpan.FromSeconds(random.Next()),TimeSpan.FromSeconds(random.Next()),Array.Empty<DHCPv6PacketSuboption>())
                };
            }

            var packet = isUnicast == true ?
                    GetRenewPacket(random, out IPv6Address _, out DUID clientDuid, out UInt32 iaId, true, options) :
                    GetRelayedRenewPacket(random, out IPv6Address _, out clientDuid, out iaId, true, options);

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

            DHCPv6Packet result = rootScope.HandleRenew(packet, GetServerPropertiesResolver());

            DHCPv6Lease lease = CheckLease(1, 2, expectedAddress, scopeId, rootScope, DateTime.UtcNow, clientDuid, iaId, false, withPrefixDelegation);
            CheckPacket(packet, expectedAddress, iaId, result, DHCPv6PacketTypes.REPLY, withPrefixDelegation == false ? DHCPv6PrefixDelegation.None : DHCPv6PrefixDelegation.FromValues(lease.PrefixDelegation.NetworkAddress, new IPv6SubnetMask(new IPv6SubnetMaskIdentifier(prefixLength)), prefixId));

            var revokedLease = rootScope.GetScopeById(scopeId).Leases.GetLeaseById(leaseId);
            Assert.False(revokedLease.IsActive());
            Assert.Equal(LeaseStates.Revoked, revokedLease.State);

            CheckEventAmount(4, rootScope);
            CheckRevokedEvent(0, scopeId, leaseId, rootScope);
            CheckLeaseCreatedEvent(1, clientDuid, iaId, scopeId, rootScope, expectedAddress, lease);
            CheckLeaseActivatedEvent(2, scopeId, lease.Id, rootScope);

            CheckHandeledEvent(3, packet, result, rootScope, scopeId, DHCPv6RenewHandledEvent.RenewErrors.NoError);
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public async Task HandleRenew_LeaseFound_ReuseIsNotAllowed_TwoPackets(Boolean withPrefixDelegation, Boolean isUnicast)
        {
            Random random = new Random();

            UInt32 prefixId = random.NextUInt32();
            Byte prefixLength = (Byte)random.Next(34, 64);
            IPv6Address prefixNetworkAddress = IPv6Address.FromString("fd01::0");

            DHCPv6PacketOption[] options = Array.Empty<DHCPv6PacketOption>();
            if (withPrefixDelegation == true)
            {
                options = new DHCPv6PacketOption[] {
                new DHCPv6PacketIdentityAssociationPrefixDelegationOption(prefixId,TimeSpan.FromSeconds(random.Next()),TimeSpan.FromSeconds(random.Next()),Array.Empty<DHCPv6PacketSuboption>())
                };
            }

            var packet = isUnicast == true ?
                    GetRenewPacket(random, out IPv6Address _, out DUID clientDuid, out UInt32 iaId, true, options) :
                    GetRelayedRenewPacket(random, out IPv6Address _, out clientDuid, out iaId, true, options);

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

            Guid serverId = random.NextGuid();

            DHCPv6Packet result = rootScope.HandleRenew(packet, GetServerPropertiesResolver(serverId));
            await Task.Delay(200);
            DHCPv6Packet secondResult = rootScope.HandleRenew(packet, GetServerPropertiesResolver(serverId));
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

            CheckHandeledEvent(3, packet, result, rootScope, scopeId, DHCPv6RenewHandledEvent.RenewErrors.NoError);
            CheckHandeledEvent(4, packet, result, rootScope, scopeId, DHCPv6RenewHandledEvent.RenewErrors.NoError);
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void HandleRenew_NoLeaseFound(Boolean withPrefixDelegation, Boolean isUnicast)
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
                    GetRenewPacket(random, out IPv6Address leasedAddress, out _, out UInt32 iaId, true, options) :
                    GetRelayedRenewPacket(random, out leasedAddress, out _, out iaId, true, options);

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

            DHCPv6Packet result = rootScope.HandleRenew(packet, GetServerPropertiesResolver());

            CheckErrorPacket(packet, leasedAddress, iaId, result, withPrefixDelegation == false ? DHCPv6PrefixDelegation.None : DHCPv6PrefixDelegation.FromValues(prefixNetworkAddress, new IPv6SubnetMask(new IPv6SubnetMaskIdentifier(prefixLength)), prefixId), DHCPv6StatusCodes.NoBinding);
            CheckEventAmount(1, rootScope);

            CheckHandeledEvent(0, packet, result, rootScope, scopeId, DHCPv6RenewHandledEvent.RenewErrors.LeaseNotFound);
        }


        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void HandleRenew_OnlyPrefix(Boolean isUnicast)
        {
            Random random = new Random();

            UInt32 prefixId = random.NextUInt32();
            Byte prefixLength = (Byte)random.Next(34, 64);
            IPv6Address prefixNetworkAddress = IPv6Address.FromString("fd01::0");

            DHCPv6PacketOption[] options = new DHCPv6PacketOption[] {
                new DHCPv6PacketIdentityAssociationPrefixDelegationOption(prefixId,TimeSpan.FromSeconds(random.Next()),TimeSpan.FromSeconds(random.Next()),new DHCPv6PacketSuboption[]{
                new DHCPv6PacketIdentityAssociationPrefixDelegationSuboption(TimeSpan.FromSeconds(random.Next()),TimeSpan.FromSeconds(random.Next()),prefixLength,prefixNetworkAddress,Array.Empty<DHCPv6PacketSuboption>())
                })
                };

            var packet = isUnicast == true ?
                    GetRenewPacket(random, out IPv6Address _, out DUID _, out UInt32 _, false, options) :
                    GetRelayedRenewPacket(random, out IPv6Address _, out DUID _, out UInt32 _, false, options);

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

            DHCPv6Packet result = rootScope.HandleRenew(packet, GetServerPropertiesResolver());

            CheckErrorPacket(packet, IPv6Address.Empty, 0, result, DHCPv6PrefixDelegation.FromValues(prefixNetworkAddress, new IPv6SubnetMask(new IPv6SubnetMaskIdentifier(prefixLength)), prefixId), DHCPv6StatusCodes.NoBinding);
            CheckEventAmount(1, rootScope);

            CheckHandeledEvent(0, packet, result, rootScope, scopeId, DHCPv6RenewHandledEvent.RenewErrors.OnlyPrefixIsNotAllowed);
        }

        [Theory]
        [InlineData(false, false, false)]
        [InlineData(false, false, true)]
        [InlineData(false, true, false)]
        [InlineData(false, true, true)]
        [InlineData(true, false, false)]
        [InlineData(true, false, true)]
        [InlineData(true, true, false)]
        [InlineData(true, true, true)]
        public async Task HandleRenew_OnlyPrefix_LeaseFound_ReuseAddress(Boolean useNullIaIdAsPrefix, Boolean isUnicast, Boolean sendSecondPacket)
        {
            Random random = new Random();

            UInt32 prefixId = useNullIaIdAsPrefix == false ? random.NextUInt32() : 0;
            Byte prefixLength = 32;
            IPv6Address prefixNetworkAddress = IPv6Address.FromString("2140:1::0");
            IPv6Address leasedAddress = IPv6Address.FromString("fe80::4");

            DHCPv6PacketOption[] options = new DHCPv6PacketOption[] {
                new DHCPv6PacketIdentityAssociationPrefixDelegationOption(prefixId,TimeSpan.FromSeconds(random.Next()),TimeSpan.FromSeconds(random.Next()),new DHCPv6PacketSuboption[]{
                new DHCPv6PacketIdentityAssociationPrefixDelegationSuboption(TimeSpan.FromSeconds(random.Next()),TimeSpan.FromSeconds(random.Next()),prefixLength,prefixNetworkAddress,Array.Empty<DHCPv6PacketSuboption>())
                })
                };

            var packet = isUnicast == true ?
                    GetRenewPacket(random, out IPv6Address _, out DUID clientDuid, out UInt32 _, false, options) :
                    GetRelayedRenewPacket(random, out IPv6Address _, out clientDuid, out UInt32 _, false, options);

            var resolverInformations = GetMockupResolver(packet, out Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>> scopeResolverMock);

            Guid scopeId = random.NextGuid();
            Guid leaseId = random.NextGuid();
            UInt32 iaId = random.NextUInt32();

            DateTime leaseCreatedAt = DateTime.UtcNow.AddHours(-random.Next(4, 10));

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
                        t1: DHCPv6TimeScale.FromDouble(0.5),
                        t2: DHCPv6TimeScale.FromDouble(0.75),
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
                    HasPrefixDelegation = true,
                    PrefixLength = prefixLength,
                    IdentityAssocationIdForPrefix = prefixId,
                    DelegatedNetworkAddress = prefixNetworkAddress,
                },
                new DHCPv6LeaseActivatedEvent
                {
                    EntityId = leaseId,
                    ScopeId = scopeId,
                },
                new DHCPv6LeaseRenewedEvent
                {
                    EntityId = leaseId,
                    ScopeId = scopeId,
                    ResetPrefix = true,
                    End = DateTime.UtcNow.AddDays(1)
                }
            });

            DHCPv6Packet result = rootScope.HandleRenew(packet, GetServerPropertiesResolver());
            DHCPv6Packet secondResponse = null;

            DHCPv6Lease lease = CheckLease(0, 1, leasedAddress, scopeId, rootScope, leaseCreatedAt, clientDuid, iaId, false, true);
            CheckPacket(packet, leasedAddress, 0, result, DHCPv6PacketTypes.REPLY, DHCPv6PrefixDelegation.FromValues(lease.PrefixDelegation.NetworkAddress, new IPv6SubnetMask(new IPv6SubnetMaskIdentifier(prefixLength)), prefixId));
            if (sendSecondPacket == true)
            {
                await Task.Delay(1000);
                secondResponse = rootScope.HandleRenew(packet, GetServerPropertiesResolver());
                CheckPacket(packet, leasedAddress, 0, secondResponse, DHCPv6PacketTypes.REPLY, DHCPv6PrefixDelegation.FromValues(lease.PrefixDelegation.NetworkAddress, new IPv6SubnetMask(new IPv6SubnetMaskIdentifier(prefixLength)), prefixId));
            }

            CheckEventAmount(sendSecondPacket == false ? 2 : 3, rootScope);

            CheckDHCPv6LeasePrefixAddedEvent(0, prefixId, scopeId, rootScope, lease);
            CheckHandeledEvent(1, packet, result, rootScope, scopeId, DHCPv6RenewHandledEvent.RenewErrors.NoError);

            if (sendSecondPacket == true)
            {
                CheckHandeledEvent(2, packet, secondResponse, rootScope, scopeId, DHCPv6RenewHandledEvent.RenewErrors.NoError);

            }

            var trigger = CheckTrigger<PrefixEdgeRouterBindingUpdatedTrigger>(rootScope);
            Assert.Null(trigger.OldBinding);
            Assert.NotNull(trigger.NewBinding);

            Assert.Equal(leasedAddress, trigger.NewBinding.Host);
            Assert.Equal(prefixNetworkAddress, trigger.NewBinding.Prefix);
            Assert.Equal(new IPv6SubnetMask(new IPv6SubnetMaskIdentifier(prefixLength)), trigger.NewBinding.Mask);
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public async Task HandleRenew_OnlyPrefix_LeaseFound_NotReuseAddress(Boolean isUnicast, Boolean sendSecondPacket)
        {
            Random random = new Random();

            UInt32 prefixId = random.NextUInt32();
            Byte prefixLength = 36;
            IPv6Address prefixNetworkAddress = IPv6Address.FromString("2140:1::0");
            IPv6Address leasedAddress = IPv6Address.FromString("fe80::4");

            DHCPv6PacketOption[] options = new DHCPv6PacketOption[] {
                new DHCPv6PacketIdentityAssociationPrefixDelegationOption(prefixId,TimeSpan.FromSeconds(random.Next()),TimeSpan.FromSeconds(random.Next()),new DHCPv6PacketSuboption[]{
                new DHCPv6PacketIdentityAssociationPrefixDelegationSuboption(TimeSpan.FromSeconds(random.Next()),TimeSpan.FromSeconds(random.Next()),prefixLength,prefixNetworkAddress,Array.Empty<DHCPv6PacketSuboption>())
                })
                };

            var packet = isUnicast == true ?
                    GetRenewPacket(random, out IPv6Address _, out DUID clientDuid, out UInt32 _, false, options) :
                    GetRelayedRenewPacket(random, out IPv6Address _, out clientDuid, out UInt32 _, false, options);

            var resolverInformations = GetMockupResolver(packet, out Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>> scopeResolverMock);

            Guid scopeId = random.NextGuid();
            Guid leaseId = random.NextGuid();
            UInt32 iaId = random.NextUInt32();

            DateTime leaseCreatedAt = DateTime.UtcNow.AddHours(-random.Next(4, 10));

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
                        t1: DHCPv6TimeScale.FromDouble(0.5),
                        t2: DHCPv6TimeScale.FromDouble(0.75),
                        reuseAddressIfPossible: false,
                        prefixDelgationInfo: DHCPv6PrefixDelgationInfo.FromValues(IPv6Address.FromString("2140:1::0"),new IPv6SubnetMaskIdentifier(32),new IPv6SubnetMaskIdentifier(prefixLength)),
                        addressAllocationStrategy: DHCPv6ScopeAddressProperties.AddressAllocationStrategies.Random),
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
                    HasPrefixDelegation = true,
                    PrefixLength = prefixLength,
                    IdentityAssocationIdForPrefix = prefixId,
                    DelegatedNetworkAddress = prefixNetworkAddress,
                },
                new DHCPv6LeaseActivatedEvent
                {
                    EntityId = leaseId,
                    ScopeId = scopeId,
                },
                new DHCPv6LeaseRenewedEvent
                {
                    EntityId = leaseId,
                    ScopeId = scopeId,
                    ResetPrefix = true,
                    End = DateTime.UtcNow.AddDays(1)
                }
            });

            DHCPv6Packet result = rootScope.HandleRenew(packet, GetServerPropertiesResolver());
            DHCPv6Lease lease = CheckLease(0, 1, leasedAddress, scopeId, rootScope, leaseCreatedAt, clientDuid, iaId, false, true);

            CheckPacket(packet, leasedAddress, 0, result, DHCPv6PacketTypes.REPLY, DHCPv6PrefixDelegation.FromValues(lease.PrefixDelegation.NetworkAddress, new IPv6SubnetMask(new IPv6SubnetMaskIdentifier(prefixLength)), prefixId));
            DHCPv6Packet secondResponse = null;
            if (sendSecondPacket == true)
            {
                await Task.Delay(1000);
                secondResponse = rootScope.HandleRenew(packet, GetServerPropertiesResolver());
                CheckPacket(packet, leasedAddress, 0, secondResponse, DHCPv6PacketTypes.REPLY, DHCPv6PrefixDelegation.FromValues(lease.PrefixDelegation.NetworkAddress, new IPv6SubnetMask(new IPv6SubnetMaskIdentifier(prefixLength)), prefixId));
            }

            CheckPacket(packet, leasedAddress, 0, result, DHCPv6PacketTypes.REPLY, DHCPv6PrefixDelegation.FromValues(lease.PrefixDelegation.NetworkAddress, new IPv6SubnetMask(new IPv6SubnetMaskIdentifier(prefixLength)), prefixId));

            CheckEventAmount(sendSecondPacket == false ? 2 : 3, rootScope);

            CheckDHCPv6LeasePrefixAddedEvent(0, prefixId, scopeId, rootScope, lease);
            CheckHandeledEvent(1, packet, result, rootScope, scopeId, DHCPv6RenewHandledEvent.RenewErrors.NoError);
            if (sendSecondPacket == true)
            {
                CheckHandeledEvent(2, packet, secondResponse, rootScope, scopeId, DHCPv6RenewHandledEvent.RenewErrors.NoError);
            }

            var trigger = CheckTrigger<PrefixEdgeRouterBindingUpdatedTrigger>(rootScope);
            Assert.Null(trigger.OldBinding);
            Assert.NotNull(trigger.NewBinding);

            Assert.Equal(leasedAddress, trigger.NewBinding.Host);
            Assert.Equal(lease.PrefixDelegation.NetworkAddress, trigger.NewBinding.Prefix);
            Assert.Equal(new IPv6SubnetMask(new IPv6SubnetMaskIdentifier(prefixLength)), trigger.NewBinding.Mask);
        }

        [Fact]
        public void HandleRenew_ScopeNotFound_ByResolver()
        {
            Random random = new Random();

            DHCPv6PacketOption[] options = Array.Empty<DHCPv6PacketOption>();

            var packet = GetRelayedRenewPacket(random, out IPv6Address _, out DUID _, out UInt32 _, true, options);
            GetMockupResolver(DHCPv6Packet.Empty, out Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>> scopeResolverMock);

            DHCPv6RootScope rootScope = GetRootScope(scopeResolverMock);

            DHCPv6Packet result = rootScope.HandleRenew(packet, GetServerPropertiesResolver());
            Assert.Equal(DHCPv6Packet.Empty, result);

            CheckEventAmount(1, rootScope);
            CheckHandeledEvent(0, packet, result, rootScope, null, DHCPv6RenewHandledEvent.RenewErrors.ScopeNotFound);
        }

        [Fact]
        public void HandleRenew_ScopeNotFound_ByAddress()
        {
            Random random = new Random();

            var packet = GetRenewPacket(random, out _, out _, out _, true);

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

            DHCPv6Packet result = rootScope.HandleRenew(packet, GetServerPropertiesResolver());
            Assert.Equal(DHCPv6Packet.Empty, result);

            //CheckErrorPacket(leasedAddress, iaId, result, DHCPv6PrefixDelegation.None,DHCPv6StatusCodes.NoBinding);
            CheckEventAmount(1, rootScope);
            CheckHandeledEvent(0, packet, result, rootScope, null, DHCPv6RenewHandledEvent.RenewErrors.ScopeNotFound);
        }
    }
}
