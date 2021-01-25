using DaAPI.Core.Listeners;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaAPI.Infrastructure.StorageEngine.DHCPv4
{
    public interface IDHCPv4ReadStore : IReadStore
    {
        Task<IEnumerable<DHCPv4Listener>> GetDHCPv4Listener();
    }
}
