using DaAPI.Core.Common;
using DaAPI.Core.Packets.DHCPv4;
using DaAPI.Core.Scopes.DHCPv4;
using DaAPI.Core.Services;
using DaAPI.TestHelper;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using static DaAPI.Core.Packets.DHCPv4.DHCPv4Packet;
using static DaAPI.Core.Scopes.DHCPv4.DHCPv4Lease;
using static DaAPI.Core.Scopes.DHCPv4.DHCPv4LeaseEvents;
using static DaAPI.Core.Scopes.DHCPv4.DHCPv4PacketHandledEvents;
using static DaAPI.Core.Scopes.DHCPv4.DHCPv4ScopeEvents;

namespace DaAPI.UnitTests.Core.Scopes.DHCPv4
{
    public class DHCPv4RootScopeTesterHandleDiscoverTester : DHCPv4RootSCopeTesterBase
    {

        private void CheckPacket(IPv4Address expectedAdress, DHCPv4Packet result)
        {
            Assert.NotNull(result);
            Assert.NotEqual(DHCPv4Packet.Empty, result);
            Assert.True(result.IsValid);

            Assert.Equal(expectedAdress, result.YourIPAdress);
            Assert.Equal(DHCPv4MessagesTypes.DHCPOFFER, result.MessageType);
        }

        private static void CheckLeaseRenewdEvent(
    Guid scopeId,
    DHCPv4RootScope rootScope, DHCPv4Lease lease)
        {
            IEnumerable<DomainEvent> changes = rootScope.GetChanges();
            Assert.NotNull(changes);
            Assert.Equal(2, changes.Count());

            Assert.IsAssignableFrom<DHCPv4LeaseRenewedEvent>(changes.First());

            DHCPv4LeaseRenewedEvent createdEvent = (DHCPv4LeaseRenewedEvent)changes.First();
            Assert.NotNull(createdEvent);

            Assert.Equal(scopeId, createdEvent.ScopeId);
            Assert.NotEqual(Guid.Empty, lease.Id);

            Assert.Equal(lease.Id, createdEvent.EntityId);
            Assert.True(createdEvent.Reset);

            Assert.Equal(lease.End, createdEvent.End);
        }

        private void CheckHandeledEvent(Int32 index, DHCPv4Packet discoverPacket, DHCPv4Packet result, DHCPv4RootScope rootScope, Guid scopeId)
        {
            IEnumerable<DomainEvent> changes = rootScope.GetChanges();

            Assert.IsAssignableFrom<DHCPv4DiscoverHandledEvent>(changes.ElementAt(index));

            DHCPv4DiscoverHandledEvent handeledEvent = (DHCPv4DiscoverHandledEvent)changes.ElementAt(index);
            Assert.Equal(discoverPacket, handeledEvent.Request);
            Assert.Equal(result, handeledEvent.Response);
            Assert.Equal(scopeId, handeledEvent.ScopeId);
            Assert.True(handeledEvent.WasSuccessfullHandled);
        }

        private void CheckHandeledEventNotSuccessfull(Int32 index, DHCPv4Packet discoverPacket, DHCPv4RootScope rootScope, Guid scopeId)
        {
            IEnumerable<DomainEvent> changes = rootScope.GetChanges();

            Assert.IsAssignableFrom<DHCPv4DiscoverHandledEvent>(changes.ElementAt(index));

            DHCPv4DiscoverHandledEvent handeledEvent = (DHCPv4DiscoverHandledEvent)changes.ElementAt(index);
            Assert.Equal(discoverPacket, handeledEvent.Request);
            Assert.Equal(DHCPv4Packet.Empty, handeledEvent.Response);
            Assert.Equal(scopeId, handeledEvent.ScopeId);
            Assert.False(handeledEvent.WasSuccessfullHandled);
        }
     
        [Fact]
        public void HandleDiscover_NoLeaseFound_AddressAvaiable()
        {
            Random random = new Random();

            IPv4HeaderInformation headerInformation =
                new IPv4HeaderInformation(IPv4Address.Empty, IPv4Address.Broadcast);

            Byte[] clientMacAdress = random.NextBytes(6);

            DHCPv4Packet discoverPacket = new DHCPv4Packet(
                headerInformation, clientMacAdress, (UInt32)random.Next(),
                IPv4Address.Empty, IPv4Address.Empty, IPv4Address.Empty,
                new DHCPv4PacketMessageTypeOption(DHCPv4Packet.DHCPv4MessagesTypes.DHCPDISCOVER)
            );

            Mock<IDHCPv4ScopeResolverManager> scopeResolverMock =
                new Mock<IDHCPv4ScopeResolverManager>(MockBehavior.Strict);

            var resolverInformations = new DHCPv4CreateScopeResolverInformation
            {
                Typename = nameof(DHCPv4RelayAgentSubnetResolver),
            };

            Mock<IDHCPv4ScopeResolver> resolverMock = new Mock<IDHCPv4ScopeResolver>(MockBehavior.Strict);
            resolverMock.Setup(x => x.PacketMeetsCondition(discoverPacket)).Returns(true);
            resolverMock.SetupGet(x => x.HasUniqueIdentifier).Returns(false);

            scopeResolverMock.Setup(x => x.InitializeResolver(resolverInformations)).Returns(resolverMock.Object);

            Guid scopeId = random.NextGuid();

            DHCPv4RootScope rootScope = GetRootScope(scopeResolverMock);
            rootScope.Load(new List<DomainEvent>{ new DHCPv4ScopeEvents.DHCPv4ScopeAddedEvent(
                new DHCPv4ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv4ScopeAddressProperties(
                        IPv4Address.FromString("192.168.178.0"),
                        IPv4Address.FromString("192.168.178.255"),
                        new List<IPv4Address>{IPv4Address.FromString("192.168.178.1") },
                        validLifetime: TimeSpan.FromDays(1),
                        addressAllocationStrategy: DHCPv4ScopeAddressProperties.DHCPv4AddressAllocationStrategies.Next),
                    ResolverInformations = resolverInformations,
                    Name = "Testscope",
                    Id = scopeId,
                })
            });

