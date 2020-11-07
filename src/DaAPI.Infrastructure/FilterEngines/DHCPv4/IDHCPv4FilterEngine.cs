using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DaAPI.Core.Packets.DHCPv4;

namespace DaAPI.Infrastructure.FilterEngines.DHCPv4
{
    public interface IDHCPv4FilterEngine
    {
        Task<Boolean> ShouldPacketBeFilterd(DHCPv4Packet packet);
    }
}
