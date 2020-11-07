using DaAPI.Core.Common;
using DaAPI.Core.Packets.DHCPv6;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DaAPI.Infrastructure.FilterEngines.DHCPv6
{
    public class DHCPv6PacketConsistencyFilter : IDHCPv6PacketFilter
    {
        private readonly ILogger<DHCPv6PacketConsistencyFilter> _logger;

        public DHCPv6PacketConsistencyFilter(
            ILogger<DHCPv6PacketConsistencyFilter> logger
            )
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task<Boolean> ShouldPacketBeFiltered(DHCPv6Packet packet)
        {
            _logger.LogDebug("ShouldPacketBeFilterd");
            if (packet.GetInnerPacket().IsClientRequest() == false)
            {
                return Task.FromResult(true);
            }

            Boolean result = packet.IsConsistent();
            return Task.FromResult(!result);
        }
    }
}
