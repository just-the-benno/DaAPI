using DaAPI.Core.Common;
using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Listeners;
using DaAPI.Core.Packets.DHCPv4;
using DaAPI.Core.Packets.DHCPv6;
using DaAPI.Core.Scopes.DHCPv6;
using DaAPI.Infrastructure.Helper;
using DaAPI.Infrastructure.StorageEngine.Converters;
using DaAPI.Infrastructure.StorageEngine.DHCPv4;
using DaAPI.Infrastructure.StorageEngine.DHCPv6;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static DaAPI.Core.Listeners.DHCPListenerEvents;
using static DaAPI.Core.Scopes.DHCPv6.DHCPv6LeaseEvents;
using static DaAPI.Core.Scopes.DHCPv6.DHCPv6PacketHandledEvents;
using static DaAPI.Shared.Requests.StatisticsControllerRequests.V1;
using static DaAPI.Shared.Responses.StatisticsControllerResponses.V1;

namespace DaAPI.Infrastructure.StorageEngine
{
    public class StorageContext : DbContext, IDHCPv6EventStore, IDHCPv6ReadStore, IDHCPv4ReadStore, IDHCPv4EventStore 
    {
        #region Fields

        private readonly JsonSerializerSettings _jsonSerializerSettings;

        #endregion

        #region Sets

        public DbSet<EventEntry> Entries { get; set; }
        public DbSet<HelperEntry> Helpers { get; set; }
        public DbSet<DHCPv6InterfaceDataModel> DHCPv6Interfaces { get; set; }
        public DbSet<DHCPv6PacketHandledEntryDataModel> DHCPv6PacketEntries { get; set; }
        public DbSet<DHCPv6LeaseEntryDataModel> DHCPv6LeaseEntries { get; set; }

        public DbSet<DHCPv4InterfaceDataModel> DHCPv4Interfaces { get; set; }
        public DbSet<DHCPv4PacketHandledEntryDataModel> DHCPv4PacketEntries { get; set; }

        #endregion

        private static readonly List<String> _leasseCorrelatedEventsNames;
        private static readonly List<String> _handledCorrelatedEventsNames;

        private static readonly SemaphoreSlim _writeSync = new SemaphoreSlim(1, 1);

        static StorageContext()
        {
            var possibleLeaseEventTypes = new[]
           {
                typeof(DHCPv6LeaseEvents.DHCPv6AddressSuspendedEvent),
                typeof(DHCPv6LeaseEvents.DHCPv6LeaseActivatedEvent),
                typeof(DHCPv6LeaseEvents.DHCPv6LeaseCanceledEvent),
                typeof(DHCPv6LeaseEvents.DHCPv6LeaseCreatedEvent),
                typeof(DHCPv6LeaseEvents.DHCPv6LeaseExpiredEvent),
                typeof(DHCPv6LeaseEvents.DHCPv6LeasePrefixActvatedEvent),
                typeof(DHCPv6LeaseEvents.DHCPv6LeasePrefixAddedEvent),
                typeof(DHCPv6LeaseEvents.DHCPv6LeaseReleasedEvent),
                typeof(DHCPv6LeaseEvents.DHCPv6LeaseReleasedEvent),
                typeof(DHCPv6LeaseEvents.DHCPv6LeaseRenewedEvent),
                typeof(DHCPv6LeaseEvents.DHCPv6LeaseRevokedEvent),
            };

            _leasseCorrelatedEventsNames = possibleLeaseEventTypes.Select(x => GetBodyType(x)).ToList();

            var possibleLeaseHandledEventTypes = new[]
          {
                typeof(DHCPv6PacketHandledEvents.DHCPv6ConfirmHandledEvent),
                typeof(DHCPv6PacketHandledEvents.DHCPv6DeclineHandledEvent),
                typeof(DHCPv6PacketHandledEvents.DHCPv6InformRequestHandledEvent),
                typeof(DHCPv6PacketHandledEvents.DHCPv6PacketHandledEvent),
                typeof(DHCPv6PacketHandledEvents.DHCPv6RebindHandledEvent),
                typeof(DHCPv6PacketHandledEvents.DHCPv6ReleaseHandledEvent),
                typeof(DHCPv6PacketHandledEvents.DHCPv6RenewHandledEvent),
                typeof(DHCPv6PacketHandledEvents.DHCPv6RequestHandledEvent),
                typeof(DHCPv6PacketHandledEvents.DHCPv6SolicitHandledEvent),
            };

            _handledCorrelatedEventsNames = possibleLeaseHandledEventTypes.Select(x => GetBodyType(x)).ToList();


        }

