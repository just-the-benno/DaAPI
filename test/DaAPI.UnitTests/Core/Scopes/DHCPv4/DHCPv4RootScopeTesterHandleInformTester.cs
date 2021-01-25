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
using static DaAPI.Core.Scopes.DHCPv4.DHCPv4PacketHandledEvents.DHCPv4InformHandledEvent;

namespace DaAPI.UnitTests.Core.Scopes.DHCPv4
{
    public class DHCPv4RootScopeTesterHandleInformTester : DHCPv4RootScopeTesterBase
    {
        private void CheckHandeledEvent(
          Int32 index, InformErros error,
          DHCPv4Packet requestPacket,
          DHCPv4Packet result,
          DHCPv4RootScope rootScope)
        {
            IEnumerable<DomainEvent> changes = rootScope.GetChanges();

            Assert.IsAssignableFrom<DHCPv4InformHandledEvent>(changes.ElementAt(index));

            DHCPv4InformHandledEvent handeledEvent = (DHCPv4InformHandledEvent)changes.ElementAt(index);
            Assert.Equal(requestPacket, handeledEvent.Request);
            Assert.Equal(result, handeledEvent.Response);
            Assert.Equal(error, handeledEvent.Error);
            if (error == InformErros.NoError)
            {
                Assert.True(handeledEvent.WasSuccessfullHandled);
            }
            else
            {
                Assert.False(handeledEvent.WasSuccessfullHandled);
            }
        }

        private static void CheckAcknowledgePacket(IPv4Address clientAddress, DHCPv4Packet result)
        {
            Assert.NotNull(result);
            Assert.NotEqual(DHCPv4Packet.Empty, result);
            Assert.True(result.IsValid);

            Assert.Equal(IPv4Address.Empty, result.YourIPAdress);

            Assert.Equal(clientAddress, result.Header.Destionation);
            Assert.Equal(clientAddress, result.ClientIPAdress);

            Assert.Equal(DHCPv4MessagesTypes.Acknowledge, result.MessageType);
        }

        [Fact]
        public void HandleInform_InformsAreAllowed()
        {
            Random random = new Random();
            IPv4Address clientAddress = IPv4Address.FromString("192.168.178.10");

            IPv4HeaderInformation headerInformation =
                new IPv4HeaderInformation(clientAddress, IPv4Address.FromString("192.168.178.1"));

            Byte[] clientMacAdress = random.NextBytes(6);

            DHCPv4Packet requestPacket = new DHCPv4Packet(
                headerInformation, clientMacAdress, (UInt32)random.Next(),
                IPv4Address.Empty, IPv4Address.Empty, clientAddress,
                new DHCPv4PacketMessageTypeOption(DHCPv4MessagesTypes.Inform)
            );

            Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>> scopeResolverMock =
               new Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);

            var resolverInformations = new CreateScopeResolverInformation
            {
                Typename = nameof(DHCPv4RelayAgentSubnetResolver),
            };

            Mock<IScopeResolver<DHCPv4Packet, IPv4Address>> resolverMock = new Mock<IScopeResolver<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);

            scopeResolverMock.Setup(x => x.InitializeResolver(resolverInformations)).Returns(resolverMock.Object);

            Guid scopeId = random.NextGuid();

            DHCPv4RootScope rootScope = GetRootScope(scopeResolverMock);
            rootScope.Load(new List<DomainEvent>{ new DHCPv4ScopeEvents.DHCPv4ScopeAddedEvent(
                new DHCPv4ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv4ScopeAddressProperties(
                        IPv4Address.FromString("192.168.178.1"),
                        IPv4Address.FromString("192.168.178.255"),
                        new List<IPv4Address>{IPv4Address.FromString("192.168.178.1") },
                        informsAreAllowd: true
                        ),
                    ResolverInformation = resolverInformations,
                    Name = "Testscope",
                    Id = scopeId,
                })
            });

            DHCPv4Packet result = rootScope.HandleInform(requestPacket);
            CheckAcknowledgePacket(clientAddress, result);

            CheckEventAmount(1, rootScope);
            CheckHandeledEvent(0, InformErros.NoError, requestPacket,result, rootScope);
        }


        [Fact]
        public void HandleInform_InformsAreNotAllowed()
        {
            Random random = new Random();
            IPv4Address clientAddress = IPv4Address.FromString("192.168.178.10");

            IPv4HeaderInformation headerInformation =
                new IPv4HeaderInformation(clientAddress, IPv4Address.FromString("192.168.178.1"));

            Byte[] clientMacAdress = random.NextBytes(6);

            DHCPv4Packet requestPacket = new DHCPv4Packet(
                headerInformation, clientMacAdress, (UInt32)random.Next(),
                IPv4Address.Empty, IPv4Address.Empty, clientAddress,
                new DHCPv4PacketMessageTypeOption(DHCPv4MessagesTypes.Inform)
            );

            Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>> scopeResolverMock =
               new Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);

            var resolverInformations = new CreateScopeResolverInformation
            {
                Typename = nameof(DHCPv4RelayAgentSubnetResolver),
            };

            Mock<IScopeResolver<DHCPv4Packet, IPv4Address>> resolverMock = new Mock<IScopeResolver<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);

            scopeResolverMock.Setup(x => x.InitializeResolver(resolverInformations)).Returns(resolverMock.Object);

            Guid scopeId = random.NextGuid();

            DHCPv4RootScope rootScope = GetRootScope(scopeResolverMock);
            rootScope.Load(new List<DomainEvent>{ new DHCPv4ScopeEvents.DHCPv4ScopeAddedEvent(
                new DHCPv4ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv4ScopeAddressProperties(
                        IPv4Address.FromString("192.168.178.1"),
                        IPv4Address.FromString("192.168.178.255"),
                        new List<IPv4Address>{IPv4Address.FromString("192.168.178.1") },
                        informsAreAllowd: false
                        ),
                    ResolverInformation = resolverInformations,
                    Name = "Testscope",
                    Id = scopeId,
                })
            });

            DHCPv4Packet result = rootScope.HandleInform(requestPacket);
            Assert.Equal(DHCPv4Packet.Empty, result);

            CheckEventAmount(1, rootScope);
            CheckHandeledEvent(0, InformErros.InformsNotAllowed, requestPacket, result, rootScope);
        }
    }
}
