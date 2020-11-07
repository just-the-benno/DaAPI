using DaAPI.Core.Common;
using DaAPI.Core.Packets.DHCPv6;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DaAPI.Infrastructure.FilterEngines.DHCPv6
{
    public class DHCPv6PacketServerIdentifierFilter : IDHCPv6PacketFilter
    {
        private readonly DUID _serverDuid;
        private readonly ILogger<DHCPv6PacketServerIdentifierFilter> _logger;

        public DHCPv6PacketServerIdentifierFilter(
            DUID serverDuid,
            ILogger<DHCPv6PacketServerIdentifierFilter> logger
            )
        {
            this._serverDuid = serverDuid ?? throw new ArgumentNullException(nameof(serverDuid));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task<Boolean> ShouldPacketBeFiltered(DHCPv6Packet packet)
        {
            _logger.LogDebug("ShouldPacketBeFiltered");

            DHCPv6Packet innerPacket = packet.GetInnerPacket();
            Boolean couldHaveDuid = innerPacket.CouldHaveDuid();

            if (couldHaveDuid == false && innerPacket.ShouldHaveDuid() == false)
            {
                return Task.FromResult(false);
            }

            DUID packetServerDuid = innerPacket.GetIdentifier(DHCPv6PacketOptionTypes.ServerIdentifer);
            if (couldHaveDuid == true && packetServerDuid == DUID.Empty)
            {
                return Task.FromResult(false);
            }

            Boolean result = packetServerDuid != _serverDuid;

            return Task.FromResult(result);
        }
    }
}
