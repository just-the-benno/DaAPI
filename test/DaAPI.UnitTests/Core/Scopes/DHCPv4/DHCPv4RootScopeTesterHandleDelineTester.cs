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
using static DaAPI.Core.Scopes.DHCPv4.DHCPv4LeaseEvents;
using static DaAPI.Core.Scopes.DHCPv4.DHCPv4PacketHandledEvents;
using static DaAPI.Core.Scopes.DHCPv4.DHCPv4PacketHandledEvents.DHCPv4DeclineHandledEvent;

namespace DaAPI.UnitTests.Core.Scopes.DHCPv4
{
    public class DHCPv4RootScopeTesterHandleDelineTester : DHCPv4RootSCopeTesterBase
    {
        private DHCPv4Packet GetDeclinePacket(Random random, IPv4Address address)
        {
            IPv4HeaderInformation headerInformation =
                    new IPv4HeaderInformation(IPv4Address.Empty, IPv4Address.Broadcast);

            Byte[] clientMacAdress = random.NextBytes(6);

            DHCPv4Packet declinePacket = new DHCPv4Packet(
                headerInformation, clientMacAdress, (UInt32)random.Next(),
                IPv4Address.Empty, IPv4Address.Empty, IPv4Address.Empty,
                new DHCPv4PacketMessageTypeOption(DHCPv4Packet.DHCPv4MessagesTypes.Decline),
                new DHCPv4PacketAddressOption(DHCPv4OptionTypes.RequestedIPAddress, address)
            );

            return declinePacket;
        }

        private void CheckDeclinedEvent(
            Int32 index, DeclineErros error,
            DHCPv4Packet requestPacket,
            DHCPv4RootScope rootScope, Guid scopeId)
        {
            IEnumerable<DomainEvent> changes = rootScope.GetChanges();

            Assert.IsAssignableFrom<DHCPv4DeclineHandledEvent>(changes.ElementAt(index));

            DHCPv4DeclineHandledEvent handeledEvent = (DHCPv4DeclineHandledEvent)changes.ElementAt(index);
            Assert.Equal(requestPacket, handeledEvent.Request);
            Assert.Equal(DHCPv4Packet.Empty, handeledEvent.Response);
            Assert.Equal(scopeId, handeledEvent.ScopeId);
            Assert.Equal(error, handeledEvent.Error);
            if (error == DeclineErros.NoError)
            {
                Assert.True(handeledEvent.WasSuccessfullHandled);
            }
            else
            {
                Assert.False(handeledEvent.WasSuccessfullHandled);
            }
        }

        private void CheckDeclineEvent(int index, Guid leaseId, IPv4Address address, Guid scopeId, DHCPv4RootScope rootScope)
        {
            IEnumerable<DomainEvent> changes = rootScope.GetChanges();

            Assert.IsAssignableFrom<DHCPv4AddressSuspendedEvent>(changes.ElementAt(index));

            DHCPv4AddressSuspendedEvent @event = (DHCPv4AddressSuspendedEvent)changes.ElementAt(index);

            Assert.NotNull(@event);
            Assert.Equal(scopeId, @event.ScopeId);
            Assert.Equal(leaseId, @event.EntityId);
            Assert.Equal(address, @event.Address);
            Assert.True((@event.SuspendedTill - DateTime.UtcNow).TotalMinutes >= (5 - 1));
        }

