using MediatR;
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

    public record CreateDHCPv6ScopeCommand(
        String Name, String Description, Guid? ParentId,
        DHCPv6ScopeAddressPropertyReqest AddressProperties, CreateScopeResolverRequest Resolver,
         IEnumerable<DHCPv6ScopePropertyRequest> Properties
        ) : IScopeChangeCommand, IRequest<Guid?>;
    
    public record DeleteDHCPv6ScopeCommand(Guid ScopeId, Boolean IncludeChildren) : IRequest<Boolean>;

    public record UpdateDHCPv6ScopeCommand(
    Guid ScopeId, String Name, String Description, Guid? ParentId,
     DHCPv6ScopeAddressPropertyReqest AddressProperties, CreateScopeResolverRequest Resolver,
     IEnumerable<DHCPv6ScopePropertyRequest> Properties
    ) : IScopeChangeCommand, IRequest<Boolean>;

}
