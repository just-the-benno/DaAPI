using DaAPI.Core.Common;
using DaAPI.Core.Notifications;
using DaAPI.Infrastructure.ServiceBus;
using DaAPI.Infrastructure.ServiceBus.Messages;
using DaAPI.Infrastructure.StorageEngine;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaAPI.Infrastructure.LeaseEngines
{
    public abstract class DHCPLeaseEngine<TEngine, TPacket, TScope>
        where TEngine : DHCPLeaseEngine<TEngine, TPacket, TScope>
        where TScope : AggregateRootWithEvents, INotificationTriggerSource
        where TPacket : class
    {
        protected ILogger<TEngine> Logger { get; init; }
        protected TScope RootScope { get; init; }
        private readonly IServiceBus _serviceBus;
        private readonly IEventStore _store;

        public DHCPLeaseEngine(TScope rootScope, IServiceBus serviceBus, IEventStore store, ILogger<TEngine> logger)
        {
            Logger = logger;
            RootScope = rootScope;
            _serviceBus = serviceBus;
            _store = store;
        }

        protected abstract TPacket GetResponse(TPacket input);

        public async Task<TPacket> HandlePacket(TPacket input)
        {
            TPacket response = null;

            try
            {
                response = GetResponse(input);

                var changes = RootScope.GetChanges();
                Logger.LogDebug("rootscope has {changeAmount} changes", changes.Count());
                if (changes.Any() == true)
                {
                    await _store.Save(RootScope);
                    Logger.LogDebug("changes saved");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("unable to handle packet", ex);
            }
            finally
            {
                // _logger.LogDebug("releasing semaphore. Ready for next process");
                // _semaphoreSlim.Release();
            }

            if (response == null)
            {
                Logger.LogDebug("unable to get a response for {packet}", input);
            }

            var triggers = RootScope.GetTriggers();
            if (triggers.Any() == true)
            {
                RootScope.ClearTriggers();
                await _serviceBus.Publish(new NewTriggerHappendMessage(triggers));
            }

            return response;
        }
    }
}
