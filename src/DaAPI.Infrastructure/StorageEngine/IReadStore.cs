using DaAPI.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaAPI.Infrastructure.StorageEngine
{
    public interface IReadStore
    {
        Task<bool> Project(IEnumerable<DomainEvent> events);
    }
}
