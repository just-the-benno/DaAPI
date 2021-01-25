using DaAPI.Core.Packets.DHCPv4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaAPI.Infrastructure.StorageEngine.DHCPv4
{
    public interface IDHCPv4EventStore : IEventStore
    {
        Task<Boolean> LogInvalidDHCPv4Packet(DHCPv4Packet packet);
        Task<Boolean> LogFilteredDHCPv4Packet(DHCPv4Packet packet, string filterName);
    }
}
