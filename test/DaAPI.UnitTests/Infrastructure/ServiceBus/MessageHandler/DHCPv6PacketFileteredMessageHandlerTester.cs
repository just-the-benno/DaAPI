using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Packets.DHCPv6;
using DaAPI.Infrastructure.LeaseEngines.DHCPv6;
using DaAPI.Infrastructure.ServiceBus;
using DaAPI.Infrastructure.ServiceBus.MessageHandler;
using DaAPI.Infrastructure.ServiceBus.Messages;
using DaAPI.Infrastructure.StorageEngine.DHCPv6;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using DaAPI.TestHelper;

namespace DaAPI.UnitTests.Infrastructure.ServiceBus.MessageHandler
{
    public class DHCPv6PacketFileteredMessageHandlerTester
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Handle(Boolean storageResult)
        {
            Random random = new Random();
            String filtername = random.GetAlphanumericString();

            IPv6HeaderInformation headerInformation = new IPv6HeaderInformation(
                IPv6Address.FromString("fe80::2"), IPv6Address.FromString("fe80::1"));

            DHCPv6Packet packet = DHCPv6RelayPacket.AsOuterRelay(headerInformation, true, 1, IPv6Address.FromString("fe80::3"), IPv6Address.FromString("fe80::4"), Array.Empty<DHCPv6PacketOption>(),
                DHCPv6Packet.AsInner(1, DHCPv6PacketTypes.ADVERTISE, new List<DHCPv6PacketOption>()));

            Mock<IDHCPv6StorageEngine> storageEngineMock = new Mock<IDHCPv6StorageEngine>(MockBehavior.Strict);
            storageEngineMock.Setup(x => x.LogFilteredDHCPv6Packet(packet, filtername)).ReturnsAsync(storageResult);

            DHCPv6PacketFileteredMessageHandler handler = new DHCPv6PacketFileteredMessageHandler(
                storageEngineMock.Object,
                Mock.Of<ILogger<DHCPv6PacketFileteredMessageHandler>>());

            await handler.Handle(new DHCPv6PacketFileteredMessage(packet, filtername), CancellationToken.None);

            storageEngineMock.Verify();
        }
    }
}
