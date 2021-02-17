using DaAPI.Core.Common;
using DaAPI.Core.Packets.DHCPv4;
using DaAPI.Core.Scopes;
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
using static DaAPI.Core.Scopes.DHCPv4.DHCPv4LeaseEvents;
using static DaAPI.Core.Scopes.DHCPv4.DHCPv4PacketHandledEvents;
using static DaAPI.Core.Scopes.DHCPv4.DHCPv4PacketHandledEvents.DHCPv4RequestHandledEvent;

namespace DaAPI.UnitTests.Core.Scopes.DHCPv4
{
    public class DHCPv4RootScopeTesterHandleRequesTester : DHCPv4RootScopeTesterBase
    {

        private void CheckAcknowledgePacket(IPv4Address expectedAdress, DHCPv4Packet result)
        {
            Assert.NotNull(result);
            Assert.NotEqual(DHCPv4Packet.Empty, result);
            Assert.True(result.IsValid);

            Assert.Equal(expectedAdress, result.YourIPAdress);
            Assert.Equal(DHCPv4MessagesTypes.Acknowledge, result.MessageType);
        }

        private void CheckNotAcknowledgePacket(DHCPv4Packet result)
        {
            Assert.NotNull(result);
            Assert.NotEqual(DHCPv4Packet.Empty, result);
            Assert.True(result.IsValid);

            Assert.Equal(IPv4Address.Broadcast, result.Header.Destionation);
            Assert.Equal(IPv4Address.Empty, result.YourIPAdress);

            Assert.Equal(DHCPv4MessagesTypes.NotAcknowledge, result.MessageType);
        }

        private static void CheckLeaseActivedEvent(Int32 index,
            Guid scopeId, DHCPv4RootScope rootScope,
            Guid leaseId
            )
        {
            IEnumerable<DomainEvent> changes = rootScope.GetChanges();
            Assert.NotNull(changes);

            Assert.IsAssignableFrom<DHCPv4LeaseActivatedEvent>(changes.ElementAt(index));

            DHCPv4LeaseActivatedEvent createdEvent = (DHCPv4LeaseActivatedEvent)changes.ElementAt(index);
            Assert.NotNull(createdEvent);

            Assert.Equal(scopeId, createdEvent.ScopeId);
            Assert.Equal(leaseId, createdEvent.EntityId);
        }

        private static void CheckLeaseRenewedEvent(Int32 index,
            Guid scopeId, DHCPv4RootScope rootScope,
            Guid leaseId,
            DateTime expectedEnd
            )
        {
            IEnumerable<DomainEvent> changes = rootScope.GetChanges();
            Assert.NotNull(changes);

            Assert.IsAssignableFrom<DHCPv4LeaseRenewedEvent>(changes.ElementAt(index));

            DHCPv4LeaseRenewedEvent createdEvent = (DHCPv4LeaseRenewedEvent)changes.ElementAt(index);
            Assert.NotNull(createdEvent);

            Assert.Equal(scopeId, createdEvent.ScopeId);
            Assert.Equal(leaseId, createdEvent.EntityId);
            Assert.False(createdEvent.Reset);
            Assert.True(Math.Abs((expectedEnd - createdEvent.End).TotalMinutes) < 2);
        }

        private void CheckHandeledEvent(
            Int32 index, RequestErros error,
            DHCPv4Packet requestPacket, DHCPv4Packet result,
            DHCPv4RootScope rootScope, Guid scopeId)
        {
            IEnumerable<DomainEvent> changes = rootScope.GetChanges();

            Assert.IsAssignableFrom<DHCPv4RequestHandledEvent>(changes.ElementAt(index));

            DHCPv4RequestHandledEvent handeledEvent = (DHCPv4RequestHandledEvent)changes.ElementAt(index);
            Assert.Equal(requestPacket, handeledEvent.Request);
            Assert.Equal(result, handeledEvent.Response);
            Assert.Equal(scopeId, handeledEvent.ScopeId);
            Assert.Equal(error, handeledEvent.Error);
            if (error == RequestErros.NoError)
            {
                Assert.True(handeledEvent.WasSuccessfullHandled);
            }
            else
            {
                Assert.False(handeledEvent.WasSuccessfullHandled);
            }
        }

        private static void CheckIfLeaseIsActive(
            Guid scopeId, Guid leaseId, DHCPv4RootScope rootScope, DateTime expectedEnd)
        {
            DHCPv4Lease lease = rootScope.GetScopeById(scopeId).Leases.GetLeaseById(leaseId);
            Assert.NotEqual(DHCPv4Lease.Empty, lease);
            Assert.True(lease.IsActive());
            Assert.Equal(LeaseStates.Active, lease.State);
            Assert.True(Math.Abs((expectedEnd - lease.End).TotalMinutes) < 2);

        }

        private static void CheckIfLeaseIsRevoked(Guid scopeId, Guid leaseId, DHCPv4RootScope rootScope)
        {
            DHCPv4Lease lease = rootScope.GetScopeById(scopeId).Leases.GetLeaseById(leaseId);
            Assert.NotEqual(DHCPv4Lease.Empty, lease);
            Assert.False(lease.IsActive());
            Assert.Equal(LeaseStates.Revoked, lease.State);
        }


        [Fact]
        public void HandleRequest_AnswerToOffer_LeaseFound_IsPending()
        {
            Random random = new Random();

            IPv4HeaderInformation headerInformation =
                new IPv4HeaderInformation(IPv4Address.Empty, IPv4Address.Broadcast);

            Byte[] clientMacAdress = random.NextBytes(6);
            IPv4Address leasedAddress = IPv4Address.FromString("192.168.178.10");

            DHCPv4Packet requestPacket = new DHCPv4Packet(
                headerInformation, clientMacAdress, (UInt32)random.Next(),
                IPv4Address.Empty, IPv4Address.Empty, IPv4Address.Empty,
                new DHCPv4PacketMessageTypeOption(DHCPv4MessagesTypes.Request),
                new DHCPv4PacketAddressOption(DHCPv4OptionTypes.ServerIdentifier, IPv4Address.FromString("192.168.178.0")),
                new DHCPv4PacketAddressOption(DHCPv4OptionTypes.RequestedIPAddress, leasedAddress)
            );

            Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>> scopeResolverMock =
                new Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);

            var resolverInformations = new CreateScopeResolverInformation
            {
                Typename = nameof(DHCPv4RelayAgentSubnetResolver),
            };

            Mock<IScopeResolver<DHCPv4Packet, IPv4Address>> resolverMock = new Mock<IScopeResolver<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);
            resolverMock.Setup(x => x.PacketMeetsCondition(requestPacket)).Returns(true);
            resolverMock.SetupGet(x => x.HasUniqueIdentifier).Returns(false);

            scopeResolverMock.Setup(x => x.InitializeResolver(resolverInformations)).Returns(resolverMock.Object);

            Guid scopeId = random.NextGuid();
            Guid leaseId = random.NextGuid();

