using DaAPI.Core.Common;
using DaAPI.Core.Listeners;
using DaAPI.Core.Packets.DHCPv4;
using DaAPI.Core.Scopes;
using DaAPI.Core.Scopes.DHCPv4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaAPI.Infrastructure.StorageEngine.DHCPv4
{
    public interface IDHCPv4StorageEngine
    {
        Task<IEnumerable<DHCPv4Listener>> GetDHCPv4Listener();
        Task<Boolean> LogInvalidDHCPv4Packet(DHCPv4Packet packet);
        Task<Boolean> LogFilteredDHCPv4Packet(DHCPv4Packet packet, string filterName);
        Task<DHCPv4RootScope> GetRootScope(IScopeResolverManager<DHCPv4Packet, IPv4Address> scopeResolverManager);
    }
}
