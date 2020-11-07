using DaAPI.Core.Common;
using DaAPI.Core.Packets;
using DaAPI.Core.Packets.DHCPv4;
using DaAPI.Infrastructure.FilterEngines.Helper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace DaAPI.Infrastructure.FilterEngines.DHCPv4
{
    public class DHCPv4RateLimiterBasedFilter : RateLimitBasedFilter<IPv4Address>, IDHCPv4RateLimitBasedFilter
    {
        #region Fields

        #endregion

        #region Properties

        public UInt16 PacketsPerSecons { get; set; } = 5;

        #endregion

        #region Constructor

        public DHCPv4RateLimiterBasedFilter(
            
            )
        {
        }

        #endregion

        #region Methods

        public bool FilterByRateLimit(DHCPv4Packet packet)
        {
            return base.FilterByRateLimit(packet.IPHeader.Source, PacketsPerSecons);
        }

        #endregion
    }
}
