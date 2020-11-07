using DaAPI.Core.Clients;
using DaAPI.Core.Common;
using DaAPI.Infrastructure.AggregateStore.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaAPI.Infrastructure.AggregateStore
{
    public class SimpleEFAggregateStore : DbContext, IDHCPv4AggregateStore
    {
        #region Fields

        private readonly ITypeProvider _typeProvider;

        #endregion

        #region DbSet
        public DbSet<EventDataModel> Events { get; set; }
        public DbSet<ActiveDHCPv4TransactionDataModel> ActiveTransactions { get; set; }
        public DbSet<DHCPv4BlockedClientDataModel> BlockedDHCPv4Clients { get; set; }

        #endregion

        public SimpleEFAggregateStore(
            ITypeProvider typeProvider,
            DbContextOptions<SimpleEFAggregateStore> options
            ) : base(options)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            this._typeProvider = typeProvider ?? throw new ArgumentNullException(nameof(typeProvider));
        }

        public async Task<Boolean> CheckIfActiveTransactionExists(uint transactionId)
        {
            Guid id = await ActiveTransactions
                .Where(x => x.TransactionIdentifier == transactionId)
                .Select(x => x.Id)
                .FirstOrDefaultAsync();

            return id != default;
        }
       

        public async Task<Boolean> CheckIfBlockedClientExists(DHCPv4ClientIdentifier identifier)
        {
            String stringRepresentations = identifier.AsUniqueString();

            Guid id = await BlockedDHCPv4Clients
                .Where(x => x.Identifier == stringRepresentations)
                .Select(x => x.Id)
                .FirstOrDefaultAsync();

            return id != default;
        }

        public async Task<DHCPv4Client> GetClientByClientIdentifier(DHCPv4ClientIdentifier clientIdentifier)
        {
            String streamId = clientIdentifier.AsUniqueString();

            var events = await Events.Where(x => x.StreamId == streamId)
                .OrderBy(x => x.Version).ToListAsync();

            if(events.Count == 0)
            {
                return DHCPv4Client.Unknow;
            }

            List<DomainEvent> deserialziedEvents = new List<DomainEvent>();

            foreach (var item in events)
            {
                Type type = _typeProvider.GetTypeForIdentifier(item.EventType);

                DomainEvent @event = (DomainEvent)
                JsonConvert.DeserializeObject(item.Content, type);

                deserialziedEvents.Add(@event);
            }

            DHCPv4Client client = new DHCPv4Client();
            client.Load(deserialziedEvents);

            return client;
        }

        public async Task Save(AggregateRootWithEvents root)
        {
            IEnumerable<DomainEvent> changes = root.GetChanges();
            String streamId = ""; // root.GetUniqueIdentifier();

            Int32 version = root.Version;
            foreach (var @event in changes)
            {
                Type eventType = @event.GetType();

                EventDataModel dataModel = new EventDataModel
                {
                    Id = Guid.NewGuid(),
                    Content = JsonConvert.SerializeObject(@event),
                    EventType = _typeProvider.GetIdentifierForType(eventType),
                    Version = version++,
                    UTCTimestamp = DateTime.UtcNow,
                    StreamId = streamId,
                };

                Events.Add(dataModel);
            }

            await SaveChangesAsync();
            
            root.ClearChanges();
        }
    }
}
