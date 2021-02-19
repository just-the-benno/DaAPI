using DaAPI.Core.Common;
using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Packets.DHCPv4;
using DaAPI.Core.Packets.DHCPv6;
using DaAPI.Infrastructure.FilterEngines.DHCPv4;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DaAPI.Infrastructure.FilterEngines.DHCPv4
{
    public class SimpleDHCPv4PacketFilterEngine : SimpleDHCPPacketFilterEngine<
        SimpleDHCPv4PacketFilterEngine, IDHCPv4PacketFilter,DHCPv4Packet,IPv4Address>, IDHCPv4PacketFilterEngine
    {
        #region Fields

        #endregion

        #region Properties

        #endregion

        #region Constructor

        public SimpleDHCPv4PacketFilterEngine(
            ILogger<SimpleDHCPv4PacketFilterEngine> logger) : this(Array.Empty<IDHCPv4PacketFilter>(), logger)
        {

        }

        public SimpleDHCPv4PacketFilterEngine(
            IEnumerable<IDHCPv4PacketFilter> filters,
            ILogger<SimpleDHCPv4PacketFilterEngine> logger
            ) : base(filters,logger)
        {
        }

        public void RemoveFilter<T>() where T : class, IDHCPv4PacketFilter => base.RemoveFilterBasedOnType<T>();

        #endregion

        #region Methods

        #endregion

    }
}