        public StorageContext(DbContextOptions<StorageContext> options) : base(options)
        {
            _jsonSerializerSettings = new JsonSerializerSettings();
            _jsonSerializerSettings.Converters.Add(new DUIDJsonConverter());

            _jsonSerializerSettings.Converters.Add(new IPv6AddressJsonConverter());
            _jsonSerializerSettings.Converters.Add(new DHCPv6PacketJsonConverter());
            _jsonSerializerSettings.Converters.Add(new DHCPv6ScopeAddressPropertiesConverter());
            _jsonSerializerSettings.Converters.Add(new DHCPv6PrefixDelgationInfoJsonConverter());
            _jsonSerializerSettings.Converters.Add(new DHCPv6ScopePropertyJsonConverter());
            _jsonSerializerSettings.Converters.Add(new DHCPv6ScopePropertiesJsonConverter());
            _jsonSerializerSettings.Converters.Add(new IPv6HeaderInformationJsonConverter());

            _jsonSerializerSettings.Converters.Add(new IPv4AddressJsonConverter());
            _jsonSerializerSettings.Converters.Add(new DHCPv4PacketJsonConverter());
            _jsonSerializerSettings.Converters.Add(new DHCPv4ScopeAddressPropertiesConverter());
            _jsonSerializerSettings.Converters.Add(new DHCPv4ScopePropertyJsonConverter());
            _jsonSerializerSettings.Converters.Add(new DHCPv4ScopePropertiesJsonConverter());
            _jsonSerializerSettings.Converters.Add(new IPv4HeaderInformationJsonConverter());
        }

        public StorageContext(DbContextOptions<StorageContext> options, JsonSerializerSettings jsonSerializerSettings) : base(options)
        {
            _jsonSerializerSettings = jsonSerializerSettings;
        }

        private static async Task<T> RunOperation<T>(Func<Task<T>> operation)
        {
            try
            {
                await _writeSync.WaitAsync();
                var operationResult = await operation();
                return operationResult;
            }
            finally
            {
                _writeSync.Release();
            }
        }

        private async Task<Boolean> SaveChangesAsyncInternal() => await SaveChangesAsync() > 0;

        private static String GetBodyType(Type input) => input.AssemblyQualifiedName;
        private static String GetBodyType<T>(T input) => GetBodyType(input.GetType());

        public async Task<Boolean> Save(AggregateRootWithEvents aggregateRoot)
        {
            return await RunOperation<Boolean>(async () =>
            {
                String streamId = $"{aggregateRoot.GetType().Name}-{aggregateRoot.Id}";
                if (aggregateRoot is DHCPv6RootScope)
                {
                    streamId = nameof(DHCPv6RootScope);
                }
                Int32 version = aggregateRoot.Version;
                Int64 rowCount = await Entries.OrderByDescending(x => x.Position).Select(x => x.Position).FirstOrDefaultAsync();

                var entries = aggregateRoot.GetChanges().Select(x => new EventEntry
                {
                    Version = ++version,
                    Position = ++rowCount,
                    StreamId = streamId,
                    Timestamp = DateTime.UtcNow,
                    BodyType = GetBodyType(x),
                    Body = JsonConvert.SerializeObject(x, _jsonSerializerSettings)
                });

                Entries.AddRange(entries);
                return await SaveChangesAsyncInternal();
            });
        }

        private String GetStreamTypeIdentifer(Type type) => $"{type.Name}";
        private String GetStreamId(Type type, Guid id) => $"{GetStreamTypeIdentifer(type)}-{id}";
        private String GetStreamId<T>(Guid id) => GetStreamId(typeof(T), id);

        public async Task<Boolean> CheckIfAggrerootExists<T>(Guid id) where T : AggregateRootWithEvents, new()
        {
            return await RunOperation(async () =>
            {
                String streamId = GetStreamId<T>(id);

                Int32 amount = await Entries.CountAsync(x => x.StreamId == streamId);
                return amount > 0;
            });
        }

        public async Task<T> GetAggregateRoot<T>(Guid id) where T : AggregateRootWithEvents, new()
        {
            return await RunOperation(async () =>
            {
                String streamId = GetStreamId<T>(id);
                T result = new T();

                var events = await GetEvents(streamId);
                result.Load(events);

                return result;
            });
        }

