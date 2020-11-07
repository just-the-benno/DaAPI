using DaAPI.Core.Packets.DHCPv6;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DaAPI.Infrastructure.LeaseEngines.DHCPv6
{
    public interface IDHCPv6LeaseEngine
    {
        Task<DHCPv6Packet> HandlePacket(DHCPv6Packet packet);
    }
}
