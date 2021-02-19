using DaAPI.Core.Packets.DHCPv4;
using DaAPI.Core.Packets.DHCPv6;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DaAPI.Infrastructure.LeaseEngines.DHCPv4
{
    public interface IDHCPv4LeaseEngine
    {
        Task<DHCPv4Packet> HandlePacket(DHCPv4Packet packet);
    }
}