        public async Task<IEnumerable<DHCPv6Listener>> GetDHCPv6Listener()
        {
            return await RunOperation(async () =>
            {
                var result = await DHCPv6Interfaces
                .OrderBy(x => x.Name).ToListAsync();

                List<DHCPv6Listener> listeners = new List<DHCPv6Listener>(result.Count);
                foreach (var item in result)
                {
                    DHCPv6Listener listener = new DHCPv6Listener();
                    listener.Load(new DomainEvent[]
                    {
                    new DHCPv6ListenerCreatedEvent {
                     Id = item.Id,
                     Name = item.Name,
                     InterfaceId = item.InterfaceId,
                     Address = item.IPv6Address,
                    }
                    });

                    listeners.Add(listener);
                }

                return listeners;
            });
        }

        public async Task<IEnumerable<DHCPv4Listener>> GetDHCPv4Listener()
        {
            return await RunOperation(async () =>
            {
                var result = await DHCPv4Interfaces
                .OrderBy(x => x.Name).ToListAsync();

                List<DHCPv4Listener> listeners = new List<DHCPv4Listener>(result.Count);
                foreach (var item in result)
                {
                    var listener = new DHCPv4Listener();
                    listener.Load(new DomainEvent[]
                    {
                        new DHCPv4ListenerCreatedEvent {
                         Id = item.Id,
                         Name = item.Name,
                         InterfaceId = item.InterfaceId,
                         Address = item.IPv6Address,
                        }
                    });

                    listeners.Add(listener);
                }

                return listeners;
            });
        }
       
        public async Task<Boolean> Project(IEnumerable<DomainEvent> events)
        {
            return await RunOperation(async () =>
            {
                Boolean hasChanges = true;
                foreach (var item in events)
                {
                    switch (item)
                    {
                        case DHCPv6ListenerCreatedEvent e:
                            DHCPv6Interfaces.Add(new DHCPv6InterfaceDataModel
                            {
                                Id = e.Id,
                                InterfaceId = e.InterfaceId,
                                IPv6Address = e.Address,
                                Name = e.Name,
                            });
                            break;

                        case DHCPv6ListenerDeletedEvent e:
                            var existingv6Interface = await DHCPv6Interfaces.FirstAsync(x => x.Id == e.Id);
                            DHCPv6Interfaces.Remove(existingv6Interface);
                            hasChanges = true;
                            break;

                        case DHCPv4ListenerCreatedEvent e:
                            DHCPv4Interfaces.Add(new DHCPv4InterfaceDataModel
                            {
                                Id = e.Id,
                                InterfaceId = e.InterfaceId,
                                IPv6Address = e.Address,
                                Name = e.Name,
                            });
                            break;

                        case DHCPv4ListenerDeletedEvent e:
                            var existingv4Interface = await DHCPv4Interfaces.FirstAsync(x => x.Id == e.Id);
                            DHCPv4Interfaces.Remove(existingv4Interface);
                            hasChanges = true;
                            break;

                        case DHCPv6PacketHandledEvent e:
                            DHCPv6PacketHandledEntryDataModel entry = new DHCPv6PacketHandledEntryDataModel
                            {
                                HandledSuccessfully = e.WasSuccessfullHandled,
                                ErrorCode = e.ErrorCode,
                                Id = Guid.NewGuid(),
                                RequestSize = e.Request.GetSize(),
                                ScopeId = e.ScopeId,
                                RequestType = e.Request.GetInnerPacket().PacketType,
                                Timestamp = e.Timestamp,
                                ResponseSize = e.Response?.GetSize(),
                                ResponseType = e.Response?.GetInnerPacket().PacketType
                            };

                            entry.SetTimestampDates();

                            DHCPv6PacketEntries.Add(entry);
                            break;

                        case DHCPv6LeaseCreatedEvent e:
                            {
                                DHCPv6LeaseEntryDataModel leaseEntry = new DHCPv6LeaseEntryDataModel
                                {
                                    Id = Guid.NewGuid(),
                                    Address = e.Address.ToString(),
                                    Start = e.StartedAt,
                                    End = e.ValidUntil,
                                    LeaseId = e.EntityId,
                                    ScopeId = e.ScopeId,
                                    Prefix = e.HasPrefixDelegation == true ? e.DelegatedNetworkAddress.ToString() : null,
                                    PrefixLength = e.HasPrefixDelegation == true ? e.PrefixLength : (Byte)0,
                                    Timestamp = e.Timestamp,
                                };

                                DHCPv6LeaseEntries.Add(leaseEntry);
                            }
                            break;

                        case DHCPv6LeaseExpiredEvent e:
                            hasChanges = await UpdateEndToLeaseEntry(e, ReasonToEndLease.Expired);
                            break;

                        case DHCPv6LeasePrefixAddedEvent e:
                            hasChanges = await UpdateLastestLeaseEntry(e, (leaseEntry) =>
                            {
                                leaseEntry.Prefix = e.NetworkAddress.ToString();
                                leaseEntry.PrefixLength = e.PrefixLength;
                            });
                            break;

                        case DHCPv6LeaseCanceledEvent e:
                            hasChanges = await UpdateEndToLeaseEntry(e, ReasonToEndLease.Canceled);
                            break;

                        case DHCPv6LeaseReleasedEvent e:
                            hasChanges = await UpdateEndToLeaseEntry(e, ReasonToEndLease.Released);
                            break;

                        case DHCPv6LeaseRenewedEvent e:
                            hasChanges = await UpdateLastestLeaseEntry(e, (leaseEntry) =>
                            {
                                leaseEntry.End = e.End;
                            });
                            break;

                        case DHCPv6LeaseRevokedEvent e:
                            hasChanges = await UpdateEndToLeaseEntry(e, ReasonToEndLease.Revoked);
                            break;

                        default:
                            hasChanges = false;
                            break;
                    }
                }

                if (hasChanges == false) { return true; }

                return await SaveChangesAsyncInternal();
            });
        }

