using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Packets.DHCPv6;
using DaAPI.Infrastructure.InterfaceEngines;
using DaAPI.Infrastructure.ServiceBus;
using DaAPI.Infrastructure.ServiceBus.MessageHandler;
using DaAPI.Infrastructure.ServiceBus.Messages;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace DaAPI.UnitTests.Infrastructure.ServiceBus.MessageHandler
{
    public class DHCPv6PacketReadyToSendMessageHandlerTester
    {
        [Fact]
        public async Task Handle()
        {
            IPv6HeaderInformation headerInformation = new IPv6HeaderInformation(
                IPv6Address.FromString("fe80::2"), IPv6Address.FromString("fe80::1"));

            DHCPv6Packet response = DHCPv6Packet.AsOuter(headerInformation, 1, DHCPv6PacketTypes.ADVERTISE, new List<DHCPv6PacketOption>());

            Mock<IDHCPv6InterfaceEngine> interfaceEngine = new Mock<IDHCPv6InterfaceEngine>(MockBehavior.Strict);
            interfaceEngine.Setup(x => x.SendPacket(response)).Returns(true).Verifiable();

            DHCPv6PacketReadyToSendMessageHandler handler = new DHCPv6PacketReadyToSendMessageHandler(
                interfaceEngine.Object,
                Mock.Of<ILogger<DHCPv6PacketReadyToSendMessageHandler>>());

            await handler.Handle(new DHCPv6PacketReadyToSendMessage(response), CancellationToken.None);

            interfaceEngine.Verify();
        }

        [Fact]
        public async Task Handle_EmptyPacket()
        {
            DHCPv6PacketReadyToSendMessageHandler handler = new DHCPv6PacketReadyToSendMessageHandler(
                Mock.Of<IDHCPv6InterfaceEngine>(MockBehavior.Strict),
                Mock.Of<ILogger<DHCPv6PacketReadyToSendMessageHandler>>());

            await handler.Handle(new DHCPv6PacketReadyToSendMessage(null), CancellationToken.None);
        }
    }
}
