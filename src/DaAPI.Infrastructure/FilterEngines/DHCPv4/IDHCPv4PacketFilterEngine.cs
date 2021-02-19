using DaAPI.Core.Packets.DHCPv4;
using DaAPI.Core.Packets.DHCPv6;
using DaAPI.Infrastructure.FilterEngines.DHCPv6;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DaAPI.Infrastructure.FilterEngines.DHCPv4
{
    public interface IDHCPv4PacketFilterEngine
    {
        public IEnumerable<IDHCPv4PacketFilter> Filters { get; }

        void AddFilter(IDHCPv4PacketFilter filter);
        void RemoveFilter<T>() where T : class, IDHCPv4PacketFilter;

        Task<(Boolean,String)> ShouldPacketBeFilterd(DHCPv4Packet packet);
    }
}
