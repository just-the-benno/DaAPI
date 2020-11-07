using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Packets;
using DaAPI.Core.Packets.DHCPv6;
using DaAPI.Infrastructure.FilterEngines.DHCPv6;
using DaAPI.Infrastructure.FilterEngines.Helper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DaAPI.Core.FilterEngines.DHCPv6
{
    public class DHCPv6RateLimitBasedFilter : RateLimitBasedFilter<IPv6Address>, IDHCPv6PacketFilter
    {
        #region Fields

        private readonly ILogger<DHCPv6RateLimitBasedFilter> _logger;

        #endregion

        #region Properties

        public UInt16 PacketsPerSecons { get; set; } = 5;

        #endregion

        #region Constructor

        public DHCPv6RateLimitBasedFilter(
            ILogger<DHCPv6RateLimitBasedFilter> logger
            )
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Methods

        public Task<Boolean> ShouldPacketBeFiltered(DHCPv6Packet packet)
        {
            _logger.LogDebug("ShouldPacketBeFiltered");

            Boolean result = base.FilterByRateLimit(packet.Header.Source, PacketsPerSecons);
            return Task.FromResult(result);
        }

        #endregion
    }
}