        private async Task<Boolean> UpdateEndToLeaseEntry(DHCPv6ScopeRelatedEvent e, ReasonToEndLease reason)
        {

            return await UpdateLastestLeaseEntry(e, (leaseEntry) =>
             {
                 leaseEntry.End = e.Timestamp;
                 leaseEntry.EndReason = reason;
             });
        }

        private async Task<Boolean> UpdateLastestLeaseEntry(DHCPv6ScopeRelatedEvent e, Action<DHCPv6LeaseEntryDataModel> updater)
        {
            var entry = await DHCPv6LeaseEntries.Where(x => x.LeaseId == e.EntityId).OrderByDescending(x => x.Timestamp).FirstOrDefaultAsync();
            if (entry != null)
            {
                updater(entry);
                return true;
            }

            return false;
        }

        private DomainEvent GetEventFromString(String eventContent, String typename) =>
            JsonConvert.DeserializeObject(eventContent,
                    Type.GetType(typename), _jsonSerializerSettings) as DomainEvent;

        public async Task<IEnumerable<DomainEvent>> GetEvents(String streamId)
        {
            return await RunOperation(async () =>
            {
                var preEvents = await Entries.Where(x => x.StreamId == streamId).Select(x => new
                {
                    x.BodyType,
                    x.Body
                }
            ).ToListAsync();

                List<DomainEvent> events = new List<DomainEvent>(preEvents.Count);
                foreach (var item in preEvents)
                {
                    DomainEvent @event = GetEventFromString(item.Body, item.BodyType);
                    events.Add(@event);
                }

                return events;
            });
        }

        public async Task<IDictionary<Guid, IEnumerable<DomainEvent>>> GetEventsStartWith(Type type)
        {
            return await RunOperation(async () =>
            {
                String typeIdentiifer = GetStreamTypeIdentifer(type);

                var preEvents = await Entries.Where(x => x.StreamId.StartsWith(typeIdentiifer)).Select(x => new
                {
                    Id = x.StreamId,
                    x.BodyType,
                    x.Body
                }
                ).ToListAsync();

                var eventsGroupedById = new Dictionary<Guid, List<DomainEvent>>();
                foreach (var item in preEvents)
                {
                    Guid id = Guid.Parse(item.Id.Substring(typeIdentiifer.Length + 1));
                    if (eventsGroupedById.ContainsKey(id) == false)
                    {
                        eventsGroupedById.Add(id, new List<DomainEvent>());

                    }

                    DomainEvent @event = GetEventFromString(item.Body, item.BodyType);
                    eventsGroupedById[id].Add(@event);
                }

                return eventsGroupedById.ToDictionary(x => x.Key, x => (IEnumerable<DomainEvent>)x.Value);
            });
        }

