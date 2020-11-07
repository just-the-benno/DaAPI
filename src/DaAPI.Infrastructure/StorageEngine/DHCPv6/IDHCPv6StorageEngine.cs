using DaAPI.Core.Common;
using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Listeners;
using DaAPI.Core.Notifications;
using DaAPI.Core.Packets.DHCPv6;
using DaAPI.Core.Scopes;
using DaAPI.Core.Scopes.DHCPv6;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DaAPI.Infrastructure.StorageEngine.DHCPv6
{
    public interface IDHCPv6StorageEngine 
    {
        Task<IEnumerable<DHCPv6Listener>> GetDHCPv6Listener();
        Task<Boolean> Save(AggregateRootWithEvents root);
        Task<DHCPv6RootScope> GetRootScope(IScopeResolverManager<DHCPv6Packet, IPv6Address> scopeResolverManager);
        Task<Boolean> CheckIfAggrerootExists<T>(Guid id) where T :  AggregateRootWithEvents, new ();
        Task<IEnumerable<NotificationPipeline>> GetAllNotificationPipeleines();
        Task<T> GetAggregateRoot<T>(Guid id) where T : AggregateRootWithEvents, new();
        Task<Boolean> DeleteAggregateRoot<T>(Guid id) where T : AggregateRootWithEvents;
        Task<Boolean> LogInvalidDHCPv6Packet(DHCPv6Packet packet);
        Task<Boolean> LogFilteredDHCPv6Packet(DHCPv6Packet packet, String filterName);
    }
}
