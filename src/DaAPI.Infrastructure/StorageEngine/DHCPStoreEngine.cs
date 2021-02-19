using DaAPI.Core.Common;
using DaAPI.Core.Notifications;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace DaAPI.Infrastructure.StorageEngine
{
    public abstract class DHCPStoreEngine<TEventStore, TReadStore>
        where TEventStore : IEventStore
        where TReadStore : IReadStore
    {
        protected TEventStore EventStore { get; private init; }
        protected TReadStore ReadStore { get; private init; }
        protected IServiceProvider Provider { get; private init; }

        public DHCPStoreEngine(IServiceProvider provider, TEventStore writeStore, TReadStore readStore)
        {
            Provider = provider;
            EventStore = writeStore;
            ReadStore = readStore;
        }

        public async Task<Boolean> Save(AggregateRootWithEvents aggregateRoot)
        {
            var events = aggregateRoot.GetChanges();
            Boolean writeResult = await EventStore.Save(aggregateRoot);
            if (writeResult == false)
            {
                return false;
            }

            aggregateRoot.ClearChanges();

            Boolean projectResult = await ReadStore.Project(events);
            if (projectResult == false)
            {
                return false;
            }

            return true;
        }

        public Task<Boolean> CheckIfAggrerootExists<T>(Guid id) where T : AggregateRootWithEvents, new()
        {
            return EventStore.CheckIfAggrerootExists<T>(id);
        }

        public Task<T> GetAggregateRoot<T>(Guid id) where T : AggregateRootWithEvents, new()
        {
            return EventStore.GetAggregateRoot<T>(id);
        }

        public async Task<IEnumerable<NotificationPipeline>> GetAllNotificationPipeleines()
        {
            var notificationEvents = await
                 EventStore.GetEventsStartWith(typeof(NotificationPipeline));

            List<NotificationPipeline> pipelines = new List<NotificationPipeline>();
            foreach (var item in notificationEvents)
            {
                NotificationPipeline pipeline = new NotificationPipeline(
                    Provider.GetService<INotificationConditionFactory>(),
                    Provider.GetService<INotificationActorFactory>(),
                    Provider.GetService<ILogger<NotificationPipeline>>());

                pipeline.Load(item.Value);
                pipelines.Add(pipeline);
            }

            return pipelines;
        }

        public Task<Boolean> DeleteAggregateRoot<T>(Guid id) where T : AggregateRootWithEvents
        {
            return EventStore.DeleteAggregateRoot<T>(id);
        }

    }
}
