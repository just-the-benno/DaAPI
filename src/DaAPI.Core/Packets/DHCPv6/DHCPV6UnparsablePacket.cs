using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Packets.DHCPv6
{
    class DHCPV6UnparsablePacket : DHCPv6Packet
    {
        public Byte[] Content { get; private set; }

        public DHCPV6UnparsablePacket(IPv6HeaderInformation ipv6Header, Byte[] data) : base(DHCPv6PacketTypes.Invalid, Array.Empty<DHCPv6PacketOption>())
        {
            Content = data;
            Header = ipv6Header;
        }

        public override int GetAsStream(byte[] packetAsByteStream, int offset)
        {
            Content.CopyTo(packetAsByteStream, offset);
            return Content.Length;
        }
    }
}
