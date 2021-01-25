using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Packets.DHCPv6;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DaAPI.Infrastructure.FilterEngines.DHCPv6
{
    public interface IDHCPv6PacketFilter : IDHCPPacketFilter<DHCPv6Packet,IPv6Address>
    {
    }
}
