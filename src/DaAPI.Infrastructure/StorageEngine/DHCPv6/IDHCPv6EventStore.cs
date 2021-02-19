using DaAPI.Core.Common;
using DaAPI.Core.Listeners;
using DaAPI.Core.Packets.DHCPv6;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DaAPI.Infrastructure.StorageEngine.DHCPv6
{
    public interface IDHCPv6EventStore : IEventStore
    {
        Task<Boolean> SaveInitialServerConfiguration(DHCPv6ServerProperties config);
        Task<Boolean> DeleteLeaseRelatedEventsOlderThan(DateTime leaseThreshold);
        Task<Boolean> DeletePacketHandledEventsOlderThan(DateTime handledEventThreshold);
        Task<Boolean> DeletePacketHandledEventMoreThan(UInt32 amount);
        Task<Boolean> LogInvalidDHCPv6Packet(DHCPv6Packet packet);
        Task<Boolean> LogFilteredDHCPv6Packet(DHCPv6Packet packet, String filterName);
    }
}
