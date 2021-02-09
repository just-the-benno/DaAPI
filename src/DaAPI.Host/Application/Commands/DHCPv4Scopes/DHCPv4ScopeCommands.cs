using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static DaAPI.Shared.Requests.DHCPv4ScopeRequests.V1;

namespace DaAPI.Host.Application.Commands.DHCPv4Scopes
{
    public interface IScopeChangeCommand
    {
        String Name { get; }
        String Description { get; }
        Guid? ParentId { get; }
        DHCPv4ScopeAddressPropertyReqest AddressProperties { get; }
        CreateScopeResolverRequest Resolver { get; }
        IEnumerable<DHCPv4ScopePropertyRequest> Properties { get; }
    }

    public record CreateDHCPv4ScopeCommand(
        String Name, String Description, Guid? ParentId,
        DHCPv4ScopeAddressPropertyReqest AddressProperties, CreateScopeResolverRequest Resolver,
         IEnumerable<DHCPv4ScopePropertyRequest> Properties
        ) : IScopeChangeCommand, IRequest<Guid?>;
    
    public record DeleteDHCPv4ScopeCommand(Guid ScopeId, Boolean IncludeChildren) : IRequest<Boolean>;

    public record UpdateDHCPv4ScopeCommand(
    Guid ScopeId, String Name, String Description, Guid? ParentId,
     DHCPv4ScopeAddressPropertyReqest AddressProperties, CreateScopeResolverRequest Resolver,
     IEnumerable<DHCPv4ScopePropertyRequest> Properties
    ) : IScopeChangeCommand, IRequest<Boolean>;

}
