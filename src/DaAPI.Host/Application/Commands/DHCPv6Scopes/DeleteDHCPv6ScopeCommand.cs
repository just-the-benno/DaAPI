using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DaAPI.Host.Application.Commands.DHCPv6Scopes
{
    public class DeleteDHCPv6ScopeCommand : IRequest<Boolean>
    {
        public Guid ScopeId { get; }
        public Boolean IncludeChildren { get; }

        public DeleteDHCPv6ScopeCommand(Guid scopeId, Boolean includeChildren)
        {
            ScopeId = scopeId;
            IncludeChildren = includeChildren;
        }

    }
}