        private const String _DHCPv6ServerConfigKey = "DHCPv6ServerConfig";

        public async Task<DHCPv6ServerProperties> GetServerProperties()
        {
            return await RunOperation(async () =>
            {
                String content = await Helpers.Where(x => x.Name == _DHCPv6ServerConfigKey).Select(x => x.Content).FirstOrDefaultAsync();
                if (String.IsNullOrEmpty(content) == true)
                {
                    return new DHCPv6ServerProperties { IsInitilized = false };
                }

                var result = JsonConvert.DeserializeObject<DHCPv6ServerProperties>(content, _jsonSerializerSettings);
                result.SetDefaultIfNeeded();

                return result;
            });
        }

        public async Task<Boolean> SaveInitialServerConfiguration(DHCPv6ServerProperties config)
        {
            return await RunOperation(async () =>
            {
                config.IsInitilized = true;
                config.SetDefaultIfNeeded();
                String content = JsonConvert.SerializeObject(config, _jsonSerializerSettings);

                Helpers.Add(new HelperEntry
                {
                    Name = _DHCPv6ServerConfigKey,
                    Content = content,
                });

                return await SaveChangesAsyncInternal();
            });
        }

        public async Task<Boolean> Exists()
        {
            return await RunOperation(async () =>
            {
                Boolean result = await base.Database.CanConnectAsync();
                return result;
            });
        }

        public async Task<Boolean> DeleteAggregateRoot<T>(Guid id) where T : AggregateRootWithEvents
        {
            return await RunOperation(async () =>
            {
                String streamIdentifier = GetStreamId<T>(id);
                var items = await Entries.Where(x => x.StreamId == streamIdentifier).ToListAsync();
                Entries.RemoveRange(items);

                return await SaveChangesAsyncInternal();
            });
        }

        private async Task<Boolean> DeleteEntriesBasedOnTimestampAndEventType(DateTime leaseThreshold, IEnumerable<String> eventTypes, Boolean isLsesed)
        {
            return await RunOperation(async () =>
            {
                var entriesToDelete = await Entries
                 .Where(x => x.Timestamp < leaseThreshold && eventTypes.Contains(x.BodyType))
                 .ToListAsync();

                Entries.RemoveRange(entriesToDelete);

                if (isLsesed == false)
                {
                    var statisticEntriesToRemove = await DHCPv6PacketEntries.Where(x => x.Timestamp < leaseThreshold).ToListAsync();
                    DHCPv6PacketEntries.RemoveRange(statisticEntriesToRemove);
                }
                else
                {
                    var leaseEntriesToRmeove = await DHCPv6LeaseEntries.Where(x => x.Timestamp < leaseThreshold).ToListAsync();
                    DHCPv6LeaseEntries.RemoveRange(leaseEntriesToRmeove);
                }

                return await SaveChangesAsyncInternal();
            });
        }

        public async Task<Boolean> DeleteLeaseRelatedEventsOlderThan(DateTime leaseThreshold) => await DeleteEntriesBasedOnTimestampAndEventType(leaseThreshold, _leasseCorrelatedEventsNames, true);
        public async Task<Boolean> DeletePacketHandledEventsOlderThan(DateTime handledEventThreshold) => await DeleteEntriesBasedOnTimestampAndEventType(handledEventThreshold, _handledCorrelatedEventsNames, false);

        public async Task<Boolean> DeletePacketHandledEventMoreThan(UInt32 threshold)
        {
            return await RunOperation(async () =>
            {
                Int32 total = await Entries.Where(x => _handledCorrelatedEventsNames.Contains(x.BodyType) == true).CountAsync();
                if (total < threshold) { return true; }

                Int32 diff = (Int32)threshold - total;

                var entriesToRemove = await Entries.Where(x => _handledCorrelatedEventsNames.Contains(x.BodyType) == true).OrderBy(x => x.Timestamp).Take(diff).ToListAsync();
                Entries.RemoveRange(entriesToRemove);

                Int32 statisticsDiff = (Int32)threshold - await DHCPv6PacketEntries.CountAsync();
                var statisticEntriesToRemove = await DHCPv6PacketEntries.OrderBy(x => x.Timestamp).Take(statisticsDiff).ToListAsync();
                DHCPv6PacketEntries.RemoveRange(statisticEntriesToRemove);

                return await SaveChangesAsyncInternal();
            });
        }

