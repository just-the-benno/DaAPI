using DaAPI.Core.Common;
using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Listeners;
using DaAPI.Core.Notifications;
using DaAPI.Core.Packets.DHCPv6;
using DaAPI.Core.Scopes;
using DaAPI.Core.Scopes.DHCPv6;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DaAPI.Infrastructure.StorageEngine.DHCPv6
{
    public class DHCPv6StorageEngine : DHCPStoreEngine<IDHCPv6EventStore, IDHCPv6ReadStore>, IDHCPv6StorageEngine
    {
        public DHCPv6StorageEngine(IServiceProvider provider) : base(
            provider,
            provider.GetRequiredService<IDHCPv6EventStore>(),
            provider.GetRequiredService<IDHCPv6ReadStore>()
            )
        {
        }

        public Task<IEnumerable<DHCPv6Listener>> GetDHCPv6Listener() => ReadStore.GetDHCPv6Listener();

        public async Task<DHCPv6RootScope> GetRootScope(IScopeResolverManager<DHCPv6Packet, IPv6Address> scopeResolverManager)
        {
            DHCPv6RootScope rootScope = new DHCPv6RootScope(Guid.NewGuid(), scopeResolverManager, Provider.GetRequiredService<ILoggerFactory>());
            if (await EventStore.Exists() == true)
            {
                IEnumerable<DomainEvent> events = await EventStore.GetEvents(nameof(DHCPv6RootScope));
                rootScope.Load(events);
            }

            return rootScope;
        }

        public Task<Boolean> LogInvalidDHCPv6Packet(DHCPv6Packet packet) => EventStore.LogInvalidDHCPv6Packet(packet);
        public Task<Boolean> LogFilteredDHCPv6Packet(DHCPv6Packet packet, String filterName) => EventStore.LogFilteredDHCPv6Packet(packet, filterName);
    }
}
