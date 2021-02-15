using DaAPI.Core.Common;
using DaAPI.Core.Packets.DHCPv4;
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
    public class DHCPv4Server : DHCPServer<
        DHCPv4Server, IPv4Address,DHCPv4Packet,
        DHCPv4PacketArrivedMessage,InvalidDHCPv4PacketArrivedMessage>
    {
        private const UInt16 _dhcpServerPort = 67;
        private const UInt16 _dhcpRelayPort = 67;
        private const UInt16 _dhcpClientPort = 68;

        public DHCPv4Server(
            IPv4Address address,
            IServiceBus serviceBus,
            ILogger<DHCPv4Server> logger) : base(address, _dhcpServerPort, 
                (packet) => new DHCPv4PacketArrivedMessage(packet),
                (packet) => new InvalidDHCPv4PacketArrivedMessage(packet),
                serviceBus,logger)
        {
        }

        protected override DHCPv4Packet ParsePacket(Byte[] sourceAddress, byte[] buffer, int offset, int size)
        {
            DHCPv4Packet packet = DHCPv4Packet.FromByteArray(ByteHelper.CopyData(buffer, offset, size),
               IPv4Address.FromByteArray(sourceAddress),
               IPv4Address.FromByteArray(base.Endpoint.Address.GetAddressBytes()));

            return packet;
        }

        protected override UInt16 GetResponsePort(DHCPv4Packet response) => response.GatewayIPAdress == IPv4Address.Empty ? _dhcpClientPort : _dhcpRelayPort;
    }
}