        private async Task<IList<DHCPv6PacketHandledEntry>> GetPacketsFromHandledEvents(Int32 amount, Guid? scopeId)
        {
            IQueryable<EventEntry> preEvents = Entries
               .Where(x => _handledCorrelatedEventsNames.Contains(x.BodyType));

            if (scopeId.HasValue == true)
            {
                preEvents = preEvents.Where(x => EF.Functions.Like(x.Body, $"%\"{nameof(DHCPv6PacketHandledEvent.ScopeId)}\":\"{scopeId}\"%"));
            }

            var entries = await preEvents.OrderByDescending(x => x.Timestamp).Take(amount)
               .ToListAsync();

            List<DHCPv6PacketHandledEntry> result = new List<DHCPv6PacketHandledEntry>(entries.Count);

            foreach (var item in entries)
            {
                DHCPv6PacketHandledEntry entry = null;
                DomainEvent @event = GetEventFromString(item.Body, item.BodyType);
                switch (@event)
                {
                    case DHCPv6SolicitHandledEvent e:
                        entry = new DHCPv6PacketHandledEntry(e);
                        break;
                    case DHCPv6DeclineHandledEvent e:
                        entry = new DHCPv6PacketHandledEntry(e);
                        break;
                    case DHCPv6InformRequestHandledEvent e:
                        entry = new DHCPv6PacketHandledEntry(e);
                        break;
                    case DHCPv6ReleaseHandledEvent e:
                        entry = new DHCPv6PacketHandledEntry(e);
                        break;
                    case DHCPv6RequestHandledEvent e:
                        entry = new DHCPv6PacketHandledEntry(e);
                        break;
                    case DHCPv6RenewHandledEvent e:
                        entry = new DHCPv6PacketHandledEntry(e);
                        break;
                    case DHCPv6RebindHandledEvent e:
                        entry = new DHCPv6PacketHandledEntry(e);
                        break;
                    case DHCPv6ConfirmHandledEvent e:
                        entry = new DHCPv6PacketHandledEntry(e);
                        break;

                    default:
                        break;
                }

                result.Add(entry);
            }

            return result;
        }

        public async Task<DashboardResponse> GetDashboardOverview()
        {
            return await RunOperation(async () =>
            {
                DateTime now = DateTime.UtcNow;

                DashboardResponse response = new DashboardResponse
                {
                    DHCPv6 = new DHCPOverview<DHCPv6LeaseEntry, DHCPv6PacketHandledEntry>
                    {
                        ActiveInterfaces = await DHCPv6Interfaces.CountAsync(),
                        ActiveLeases = await DHCPv6LeaseEntries.Where(x => now >= x.Start && now <= x.End).OrderByDescending(x => x.End).Select(x => new DHCPv6LeaseEntry
                        {
                            Address = x.Address,
                            End = x.End,
                            EndReason = x.EndReason,
                            LeaseId = x.LeaseId,
                            Prefix = x.Prefix,
                            PrefixLength = x.PrefixLength,
                            ScopeId = x.ScopeId,
                            Start = x.Start,
                            Timestamp = x.Timestamp,
                        }).Take(1000).ToListAsync(),
                        Packets = await GetPacketsFromHandledEvents(100, null),
                    },
                };

                return response;
            });
        }

        public async Task<IDictionary<DateTime, IDictionary<DHCPv6PacketTypes, Int32>>> GetIncomingDHCPv6PacketTypes(DateTime? start, DateTime? end, GroupStatisticsResultBy groupedBy)
        {
            return await RunOperation(async () =>
            {
                IQueryable<DHCPv6PacketHandledEntryDataModel> packets = GetPrefiltedPackets(start, end);

                var packetEntryResult = await packets.ToListAsync();

                IEnumerable<IGrouping<DateTime, DHCPv6PacketHandledEntryDataModel>> groupedResult = null;
                switch (groupedBy)
                {
                    case GroupStatisticsResultBy.Day:
                        groupedResult = packetEntryResult.GroupBy(x => x.TimestampDay);
                        break;
                    case GroupStatisticsResultBy.Week:
                        groupedResult = packetEntryResult.GroupBy(x => x.TimestampWeek);
                        break;
                    case GroupStatisticsResultBy.Month:
                        groupedResult = packetEntryResult.GroupBy(x => x.TimestampMonth);
                        break;
                    default:
                        break;
                }

                var result = groupedResult.Select(x => new
                {
                    Key = x.Key,
                    RequestTypes = x.Select(x => x.RequestType)
                }).ToDictionary(
                        x => x.Key,
                        x => x.RequestTypes.GroupBy(y => y).ToDictionary(y => y.Key, y => y.Count()) as IDictionary<DHCPv6PacketTypes, Int32>);

                return result;
            });
        }

