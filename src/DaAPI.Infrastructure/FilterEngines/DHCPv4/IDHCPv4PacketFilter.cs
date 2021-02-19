using DaAPI.Core.Common;
using DaAPI.Core.Packets.DHCPv4;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DaAPI.Infrastructure.FilterEngines.DHCPv4
{
    public interface IDHCPv4PacketFilter : IDHCPPacketFilter<DHCPv4Packet, IPv4Address>
    {
    }
}
