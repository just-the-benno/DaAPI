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
    public class SimpleDHCPv6PacketFilterEngine : IDHCPv6PacketFilterEngine
    {
        #region Fields

        private readonly List<IDHCPv6PacketFilter> _filters = new List<IDHCPv6PacketFilter>();
        private readonly ILogger<SimpleDHCPv6PacketFilterEngine> _logger;

        #endregion

        #region Properties

        public IEnumerable<IDHCPv6PacketFilter> Filters => _filters.AsEnumerable();

        #endregion

        #region Constructor

        public SimpleDHCPv6PacketFilterEngine(
            ILogger<SimpleDHCPv6PacketFilterEngine> logger) : this(Array.Empty<IDHCPv6PacketFilter>(), logger)
        {

        }

        public SimpleDHCPv6PacketFilterEngine(
            IEnumerable<IDHCPv6PacketFilter> filters,
            ILogger<SimpleDHCPv6PacketFilterEngine> logger
            )
        {
            if (filters is null || filters.Count(x => x == null) > 0)
            {
                throw new ArgumentNullException(nameof(filters));
            }

            _filters = new List<IDHCPv6PacketFilter>(filters);
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Methods

        public void AddFilter(IDHCPv6PacketFilter filter)
        {
            if (filter is null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            _filters.Add(filter);
        }

        public void RemoveFilter<T>() where T : class, IDHCPv6PacketFilter
        {
            var elements = _filters.OfType<T>().ToList();
            _filters.RemoveAll(x => elements.Contains(x) == true);
        }

        public async Task<(Boolean, String)> ShouldPacketBeFilterd(DHCPv6Packet packet)
        {
            foreach (var item in _filters)
            {
                _logger.LogDebug("applting {name} filter", item.ToString());
                Boolean shouldBeFiltered = await item.ShouldPacketBeFiltered(packet);
                if (shouldBeFiltered == true)
                {
                    _logger.LogInformation("packet {type} didn'nt pass filter {name} and should be dropped", packet.PacketType, item.ToString());
                    return (true, item.GetType().Name);
                }
                else
                {
                    _logger.LogDebug("packet {type} pass filter {name}", packet.PacketType, item.ToString());
                }

                _logger.LogDebug("packet {type}  passed all filters", packet.PacketType);
            }

            return (false,String.Empty);
        }

        #endregion

    }
}