            IPv4Address expectedAdress = IPv4Address.FromString("192.168.178.0");

            DHCPv4Packet result = rootScope.HandleDiscover(discoverPacket);
            CheckPacket(expectedAdress, result);

            DHCPv4Lease lease = CheckLease(0, 1, expectedAdress, scopeId, rootScope, DateTime.UtcNow);

            CheckEventAmount(2, rootScope);
            CheckLeaseCreatedEvent(0, clientMacAdress, scopeId, rootScope, expectedAdress, lease);
            CheckHandeledEvent(1, discoverPacket, result, rootScope, scopeId);
        }

        [Theory]
        [InlineData(DHCPv4ScopeAddressProperties.DHCPv4AddressAllocationStrategies.Next)]
        [InlineData(DHCPv4ScopeAddressProperties.DHCPv4AddressAllocationStrategies.Random)]
        public void HandleDiscover_NoLeaseFound_AddressAvaiable_SingleAddressPool(
            DHCPv4ScopeAddressProperties.DHCPv4AddressAllocationStrategies allocationStrategy
            )
        {
            Random random = new Random();

            IPv4HeaderInformation headerInformation =
                new IPv4HeaderInformation(IPv4Address.Empty, IPv4Address.Broadcast);

            Byte[] clientMacAdress = random.NextBytes(6);

            DHCPv4Packet discoverPacket = new DHCPv4Packet(
                headerInformation, clientMacAdress, (UInt32)random.Next(),
                IPv4Address.Empty, IPv4Address.Empty, IPv4Address.Empty,
                new DHCPv4PacketMessageTypeOption(DHCPv4Packet.DHCPv4MessagesTypes.DHCPDISCOVER)
            );

            Mock<IDHCPv4ScopeResolverManager> scopeResolverMock =
                new Mock<IDHCPv4ScopeResolverManager>(MockBehavior.Strict);

            var resolverInformations = new DHCPv4CreateScopeResolverInformation
            {
                Typename = nameof(DHCPv4RelayAgentSubnetResolver),
            };

            Mock<IDHCPv4ScopeResolver> resolverMock = new Mock<IDHCPv4ScopeResolver>(MockBehavior.Strict);
            resolverMock.Setup(x => x.PacketMeetsCondition(discoverPacket)).Returns(true);
            resolverMock.SetupGet(x => x.HasUniqueIdentifier).Returns(false);

            scopeResolverMock.Setup(x => x.InitializeResolver(resolverInformations)).Returns(resolverMock.Object);

            Guid scopeId = random.NextGuid();

            DHCPv4RootScope rootScope = GetRootScope(scopeResolverMock);
            rootScope.Load(new List<DomainEvent>{ new DHCPv4ScopeEvents.DHCPv4ScopeAddedEvent(
                new DHCPv4ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv4ScopeAddressProperties(
                        IPv4Address.FromString("192.168.178.10"),
                        IPv4Address.FromString("192.168.178.10"),
                        new List<IPv4Address>(),
                        validLifetime: TimeSpan.FromDays(1),
                        addressAllocationStrategy: allocationStrategy),
                    ResolverInformations = resolverInformations,
                    Name = "Testscope",
                    Id = scopeId,
                })
            });

            IPv4Address expectedAdress = IPv4Address.FromString("192.168.178.10");

            DHCPv4Packet result = rootScope.HandleDiscover(discoverPacket);
            CheckPacket(expectedAdress, result);

            DHCPv4Lease lease = CheckLease(0, 1, expectedAdress, scopeId, rootScope, DateTime.UtcNow);

