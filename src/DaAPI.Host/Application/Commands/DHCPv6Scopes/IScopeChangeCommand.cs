using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static DaAPI.Shared.Requests.DHCPv6ScopeRequests.V1;

namespace DaAPI.Host.Application.Commands.DHCPv6Scopes
{
    public interface IScopeChangeCommand
    {
         String Name { get; }
         String Description { get; }
         Guid? ParentId { get; }
         DHCPv6ScopeAddressPropertyReqest AddressProperties { get; }
         CreateScopeResolverRequest Resolver { get; }
         IEnumerable<DHCPv6ScopePropertyRequest> Properties { get; }
    }
}