            DateTime leaseCreatedAt = DateTime.UtcNow.AddHours(-1);
            DHCPv4RootScope rootScope = GetRootScope(scopeResolverMock);
            rootScope.Load(new List<DomainEvent>{ new DHCPv4ScopeEvents.DHCPv4ScopeAddedEvent(
                new DHCPv4ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv4ScopeAddressProperties(
                        IPv4Address.FromString("192.168.178.1"),
                        IPv4Address.FromString("192.168.178.255"),
                        new List<IPv4Address>{IPv4Address.FromString("192.168.178.1") },
                        leaseTime: TimeSpan.FromDays(1),
                        maskLength: 24
                        ),
                    ResolverInformation = resolverInformations,
                    Name = "Testscope",
                    Id = scopeId,
                }),
                new DHCPv4LeaseCreatedEvent
                {
                    EntityId = leaseId,
                    Address = leasedAddress,
                    HardwareAddress = clientMacAdress,
                    ScopeId = scopeId,
                    UniqueIdentifier = null,
                    StartedAt = leaseCreatedAt,
                    ValidUntil = DateTime.UtcNow.AddDays(1),
                 },
            });

            DHCPv4Packet result = rootScope.HandleRequest(requestPacket);
            CheckAcknowledgePacket(leasedAddress, result);

            CheckIfLeaseIsActive(scopeId, leaseId, rootScope, DateTime.UtcNow.AddHours(24));

            CheckEventAmount(2, rootScope);
            CheckLeaseActivedEvent(0, scopeId, rootScope, leaseId);
            CheckHandeledEvent(1, RequestErros.NoError, requestPacket, result, rootScope, scopeId);
        }

        [Fact]
        public void HandleRequest_AnswerToOffer_LeaseFound_NotPending()
        {
            Random random = new Random();

            IPv4HeaderInformation headerInformation =
                new IPv4HeaderInformation(IPv4Address.Empty, IPv4Address.Broadcast);

            Byte[] clientMacAdress = random.NextBytes(6);
            IPv4Address leasedAddress = IPv4Address.FromString("192.168.178.10");

            DHCPv4Packet requestPacket = new DHCPv4Packet(
                headerInformation, clientMacAdress, (UInt32)random.Next(),
                IPv4Address.Empty, IPv4Address.Empty, IPv4Address.Empty,
                new DHCPv4PacketMessageTypeOption(DHCPv4MessagesTypes.Request),
                new DHCPv4PacketAddressOption(DHCPv4OptionTypes.ServerIdentifier, IPv4Address.FromString("192.168.178.0")),
                new DHCPv4PacketAddressOption(DHCPv4OptionTypes.RequestedIPAddress, leasedAddress)
            );

            Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>> scopeResolverMock =
                new Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);

            var resolverInformations = new CreateScopeResolverInformation
            {
                Typename = nameof(DHCPv4RelayAgentSubnetResolver),
            };

            Mock<IScopeResolver<DHCPv4Packet, IPv4Address>> resolverMock = new Mock<IScopeResolver<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);
            resolverMock.Setup(x => x.PacketMeetsCondition(requestPacket)).Returns(true);
            resolverMock.SetupGet(x => x.HasUniqueIdentifier).Returns(false);

            scopeResolverMock.Setup(x => x.InitializeResolver(resolverInformations)).Returns(resolverMock.Object);

            Guid scopeId = random.NextGuid();
            Guid leaseId = random.NextGuid();

            DateTime leaseCreatedAt = DateTime.UtcNow.AddHours(-1);
            DHCPv4RootScope rootScope = GetRootScope(scopeResolverMock);
            rootScope.Load(new List<DomainEvent>{ new DHCPv4ScopeEvents.DHCPv4ScopeAddedEvent(
                new DHCPv4ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv4ScopeAddressProperties(
                        IPv4Address.FromString("192.168.178.1"),
                        IPv4Address.FromString("192.168.178.255"),
                        new List<IPv4Address>{IPv4Address.FromString("192.168.178.1") },
                        leaseTime: TimeSpan.FromDays(1)
                        ),
                    ResolverInformation = resolverInformations,
                    Name = "Testscope",
                    Id = scopeId,
                }),
                new DHCPv4LeaseCreatedEvent
                {
                    EntityId = leaseId,
                    Address = leasedAddress,
                    HardwareAddress = clientMacAdress,
                    ScopeId = scopeId,
                    UniqueIdentifier = null,
                    StartedAt = leaseCreatedAt,
                    ValidUntil = DateTime.UtcNow.AddDays(1),
                 },
                new DHCPv4LeaseActivatedEvent
                {
                    EntityId = leaseId,
                    ScopeId = scopeId,
                }
            });

            DHCPv4Packet result = rootScope.HandleRequest(requestPacket);
            CheckNotAcknowledgePacket(result);

            CheckIfLeaseIsActive(scopeId, leaseId, rootScope, DateTime.UtcNow.AddHours(24));

            CheckEventAmount(1, rootScope);
            CheckHandeledEvent(0, RequestErros.LeaseNotPending, requestPacket, result, rootScope, scopeId);
        }

        [Fact]
        public void HandleRequest_AnswerToOffer_LeaseNotFound()
        {
            Random random = new Random();

            IPv4HeaderInformation headerInformation =
                new IPv4HeaderInformation(IPv4Address.Empty, IPv4Address.Broadcast);

            Byte[] clientMacAdress = random.NextBytes(6);
            IPv4Address leasedAddress = IPv4Address.FromString("192.168.178.10");

            DHCPv4Packet requestPacket = new DHCPv4Packet(
                headerInformation, clientMacAdress, (UInt32)random.Next(),
                IPv4Address.Empty, IPv4Address.Empty, IPv4Address.Empty,
                new DHCPv4PacketMessageTypeOption(DHCPv4MessagesTypes.Request),
                new DHCPv4PacketAddressOption(DHCPv4OptionTypes.ServerIdentifier, IPv4Address.FromString("192.168.178.0")),
                new DHCPv4PacketAddressOption(DHCPv4OptionTypes.RequestedIPAddress, leasedAddress)
            );

            Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>> scopeResolverMock =
                new Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);

            var resolverInformations = new CreateScopeResolverInformation
            {
                Typename = nameof(DHCPv4RelayAgentSubnetResolver),
            };

            Mock<IScopeResolver<DHCPv4Packet, IPv4Address>> resolverMock = new Mock<IScopeResolver<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);
            resolverMock.Setup(x => x.PacketMeetsCondition(requestPacket)).Returns(true);
            resolverMock.SetupGet(x => x.HasUniqueIdentifier).Returns(false);

            scopeResolverMock.Setup(x => x.InitializeResolver(resolverInformations)).Returns(resolverMock.Object);

            Guid scopeId = random.NextGuid();
            Guid leaseId = random.NextGuid();

            DateTime leaseCreatedAt = DateTime.UtcNow.AddHours(-1);
            DHCPv4RootScope rootScope = GetRootScope(scopeResolverMock);
            rootScope.Load(new List<DomainEvent>{ new DHCPv4ScopeEvents.DHCPv4ScopeAddedEvent(
                new DHCPv4ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv4ScopeAddressProperties(
                        IPv4Address.FromString("192.168.178.1"),
                        IPv4Address.FromString("192.168.178.255"),
                        new List<IPv4Address>{IPv4Address.FromString("192.168.178.1") },
                        leaseTime: TimeSpan.FromDays(1)
                        ),
                    ResolverInformation = resolverInformations,
                    Name = "Testscope",
                    Id = scopeId,
                })
            });

            DHCPv4Packet result = rootScope.HandleRequest(requestPacket);
            CheckNotAcknowledgePacket(result);

            CheckEventAmount(1, rootScope);
            CheckHandeledEvent(0, RequestErros.LeaseNotFound, requestPacket, result, rootScope, scopeId);
        }

        private DHCPv4Packet GetRequestPacket(
            Random random,
            IPv4Address leasedAddress,
            DHCPv4Packet.DHCPv4PacketRequestType requestType)
        {

            IPv4HeaderInformation headerInformation =
                new IPv4HeaderInformation(
                    requestType == DHCPv4PacketRequestType.Rebinding ? IPv4Address.Broadcast : leasedAddress,
                    IPv4Address.FromString("192.168.178.1"));

            Byte[] clientMacAdress = random.NextBytes(6);

            DHCPv4Packet requestPacket = new DHCPv4Packet(
                headerInformation, clientMacAdress, (UInt32)random.Next(),
                IPv4Address.Empty, IPv4Address.Empty, leasedAddress,
                new DHCPv4PacketMessageTypeOption(DHCPv4MessagesTypes.Request)
            );

            return requestPacket;
        }


        [Theory]
        [InlineData(DHCPv4Packet.DHCPv4PacketRequestType.Renewing)]
        [InlineData(DHCPv4Packet.DHCPv4PacketRequestType.Rebinding)]
        public void HandleRequest_SupportDirectUnicast_LeaseFound_LeaseActive_ReuseIsAllowed(DHCPv4Packet.DHCPv4PacketRequestType packetType)
        {
            Random random = new Random();

            IPv4Address leasedAddress = IPv4Address.FromString("192.168.178.10");
            DHCPv4Packet requestPacket = GetRequestPacket(random, leasedAddress, packetType);

            Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>> scopeResolverMock =
                new Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);

            var resolverInformations = new CreateScopeResolverInformation
            {
                Typename = nameof(DHCPv4RelayAgentSubnetResolver),
            };

            Mock<IScopeResolver<DHCPv4Packet, IPv4Address>> resolverMock = new Mock<IScopeResolver<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);
            resolverMock.Setup(x => x.PacketMeetsCondition(requestPacket)).Returns(true);
            resolverMock.SetupGet(x => x.HasUniqueIdentifier).Returns(false);

            scopeResolverMock.Setup(x => x.InitializeResolver(resolverInformations)).Returns(resolverMock.Object);

            Guid scopeId = random.NextGuid();
            Guid leaseId = random.NextGuid();

            DateTime leaseCreatedAt = DateTime.UtcNow.AddHours(-1);
            DHCPv4RootScope rootScope = GetRootScope(scopeResolverMock);
            rootScope.Load(new List<DomainEvent>{ new DHCPv4ScopeEvents.DHCPv4ScopeAddedEvent(
                new DHCPv4ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv4ScopeAddressProperties(
                        IPv4Address.FromString("192.168.178.1"),
                        IPv4Address.FromString("192.168.178.255"),
                        new List<IPv4Address>{IPv4Address.FromString("192.168.178.1") },
                        leaseTime: TimeSpan.FromDays(1),
                        supportDirectUnicast:  true,
                        reuseAddressIfPossible: true,
                        maskLength: 24
                        ),
                    ResolverInformation = resolverInformations,
                    Name = "Testscope",
                    Id = scopeId,
                }),
                new DHCPv4LeaseCreatedEvent
                {
                    EntityId = leaseId,
                    Address = leasedAddress,
                    HardwareAddress = requestPacket.ClientHardwareAddress,
                    ScopeId = scopeId,
                    UniqueIdentifier = null,
                    StartedAt = leaseCreatedAt,
                    ValidUntil = DateTime.UtcNow.AddDays(1),
                 },
                new DHCPv4LeaseActivatedEvent
                {
                    EntityId = leaseId,
                    ScopeId = scopeId,
                }
            });

            DHCPv4Packet result = rootScope.HandleRequest(requestPacket);
            CheckAcknowledgePacket(leasedAddress, result);

            CheckIfLeaseIsActive(scopeId, leaseId, rootScope, DateTime.UtcNow.AddHours(24));

            CheckEventAmount(2, rootScope);
            CheckLeaseRenewedEvent(0, scopeId, rootScope, leaseId, DateTime.UtcNow.AddHours(24));
            CheckHandeledEvent(1, RequestErros.NoError, requestPacket, result, rootScope, scopeId);
        }

        [Theory]
        [InlineData(DHCPv4Packet.DHCPv4PacketRequestType.Renewing)]
        [InlineData(DHCPv4Packet.DHCPv4PacketRequestType.Rebinding)]

        public void HandleRequest_SupportDirectUnicast_LeaseFound_LeaseActive_ReuseIsNotAllowed(DHCPv4Packet.DHCPv4PacketRequestType requestType)
        {
            Random random = new Random();

            IPv4Address leasedAddress = IPv4Address.FromString("192.168.178.10");
            IPv4Address expectedAddress = IPv4Address.FromString("192.168.178.2");

            DHCPv4Packet requestPacket = GetRequestPacket(random, leasedAddress, requestType);

            Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>> scopeResolverMock =
                new Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);

            var resolverInformations = new CreateScopeResolverInformation
            {
                Typename = nameof(DHCPv4RelayAgentSubnetResolver),
            };

            Mock<IScopeResolver<DHCPv4Packet, IPv4Address>> resolverMock = new Mock<IScopeResolver<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);
            resolverMock.Setup(x => x.PacketMeetsCondition(requestPacket)).Returns(true);
            resolverMock.SetupGet(x => x.HasUniqueIdentifier).Returns(false);

            scopeResolverMock.Setup(x => x.InitializeResolver(resolverInformations)).Returns(resolverMock.Object);

            Guid scopeId = random.NextGuid();
            Guid leaseId = random.NextGuid();

            DateTime leaseCreatedAt = DateTime.UtcNow.AddHours(-1);
            DHCPv4RootScope rootScope = GetRootScope(scopeResolverMock);
            rootScope.Load(new List<DomainEvent>{ new DHCPv4ScopeEvents.DHCPv4ScopeAddedEvent(
                new DHCPv4ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv4ScopeAddressProperties(
                        IPv4Address.FromString("192.168.178.1"),
                        IPv4Address.FromString("192.168.178.255"),
                        new List<IPv4Address>{IPv4Address.FromString("192.168.178.1") },
                        leaseTime: TimeSpan.FromDays(1),
                        supportDirectUnicast: true,
                        reuseAddressIfPossible: false,
                        maskLength: 24,
                        addressAllocationStrategy: DHCPv4ScopeAddressProperties.AddressAllocationStrategies.Next
                        ),
                    ResolverInformation = resolverInformations,
                    Name = "Testscope",
                    Id = scopeId,
                }),
                new DHCPv4LeaseCreatedEvent
                {
                    EntityId = leaseId,
                    Address = leasedAddress,
                    HardwareAddress = requestPacket.ClientHardwareAddress,
                    ScopeId = scopeId,
                    UniqueIdentifier = null,
                    StartedAt = leaseCreatedAt,
                    ValidUntil = DateTime.UtcNow.AddDays(1),
                 },
                new DHCPv4LeaseActivatedEvent
                {
                    EntityId = leaseId,
                    ScopeId = scopeId,
                }
            });

            DHCPv4Packet result = rootScope.HandleRequest(requestPacket);
            CheckAcknowledgePacket(expectedAddress, result);

            CheckIfLeaseIsRevoked(scopeId, leaseId, rootScope);
            DHCPv4Lease lease = CheckLease(1, 2, expectedAddress, scopeId, rootScope, DateTime.UtcNow);

            CheckEventAmount(3, rootScope);
            CheckLeaseCreatedEvent(1, requestPacket.ClientHardwareAddress, scopeId, rootScope, expectedAddress, lease);
            CheckRevokedEvent(0, scopeId, leaseId, rootScope);

            CheckHandeledEvent(2, RequestErros.NoError, requestPacket, result, rootScope, scopeId);
        }

        [Theory]
        [InlineData(DHCPv4Packet.DHCPv4PacketRequestType.Renewing)]
        [InlineData(DHCPv4Packet.DHCPv4PacketRequestType.Rebinding)]

        public void HandleRequest_SupportDirectUnicast_LeaseFound_LeaseNotActive(DHCPv4Packet.DHCPv4PacketRequestType requestType)
        {
            Random random = new Random();

            IPv4Address leasedAddress = IPv4Address.FromString("192.168.178.10");

            DHCPv4Packet requestPacket = GetRequestPacket(random, leasedAddress, requestType);

            Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>> scopeResolverMock =
                new Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);

            var resolverInformations = new CreateScopeResolverInformation
            {
                Typename = nameof(DHCPv4RelayAgentSubnetResolver),
            };

            Mock<IScopeResolver<DHCPv4Packet, IPv4Address>> resolverMock = new Mock<IScopeResolver<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);
            resolverMock.Setup(x => x.PacketMeetsCondition(requestPacket)).Returns(true);
            resolverMock.SetupGet(x => x.HasUniqueIdentifier).Returns(false);

            scopeResolverMock.Setup(x => x.InitializeResolver(resolverInformations)).Returns(resolverMock.Object);

            Guid scopeId = random.NextGuid();
            Guid leaseId = random.NextGuid();

            DateTime leaseCreatedAt = DateTime.UtcNow.AddHours(-1);
            DHCPv4RootScope rootScope = GetRootScope(scopeResolverMock);
            rootScope.Load(new List<DomainEvent>{ new DHCPv4ScopeEvents.DHCPv4ScopeAddedEvent(
                new DHCPv4ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv4ScopeAddressProperties(
                        IPv4Address.FromString("192.168.178.1"),
                        IPv4Address.FromString("192.168.178.255"),
                        new List<IPv4Address>{IPv4Address.FromString("192.168.178.1") },
                        supportDirectUnicast: true
                        ),
                    ResolverInformation = resolverInformations,
                    Name = "Testscope",
                    Id = scopeId,
                }),
                new DHCPv4LeaseCreatedEvent
                {
                    EntityId = leaseId,
                    Address = leasedAddress,
                    HardwareAddress = requestPacket.ClientHardwareAddress,
                    ScopeId = scopeId,
                    UniqueIdentifier = null,
                    StartedAt = leaseCreatedAt,
                    ValidUntil = DateTime.UtcNow.AddDays(1),
                 }
            });

            DHCPv4Packet result = rootScope.HandleRequest(requestPacket);
            CheckNotAcknowledgePacket(result);

            CheckEventAmount(1, rootScope);
            CheckHandeledEvent(0, RequestErros.LeaseNotActive, requestPacket, result, rootScope, scopeId);
        }

        [Theory]
        [InlineData(DHCPv4Packet.DHCPv4PacketRequestType.Renewing)]
        [InlineData(DHCPv4Packet.DHCPv4PacketRequestType.Rebinding)]

        public void HandleRequest_SupportDirectUnicast_LeaseNotFound(DHCPv4Packet.DHCPv4PacketRequestType requestType)
        {
            Random random = new Random();

            IPv4Address leasedAddress = IPv4Address.FromString("192.168.178.10");

            DHCPv4Packet requestPacket = GetRequestPacket(random, leasedAddress, requestType);

            Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>> scopeResolverMock =
                new Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);

            var resolverInformations = new CreateScopeResolverInformation
            {
                Typename = nameof(DHCPv4RelayAgentSubnetResolver),
            };

            Mock<IScopeResolver<DHCPv4Packet, IPv4Address>> resolverMock = new Mock<IScopeResolver<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);
            resolverMock.Setup(x => x.PacketMeetsCondition(requestPacket)).Returns(true);
            resolverMock.SetupGet(x => x.HasUniqueIdentifier).Returns(false);

            scopeResolverMock.Setup(x => x.InitializeResolver(resolverInformations)).Returns(resolverMock.Object);

            Guid scopeId = random.NextGuid();
            Guid leaseId = random.NextGuid();

            DateTime leaseCreatedAt = DateTime.UtcNow.AddHours(-1);
            DHCPv4RootScope rootScope = GetRootScope(scopeResolverMock);
            rootScope.Load(new List<DomainEvent>{ new DHCPv4ScopeEvents.DHCPv4ScopeAddedEvent(
                new DHCPv4ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv4ScopeAddressProperties(
                        IPv4Address.FromString("192.168.178.1"),
                        IPv4Address.FromString("192.168.178.255"),
                        new List<IPv4Address>{IPv4Address.FromString("192.168.178.1") },
                        supportDirectUnicast: true
                        ),
                    ResolverInformation = resolverInformations,
                    Name = "Testscope",
                    Id = scopeId,
                })
            });

            DHCPv4Packet result = rootScope.HandleRequest(requestPacket);

            if (requestType == DHCPv4PacketRequestType.Renewing)
            {
                CheckNotAcknowledgePacket(result);
            }
            else
            {

                Assert.Equal(DHCPv4Packet.Empty, result);
            }

            CheckEventAmount(1, rootScope);
            CheckHandeledEvent(0, RequestErros.LeaseNotFound, requestPacket, result, rootScope, scopeId);
        }


        [Fact]
        public void HandleRequest_Renewing_DirectUnicastNotSupported()
        {
            Random random = new Random();

            IPv4Address leasedAddress = IPv4Address.FromString("192.168.178.10");

            IPv4HeaderInformation headerInformation =
                new IPv4HeaderInformation(leasedAddress, IPv4Address.FromString("192.168.178.1"));

            Byte[] clientMacAdress = random.NextBytes(6);

            DHCPv4Packet requestPacket = new DHCPv4Packet(
                headerInformation, clientMacAdress, (UInt32)random.Next(),
                IPv4Address.Empty, IPv4Address.Empty, leasedAddress,
                new DHCPv4PacketMessageTypeOption(DHCPv4MessagesTypes.Request)
            );

            Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>> scopeResolverMock =
                new Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);

            var resolverInformations = new CreateScopeResolverInformation
            {
                Typename = nameof(DHCPv4RelayAgentSubnetResolver),
            };

            Mock<IScopeResolver<DHCPv4Packet, IPv4Address>> resolverMock = new Mock<IScopeResolver<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);
            resolverMock.Setup(x => x.PacketMeetsCondition(requestPacket)).Returns(true);
            resolverMock.SetupGet(x => x.HasUniqueIdentifier).Returns(false);

            scopeResolverMock.Setup(x => x.InitializeResolver(resolverInformations)).Returns(resolverMock.Object);

            Guid scopeId = random.NextGuid();
            Guid leaseId = random.NextGuid();

            DateTime leaseCreatedAt = DateTime.UtcNow.AddHours(-1);
            DHCPv4RootScope rootScope = GetRootScope(scopeResolverMock);
            rootScope.Load(new List<DomainEvent>{ new DHCPv4ScopeEvents.DHCPv4ScopeAddedEvent(
                new DHCPv4ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv4ScopeAddressProperties(
                        IPv4Address.FromString("192.168.178.1"),
                        IPv4Address.FromString("192.168.178.255"),
                        new List<IPv4Address>{IPv4Address.FromString("192.168.178.1") },
                        leaseTime: TimeSpan.FromDays(1),
                        supportDirectUnicast: false,
                        reuseAddressIfPossible: true
                        ),
                    ResolverInformation = resolverInformations,
                    Name = "Testscope",
                    Id = scopeId,
                }),
                new DHCPv4LeaseCreatedEvent
                {
                    EntityId = leaseId,
                    Address = leasedAddress,
                    HardwareAddress = clientMacAdress,
                    ScopeId = scopeId,
                    UniqueIdentifier = null,
                    StartedAt = leaseCreatedAt,
                    ValidUntil = DateTime.UtcNow.AddDays(1),
                 },
                new DHCPv4LeaseActivatedEvent
                {
                    EntityId = leaseId,
                    ScopeId = scopeId,
                }
            });

            DHCPv4Packet result = rootScope.HandleRequest(requestPacket);
            CheckNotAcknowledgePacket(result);

            CheckEventAmount(1, rootScope);
            CheckHandeledEvent(0, RequestErros.RenewingNotAllowed, requestPacket, result, rootScope, scopeId);
        }

        [Fact]
        public void HandleRequest_Init_LeaseFound_LeaseIsActive_ReuseAllowed()
        {
            Random random = new Random();

            IPv4Address leasedAddress = IPv4Address.FromString("192.168.178.10");

            IPv4HeaderInformation headerInformation =
                new IPv4HeaderInformation(leasedAddress, IPv4Address.FromString("192.168.178.1"));

            Byte[] clientMacAdress = random.NextBytes(6);

            DHCPv4Packet requestPacket = new DHCPv4Packet(
                headerInformation, clientMacAdress, (UInt32)random.Next(),
                IPv4Address.Empty, IPv4Address.Empty, IPv4Address.Empty,
                new DHCPv4PacketMessageTypeOption(DHCPv4MessagesTypes.Request),
                new DHCPv4PacketAddressOption(DHCPv4OptionTypes.RequestedIPAddress, leasedAddress)
            );

            Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>> scopeResolverMock =
                new Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);

            var resolverInformations = new CreateScopeResolverInformation
            {
                Typename = nameof(DHCPv4RelayAgentSubnetResolver),
            };

            Mock<IScopeResolver<DHCPv4Packet, IPv4Address>> resolverMock = new Mock<IScopeResolver<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);
            resolverMock.Setup(x => x.PacketMeetsCondition(requestPacket)).Returns(true);
            resolverMock.SetupGet(x => x.HasUniqueIdentifier).Returns(false);

            scopeResolverMock.Setup(x => x.InitializeResolver(resolverInformations)).Returns(resolverMock.Object);

            Guid scopeId = random.NextGuid();
            Guid leaseId = random.NextGuid();

            DateTime leaseCreatedAt = DateTime.UtcNow.AddHours(-1);
            DHCPv4RootScope rootScope = GetRootScope(scopeResolverMock);
            rootScope.Load(new List<DomainEvent>{ new DHCPv4ScopeEvents.DHCPv4ScopeAddedEvent(
                new DHCPv4ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv4ScopeAddressProperties(
                        IPv4Address.FromString("192.168.178.1"),
                        IPv4Address.FromString("192.168.178.255"),
                        new List<IPv4Address>{IPv4Address.FromString("192.168.178.1") },
                        leaseTime: TimeSpan.FromDays(1),
                        maskLength: 24,
                        reuseAddressIfPossible: true
                        ),
                    ResolverInformation = resolverInformations,
                    Name = "Testscope",
                    Id = scopeId,
                }),
                new DHCPv4LeaseCreatedEvent
                {
                    EntityId = leaseId,
                    Address = leasedAddress,
                    HardwareAddress = clientMacAdress,
                    ScopeId = scopeId,
                    UniqueIdentifier = null,
                    StartedAt = leaseCreatedAt,
                    ValidUntil = DateTime.UtcNow.AddDays(1),
                 },
                new DHCPv4LeaseActivatedEvent
                {
                    EntityId = leaseId,
                    ScopeId = scopeId,
                }
            });

            DHCPv4Packet result = rootScope.HandleRequest(requestPacket);
            CheckAcknowledgePacket(leasedAddress, result);
            CheckIfLeaseIsActive(scopeId, leaseId, rootScope, DateTime.UtcNow.AddHours(24));

            CheckEventAmount(2, rootScope);
            CheckLeaseRenewedEvent(0, scopeId, rootScope, leaseId, DateTime.UtcNow.AddHours(24));
            CheckHandeledEvent(1, RequestErros.NoError, requestPacket, result, rootScope, scopeId);
        }

        [Fact]
        public void HandleRequest_Init_LeaseFound_LeaseIsActive_ReuseNotAllowed()
        {
            Random random = new Random();

            IPv4Address leasedAddress = IPv4Address.FromString("192.168.178.10");
            IPv4Address expectedAddress = IPv4Address.FromString("192.168.178.2");


            IPv4HeaderInformation headerInformation =
                new IPv4HeaderInformation(leasedAddress, IPv4Address.FromString("192.168.178.1"));

            Byte[] clientMacAdress = random.NextBytes(6);

            DHCPv4Packet requestPacket = new DHCPv4Packet(
                headerInformation, clientMacAdress, (UInt32)random.Next(),
                IPv4Address.Empty, IPv4Address.Empty, IPv4Address.Empty,
                new DHCPv4PacketMessageTypeOption(DHCPv4MessagesTypes.Request),
                new DHCPv4PacketAddressOption(DHCPv4OptionTypes.RequestedIPAddress, leasedAddress)
            );

            Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>> scopeResolverMock =
                new Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);

            var resolverInformations = new CreateScopeResolverInformation
            {
                Typename = nameof(DHCPv4RelayAgentSubnetResolver),
            };

            Mock<IScopeResolver<DHCPv4Packet, IPv4Address>> resolverMock = new Mock<IScopeResolver<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);
            resolverMock.Setup(x => x.PacketMeetsCondition(requestPacket)).Returns(true);
            resolverMock.SetupGet(x => x.HasUniqueIdentifier).Returns(false);

            scopeResolverMock.Setup(x => x.InitializeResolver(resolverInformations)).Returns(resolverMock.Object);

            Guid scopeId = random.NextGuid();
            Guid leaseId = random.NextGuid();

            DateTime leaseCreatedAt = DateTime.UtcNow.AddHours(-1);
            DHCPv4RootScope rootScope = GetRootScope(scopeResolverMock);
            rootScope.Load(new List<DomainEvent>{ new DHCPv4ScopeEvents.DHCPv4ScopeAddedEvent(
                new DHCPv4ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv4ScopeAddressProperties(
                        IPv4Address.FromString("192.168.178.1"),
                        IPv4Address.FromString("192.168.178.255"),
                        new List<IPv4Address>{IPv4Address.FromString("192.168.178.1") },
                        leaseTime: TimeSpan.FromDays(1),
                        reuseAddressIfPossible: false,
                        maskLength: 24,
                        addressAllocationStrategy: DHCPv4ScopeAddressProperties.AddressAllocationStrategies.Next
                        ),
                    ResolverInformation = resolverInformations,
                    Name = "Testscope",
                    Id = scopeId,
                }),
                new DHCPv4LeaseCreatedEvent
                {
                    EntityId = leaseId,
                    Address = leasedAddress,
                    HardwareAddress = clientMacAdress,
                    ScopeId = scopeId,
                    UniqueIdentifier = null,
                    StartedAt = leaseCreatedAt,
                    ValidUntil = DateTime.UtcNow.AddDays(1),
                 },
                new DHCPv4LeaseActivatedEvent
                {
                    EntityId = leaseId,
                    ScopeId = scopeId,
                }
            });

            DHCPv4Packet result = rootScope.HandleRequest(requestPacket);
            CheckAcknowledgePacket(expectedAddress, result);

            CheckIfLeaseIsRevoked(scopeId, leaseId, rootScope);
            DHCPv4Lease lease = CheckLease(1, 2, expectedAddress, scopeId, rootScope, DateTime.UtcNow);

            CheckEventAmount(3, rootScope);
            CheckLeaseCreatedEvent(1, requestPacket.ClientHardwareAddress, scopeId, rootScope, expectedAddress, lease);
            CheckRevokedEvent(0, scopeId, leaseId, rootScope);

            CheckHandeledEvent(2, RequestErros.NoError, requestPacket, result, rootScope, scopeId);
        }


        [Fact]
        public void HandleRequest_Init_LeaseFound_LeaseIsNotActive_ReuseAllowed_AddressNotActive()
        {
            Random random = new Random();

            IPv4Address leasedAddress = IPv4Address.FromString("192.168.178.10");
            IPv4Address expectedAddress = IPv4Address.FromString("192.168.178.2");


            IPv4HeaderInformation headerInformation =
                new IPv4HeaderInformation(leasedAddress, IPv4Address.FromString("192.168.178.1"));

            Byte[] clientMacAdress = random.NextBytes(6);

            DHCPv4Packet requestPacket = new DHCPv4Packet(
                headerInformation, clientMacAdress, (UInt32)random.Next(),
                IPv4Address.Empty, IPv4Address.Empty, IPv4Address.Empty,
                new DHCPv4PacketMessageTypeOption(DHCPv4MessagesTypes.Request),
                new DHCPv4PacketAddressOption(DHCPv4OptionTypes.RequestedIPAddress, leasedAddress)
            );

            Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>> scopeResolverMock =
                new Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);

            var resolverInformations = new CreateScopeResolverInformation
            {
                Typename = nameof(DHCPv4RelayAgentSubnetResolver),
            };

            Mock<IScopeResolver<DHCPv4Packet, IPv4Address>> resolverMock = new Mock<IScopeResolver<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);
            resolverMock.Setup(x => x.PacketMeetsCondition(requestPacket)).Returns(true);
            resolverMock.SetupGet(x => x.HasUniqueIdentifier).Returns(false);

            scopeResolverMock.Setup(x => x.InitializeResolver(resolverInformations)).Returns(resolverMock.Object);

            Guid scopeId = random.NextGuid();
            Guid leaseId = random.NextGuid();

            DateTime leaseCreatedAt = DateTime.UtcNow.AddHours(-1);
            DHCPv4RootScope rootScope = GetRootScope(scopeResolverMock);
            rootScope.Load(new List<DomainEvent>{ new DHCPv4ScopeEvents.DHCPv4ScopeAddedEvent(
                new DHCPv4ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv4ScopeAddressProperties(
                        IPv4Address.FromString("192.168.178.1"),
                        IPv4Address.FromString("192.168.178.255"),
                        new List<IPv4Address>{IPv4Address.FromString("192.168.178.1") },
                        leaseTime: TimeSpan.FromDays(1),
                        reuseAddressIfPossible: true,
                        maskLength: 24,
                        addressAllocationStrategy: DHCPv4ScopeAddressProperties.AddressAllocationStrategies.Next
                        ),
                    ResolverInformation = resolverInformations,
                    Name = "Testscope",
                    Id = scopeId,
                }),
                new DHCPv4LeaseCreatedEvent
                {
                    EntityId = leaseId,
                    Address = leasedAddress,
                    HardwareAddress = clientMacAdress,
                    ScopeId = scopeId,
                    UniqueIdentifier = null,
                    StartedAt = leaseCreatedAt,
                    ValidUntil = DateTime.UtcNow.AddDays(1),
                 },
                new DHCPv4LeaseActivatedEvent
                {
                    EntityId = leaseId,
                    ScopeId = scopeId,
                },
                new DHCPv4LeaseExpiredEvent
                {
                    EntityId = leaseId,
                    ScopeId = scopeId,
                }
            });

            DHCPv4Packet result = rootScope.HandleRequest(requestPacket);
            CheckAcknowledgePacket(leasedAddress, result);
            CheckIfLeaseIsActive(scopeId, leaseId, rootScope, DateTime.UtcNow.AddHours(24));

            CheckEventAmount(2, rootScope);
            CheckLeaseRenewedEvent(0, scopeId, rootScope, leaseId, DateTime.UtcNow.AddHours(24));
            CheckHandeledEvent(1, RequestErros.NoError, requestPacket, result, rootScope, scopeId);
        }

        [Fact]
        public void HandleRequest_Init_LeaseFound_LeaseIsNotActive_ReuseAllowed_AddressActive()
        {
            Random random = new Random();

            IPv4Address leasedAddress = IPv4Address.FromString("192.168.178.10");
            IPv4Address expectedAddress = IPv4Address.FromString("192.168.178.2");

            IPv4HeaderInformation headerInformation =
                new IPv4HeaderInformation(leasedAddress, IPv4Address.FromString("192.168.178.1"));

            Byte[] clientMacAdress = random.NextBytes(6);

            DHCPv4Packet requestPacket = new DHCPv4Packet(
                headerInformation, clientMacAdress, (UInt32)random.Next(),
                IPv4Address.Empty, IPv4Address.Empty, IPv4Address.Empty,
                new DHCPv4PacketMessageTypeOption(DHCPv4MessagesTypes.Request),
                new DHCPv4PacketAddressOption(DHCPv4OptionTypes.RequestedIPAddress, leasedAddress)
            );

            Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>> scopeResolverMock =
                new Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);

            var resolverInformations = new CreateScopeResolverInformation
            {
                Typename = nameof(DHCPv4RelayAgentSubnetResolver),
            };

            Mock<IScopeResolver<DHCPv4Packet, IPv4Address>> resolverMock = new Mock<IScopeResolver<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);
            resolverMock.Setup(x => x.PacketMeetsCondition(requestPacket)).Returns(true);
            resolverMock.SetupGet(x => x.HasUniqueIdentifier).Returns(false);

            scopeResolverMock.Setup(x => x.InitializeResolver(resolverInformations)).Returns(resolverMock.Object);

            Guid scopeId = random.NextGuid();
            Guid leaseId = random.NextGuid();
            Guid secondLeaseId = random.NextGuid();

            DateTime leaseCreatedAt = DateTime.UtcNow.AddHours(-1);
            DHCPv4RootScope rootScope = GetRootScope(scopeResolverMock);
            rootScope.Load(new List<DomainEvent>{ new DHCPv4ScopeEvents.DHCPv4ScopeAddedEvent(
                new DHCPv4ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv4ScopeAddressProperties(
                        IPv4Address.FromString("192.168.178.1"),
                        IPv4Address.FromString("192.168.178.255"),
                        new List<IPv4Address>{IPv4Address.FromString("192.168.178.1") },
                        leaseTime: TimeSpan.FromDays(1),
                        reuseAddressIfPossible: true,
                        maskLength: 24,
                        addressAllocationStrategy: DHCPv4ScopeAddressProperties.AddressAllocationStrategies.Next
                        ),
                    ResolverInformation = resolverInformations,
                    Name = "Testscope",
                    Id = scopeId,
                }),
                new DHCPv4LeaseCreatedEvent
                {
                    EntityId = leaseId,
                    Address = leasedAddress,
                    HardwareAddress = clientMacAdress,
                    ScopeId = scopeId,
                    UniqueIdentifier = null,
                    StartedAt = leaseCreatedAt,
                    ValidUntil = DateTime.UtcNow.AddDays(1),
                 },
                new DHCPv4LeaseActivatedEvent
                {
                    EntityId = leaseId,
                    ScopeId = scopeId,
                },
                new DHCPv4LeaseExpiredEvent
                {
                    EntityId = leaseId,
                    ScopeId = scopeId,
                },
                new DHCPv4LeaseCreatedEvent
                {
                    EntityId = secondLeaseId,
                    Address = leasedAddress,
                    HardwareAddress = random.NextBytes(6),
                    ScopeId = scopeId,
                    UniqueIdentifier = null,
                    StartedAt = leaseCreatedAt,
                    ValidUntil = DateTime.UtcNow.AddDays(1),
                 },
                new DHCPv4LeaseActivatedEvent
                {
                    EntityId = secondLeaseId,
                    ScopeId = scopeId,
                },
            });

            DHCPv4Packet result = rootScope.HandleRequest(requestPacket);
            CheckAcknowledgePacket(expectedAddress, result);

            DHCPv4Lease lease = CheckLease(2, 3, expectedAddress, scopeId, rootScope, DateTime.UtcNow);

            CheckEventAmount(2, rootScope);
            CheckLeaseCreatedEvent(0, clientMacAdress, scopeId, rootScope, expectedAddress, lease);
            CheckHandeledEvent(1, RequestErros.NoError, requestPacket, result, rootScope, scopeId);
        }

        [Fact]
        public void HandleRequest_Init_LeaseFound_LeaseIsNotActive_ReuseNotAllowed()
        {
            Random random = new Random();

            IPv4Address leasedAddress = IPv4Address.FromString("192.168.178.10");
            IPv4Address expectedAddress = IPv4Address.FromString("192.168.178.2");

            IPv4HeaderInformation headerInformation =
                new IPv4HeaderInformation(leasedAddress, IPv4Address.FromString("192.168.178.1"));

            Byte[] clientMacAdress = random.NextBytes(6);

            DHCPv4Packet requestPacket = new DHCPv4Packet(
                headerInformation, clientMacAdress, (UInt32)random.Next(),
                IPv4Address.Empty, IPv4Address.Empty, IPv4Address.Empty,
                new DHCPv4PacketMessageTypeOption(DHCPv4MessagesTypes.Request),
                new DHCPv4PacketAddressOption(DHCPv4OptionTypes.RequestedIPAddress, leasedAddress)
            );

            Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>> scopeResolverMock =
                new Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);

            var resolverInformations = new CreateScopeResolverInformation
            {
                Typename = nameof(DHCPv4RelayAgentSubnetResolver),
            };

            Mock<IScopeResolver<DHCPv4Packet, IPv4Address>> resolverMock = new Mock<IScopeResolver<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);
            resolverMock.Setup(x => x.PacketMeetsCondition(requestPacket)).Returns(true);
            resolverMock.SetupGet(x => x.HasUniqueIdentifier).Returns(false);

            scopeResolverMock.Setup(x => x.InitializeResolver(resolverInformations)).Returns(resolverMock.Object);

            Guid scopeId = random.NextGuid();
            Guid leaseId = random.NextGuid();
            Guid secondLeaseId = random.NextGuid();

            DateTime leaseCreatedAt = DateTime.UtcNow.AddHours(-1);
            DHCPv4RootScope rootScope = GetRootScope(scopeResolverMock);
            rootScope.Load(new List<DomainEvent>{ new DHCPv4ScopeEvents.DHCPv4ScopeAddedEvent(
                new DHCPv4ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv4ScopeAddressProperties(
                        IPv4Address.FromString("192.168.178.1"),
                        IPv4Address.FromString("192.168.178.255"),
                        new List<IPv4Address>{IPv4Address.FromString("192.168.178.1") },
                        leaseTime: TimeSpan.FromDays(1),
                        reuseAddressIfPossible: false,
                        maskLength: 24,
                        addressAllocationStrategy: DHCPv4ScopeAddressProperties.AddressAllocationStrategies.Next
                        ),
                    ResolverInformation = resolverInformations,
                    Name = "Testscope",
                    Id = scopeId,
                }),
                new DHCPv4LeaseCreatedEvent
                {
                    EntityId = leaseId,
                    Address = leasedAddress,
                    HardwareAddress = clientMacAdress,
                    ScopeId = scopeId,
                    UniqueIdentifier = null,
                    StartedAt = leaseCreatedAt,
                    ValidUntil = DateTime.UtcNow.AddDays(1),
                 },
                new DHCPv4LeaseActivatedEvent
                {
                    EntityId = leaseId,
                    ScopeId = scopeId,
                },
                new DHCPv4LeaseExpiredEvent
                {
                    EntityId = leaseId,
                    ScopeId = scopeId,
                },
            });

            DHCPv4Packet result = rootScope.HandleRequest(requestPacket);
            CheckAcknowledgePacket(expectedAddress, result);

            DHCPv4Lease lease = CheckLease(1, 2, expectedAddress, scopeId, rootScope, DateTime.UtcNow);

            CheckEventAmount(2, rootScope);
            CheckLeaseCreatedEvent(0, clientMacAdress, scopeId, rootScope, expectedAddress, lease);
            CheckHandeledEvent(1, RequestErros.NoError, requestPacket, result, rootScope, scopeId);
        }

        [Fact]
        public void HandleRequest_Init_LeaseNotFound()
        {
            Random random = new Random();

            IPv4Address leasedAddress = IPv4Address.FromString("192.168.178.10");
            IPv4Address expectedAddress = IPv4Address.FromString("192.168.178.2");

            IPv4HeaderInformation headerInformation =
                new IPv4HeaderInformation(leasedAddress, IPv4Address.FromString("192.168.178.1"));

            Byte[] clientMacAdress = random.NextBytes(6);

            DHCPv4Packet requestPacket = new DHCPv4Packet(
                headerInformation, clientMacAdress, (UInt32)random.Next(),
                IPv4Address.Empty, IPv4Address.Empty, IPv4Address.Empty,
                new DHCPv4PacketMessageTypeOption(DHCPv4MessagesTypes.Request),
                new DHCPv4PacketAddressOption(DHCPv4OptionTypes.RequestedIPAddress, leasedAddress)
            );

            Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>> scopeResolverMock =
                new Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);

            var resolverInformations = new CreateScopeResolverInformation
            {
                Typename = nameof(DHCPv4RelayAgentSubnetResolver),
            };

            Mock<IScopeResolver<DHCPv4Packet, IPv4Address>> resolverMock = new Mock<IScopeResolver<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);
            resolverMock.Setup(x => x.PacketMeetsCondition(requestPacket)).Returns(true);
            resolverMock.SetupGet(x => x.HasUniqueIdentifier).Returns(false);

            scopeResolverMock.Setup(x => x.InitializeResolver(resolverInformations)).Returns(resolverMock.Object);

            Guid scopeId = random.NextGuid();
            Guid leaseId = random.NextGuid();
            Guid secondLeaseId = random.NextGuid();

            DateTime leaseCreatedAt = DateTime.UtcNow.AddHours(-1);
            DHCPv4RootScope rootScope = GetRootScope(scopeResolverMock);
            rootScope.Load(new List<DomainEvent>{ new DHCPv4ScopeEvents.DHCPv4ScopeAddedEvent(
                new DHCPv4ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv4ScopeAddressProperties(
                        IPv4Address.FromString("192.168.178.1"),
                        IPv4Address.FromString("192.168.178.255"),
                        new List<IPv4Address>{IPv4Address.FromString("192.168.178.1") },
                        leaseTime: TimeSpan.FromDays(1)
                        ),
                    ResolverInformation = resolverInformations,
                    Name = "Testscope",
                    Id = scopeId,
                }),
            });

            DHCPv4Packet result = rootScope.HandleRequest(requestPacket);
            Assert.Equal(DHCPv4Packet.Empty, result);

            CheckEventAmount(1, rootScope);
            CheckHandeledEvent(0, RequestErros.LeaseNotFound, requestPacket, result, rootScope, scopeId);
        }

        [Theory]
        [InlineData(DHCPv4ScopeAddressProperties.AddressAllocationStrategies.Next)]
        [InlineData(DHCPv4ScopeAddressProperties.AddressAllocationStrategies.Random)]
        public void HandleDiscover_NoLeaseFound_NoAddressAvaiable(DHCPv4ScopeAddressProperties.AddressAllocationStrategies allocationStrategy)
        {
            Random random = new Random();

            IPv4Address leasedAddress = IPv4Address.FromString("192.168.178.3");

            DHCPv4Packet requestPacket = GetRequestPacket(random, leasedAddress, DHCPv4PacketRequestType.Renewing);

            Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>> scopeResolverMock =
                new Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);

            var resolverInformations = new CreateScopeResolverInformation
            {
                Typename = nameof(DHCPv4RelayAgentSubnetResolver),
            };

            Mock<IScopeResolver<DHCPv4Packet, IPv4Address>> resolverMock = new Mock<IScopeResolver<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);
            resolverMock.Setup(x => x.PacketMeetsCondition(requestPacket)).Returns(true);
            resolverMock.SetupGet(x => x.HasUniqueIdentifier).Returns(false);

            scopeResolverMock.Setup(x => x.InitializeResolver(resolverInformations)).Returns(resolverMock.Object);

            Guid scopeId = random.NextGuid();
            Guid leaseId = random.NextGuid();

            DateTime leaseCreatedAt = DateTime.UtcNow.AddHours(-1);
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
                        leaseTime: TimeSpan.FromDays(1),
                        supportDirectUnicast: true,
                        reuseAddressIfPossible : false,
                        addressAllocationStrategy: allocationStrategy),
                    ResolverInformation = resolverInformations,
                    Name = "Testscope",
                    Id = scopeId,
                }),
                new DHCPv4LeaseCreatedEvent
                {
                    EntityId = leaseId,
                    Address = leasedAddress,
                    HardwareAddress = requestPacket.ClientHardwareAddress,
                    ScopeId = scopeId,
                    UniqueIdentifier = null,
                    StartedAt = leaseCreatedAt,
                    ValidUntil = DateTime.UtcNow.AddDays(1),
                 },
                new DHCPv4LeaseActivatedEvent
                {
                    EntityId = leaseId,
                    ScopeId = scopeId,
                }
            });

            DHCPv4Packet result = rootScope.HandleRequest(requestPacket);
            CheckNotAcknowledgePacket(result);

            CheckEventAmount(3, rootScope);
            DHCPv4ScopeAddressesAreExhaustedEvent(1, rootScope, scopeId);
            CheckHandeledEvent(2, RequestErros.NoAddressAvaiable, requestPacket, result, rootScope, scopeId);
        }
    }
}