            CheckEventAmount(2, rootScope);
            CheckLeaseCreatedEvent(0, clientMacAdress, scopeId, rootScope, expectedAdress, lease);
            CheckHandeledEvent(1, discoverPacket, result, rootScope, scopeId);
        }


        [Theory]
        [InlineData(DHCPv4ScopeAddressProperties.DHCPv4AddressAllocationStrategies.Next)]
        [InlineData(DHCPv4ScopeAddressProperties.DHCPv4AddressAllocationStrategies.Random)]
        public void HandleDiscover_NoLeaseFound_AddressAvaiable_FirstFreeAddress(
            DHCPv4ScopeAddressProperties.DHCPv4AddressAllocationStrategies allocationStrategy)
        {
            Random random = new Random();

            IPv4HeaderInformation headerInformation =
                new IPv4HeaderInformation(IPv4Address.Empty, IPv4Address.Broadcast);

            Byte[] clientMacAdress = random.NextBytes(6);

            DHCPv4Packet discoverPacket = new DHCPv4Packet(
                headerInformation, clientMacAdress, (UInt32)random.Next(),
                IPv4Address.Empty, IPv4Address.Empty, IPv4Address.Empty,
                new DHCPv4PacketMessageTypeOption(DHCPv4Packet.DHCPv4MessagesTypes.DHCPDISCOVER)
            );

            Mock<IDHCPv4ScopeResolverManager> scopeResolverMock =
                new Mock<IDHCPv4ScopeResolverManager>(MockBehavior.Strict);

            var resolverInformations = new DHCPv4CreateScopeResolverInformation
            {
                Typename = nameof(DHCPv4RelayAgentSubnetResolver),
            };

            Byte avaiableAddress = random.NextByte();
            List<IPv4Address> excludedAddress = new List<IPv4Address>(255);
            for (int i = 0; i <= Byte.MaxValue; i++)
            {
                if (i != avaiableAddress)
                {
                    excludedAddress.Add(IPv4Address.FromBytes(192, 168, 178, (Byte)i));
                }
            }

            Mock<IDHCPv4ScopeResolver> resolverMock = new Mock<IDHCPv4ScopeResolver>(MockBehavior.Strict);
            resolverMock.Setup(x => x.PacketMeetsCondition(discoverPacket)).Returns(true);
            resolverMock.SetupGet(x => x.HasUniqueIdentifier).Returns(false);

            scopeResolverMock.Setup(x => x.InitializeResolver(resolverInformations)).Returns(resolverMock.Object);

            Guid scopeId = random.NextGuid();

            DHCPv4RootScope rootScope = GetRootScope(scopeResolverMock);
            rootScope.Load(new List<DomainEvent>{ new DHCPv4ScopeEvents.DHCPv4ScopeAddedEvent(
                new DHCPv4ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv4ScopeAddressProperties(
                        IPv4Address.FromString("192.168.178.0"),
                        IPv4Address.FromString("192.168.178.255"),
                        excludedAddress,
                        validLifetime: TimeSpan.FromDays(1),
                        addressAllocationStrategy: allocationStrategy),
                    ResolverInformations = resolverInformations,
                    Name = "Testscope",
                    Id = scopeId,
                })
            });

            IPv4Address expectedAdress = IPv4Address.FromBytes(192,168,178,avaiableAddress);

            DHCPv4Packet result = rootScope.HandleDiscover(discoverPacket);
            CheckPacket(expectedAdress, result);

            DHCPv4Lease lease = CheckLease(0, 1, expectedAdress, scopeId, rootScope, DateTime.UtcNow);

            CheckEventAmount(2, rootScope);
            CheckLeaseCreatedEvent(0, clientMacAdress, scopeId, rootScope, expectedAdress, lease);
            CheckHandeledEvent(1, discoverPacket, result, rootScope, scopeId);
        }


     

        [Fact]
        public void HandleDiscover_LeaseFound_ReuseEnabled()
        {
            Random random = new Random();

            IPv4HeaderInformation headerInformation =
                new IPv4HeaderInformation(IPv4Address.Empty, IPv4Address.Broadcast);

            Byte[] clientMacAdress = random.NextBytes(6);

            DHCPv4Packet discoverPacket = new DHCPv4Packet(
                headerInformation, clientMacAdress, (UInt32)random.Next(),
                IPv4Address.Empty, IPv4Address.Empty, IPv4Address.Empty,
                new DHCPv4PacketMessageTypeOption(DHCPv4Packet.DHCPv4MessagesTypes.DHCPDISCOVER)
            );

            Mock<IDHCPv4ScopeResolverManager> scopeResolverMock =
                new Mock<IDHCPv4ScopeResolverManager>(MockBehavior.Strict);

            var resolverInformations = new DHCPv4CreateScopeResolverInformation
            {
                Typename = nameof(DHCPv4RelayAgentSubnetResolver),
            };

            Mock<IDHCPv4ScopeResolver> resolverMock = new Mock<IDHCPv4ScopeResolver>(MockBehavior.Strict);
            resolverMock.Setup(x => x.PacketMeetsCondition(discoverPacket)).Returns(true);
            resolverMock.SetupGet(x => x.HasUniqueIdentifier).Returns(false);

            scopeResolverMock.Setup(x => x.InitializeResolver(resolverInformations)).Returns(resolverMock.Object);

            Guid scopeId = random.NextGuid();
            Guid leaseId = random.NextGuid();

            IPv4Address leasedAddress = IPv4Address.FromString("192.168.178.10");
            DateTime leaseCreatedAt = DateTime.UtcNow.AddHours(-1);
            DHCPv4RootScope rootScope = GetRootScope(scopeResolverMock);
            rootScope.Load(new List<DomainEvent>{ new DHCPv4ScopeEvents.DHCPv4ScopeAddedEvent(
                new DHCPv4ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv4ScopeAddressProperties(
                        IPv4Address.FromString("192.168.178.0"),
                        IPv4Address.FromString("192.168.178.255"),
                        new List<IPv4Address>{IPv4Address.FromString("192.168.178.1") },
                        validLifetime: TimeSpan.FromDays(1),
                        reuseAddressIfPossible: true,
                        addressAllocationStrategy: DHCPv4ScopeAddressProperties.DHCPv4AddressAllocationStrategies.Next),
                    ResolverInformations = resolverInformations,
                    Name = "Testscope",
                    Id = scopeId,
                }),
                new DHCPv4LeaseCreatedEvent
                {
                    EntityId = leaseId,
                    Address = leasedAddress,
                    ClientIdentifier = DHCPv4ClientIdentifier.FromHwAddress(clientMacAdress),
                    ScopeId = scopeId,
                    StartedAt = leaseCreatedAt,
                    ValidUntil = DateTime.UtcNow.AddDays(1),
                 },
            }); ;

            DHCPv4Packet result = rootScope.HandleDiscover(discoverPacket);
            CheckPacket(leasedAddress, result);

            DHCPv4Lease lease = CheckLease(0, 1, leasedAddress, scopeId, rootScope, leaseCreatedAt);
            Assert.Equal(leaseId, lease.Id);

            CheckEventAmount(2, rootScope);
            CheckLeaseRenewdEvent(scopeId, rootScope, lease);
            CheckHandeledEvent(1, discoverPacket, result, rootScope, scopeId);
        }

        [Fact]
        public void HandleDiscover_LeaseFound_ReuseNotEnabled()
        {
            Random random = new Random();

            IPv4HeaderInformation headerInformation =
                new IPv4HeaderInformation(IPv4Address.Empty, IPv4Address.Broadcast);

            Byte[] clientMacAdress = random.NextBytes(6);

            DHCPv4Packet discoverPacket = new DHCPv4Packet(
                headerInformation, clientMacAdress, (UInt32)random.Next(),
                IPv4Address.Empty, IPv4Address.Empty, IPv4Address.Empty,
                new DHCPv4PacketMessageTypeOption(DHCPv4Packet.DHCPv4MessagesTypes.DHCPDISCOVER)
            );

            Mock<IDHCPv4ScopeResolverManager> scopeResolverMock =
                new Mock<IDHCPv4ScopeResolverManager>(MockBehavior.Strict);

            var resolverInformations = new DHCPv4CreateScopeResolverInformation
            {
                Typename = nameof(DHCPv4RelayAgentSubnetResolver),
            };

            Mock<IDHCPv4ScopeResolver> resolverMock = new Mock<IDHCPv4ScopeResolver>(MockBehavior.Strict);
            resolverMock.Setup(x => x.PacketMeetsCondition(discoverPacket)).Returns(true);
            resolverMock.SetupGet(x => x.HasUniqueIdentifier).Returns(false);

            scopeResolverMock.Setup(x => x.InitializeResolver(resolverInformations)).Returns(resolverMock.Object);

            Guid scopeId = random.NextGuid();
            Guid leaseId = random.NextGuid();

            IPv4Address leasedAddress = IPv4Address.FromString("192.168.178.10");
            IPv4Address expectedAddress = IPv4Address.FromString("192.168.178.2");

            DateTime leaseCreatedAt = DateTime.UtcNow.AddHours(-1);
            DHCPv4RootScope rootScope = GetRootScope(scopeResolverMock);
            rootScope.Load(new List<DomainEvent>{ new DHCPv4ScopeEvents.DHCPv4ScopeAddedEvent(
                new DHCPv4ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv4ScopeAddressProperties(
                        IPv4Address.FromString("192.168.178.1"),
                        IPv4Address.FromString("192.168.178.255"),
                        new List<IPv4Address>{
                            IPv4Address.FromString("192.168.178.1") 
                        },
                        validLifetime: TimeSpan.FromDays(1),
                        reuseAddressIfPossible: false,
                        addressAllocationStrategy: DHCPv4ScopeAddressProperties.DHCPv4AddressAllocationStrategies.Next),
                    ResolverInformations = resolverInformations,
                    Name = "Testscope",
                    Id = scopeId,
                }),
                new DHCPv4LeaseCreatedEvent
                {
                    EntityId = leaseId,
                    Address = leasedAddress,
                    ClientIdentifier = DHCPv4ClientIdentifier.FromHwAddress(clientMacAdress),
                    ScopeId = scopeId,
                    StartedAt = leaseCreatedAt,
                    ValidUntil = DateTime.UtcNow.AddDays(1),
                 },
            }); ;

            DHCPv4Packet result = rootScope.HandleDiscover(discoverPacket);
            CheckPacket(expectedAddress, result);

            DHCPv4Lease lease = CheckLease(1, 2, expectedAddress, scopeId, rootScope, DateTime.UtcNow);

            var revokedLease = rootScope.GetScopeById(scopeId).Leases.GetLeaseById(leaseId);
            Assert.False(revokedLease.IsActive());
            Assert.Equal(DHCPv4LeaseStates.Revoked, revokedLease.State);

            CheckEventAmount(3, rootScope);

            CheckRevokedEvent(0, scopeId, leaseId, rootScope);
            CheckLeaseCreatedEvent(1, clientMacAdress, scopeId, rootScope, expectedAddress, lease);

            CheckHandeledEvent(2, discoverPacket, result, rootScope, scopeId);
        }

        [Fact]
        public void HandleDiscover_LeaseFound_ReuseNotEnabled_ExcludedFoundLease()
        {
            Random random = new Random();

            IPv4HeaderInformation headerInformation =
                new IPv4HeaderInformation(IPv4Address.Empty, IPv4Address.Broadcast);

            Byte[] clientMacAdress = random.NextBytes(6);

            DHCPv4Packet discoverPacket = new DHCPv4Packet(
                headerInformation, clientMacAdress, (UInt32)random.Next(),
                IPv4Address.Empty, IPv4Address.Empty, IPv4Address.Empty,
                new DHCPv4PacketMessageTypeOption(DHCPv4Packet.DHCPv4MessagesTypes.DHCPDISCOVER)
            );

            Mock<IDHCPv4ScopeResolverManager> scopeResolverMock =
                new Mock<IDHCPv4ScopeResolverManager>(MockBehavior.Strict);

            var resolverInformations = new DHCPv4CreateScopeResolverInformation
            {
                Typename = nameof(DHCPv4RelayAgentSubnetResolver),
            };

            Mock<IDHCPv4ScopeResolver> resolverMock = new Mock<IDHCPv4ScopeResolver>(MockBehavior.Strict);
            resolverMock.Setup(x => x.PacketMeetsCondition(discoverPacket)).Returns(true);
            resolverMock.SetupGet(x => x.HasUniqueIdentifier).Returns(false);

            scopeResolverMock.Setup(x => x.InitializeResolver(resolverInformations)).Returns(resolverMock.Object);

            Guid scopeId = random.NextGuid();
            Guid leaseId = random.NextGuid();

            IPv4Address leasedAddress = IPv4Address.FromString("192.168.178.2");
            IPv4Address expectedAddress = IPv4Address.FromString("192.168.178.3");

            DateTime leaseCreatedAt = DateTime.UtcNow.AddHours(-1);
            DHCPv4RootScope rootScope = GetRootScope(scopeResolverMock);
            rootScope.Load(new List<DomainEvent>{ new DHCPv4ScopeEvents.DHCPv4ScopeAddedEvent(
                new DHCPv4ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv4ScopeAddressProperties(
                        IPv4Address.FromString("192.168.178.1"),
                        IPv4Address.FromString("192.168.178.255"),
                        new List<IPv4Address>{IPv4Address.FromString("192.168.178.1") },
                        validLifetime: TimeSpan.FromDays(1),
                        reuseAddressIfPossible: false,
                        addressAllocationStrategy: DHCPv4ScopeAddressProperties.DHCPv4AddressAllocationStrategies.Next),
                    ResolverInformations = resolverInformations,
                    Name = "Testscope",
                    Id = scopeId,
                }),
                new DHCPv4LeaseCreatedEvent
                {
                    EntityId = leaseId,
                    Address = leasedAddress,
                    ClientIdentifier = DHCPv4ClientIdentifier.FromHwAddress(clientMacAdress),
                    ScopeId = scopeId,
                    StartedAt = leaseCreatedAt,
                    ValidUntil = DateTime.UtcNow.AddDays(1),
                 },
            }); ;

            DHCPv4Packet result = rootScope.HandleDiscover(discoverPacket);
            CheckPacket(expectedAddress, result);

            DHCPv4Lease lease = CheckLease(1, 2, expectedAddress, scopeId, rootScope, DateTime.UtcNow);

            var revokedLease = rootScope.GetScopeById(scopeId).Leases.GetLeaseById(leaseId);
            Assert.False(revokedLease.IsActive());
            Assert.Equal(DHCPv4LeaseStates.Revoked, revokedLease.State);

            CheckEventAmount(3, rootScope);

            CheckRevokedEvent(0, scopeId, leaseId, rootScope);
            CheckLeaseCreatedEvent(1, clientMacAdress, scopeId, rootScope, expectedAddress, lease);

            CheckHandeledEvent(2, discoverPacket, result, rootScope, scopeId);
        }

        [Fact]
        public void HandleDiscover_LeaseFoundByUniqueIdentifier_NoReuse()
        {
            Random random = new Random();

            IPv4HeaderInformation headerInformation =
                new IPv4HeaderInformation(IPv4Address.Empty, IPv4Address.Broadcast);

            Byte[] clientMacAdress = random.NextBytes(6);
            Byte[] uniqueIdentifier = random.NextBytes(20);

            DHCPv4Packet discoverPacket = new DHCPv4Packet(
                headerInformation, clientMacAdress, (UInt32)random.Next(),
                IPv4Address.Empty, IPv4Address.Empty, IPv4Address.Empty,
                new DHCPv4PacketMessageTypeOption(DHCPv4Packet.DHCPv4MessagesTypes.DHCPDISCOVER)
            );

            Mock<IDHCPv4ScopeResolverManager> scopeResolverMock =
                new Mock<IDHCPv4ScopeResolverManager>(MockBehavior.Strict);

            var resolverInformations = new DHCPv4CreateScopeResolverInformation
            {
                Typename = nameof(DHCPv4RelayAgentSubnetResolver),
            };

            Mock<IDHCPv4ScopeResolver> resolverMock = new Mock<IDHCPv4ScopeResolver>(MockBehavior.Strict);
            resolverMock.Setup(x => x.PacketMeetsCondition(discoverPacket)).Returns(true);
            resolverMock.SetupGet(x => x.HasUniqueIdentifier).Returns(true);
            resolverMock.Setup(x => x.GetUniqueIdentifier(discoverPacket)).Returns(uniqueIdentifier);

            scopeResolverMock.Setup(x => x.InitializeResolver(resolverInformations)).Returns(resolverMock.Object);

            Guid scopeId = random.NextGuid();
            Guid leaseId = random.NextGuid();

            IPv4Address leasedAddress = IPv4Address.FromString("192.168.178.10");
            IPv4Address expectedAddress = IPv4Address.FromString("192.168.178.3");

            DateTime leaseCreatedAt = DateTime.UtcNow.AddHours(-1);
            DHCPv4RootScope rootScope = GetRootScope(scopeResolverMock);
            rootScope.Load(new List<DomainEvent>{ new DHCPv4ScopeEvents.DHCPv4ScopeAddedEvent(
                new DHCPv4ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv4ScopeAddressProperties(
                        IPv4Address.FromString("192.168.178.1"),
                        IPv4Address.FromString("192.168.178.255"),
                        new List<IPv4Address>{
                            IPv4Address.FromString("192.168.178.1"),
                            IPv4Address.FromString("192.168.178.2"),
                        },
                        validLifetime: TimeSpan.FromDays(1),
                        reuseAddressIfPossible: false,
                        addressAllocationStrategy: DHCPv4ScopeAddressProperties.DHCPv4AddressAllocationStrategies.Next),
                    ResolverInformations = resolverInformations,
                    Name = "Testscope",
                    Id = scopeId,
                }),
                new DHCPv4LeaseCreatedEvent
                {
                    EntityId = leaseId,
                    Address = leasedAddress,
                    ClientIdentifier = DHCPv4ClientIdentifier.FromHwAddress(random.NextBytes(6)),
                    ScopeId = scopeId,
                    UniqueIdentiifer = uniqueIdentifier,
                    StartedAt = leaseCreatedAt,
                    ValidUntil = DateTime.UtcNow.AddDays(1),
                 },
            }); ;

            DHCPv4Packet result = rootScope.HandleDiscover(discoverPacket);
            CheckPacket(expectedAddress, result);

            DHCPv4Lease lease = CheckLease(
                1, 2, expectedAddress, scopeId, rootScope, DateTime.UtcNow, uniqueIdentifier);

            var revokedLease = rootScope.GetScopeById(scopeId).Leases.GetLeaseById(leaseId);
            Assert.False(revokedLease.IsActive());
            Assert.Equal(DHCPv4LeaseStates.Revoked, revokedLease.State);

            CheckEventAmount(3, rootScope);

            CheckRevokedEvent(0, scopeId, leaseId, rootScope);
            CheckLeaseCreatedEvent
                (1, clientMacAdress, scopeId, rootScope, expectedAddress, lease, uniqueIdentifier);

            CheckHandeledEvent(2, discoverPacket, result, rootScope, scopeId);
        }

        [Fact]
        public void HandleDiscover_LeaseFoundByUniqueIdentifier_NoReuse_ExcludedFoundLease()
        {
            Random random = new Random();

            IPv4HeaderInformation headerInformation =
                new IPv4HeaderInformation(IPv4Address.Empty, IPv4Address.Broadcast);

            Byte[] clientMacAdress = random.NextBytes(6);
            Byte[] uniqueIdentifier = random.NextBytes(20);

            DHCPv4Packet discoverPacket = new DHCPv4Packet(
                headerInformation, clientMacAdress, (UInt32)random.Next(),
                IPv4Address.Empty, IPv4Address.Empty, IPv4Address.Empty,
                new DHCPv4PacketMessageTypeOption(DHCPv4Packet.DHCPv4MessagesTypes.DHCPDISCOVER)
            );

            Mock<IDHCPv4ScopeResolverManager> scopeResolverMock =
                new Mock<IDHCPv4ScopeResolverManager>(MockBehavior.Strict);

            var resolverInformations = new DHCPv4CreateScopeResolverInformation
            {
                Typename = nameof(DHCPv4RelayAgentSubnetResolver),
            };

            Mock<IDHCPv4ScopeResolver> resolverMock = new Mock<IDHCPv4ScopeResolver>(MockBehavior.Strict);
            resolverMock.Setup(x => x.PacketMeetsCondition(discoverPacket)).Returns(true);
            resolverMock.SetupGet(x => x.HasUniqueIdentifier).Returns(true);
            resolverMock.Setup(x => x.GetUniqueIdentifier(discoverPacket)).Returns(uniqueIdentifier);

            scopeResolverMock.Setup(x => x.InitializeResolver(resolverInformations)).Returns(resolverMock.Object);

            Guid scopeId = random.NextGuid();
            Guid leaseId = random.NextGuid();

            IPv4Address leasedAddress = IPv4Address.FromString("192.168.178.2");
            IPv4Address expectedAddress = IPv4Address.FromString("192.168.178.3");

            DateTime leaseCreatedAt = DateTime.UtcNow.AddHours(-1);
            DHCPv4RootScope rootScope = GetRootScope(scopeResolverMock);
            rootScope.Load(new List<DomainEvent>{ new DHCPv4ScopeEvents.DHCPv4ScopeAddedEvent(
                new DHCPv4ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv4ScopeAddressProperties(
                        IPv4Address.FromString("192.168.178.1"),
                        IPv4Address.FromString("192.168.178.255"),
                        new List<IPv4Address>{IPv4Address.FromString("192.168.178.1") },
                        validLifetime: TimeSpan.FromDays(1),
                        reuseAddressIfPossible: false,
                        addressAllocationStrategy: DHCPv4ScopeAddressProperties.DHCPv4AddressAllocationStrategies.Next),
                    ResolverInformations = resolverInformations,
                    Name = "Testscope",
                    Id = scopeId,
                }),
                new DHCPv4LeaseCreatedEvent
                {
                    EntityId = leaseId,
                    Address = leasedAddress,
                    ClientIdentifier = DHCPv4ClientIdentifier.FromHwAddress(random.NextBytes(6)),
                    ScopeId = scopeId,
                    UniqueIdentiifer = uniqueIdentifier,
                    StartedAt = leaseCreatedAt,
                    ValidUntil = DateTime.UtcNow.AddDays(1),
                 },
            }); ;

            DHCPv4Packet result = rootScope.HandleDiscover(discoverPacket);
            CheckPacket(expectedAddress, result);

            DHCPv4Lease lease = CheckLease(
                1, 2, expectedAddress, scopeId, rootScope, DateTime.UtcNow, uniqueIdentifier);

            var revokedLease = rootScope.GetScopeById(scopeId).Leases.GetLeaseById(leaseId);
            Assert.False(revokedLease.IsActive());
            Assert.Equal(DHCPv4LeaseStates.Revoked, revokedLease.State);

            CheckEventAmount(3, rootScope);

            CheckRevokedEvent(0, scopeId, leaseId, rootScope);
            CheckLeaseCreatedEvent
                (1, clientMacAdress, scopeId, rootScope, expectedAddress, lease, uniqueIdentifier);

            CheckHandeledEvent(2, discoverPacket, result, rootScope, scopeId);
        }

        [Fact]
        public void HandleDiscover_LeaseFoundByUniqueIdentifier_Reuse()
        {
            Random random = new Random();

            IPv4HeaderInformation headerInformation =
                new IPv4HeaderInformation(IPv4Address.Empty, IPv4Address.Broadcast);

            Byte[] clientMacAdress = random.NextBytes(6);
            Byte[] uniqueIdentifier = random.NextBytes(20);

            DHCPv4Packet discoverPacket = new DHCPv4Packet(
                headerInformation, clientMacAdress, (UInt32)random.Next(),
                IPv4Address.Empty, IPv4Address.Empty, IPv4Address.Empty,
                new DHCPv4PacketMessageTypeOption(DHCPv4Packet.DHCPv4MessagesTypes.DHCPDISCOVER)
            );

            Mock<IDHCPv4ScopeResolverManager> scopeResolverMock =
                new Mock<IDHCPv4ScopeResolverManager>(MockBehavior.Strict);

            var resolverInformations = new DHCPv4CreateScopeResolverInformation
            {
                Typename = nameof(DHCPv4RelayAgentSubnetResolver),
            };

            Mock<IDHCPv4ScopeResolver> resolverMock = new Mock<IDHCPv4ScopeResolver>(MockBehavior.Strict);
            resolverMock.Setup(x => x.PacketMeetsCondition(discoverPacket)).Returns(true);
            resolverMock.SetupGet(x => x.HasUniqueIdentifier).Returns(true);
            resolverMock.Setup(x => x.GetUniqueIdentifier(discoverPacket)).Returns(uniqueIdentifier);

            scopeResolverMock.Setup(x => x.InitializeResolver(resolverInformations)).Returns(resolverMock.Object);

            Guid scopeId = random.NextGuid();
            Guid leaseId = random.NextGuid();

            IPv4Address leasedAddress = IPv4Address.FromString("192.168.178.10");

            DateTime leaseCreatedAt = DateTime.UtcNow.AddHours(-1);
            DHCPv4RootScope rootScope = GetRootScope(scopeResolverMock);
            rootScope.Load(new List<DomainEvent>{ new DHCPv4ScopeEvents.DHCPv4ScopeAddedEvent(
                new DHCPv4ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv4ScopeAddressProperties(
                        IPv4Address.FromString("192.168.178.0"),
                        IPv4Address.FromString("192.168.178.255"),
                        new List<IPv4Address>{IPv4Address.FromString("192.168.178.1") },
                        validLifetime: TimeSpan.FromDays(1),
                        reuseAddressIfPossible: true,
                        addressAllocationStrategy: DHCPv4ScopeAddressProperties.DHCPv4AddressAllocationStrategies.Next),
                    ResolverInformations = resolverInformations,
                    Name = "Testscope",
                    Id = scopeId,
                }),
                new DHCPv4LeaseCreatedEvent
                {
                    EntityId = leaseId,
                    Address = leasedAddress,
                    ClientIdentifier = DHCPv4ClientIdentifier.FromHwAddress(random.NextBytes(6)),
                    ScopeId = scopeId,
                    UniqueIdentiifer = uniqueIdentifier,
                    StartedAt = leaseCreatedAt,
                    ValidUntil = DateTime.UtcNow.AddDays(1),
                 },
            }); ;

            DHCPv4Packet result = rootScope.HandleDiscover(discoverPacket);
            CheckPacket(leasedAddress, result);

            DHCPv4Lease lease = CheckLease(
                1, 2, leasedAddress, scopeId, rootScope, DateTime.UtcNow, uniqueIdentifier);

            var revokedLease = rootScope.GetScopeById(scopeId).Leases.GetLeaseById(leaseId);
            Assert.False(revokedLease.IsActive());
            Assert.Equal(DHCPv4LeaseStates.Revoked, revokedLease.State);

            CheckEventAmount(3, rootScope);
            CheckLeaseCreatedEvent
                (1, clientMacAdress, scopeId, rootScope, leasedAddress, lease, uniqueIdentifier);

            CheckHandeledEvent(2, discoverPacket, result, rootScope, scopeId);
        }

        [Fact]
        public void HandleDiscover_LeaseNotFoundByUniqueIdentifier_Reuse()
        {
            Random random = new Random();

            IPv4HeaderInformation headerInformation =
                new IPv4HeaderInformation(IPv4Address.Empty, IPv4Address.Broadcast);

            Byte[] clientMacAdress = random.NextBytes(6);
            Byte[] uniqueIdentifier = random.NextBytes(20);

            DHCPv4Packet discoverPacket = new DHCPv4Packet(
                headerInformation, clientMacAdress, (UInt32)random.Next(),
                IPv4Address.Empty, IPv4Address.Empty, IPv4Address.Empty,
                new DHCPv4PacketMessageTypeOption(DHCPv4Packet.DHCPv4MessagesTypes.DHCPDISCOVER)
            );

            Mock<IDHCPv4ScopeResolverManager> scopeResolverMock =
                new Mock<IDHCPv4ScopeResolverManager>(MockBehavior.Strict);

            var resolverInformations = new DHCPv4CreateScopeResolverInformation
            {
                Typename = nameof(DHCPv4RelayAgentSubnetResolver),
            };

            Mock<IDHCPv4ScopeResolver> resolverMock = new Mock<IDHCPv4ScopeResolver>(MockBehavior.Strict);
            resolverMock.Setup(x => x.PacketMeetsCondition(discoverPacket)).Returns(true);
            resolverMock.SetupGet(x => x.HasUniqueIdentifier).Returns(true);
            resolverMock.Setup(x => x.GetUniqueIdentifier(discoverPacket)).Returns(uniqueIdentifier);

            scopeResolverMock.Setup(x => x.InitializeResolver(resolverInformations)).Returns(resolverMock.Object);

            Guid scopeId = random.NextGuid();
            Guid leaseId = random.NextGuid();

            IPv4Address leasedAddress = IPv4Address.FromString("192.168.178.10");
            IPv4Address expectedAddress = IPv4Address.FromString("192.168.178.2");

            DateTime leaseCreatedAt = DateTime.UtcNow.AddHours(-1);
            DHCPv4RootScope rootScope = GetRootScope(scopeResolverMock);
            rootScope.Load(new List<DomainEvent>{ new DHCPv4ScopeEvents.DHCPv4ScopeAddedEvent(
                new DHCPv4ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv4ScopeAddressProperties(
                        IPv4Address.FromString("192.168.178.1"),
                        IPv4Address.FromString("192.168.178.255"),
                        new List<IPv4Address>{IPv4Address.FromString("192.168.178.1") },
                        validLifetime: TimeSpan.FromDays(1),
                        reuseAddressIfPossible: true,
                        addressAllocationStrategy: DHCPv4ScopeAddressProperties.DHCPv4AddressAllocationStrategies.Next),
                    ResolverInformations = resolverInformations,
                    Name = "Testscope",
                    Id = scopeId,
                }),
                new DHCPv4LeaseCreatedEvent
                {
                    EntityId = leaseId,
                    Address = leasedAddress,
                    ClientIdentifier = DHCPv4ClientIdentifier.FromHwAddress(random.NextBytes(6)),
                    ScopeId = scopeId,
                    UniqueIdentiifer = null,
                    StartedAt = leaseCreatedAt,
                    ValidUntil = DateTime.UtcNow.AddDays(1),
                 },
            }); ;

            DHCPv4Packet result = rootScope.HandleDiscover(discoverPacket);
            CheckPacket(expectedAddress, result);

            DHCPv4Lease lease = CheckLease(
                1, 2, expectedAddress, scopeId, rootScope, DateTime.UtcNow, uniqueIdentifier);

            CheckEventAmount(2, rootScope);
            CheckLeaseCreatedEvent
                (0, clientMacAdress, scopeId, rootScope, expectedAddress, lease, uniqueIdentifier);

            CheckHandeledEvent(1, discoverPacket, result, rootScope, scopeId);
        }

        [Theory]
        [InlineData(DHCPv4ScopeAddressProperties.DHCPv4AddressAllocationStrategies.Next)]
        [InlineData(DHCPv4ScopeAddressProperties.DHCPv4AddressAllocationStrategies.Random)]
        public void HandleDiscover_NoLeaseFound_NoAddressAvaiable(DHCPv4ScopeAddressProperties.DHCPv4AddressAllocationStrategies allocationStrategy)
        {
            Random random = new Random();

            IPv4HeaderInformation headerInformation =
                new IPv4HeaderInformation(IPv4Address.Empty, IPv4Address.Broadcast);

            Byte[] clientMacAdress = random.NextBytes(6);

            DHCPv4Packet discoverPacket = new DHCPv4Packet(
                headerInformation, clientMacAdress, (UInt32)random.Next(),
                IPv4Address.Empty, IPv4Address.Empty, IPv4Address.Empty,
                new DHCPv4PacketMessageTypeOption(DHCPv4Packet.DHCPv4MessagesTypes.DHCPDISCOVER)
            );

            Mock<IDHCPv4ScopeResolverManager> scopeResolverMock =
                new Mock<IDHCPv4ScopeResolverManager>(MockBehavior.Strict);

            var resolverInformations = new DHCPv4CreateScopeResolverInformation
            {
                Typename = nameof(DHCPv4RelayAgentSubnetResolver),
            };

            Mock<IDHCPv4ScopeResolver> resolverMock = new Mock<IDHCPv4ScopeResolver>(MockBehavior.Strict);
            resolverMock.Setup(x => x.PacketMeetsCondition(discoverPacket)).Returns(true);
            resolverMock.SetupGet(x => x.HasUniqueIdentifier).Returns(false);

            scopeResolverMock.Setup(x => x.InitializeResolver(resolverInformations)).Returns(resolverMock.Object);

            Guid scopeId = random.NextGuid();

            DHCPv4RootScope rootScope = GetRootScope(scopeResolverMock);
            rootScope.Load(new List<DomainEvent>{ new DHCPv4ScopeEvents.DHCPv4ScopeAddedEvent(
                new DHCPv4ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv4ScopeAddressProperties(
                        IPv4Address.FromString("192.168.178.0"),
                        IPv4Address.FromString("192.168.178.3"),
                        new List<IPv4Address>{
                            IPv4Address.FromString("192.168.178.0"),
                            IPv4Address.FromString("192.168.178.1"),
                            IPv4Address.FromString("192.168.178.2")},
                        validLifetime: TimeSpan.FromDays(1),
                        addressAllocationStrategy: allocationStrategy),
                    ResolverInformations = resolverInformations,
                    Name = "Testscope",
                    Id = scopeId,
                }),
                new DHCPv4LeaseCreatedEvent
                {
                    EntityId = random.NextGuid(),
                    Address = IPv4Address.FromString("192.168.178.3"),
                    ClientIdentifier = DHCPv4ClientIdentifier.FromHwAddress(random.NextBytes(6)),
                    ScopeId = scopeId,
                    UniqueIdentiifer = null,
                    StartedAt = DateTime.UtcNow.AddDays(-1),
                    ValidUntil = DateTime.UtcNow.AddDays(1),
                 },
            });

            DHCPv4Packet result = rootScope.HandleDiscover(discoverPacket);
            Assert.Equal(DHCPv4Packet.Empty, result);

            CheckEventAmount(2, rootScope);
            DHCPv4ScopeAddressesAreExhaustedEvent(0, rootScope, scopeId);
            CheckHandeledEventNotSuccessfull(1, discoverPacket, rootScope, scopeId);
        }
    }
}
