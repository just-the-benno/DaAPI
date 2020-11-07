using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static DaAPI.Shared.Requests.DHCPv6ScopeRequests.V1;

namespace DaAPI.Host.Application.Commands.DHCPv6Scopes
{
    public class CreateDHCPv6ScopeCommand : IScopeChangeCommand, IRequest<Guid?>
    {
        public String Name { get; }
        public String Description { get; }
        public Guid? ParentId { get; }
        public DHCPv6ScopeAddressPropertyReqest AddressProperties { get; }
        public CreateScopeResolverRequest Resolver { get; }
        public IEnumerable<DHCPv6ScopePropertyRequest> Properties { get; }

        public CreateDHCPv6ScopeCommand(
            String name, String description,Guid? parentId, 
            DHCPv6ScopeAddressPropertyReqest addressProperties,
            CreateScopeResolverRequest resolver,
            IEnumerable<DHCPv6ScopePropertyRequest> properties
            )
        {
            Name = name;
            Description = description;
            ParentId = parentId;
            AddressProperties = addressProperties;
            Resolver = resolver;
            Properties = properties;
        }
    }
}