        public async Task<IDictionary<DateTime, Int32>> GetFileredDHCPv6Packets(DateTime? start, DateTime? end, GroupStatisticsResultBy groupedBy)
        {
            return await RunOperation(async () =>
            {
                IQueryable<DHCPv6PacketHandledEntryDataModel> packets = GetPrefiltedPackets(start, end);
                packets = packets.Where(x => x.FilteredBy != null);
                return await GroupPackets(groupedBy, packets);
            });
        }

        public async Task<IDictionary<DateTime, Int32>> GetErrorDHCPv6Packets(DateTime? start, DateTime? end, GroupStatisticsResultBy groupedBy)
        {
            return await RunOperation(async () =>
            {
                IQueryable<DHCPv6PacketHandledEntryDataModel> packets = GetPrefiltedPackets(start, end);
                packets = packets.Where(x => x.InvalidRequest == true);
                return await GroupPackets(groupedBy, packets);
            });
        }

        public async Task<IDictionary<DateTime, Int32>> GetIncomingDHCPv6PacketAmount(DateTime? start, DateTime? end, GroupStatisticsResultBy groupedBy)
        {
            return await RunOperation(async () =>
            {
                IQueryable<DHCPv6PacketHandledEntryDataModel> packets = GetPrefiltedPackets(start, end);
                return await GroupPackets(groupedBy, packets);
            });
        }

        private static async Task<IDictionary<DateTime, int>> GroupPackets(GroupStatisticsResultBy groupedBy, IQueryable<DHCPv6PacketHandledEntryDataModel> packets)
        {

            IQueryable<IGrouping<DateTime, DHCPv6PacketHandledEntryDataModel>> groupedResult = null;
            switch (groupedBy)
            {
                case GroupStatisticsResultBy.Day:
                    groupedResult = packets.GroupBy(x => x.TimestampDay);
                    break;
                case GroupStatisticsResultBy.Week:
                    groupedResult = packets.GroupBy(x => x.TimestampWeek);
                    break;
                case GroupStatisticsResultBy.Month:
                    groupedResult = packets.GroupBy(x => x.TimestampMonth);
                    break;
                default:
                    break;
            }

            var result = await groupedResult.Select(x => new
            {
                Key = x.Key,
                Amount = x.Count()
            }).ToDictionaryAsync(x => x.Key, x => x.Amount);

            return result;

        }

        private IQueryable<DHCPv6PacketHandledEntryDataModel> GetPrefiltedPackets(DateTime? start, DateTime? end)
        {
            IQueryable<DHCPv6PacketHandledEntryDataModel> packets = DHCPv6PacketEntries;
            if (start.HasValue == true)
            {
                packets = packets.Where(x => x.Timestamp >= start);
            }
            if (end.HasValue == true)
            {
                packets = packets.Where(x => x.Timestamp <= end.Value);
            }

            return packets;
        }

        public async Task<IDictionary<Int32, Int32>> GetErrorCodesPerDHCPV6RequestType(DateTime? start, DateTime? end, DHCPv6PacketTypes type)
        {
            return await RunOperation(async () =>
            {
                IQueryable<DHCPv6PacketHandledEntryDataModel> packets = GetPrefiltedPackets(start, end);
                packets = packets.Where(x => x.RequestType == type);

                var result = await packets.GroupBy(x => x.ErrorCode).Select(x => new
                {
                    Key = x.Key,
                    Amount = x.Count()
                }).ToDictionaryAsync(x => x.Key, x => x.Amount);

                return result;
            });
        }

        private class DateTimeRange
        {
            public DateTime RangeStart { get; set; }
            public DateTime RangeEnd { get; set; }
        }

        private Int32 CountTimeIntersection(DateTime start, DateTime end, IEnumerable<DateTimeRange> ranges) =>
            ranges.Count(x => start < x.RangeEnd && end >= x.RangeStart);

