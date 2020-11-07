using DaAPI.Core.Common;
using DaAPI.Core.Listeners;
using DaAPI.Core.Packets.DHCPv6;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DaAPI.Infrastructure.StorageEngine.DHCPv6
{
    public interface IDHCPv6EventStore
    {
        Task<Boolean> Save(AggregateRootWithEvents rootScope);
        Task<IEnumerable<DomainEvent>> GetEvents(String streamId);
        Task<Boolean> DeleteAggregateRoot<T>(Guid id) where T : AggregateRootWithEvents;

        Task<Boolean> SaveInitialServerConfiguration(DHCPv6ServerProperties config);
        Task<Boolean> CheckIfAggrerootExists<T>(Guid id) where T : AggregateRootWithEvents, new();
        Task<T> GetAggregateRoot<T>(Guid id) where T : AggregateRootWithEvents, new();
        Task<Boolean> Exists();
        Task<IDictionary<Guid, IEnumerable<DomainEvent>>> GetEventsStartWith(Type type);
        Task<Boolean> DeleteLeaseRelatedEventsOlderThan(DateTime leaseThreshold);
        Task<Boolean> DeletePacketHandledEventsOlderThan(DateTime handledEventThreshold);
        Task<Boolean> DeletePacketHandledEventMoreThan(UInt32 amount);
        Task<Boolean> LogInvaliidDHCPv6Packet(DHCPv6Packet packet);
        Task<Boolean> LogFilteredDHCPv6Packet(DHCPv6Packet packet, String filterName);
    }
}
