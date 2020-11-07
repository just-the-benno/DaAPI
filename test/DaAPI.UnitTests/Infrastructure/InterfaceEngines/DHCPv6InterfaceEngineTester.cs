using DaAPI.Core.Common;
using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Listeners;
using DaAPI.Core.Packets.DHCPv6;
using DaAPI.Infrastructure.InterfaceEngines;
using DaAPI.Infrastructure.ServiceBus;
using DaAPI.Infrastructure.ServiceBus.Messages;
using DaAPI.Infrastructure.StorageEngine;
using DaAPI.Infrastructure.StorageEngine.DHCPv6;
using DaAPI.TestHelper;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace DaAPI.UnitTests.Infrastructure.InterfaceEngines
{
    [Collection("SocketTester")]
    public class DHCPv6InterfaceEngineTester
    {
        [Fact]
        public void GetPossibleListeners()
        {
            IDHCPv6InterfaceEngine engine = new DHCPv6InterfaceEngine(
                Mock.Of<IServiceBus>(MockBehavior.Strict),
                Mock.Of<IDHCPv6StorageEngine>(MockBehavior.Strict),
                Mock.Of<ILoggerFactory>()
                );

            var result = engine.GetPossibleListeners();
            Assert.NotNull(result);
            Assert.NotEmpty(result);

            Assert.Equal(0, result.Count(x => x == null));
        }

        [Fact]
        public async Task GetActiveListener()
        {
            List<DHCPv6Listener> listeners = new List<DHCPv6Listener>();

            var storageMock = new Mock<IDHCPv6StorageEngine>(MockBehavior.Strict);
            storageMock.Setup(x => x.GetDHCPv6Listener()).ReturnsAsync(listeners).Verifiable();

            IDHCPv6InterfaceEngine engine = new DHCPv6InterfaceEngine(
                Mock.Of<IServiceBus>(MockBehavior.Strict),
                storageMock.Object,
                Mock.Of<ILoggerFactory>()
                );

            var result = await engine.GetActiveListeners();
            Assert.NotNull(result);
            Assert.Equal(listeners, result);

            storageMock.Verify();
        }

        [Fact]
        public async Task OpenListener_SendPacket_CloseListener()
        {
            var comparer = new ByteArrayComparer();
            DHCPv6Packet dummyPacket = new DHCPv6Packet(null, 4, DHCPv6PacketTypes.Solicit, new List<DHCPv6PacketOption>
            {
                new DHCPv6PacketTrueOption(DHCPv6PacketOptionTypes.RapitCommit),
                new DHCPv6PacketByteOption(DHCPv6PacketOptionTypes.Preference,14),
            });

            Int32 packetsReceived = 0;
            Byte[] packetAsByte = dummyPacket.GetAsStream();

            var serviceBusMock = new Mock<IServiceBus>(MockBehavior.Strict);
            serviceBusMock.Setup(x => x.Publish(
                It.Is<DHCPv6PacketArrivedMessage>(y => comparer.Equals(y.Packet.GetAsStream(), packetAsByte) == true))).Callback(() => packetsReceived++).Returns(Task.CompletedTask).Verifiable();

            Mock<ILoggerFactory> factoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);
            factoryMock.Setup(x => x.CreateLogger(It.IsAny<String>())).Returns(Mock.Of<ILogger<DHCPv6Server>>());

            IDHCPv6InterfaceEngine engine = new DHCPv6InterfaceEngine(
                serviceBusMock.Object,
                Mock.Of<IDHCPv6StorageEngine>(MockBehavior.Strict),
                factoryMock.Object
                );

            var possibleListener = engine.GetPossibleListeners();
            var listener = possibleListener.First();
            engine.OpenListener(listener);


            IPAddress address = new IPAddress(listener.Address.GetBytes());
            IPEndPoint ownEndPoint = new IPEndPoint(address, 546);
            IPEndPoint serverEndPoint = new IPEndPoint(address, 547);

            UdpClient client = new UdpClient(ownEndPoint);
            client.Send(packetAsByte, packetAsByte.Length, serverEndPoint);

            Int32 trysLeft = 100;
            while (trysLeft-- > 0 && packetsReceived == 0)
            {
                await Task.Delay(1000);
            }

            engine.CloseListener(listener);
            engine.Dispose();

            client.Send(packetAsByte, packetAsByte.Length, serverEndPoint);
            await Task.Delay(3000);

            Assert.Equal(1, packetsReceived);
            client.Dispose();

            serviceBusMock.Verify(x => x.Publish(
      It.Is<DHCPv6PacketArrivedMessage>(y => comparer.Equals(y.Packet.GetAsStream(), packetAsByte) == true)), Times.Once);
        }

        [Fact]
        public async Task OpenListener_ReceiveWrongPacket()
        {
            Random random = new Random();
            var comparer = new ByteArrayComparer();
            Byte[] randomBytes = random.NextBytes(20);
            Int32 packetsReceived = 0;

            var serviceBusMock = new Mock<IServiceBus>(MockBehavior.Strict);
            serviceBusMock.Setup(x => x.Publish(
                It.Is<InvalidDHCPv6PacketArrivedMessage>(y => comparer.Equals(y.Packet.GetAsStream(), randomBytes) == true))).Callback(() => packetsReceived++).Returns(Task.CompletedTask).Verifiable();

            Mock<ILoggerFactory> factoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);
            factoryMock.Setup(x => x.CreateLogger(It.IsAny<String>())).Returns(Mock.Of<ILogger<DHCPv6Server>>());

            IDHCPv6InterfaceEngine engine = new DHCPv6InterfaceEngine(
                serviceBusMock.Object,
                Mock.Of<IDHCPv6StorageEngine>(MockBehavior.Strict),
                factoryMock.Object
                );

            var possibleListener = engine.GetPossibleListeners();
            var listener = possibleListener.First();
            engine.OpenListener(listener);

            IPAddress address = new IPAddress(listener.Address.GetBytes());
            IPEndPoint ownEndPoint = new IPEndPoint(address, 546);
            IPEndPoint serverEndPoint = new IPEndPoint(address, 547);

            UdpClient client = new UdpClient(ownEndPoint);

            client.Send(randomBytes, randomBytes.Length, serverEndPoint);

            Int32 trysLeft = 100;
            while (trysLeft-- > 0 && packetsReceived == 0)
            {
                await Task.Delay(1000);
            }

            engine.CloseListener(listener);
            client.Dispose();

            Assert.Equal(1, packetsReceived);

            serviceBusMock.Verify(x => x.Publish(
      It.Is<InvalidDHCPv6PacketArrivedMessage>(y => comparer.Equals(y.Packet.GetAsStream(), randomBytes) == true)), Times.Once);
        }

        [Fact]
        public async Task OpenListener_SendMultiplePackets()
        {
            var comparer = new ByteArrayComparer();

            DHCPv6Packet firstDummyPacket = new DHCPv6Packet(null, 4, DHCPv6PacketTypes.Solicit, new List<DHCPv6PacketOption>
            {
                new DHCPv6PacketTrueOption(DHCPv6PacketOptionTypes.RapitCommit),
                new DHCPv6PacketByteOption(DHCPv6PacketOptionTypes.Preference,14),
            });

            DHCPv6Packet secondDummyPacket = new DHCPv6Packet(null, 4, DHCPv6PacketTypes.Solicit, new List<DHCPv6PacketOption>
            {
                new DHCPv6PacketTrueOption(DHCPv6PacketOptionTypes.RapitCommit),
                new DHCPv6PacketByteOption(DHCPv6PacketOptionTypes.Preference,250),
            });

            Int32 packetsReceived = 0;
            Byte[] firstPacketAsByte = firstDummyPacket.GetAsStream();
            Byte[] secondPacketAsByte = secondDummyPacket.GetAsStream();

            var serviceBusMock = new Mock<IServiceBus>(MockBehavior.Strict);
            serviceBusMock.Setup(x => x.Publish(
                It.Is<DHCPv6PacketArrivedMessage>(y => packetsReceived % 2 == 0 ? comparer.Equals(y.Packet.GetAsStream(), firstPacketAsByte) : comparer.Equals(y.Packet.GetAsStream(), secondPacketAsByte)))).Callback(() => packetsReceived++).Returns(Task.CompletedTask).Verifiable();

            Mock<ILoggerFactory> factoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);
            factoryMock.Setup(x => x.CreateLogger(It.IsAny<String>())).Returns(Mock.Of<ILogger<DHCPv6Server>>());

            IDHCPv6InterfaceEngine engine = new DHCPv6InterfaceEngine(
                serviceBusMock.Object,
                Mock.Of<IDHCPv6StorageEngine>(MockBehavior.Strict),
                factoryMock.Object
                );

            var possibleListener = engine.GetPossibleListeners();
            var listener = possibleListener.First();
            engine.OpenListener(listener);

            IPAddress address = new IPAddress(listener.Address.GetBytes());
            IPEndPoint ownEndPoint = new IPEndPoint(address, 546);
            IPEndPoint serverEndPoint = new IPEndPoint(address, 547);

            UdpClient client = new UdpClient(ownEndPoint);

            Int32 packetAmount = 10;
            for (int i = 0; i < packetAmount; i++)
            {
                if (i % 2 == 0)
                {
                    client.Send(firstPacketAsByte, firstPacketAsByte.Length, serverEndPoint);
                }
                else
                {
                    client.Send(secondPacketAsByte, secondPacketAsByte.Length, serverEndPoint);
                }

                Int32 trysLeft = 1000;
                while (trysLeft-- > 0 && packetsReceived != i + 1)
                {
                    await Task.Delay(1);
                }
            }

            engine.CloseListener(listener);
            client.Dispose();

            Assert.Equal(packetAmount, packetsReceived);
        }


        [Fact]
        public async Task SendPacket()
        {
            Mock<ILoggerFactory> factoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);
            factoryMock.Setup(x => x.CreateLogger(It.IsAny<String>())).Returns(Mock.Of<ILogger<DHCPv6Server>>());

            DHCPv6InterfaceEngine engine = new DHCPv6InterfaceEngine(
                Mock.Of<IServiceBus>(MockBehavior.Strict),
                Mock.Of<IDHCPv6StorageEngine>(MockBehavior.Strict),
                factoryMock.Object
                );

            var possibleListener = engine.GetPossibleListeners();
            var listener = possibleListener.First();
            engine.OpenListener(listener);

            DHCPv6Packet responsePacket = DHCPv6Packet.AsOuter(
                new IPv6HeaderInformation(listener.Address, listener.Address), 4, DHCPv6PacketTypes.Solicit, new List<DHCPv6PacketOption>
            {
                new DHCPv6PacketTrueOption(DHCPv6PacketOptionTypes.RapitCommit),
                new DHCPv6PacketByteOption(DHCPv6PacketOptionTypes.Preference,14),
            });

            IPAddress address = new IPAddress(listener.Address.GetBytes());
            IPEndPoint localEndPoint = new IPEndPoint(address, 546);
            UdpClient client = new UdpClient(localEndPoint);
            client.Connect(new IPEndPoint(address, 547));
            try
            {
                Boolean sended = engine.SendPacket(responsePacket);
                Assert.True(sended);
                var result = await client.ReceiveAsync();

                Assert.True(result.Buffer.Length > 0);

                DHCPv6Packet receivedPacket = DHCPv6Packet.FromByteArray(result.Buffer,
                    new IPv6HeaderInformation(listener.Address, listener.Address));

                Assert.Equal(responsePacket, receivedPacket);
            }
            finally
            {
                client.Dispose();
            }
        }
    }
}
