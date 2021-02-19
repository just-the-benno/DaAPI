using DaAPI.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaAPI.Infrastructure.StorageEngine
{
    public interface IEventStore
    {
        Task<Boolean> Save(AggregateRootWithEvents aggregateRoot);
        Task<Boolean> CheckIfAggrerootExists<T>(Guid id) where T : AggregateRootWithEvents, new();
        Task<T> GetAggregateRoot<T>(Guid id) where T : AggregateRootWithEvents, new();
        Task<IDictionary<Guid, IEnumerable<DomainEvent>>> GetEventsStartWith(Type type);
        Task<Boolean> DeleteAggregateRoot<T>(Guid id) where T : AggregateRootWithEvents;
        Task<Boolean> Exists();
        Task<IEnumerable<DomainEvent>> GetEvents(String typename);
    }
}
