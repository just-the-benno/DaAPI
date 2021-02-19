using DaAPI.Core.Common;
using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Packets.DHCPv6;
using DaAPI.Infrastructure.ServiceBus;
using DaAPI.Infrastructure.ServiceBus.Messages;
using Microsoft.Extensions.Logging;
using NetCoreServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DaAPI.Infrastructure.InterfaceEngines
{
    public class DHCPv6Server : DHCPServer<
        DHCPv6Server,IPv6Address,DHCPv6Packet,
        DHCPv6PacketArrivedMessage,InvalidDHCPv6PacketArrivedMessage>
    {
        private const UInt16 _dhcpServerPort = 547;
        private const UInt16 _dhcpRelayPort = 547;
        private const UInt16 _dhcpClientPort = 546;

        public DHCPv6Server(
            IPv6Address address,
            IServiceBus serviceBus,
            ILogger<DHCPv6Server> logger) : base(address, _dhcpServerPort, 
                (packet) => new DHCPv6PacketArrivedMessage(packet),
                (packet) => new InvalidDHCPv6PacketArrivedMessage(packet),
                serviceBus,logger)
        {
        }

        protected override DHCPv6Packet ParsePacket(Byte[] sourceAddress, byte[] buffer, int offset, int size)
        {
            DHCPv6Packet packet = DHCPv6Packet.FromByteArray(ByteHelper.CopyData(buffer, offset, size),
           new IPv6HeaderInformation(
               IPv6Address.FromByteArray(sourceAddress),
               IPv6Address.FromByteArray(base.Endpoint.Address.GetAddressBytes()))
           );

            return packet;
        }

        protected override UInt16 GetResponsePort(DHCPv6Packet response) => response is DHCPv6RelayPacket ? _dhcpRelayPort : _dhcpClientPort;
    }
}
