using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Packets.DHCPv6;
using DaAPI.Infrastructure.LeaseEngines.DHCPv6;
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
    public class ValidDHCPv6PacketArrivedMessageHandlerTester
    {
        [Fact]
        public async Task Handle_ResponseAvaiable()
        {
            IPv6HeaderInformation headerInformation = new IPv6HeaderInformation(
                IPv6Address.FromString("fe80::2"), IPv6Address.FromString("fe80::1"));

            DHCPv6Packet packet = DHCPv6Packet.AsOuter(headerInformation, 1, DHCPv6PacketTypes.Solicit, new List<DHCPv6PacketOption>());
            DHCPv6Packet reseponsePacket = DHCPv6Packet.AsOuter(IPv6HeaderInformation.AsResponse(headerInformation), 1, DHCPv6PacketTypes.ADVERTISE, new List<DHCPv6PacketOption>());

            Mock<IDHCPv6LeaseEngine> leaseEngineMock = new Mock<IDHCPv6LeaseEngine>(MockBehavior.Strict);
            leaseEngineMock.Setup(x => x.HandlePacket(packet)).ReturnsAsync(reseponsePacket).Verifiable();

            Mock<IServiceBus> serviceBusMock = new Mock<IServiceBus>(MockBehavior.Strict);
            serviceBusMock.Setup(x => x.Publish(It.Is<DHCPv6PacketReadyToSendMessage>(y => y.Packet == reseponsePacket))).Returns(Task.CompletedTask).Verifiable();

            ValidDHCPv6PacketArrivedMessageHandler handler = new ValidDHCPv6PacketArrivedMessageHandler(
                serviceBusMock.Object, leaseEngineMock.Object,
                Mock.Of<ILogger<ValidDHCPv6PacketArrivedMessageHandler>>());

            await handler.Handle(new ValidDHCPv6PacketArrivedMessage(packet), CancellationToken.None);

            leaseEngineMock.Verify();
            serviceBusMock.Verify();
        }

        [Fact]
        public async Task Handle_NoResponseAvaiable()
        {
            IPv6HeaderInformation headerInformation = new IPv6HeaderInformation(
                IPv6Address.FromString("fe80::2"), IPv6Address.FromString("fe80::1"));

            DHCPv6Packet packet = DHCPv6Packet.AsOuter(headerInformation, 1, DHCPv6PacketTypes.Solicit, new List<DHCPv6PacketOption>());
            DHCPv6Packet reseponsePacket = DHCPv6Packet.Empty;

            Mock<IDHCPv6LeaseEngine> leaseEngineMock = new Mock<IDHCPv6LeaseEngine>(MockBehavior.Strict);
            leaseEngineMock.Setup(x => x.HandlePacket(packet)).ReturnsAsync(reseponsePacket).Verifiable();

            Mock<IServiceBus> serviceBusMock = new Mock<IServiceBus>(MockBehavior.Strict);

            ValidDHCPv6PacketArrivedMessageHandler handler = new ValidDHCPv6PacketArrivedMessageHandler(
                Mock.Of<IServiceBus>(MockBehavior.Strict), leaseEngineMock.Object,
                Mock.Of<ILogger<ValidDHCPv6PacketArrivedMessageHandler>>());

            await handler.Handle(new ValidDHCPv6PacketArrivedMessage(packet), CancellationToken.None);

            leaseEngineMock.Verify();
            serviceBusMock.Verify();
        }
    }
}
