using DaAPI.Core.Packets.DHCPv6;
using DaAPI.Infrastructure.FilterEngines.DHCPv6;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DaAPI.Infrastructure.FilterEngines.DHCPv6
{
    public interface IDHCPv6PacketFilterEngine
    {
        public IEnumerable<IDHCPv6PacketFilter> Filters { get; }

        void AddFilter(IDHCPv6PacketFilter filter);
        void RemoveFilter<T>() where T : class, IDHCPv6PacketFilter;

        Task<(Boolean,String)> ShouldPacketBeFilterd(DHCPv6Packet packet);
    }
}
