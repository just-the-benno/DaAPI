using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Packets.DHCPv6;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DaAPI.Infrastructure.FilterEngines.DHCPv6
{
    public class SimpleDHCPv6PacketFilterEngine : SimpleDHCPPacketFilterEngine<
        SimpleDHCPv6PacketFilterEngine, IDHCPv6PacketFilter, DHCPv6Packet, IPv6Address>, IDHCPv6PacketFilterEngine
    {
        #region Fields

        #endregion

        #region Properties

        #endregion

        #region Constructor

        public SimpleDHCPv6PacketFilterEngine(
            ILogger<SimpleDHCPv6PacketFilterEngine> logger) : this(Array.Empty<IDHCPv6PacketFilter>(), logger)
        {

        }

        public SimpleDHCPv6PacketFilterEngine(
            IEnumerable<IDHCPv6PacketFilter> filters,
            ILogger<SimpleDHCPv6PacketFilterEngine> logger
            ) : base(filters, logger)
        {
        }

        public void RemoveFilter<T>() where T : class, IDHCPv6PacketFilter => base.RemoveFilterBasedOnType<T>();

        #endregion

        #region Methods

        #endregion

    }
}