        public async Task<IDictionary<DateTime, Int32>> GetActiveDHCPv6Leases(DateTime? start, DateTime? end, GroupStatisticsResultBy groupedBy)
        {
            return await RunOperation(async () =>
            {
                start ??= await DHCPv6LeaseEntries.OrderBy(x => x.Timestamp).Select(x => x.Timestamp).FirstOrDefaultAsync();
                if (start.Value == default)
                {
                    return new Dictionary<DateTime, Int32>();
                }

                end ??= DateTime.UtcNow;
                DateTime currentStart;

                var preResult = await DHCPv6LeaseEntries.Where(x => start.Value < x.End && end.Value >= x.Start)
                    .Select(x => new DateTimeRange { RangeStart = x.Start, RangeEnd = x.End }).ToListAsync();

                if (preResult.Count == 0)
                {
                    return new Dictionary<DateTime, Int32>();
                }

                DateTime firstStart = start.Value;
                Func<DateTime, DateTime> timeAdjuster;
                switch (groupedBy)
                {
                    case GroupStatisticsResultBy.Day:
                        currentStart = firstStart.Date.AddDays(1);
                        timeAdjuster = x => x.AddDays(1);
                        break;
                    case GroupStatisticsResultBy.Week:
                        currentStart = firstStart.GetFirstWeekDay().AddDays(7);
                        timeAdjuster = x => x.AddDays(7);
                        break;
                    case GroupStatisticsResultBy.Month:
                        currentStart = new DateTime(firstStart.Year, firstStart.Month, 1).AddMonths(1);
                        timeAdjuster = x => x.AddMonths(1);

                        break;
                    default:
                        throw new NotImplementedException();
                }

                Dictionary<DateTime, Int32> result = new Dictionary<DateTime, int>
            {
                { firstStart, CountTimeIntersection(firstStart, currentStart, preResult) }
            };
                while (currentStart < end.Value)
                {
                    DateTime intervallEnd = timeAdjuster(currentStart);

                    Int32 elements = CountTimeIntersection(currentStart, intervallEnd, preResult);
                    result.Add(currentStart, elements);

                    currentStart = intervallEnd;
                }

                return result;
            });
        }

        private async Task<Boolean> AddDHCPv6PacketHandledEntryDataModel(DHCPv6Packet packet, Action<DHCPv6PacketHandledEntryDataModel> modifier)
        {
            return await RunOperation(async () =>
            {
                var entry = new DHCPv6PacketHandledEntryDataModel
                {
                    RequestSize = packet.GetSize(),
                    RequestType = packet.GetInnerPacket().PacketType,
                    Timestamp = DateTime.UtcNow,
                    Id = Guid.NewGuid(),
                };
                modifier?.Invoke(entry);

                entry.SetTimestampDates();

                DHCPv6PacketEntries.Add(entry);
                return await SaveChangesAsyncInternal();
            });
        }

        public async Task<Boolean> LogInvalidDHCPv6Packet(DHCPv6Packet packet) => await AddDHCPv6PacketHandledEntryDataModel(packet, (x) => x.InvalidRequest = true);
        public async Task<Boolean> LogFilteredDHCPv6Packet(DHCPv6Packet packet, String filterName) => await AddDHCPv6PacketHandledEntryDataModel(packet, (x) => x.FilteredBy = filterName);

        public async Task<IEnumerable<DHCPv6PacketHandledEntry>> GetHandledDHCPv6PacketByScopeId(Guid scopeId, Int32 amount) => await GetPacketsFromHandledEvents(amount, scopeId);

        private async Task<Boolean> AddDHCPv4PacketHandledEntryDataModel(DHCPv4Packet packet, Action<DHCPv4PacketHandledEntryDataModel> modifier)
        {
            return await RunOperation(async () =>
            {
                var entry = new DHCPv4PacketHandledEntryDataModel
                {
                    RequestSize = packet.GetSize(),
                    RequestType = packet.MessageType,
                    Timestamp = DateTime.UtcNow,
                    Id = Guid.NewGuid(),
                };
                modifier?.Invoke(entry);

                entry.SetTimestampDates();

                DHCPv4PacketEntries.Add(entry);
                return await SaveChangesAsyncInternal();
            });
        }

        public async Task<Boolean> LogInvalidDHCPv4Packet(DHCPv4Packet packet) => await AddDHCPv4PacketHandledEntryDataModel(packet, (x) => x.InvalidRequest = true);
        public async Task<Boolean> LogFilteredDHCPv4Packet(DHCPv4Packet packet, String filterName) => await AddDHCPv4PacketHandledEntryDataModel(packet, (x) => x.FilteredBy = filterName);

    }
}
