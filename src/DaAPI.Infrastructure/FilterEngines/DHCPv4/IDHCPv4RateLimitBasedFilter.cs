using DaAPI.Core.Packets;
using DaAPI.Core.Packets.DHCPv4;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Infrastructure.FilterEngines.DHCPv4
{
    public interface IDHCPv4RateLimitBasedFilter
    {
        UInt16 PacketsPerSecons { get; set; }
        Boolean FilterByRateLimit(DHCPv4Packet packet);
    }
}