        [Fact]
        public void HandleDecline_AcceptDecline_RequesAddressExits_LeaseExists_LeaseIsPending_AddressIsNotSuspended()
        {
            Random random = new Random();

            IPv4Address leasedAddress = IPv4Address.FromString("192.168.178.3");

            DHCPv4Packet requestPacket = GetDeclinePacket(random, leasedAddress);

            Mock<IDHCPv4ScopeResolverManager> scopeResolverMock =
                new Mock<IDHCPv4ScopeResolverManager>(MockBehavior.Strict);

            var resolverInformations = new DHCPv4CreateScopeResolverInformation
            {
                Typename = nameof(DHCPv4RelayAgentSubnetResolver),
            };

            Mock<IDHCPv4ScopeResolver> resolverMock = new Mock<IDHCPv4ScopeResolver>(MockBehavior.Strict);
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
                        IPv4Address.FromString("192.168.178.100"),
                        new List<IPv4Address>{
                            IPv4Address.FromString("192.168.178.0") },
                        validLifetime: TimeSpan.FromDays(1),
                        acceptDecline : true),
                    ResolverInformations = resolverInformations,
                    Name = "Testscope",
                    Id = scopeId,
                }),
                new DHCPv4LeaseCreatedEvent
                {
                    EntityId = leaseId,
                    Address = leasedAddress,
                    ClientIdentifier = DHCPv4ClientIdentifier.FromHwAddress(requestPacket.ClientHardwareAddress),
                    ScopeId = scopeId,
                    UniqueIdentiifer = null,
                    StartedAt = leaseCreatedAt,
                    ValidUntil = DateTime.UtcNow.AddDays(1),
                 },
            });

            DHCPv4Packet result = rootScope.HandleDecline(requestPacket);
            Assert.Equal(DHCPv4Packet.Empty, result);

            DHCPv4Lease lease = rootScope.GetScopeById(scopeId).Leases.GetLeaseById(leaseId);
            Assert.Equal(DHCPv4Lease.DHCPv4LeaseStates.Suspended, lease.State);

            CheckEventAmount(2, rootScope);
            CheckDeclineEvent(0, leaseId, leasedAddress, scopeId, rootScope);
            CheckDeclinedEvent(1, DeclineErros.NoError, requestPacket, rootScope, scopeId);
        }

        [Fact]
        public void HandleDecline_AcceptDecline_RequesAddressExits_LeaseExists_LeaseIsActive_AddressIsNotSuspended()
        {
            Random random = new Random();

            IPv4Address leasedAddress = IPv4Address.FromString("192.168.178.3");

            DHCPv4Packet requestPacket = GetDeclinePacket(random, leasedAddress);

            Mock<IDHCPv4ScopeResolverManager> scopeResolverMock =
                new Mock<IDHCPv4ScopeResolverManager>(MockBehavior.Strict);

            var resolverInformations = new DHCPv4CreateScopeResolverInformation
            {
                Typename = nameof(DHCPv4RelayAgentSubnetResolver),
            };

            Mock<IDHCPv4ScopeResolver> resolverMock = new Mock<IDHCPv4ScopeResolver>(MockBehavior.Strict);
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
                        IPv4Address.FromString("192.168.178.100"),
                        new List<IPv4Address>{
                            IPv4Address.FromString("192.168.178.0") },
                        validLifetime: TimeSpan.FromDays(1),
                        acceptDecline : true),
                    ResolverInformations = resolverInformations,
                    Name = "Testscope",
                    Id = scopeId,
                }),
                new DHCPv4LeaseCreatedEvent
                {
                    EntityId = leaseId,
                    Address = leasedAddress,
                    ClientIdentifier = DHCPv4ClientIdentifier.FromHwAddress(requestPacket.ClientHardwareAddress),
                    ScopeId = scopeId,
                    UniqueIdentiifer = null,
                    StartedAt = leaseCreatedAt,
                    ValidUntil = DateTime.UtcNow.AddDays(1),
                 },
                new DHCPv4LeaseActivatedEvent
                {
                    EntityId = leaseId,
                    ScopeId = scopeId,
                }
            });

            DHCPv4Packet result = rootScope.HandleDecline(requestPacket);
            Assert.Equal(DHCPv4Packet.Empty, result);

            DHCPv4Lease lease = rootScope.GetScopeById(scopeId).Leases.GetLeaseById(leaseId);
            Assert.Equal(DHCPv4Lease.DHCPv4LeaseStates.Suspended, lease.State);

            CheckEventAmount(2, rootScope);
            CheckDeclineEvent(0, leaseId, leasedAddress, scopeId, rootScope);
            CheckDeclinedEvent(1, DeclineErros.NoError, requestPacket, rootScope, scopeId);
        }

        [Fact]
        public void HandleDecline_AcceptDecline_RequesAddressExits_LeaseExists_LeaseIsPending_AddressIsSuspended()
        {
            Random random = new Random();

            IPv4Address leasedAddress = IPv4Address.FromString("192.168.178.3");

            DHCPv4Packet requestPacket = GetDeclinePacket(random, leasedAddress);

            Mock<IDHCPv4ScopeResolverManager> scopeResolverMock =
                new Mock<IDHCPv4ScopeResolverManager>(MockBehavior.Strict);

            var resolverInformations = new DHCPv4CreateScopeResolverInformation
            {
                Typename = nameof(DHCPv4RelayAgentSubnetResolver),
            };

            Mock<IDHCPv4ScopeResolver> resolverMock = new Mock<IDHCPv4ScopeResolver>(MockBehavior.Strict);
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
                        IPv4Address.FromString("192.168.178.0"),
                        IPv4Address.FromString("192.168.178.100"),
                        new List<IPv4Address>{
                            IPv4Address.FromString("192.168.178.0") },
                        validLifetime: TimeSpan.FromDays(1),
                        acceptDecline : true),
                    ResolverInformations = resolverInformations,
                    Name = "Testscope",
                    Id = scopeId,
                }),
                new DHCPv4LeaseCreatedEvent
                {
                    EntityId = leaseId,
                    Address = leasedAddress,
                    ClientIdentifier = DHCPv4ClientIdentifier.FromHwAddress(requestPacket.ClientHardwareAddress),
                    ScopeId = scopeId,
                    UniqueIdentiifer = null,
                    StartedAt = leaseCreatedAt,
                    ValidUntil = DateTime.UtcNow.AddDays(1),
                 },
                new DHCPv4LeaseCreatedEvent
                {
                    EntityId = secondLeaseId,
                    Address = leasedAddress,
                    ClientIdentifier = DHCPv4ClientIdentifier.FromHwAddress(requestPacket.ClientHardwareAddress),
                    ScopeId = scopeId,
                    UniqueIdentiifer = null,
                    StartedAt = leaseCreatedAt,
                    ValidUntil = DateTime.UtcNow.AddDays(1),
                 },
                new DHCPv4AddressSuspendedEvent
                {
                    EntityId = secondLeaseId,
                    ScopeId = scopeId,
                    Address = leasedAddress,
                    SuspendedTill = DateTime.UtcNow.AddHours(12),
                }
            });

            DHCPv4Packet result = rootScope.HandleDecline(requestPacket);
            Assert.Equal(DHCPv4Packet.Empty, result);

            DHCPv4Lease lease = rootScope.GetScopeById(scopeId).Leases.GetLeaseById(leaseId);
            Assert.Equal(DHCPv4Lease.DHCPv4LeaseStates.Pending, lease.State);

            CheckEventAmount(1, rootScope);
            CheckDeclinedEvent(0, DeclineErros.AddressAlreadySuspended, requestPacket, rootScope, scopeId);
        }

        [Fact]
        public void HandleDecline_AcceptDecline_RequesAddressExits_LeaseExists_LeaseNorPendingOrActive()
        {
            Random random = new Random();

            IPv4Address leasedAddress = IPv4Address.FromString("192.168.178.3");

            DHCPv4Packet requestPacket = GetDeclinePacket(random, leasedAddress);

            Mock<IDHCPv4ScopeResolverManager> scopeResolverMock =
                new Mock<IDHCPv4ScopeResolverManager>(MockBehavior.Strict);

            var resolverInformations = new DHCPv4CreateScopeResolverInformation
            {
                Typename = nameof(DHCPv4RelayAgentSubnetResolver),
            };

            Mock<IDHCPv4ScopeResolver> resolverMock = new Mock<IDHCPv4ScopeResolver>(MockBehavior.Strict);
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
                        IPv4Address.FromString("192.168.178.100"),
                        new List<IPv4Address>{
                            IPv4Address.FromString("192.168.178.0") },
                        validLifetime: TimeSpan.FromDays(1),
                        acceptDecline : true),
                    ResolverInformations = resolverInformations,
                    Name = "Testscope",
                    Id = scopeId,
                }),
                new DHCPv4LeaseCreatedEvent
                {
                    EntityId = leaseId,
                    Address = leasedAddress,
                    ClientIdentifier = DHCPv4ClientIdentifier.FromHwAddress(requestPacket.ClientHardwareAddress),
                    ScopeId = scopeId,
                    UniqueIdentiifer = null,
                    StartedAt = leaseCreatedAt,
                    ValidUntil = DateTime.UtcNow.AddDays(1),
                 },
                new DHCPv4LeaseActivatedEvent
                {
                    EntityId = leaseId,
                    ScopeId = scopeId,
                },
                new DHCPv4LeaseCanceledEvent
                {
                    EntityId = leaseId,
                    ScopeId = scopeId,
                }
            });

            DHCPv4Packet result = rootScope.HandleDecline(requestPacket);
            Assert.Equal(DHCPv4Packet.Empty, result);

            DHCPv4Lease lease = rootScope.GetScopeById(scopeId).Leases.GetLeaseById(leaseId);
            Assert.Equal(DHCPv4Lease.DHCPv4LeaseStates.Canceled, lease.State);

            CheckEventAmount(1, rootScope);
            CheckDeclinedEvent(0, DeclineErros.LeaseInInvalidState, requestPacket, rootScope, scopeId);
        }

        [Fact]
        public void HandleDecline_AcceptDecline_RequesAddressExits_LeaseNotExists()
        {
            Random random = new Random();

            IPv4Address leasedAddress = IPv4Address.FromString("192.168.178.3");
            IPv4Address requestedAddress = IPv4Address.FromString("192.168.178.4");


            DHCPv4Packet requestPacket = GetDeclinePacket(random, requestedAddress);

            Mock<IDHCPv4ScopeResolverManager> scopeResolverMock =
                new Mock<IDHCPv4ScopeResolverManager>(MockBehavior.Strict);

            var resolverInformations = new DHCPv4CreateScopeResolverInformation
            {
                Typename = nameof(DHCPv4RelayAgentSubnetResolver),
            };

            Mock<IDHCPv4ScopeResolver> resolverMock = new Mock<IDHCPv4ScopeResolver>(MockBehavior.Strict);
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
                        IPv4Address.FromString("192.168.178.100"),
                        new List<IPv4Address>{
                            IPv4Address.FromString("192.168.178.0") },
                        validLifetime: TimeSpan.FromDays(1),
                        acceptDecline : true),
                    ResolverInformations = resolverInformations,
                    Name = "Testscope",
                    Id = scopeId,
                }),
                new DHCPv4LeaseCreatedEvent
                {
                    EntityId = leaseId,
                    Address = leasedAddress,
                    ClientIdentifier = DHCPv4ClientIdentifier.FromHwAddress(requestPacket.ClientHardwareAddress),
                    ScopeId = scopeId,
                    UniqueIdentiifer = null,
                    StartedAt = leaseCreatedAt,
                    ValidUntil = DateTime.UtcNow.AddDays(1),
                 },
                new DHCPv4LeaseActivatedEvent
                {
                    EntityId = leaseId,
                    ScopeId = scopeId,
                }
            });

            DHCPv4Packet result = rootScope.HandleDecline(requestPacket);
            Assert.Equal(DHCPv4Packet.Empty, result);

            DHCPv4Lease lease = rootScope.GetScopeById(scopeId).Leases.GetLeaseById(leaseId);
            Assert.Equal(DHCPv4Lease.DHCPv4LeaseStates.Active, lease.State);

            CheckEventAmount(1, rootScope);
            CheckDeclinedEvent(0, DeclineErros.LeaseNotFound, requestPacket, rootScope, scopeId);
        }


        [Fact]
        public void HandleDecline_NpAcceptDecline()
        {
            Random random = new Random();

            IPv4Address leasedAddress = IPv4Address.FromString("192.168.178.3");

            DHCPv4Packet requestPacket = GetDeclinePacket(random, leasedAddress);

            Mock<IDHCPv4ScopeResolverManager> scopeResolverMock =
                new Mock<IDHCPv4ScopeResolverManager>(MockBehavior.Strict);

            var resolverInformations = new DHCPv4CreateScopeResolverInformation
            {
                Typename = nameof(DHCPv4RelayAgentSubnetResolver),
            };

            Mock<IDHCPv4ScopeResolver> resolverMock = new Mock<IDHCPv4ScopeResolver>(MockBehavior.Strict);
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
                        IPv4Address.FromString("192.168.178.100"),
                        new List<IPv4Address>{
                            IPv4Address.FromString("192.168.178.0") },
                        validLifetime: TimeSpan.FromDays(1),
                        acceptDecline : false),
                    ResolverInformations = resolverInformations,
                    Name = "Testscope",
                    Id = scopeId,
                }),
                new DHCPv4LeaseCreatedEvent
                {
                    EntityId = leaseId,
                    Address = leasedAddress,
                    ClientIdentifier = DHCPv4ClientIdentifier.FromHwAddress(requestPacket.ClientHardwareAddress),
                    ScopeId = scopeId,
                    UniqueIdentiifer = null,
                    StartedAt = leaseCreatedAt,
                    ValidUntil = DateTime.UtcNow.AddDays(1),
                 },
                new DHCPv4LeaseActivatedEvent
                {
                    EntityId = leaseId,
                    ScopeId = scopeId,
                },
            });

            DHCPv4Packet result = rootScope.HandleDecline(requestPacket);
            Assert.Equal(DHCPv4Packet.Empty, result);

            DHCPv4Lease lease = rootScope.GetScopeById(scopeId).Leases.GetLeaseById(leaseId);
            Assert.Equal(DHCPv4Lease.DHCPv4LeaseStates.Active, lease.State);

            CheckEventAmount(1, rootScope);
            CheckDeclinedEvent(0, DeclineErros.DeclineNotAllowed, requestPacket, rootScope, scopeId);
        }


    }
}
