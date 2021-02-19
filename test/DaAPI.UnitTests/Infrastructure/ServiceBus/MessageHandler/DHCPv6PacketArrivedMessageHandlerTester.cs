using Castle.Core.Logging;
using DaAPI.Core.Packets.DHCPv6;
using DaAPI.Infrastructure.FilterEngines.DHCPv6;
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
    public class DHCPv6PacketArrivedMessageHandlerTester
    {
        [Fact]
        public async Task Handle_PacketFiltered()
        {
            DHCPv6Packet packet = DHCPv6Packet.AsInner(1, DHCPv6PacketTypes.Solicit, new List<DHCPv6PacketOption>());

            Mock<IDHCPv6PacketFilterEngine> filterEngineMock = new Mock<IDHCPv6PacketFilterEngine>(MockBehavior.Strict);
            filterEngineMock.Setup(x => x.ShouldPacketBeFilterd(packet)).ReturnsAsync((true, "Filter1")).Verifiable();

            Mock<IServiceBus> serviceBusMock = new Mock<IServiceBus>(MockBehavior.Strict);
            serviceBusMock.Setup(x => x.Publish(It.Is<DHCPv6PacketFilteredMessage>(y => y.Packet == packet))).Returns(Task.CompletedTask).Verifiable();

            DHCPv6PacketArrivedMessageHandler handler = new DHCPv6PacketArrivedMessageHandler(
                serviceBusMock.Object, filterEngineMock.Object,
                Mock.Of<ILogger<DHCPv6PacketArrivedMessageHandler>>());

            await handler.Handle(new DHCPv6PacketArrivedMessage(packet), CancellationToken.None);

            filterEngineMock.Verify();
            serviceBusMock.Verify();
        }

        [Fact]
        public async Task Handle_PacketNotFiltered()
        {
            DHCPv6Packet packet = DHCPv6Packet.AsInner(1, DHCPv6PacketTypes.Solicit, new List<DHCPv6PacketOption>());

            Mock<IDHCPv6PacketFilterEngine> filterEngineMock = new Mock<IDHCPv6PacketFilterEngine>(MockBehavior.Strict);
            filterEngineMock.Setup(x => x.ShouldPacketBeFilterd(packet)).ReturnsAsync((false, null)).Verifiable();

            Mock<IServiceBus> serviceBusMock = new Mock<IServiceBus>(MockBehavior.Strict);
            serviceBusMock.Setup(x => x.Publish(It.Is<ValidDHCPv6PacketArrivedMessage>(y => y.Packet == packet))).Returns(Task.CompletedTask).Verifiable();

            DHCPv6PacketArrivedMessageHandler handler = new DHCPv6PacketArrivedMessageHandler(
                serviceBusMock.Object, filterEngineMock.Object,
                Mock.Of<ILogger<DHCPv6PacketArrivedMessageHandler>>());

            await handler.Handle(new DHCPv6PacketArrivedMessage(packet), CancellationToken.None);

            filterEngineMock.Verify();
            serviceBusMock.Verify();
        }

    }
}
