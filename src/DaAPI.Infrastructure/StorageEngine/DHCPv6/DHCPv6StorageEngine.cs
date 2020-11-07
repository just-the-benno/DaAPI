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
    public class DHCPv6StorageEngine : IDHCPv6StorageEngine
    {
        private readonly IDHCPv6ReadStore _readStore;
        private readonly IDHCPv6EventStore _writeStore;
        private readonly IServiceProvider _provider;

        public DHCPv6StorageEngine(IServiceProvider provider)
        {
            this._readStore = provider.GetRequiredService<IDHCPv6ReadStore>();
            this._writeStore = provider.GetRequiredService<IDHCPv6EventStore>();
            this._provider = provider;
        }

        public Task<IEnumerable<DHCPv6Listener>> GetDHCPv6Listener() => _readStore.GetDHCPv6Listener();

        public async Task<Boolean> Save(AggregateRootWithEvents aggregateRoot)
        {
            var events = aggregateRoot.GetChanges();
            Boolean writeResult = await _writeStore.Save(aggregateRoot);
            if (writeResult == false)
            {
                return false;
            }

            aggregateRoot.ClearChanges();

            Boolean projectResult = await _readStore.Project(events);
            if (projectResult == false)
            {
                return false;
            }

            return true;
        }

        public async Task<DHCPv6RootScope> GetRootScope(IScopeResolverManager<DHCPv6Packet, IPv6Address> scopeResolverManager)
        {
            DHCPv6RootScope rootScope = new DHCPv6RootScope(Guid.NewGuid(), scopeResolverManager, _provider.GetRequiredService<ILoggerFactory>());
            if (await _writeStore.Exists() == true)
            {
                IEnumerable<DomainEvent> events = await _writeStore.GetEvents(nameof(DHCPv6RootScope));
                rootScope.Load(events);
            }

            return rootScope;
        }

        public Task<Boolean> CheckIfAggrerootExists<T>(Guid id) where T : AggregateRootWithEvents, new()
        {
            return _writeStore.CheckIfAggrerootExists<T>(id);
        }

        public Task<T> GetAggregateRoot<T>(Guid id) where T : AggregateRootWithEvents, new()
        {
            return _writeStore.GetAggregateRoot<T>(id);
        }

        public async Task<IEnumerable<NotificationPipeline>> GetAllNotificationPipeleines()
        {
            var notificationEvents = await
                 _writeStore.GetEventsStartWith(typeof(NotificationPipeline));

            List<NotificationPipeline> pipelines = new List<NotificationPipeline>();
            foreach (var item in notificationEvents)
            {
                NotificationPipeline pipeline = new NotificationPipeline(
                    _provider.GetService<INotificationConditionFactory>(),
                    _provider.GetService<INotificationActorFactory>(),
                    _provider.GetService<ILogger<NotificationPipeline>>());

                pipeline.Load(item.Value);
                pipelines.Add(pipeline);
            }


            return pipelines;
        }

        public Task<Boolean> DeleteAggregateRoot<T>(Guid id) where T : AggregateRootWithEvents
        {
            return _writeStore.DeleteAggregateRoot<T>(id);
        }

        public Task<Boolean> LogInvalidDHCPv6Packet(DHCPv6Packet packet) => _writeStore.LogInvaliidDHCPv6Packet(packet);

        public Task<Boolean> LogFilteredDHCPv6Packet(DHCPv6Packet packet, String filterName) => _writeStore.LogFilteredDHCPv6Packet(packet, filterName);
    }
}
