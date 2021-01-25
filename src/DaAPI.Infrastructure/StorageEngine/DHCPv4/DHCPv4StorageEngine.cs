using DaAPI.Core.Common;
using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Listeners;
using DaAPI.Core.Notifications;
using DaAPI.Core.Packets.DHCPv4;
using DaAPI.Core.Packets.DHCPv6;
using DaAPI.Core.Scopes;
using DaAPI.Core.Scopes.DHCPv4;
using DaAPI.Core.Scopes.DHCPv6;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DaAPI.Infrastructure.StorageEngine.DHCPv4
{
    public class DHCPv4StorageEngine : DHCPStoreEngine<IDHCPv4EventStore, IDHCPv4ReadStore>, IDHCPv4StorageEngine
    {
        public DHCPv4StorageEngine(IServiceProvider provider) : base(
            provider,
            provider.GetRequiredService<IDHCPv4EventStore>(),
            provider.GetRequiredService<IDHCPv4ReadStore>()
            )
        {
        }

        public Task<IEnumerable<DHCPv4Listener>> GetDHCPv4Listener() => ReadStore.GetDHCPv4Listener();

        public Task<Boolean> LogInvalidDHCPv4Packet(DHCPv4Packet packet) => EventStore.LogInvalidDHCPv4Packet(packet);
        public Task<Boolean> LogFilteredDHCPv4Packet(DHCPv4Packet packet, String filterName) => EventStore.LogFilteredDHCPv4Packet(packet, filterName);


        public async Task<DHCPv4RootScope> GetRootScope(IScopeResolverManager<DHCPv4Packet, IPv4Address> scopeResolverManager)
        {
            DHCPv4RootScope rootScope = new DHCPv4RootScope(Guid.NewGuid(), scopeResolverManager, Provider.GetRequiredService<ILoggerFactory>());
            if (await EventStore.Exists() == true)
            {
                IEnumerable<DomainEvent> events = await EventStore.GetEvents(nameof(DHCPv4RootScope));
                rootScope.Load(events);
            }

            return rootScope;
        }
    }
}
