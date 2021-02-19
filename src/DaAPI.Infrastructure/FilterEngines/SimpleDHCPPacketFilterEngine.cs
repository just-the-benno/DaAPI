using DaAPI.Core.Common;
using DaAPI.Core.Packets;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaAPI.Infrastructure.FilterEngines
{
    public abstract class SimpleDHCPPacketFilterEngine<TFilterEngine, TPacketFilter, TPacket, TAddress>
        where TFilterEngine : SimpleDHCPPacketFilterEngine<TFilterEngine, TPacketFilter, TPacket, TAddress>
        where TPacketFilter : IDHCPPacketFilter<TPacket, TAddress>
        where TPacket : DHCPPacket<TPacket, TAddress>
        where TAddress : IPAddress<TAddress>

    {
        #region Fields

        private readonly List<TPacketFilter> _filters = new();
        private readonly ILogger<TFilterEngine> _logger;

        #endregion

        #region Properties

        public IEnumerable<TPacketFilter> Filters => _filters.AsEnumerable();

        #endregion

        #region Constructor

        public SimpleDHCPPacketFilterEngine(
            ILogger<TFilterEngine> logger) : this(Array.Empty<TPacketFilter>(), logger)
        {

        }

        public SimpleDHCPPacketFilterEngine(
            IEnumerable<TPacketFilter> filters,
            ILogger<TFilterEngine> logger
            )
        {
            if (filters is null || filters.Count(x => x == null) > 0)
            {
                throw new ArgumentNullException(nameof(filters));
            }

            _filters = new List<TPacketFilter>(filters);
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Methods

        public void AddFilter(TPacketFilter filter)
        {
            if (filter is null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            _filters.Add(filter);
        }

        public void RemoveFilterBasedOnType<T>() where T : TPacketFilter
        {
            var elements = _filters.OfType<T>().Cast<IDHCPPacketFilter<TPacket, TAddress>>().ToList();
            _filters.RemoveAll(x => elements.Contains(x) == true);
        }

        public async Task<(Boolean, String)> ShouldPacketBeFilterd(TPacket packet)
        {
            foreach (var item in _filters)
            {
                _logger.LogDebug("applting {name} filter", item.ToString());
                Boolean shouldBeFiltered = await item.ShouldPacketBeFiltered(packet);
                if (shouldBeFiltered == true)
                {
                    return (true, item.GetType().Name);
                }
            }

            return (false, String.Empty);
        }

        #endregion

    }
}
