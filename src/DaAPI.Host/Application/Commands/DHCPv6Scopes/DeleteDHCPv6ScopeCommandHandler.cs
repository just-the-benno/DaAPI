using DaAPI.Core.Scopes;
using DaAPI.Core.Scopes.DHCPv6;
using DaAPI.Infrastructure.StorageEngine.DHCPv6;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DaAPI.Host.Application.Commands.DHCPv6Scopes
{
    public class DeleteDHCPv6ScopeCommandHandler : IRequestHandler<DeleteDHCPv6ScopeCommand, Boolean>
    {
        private readonly DHCPv6RootScope rootScope;
        private readonly IDHCPv6StorageEngine storageEngine;
        private readonly ILogger<DeleteDHCPv6ScopeCommandHandler> logger;

        public DeleteDHCPv6ScopeCommandHandler(
            IDHCPv6StorageEngine storageEngine,
            DHCPv6RootScope rootScope,
            ILogger<DeleteDHCPv6ScopeCommandHandler> logger
            )
        {
            this.rootScope = rootScope;
            this.storageEngine = storageEngine;
            this.logger = logger;
        }

        public async Task<Boolean> Handle(DeleteDHCPv6ScopeCommand request, CancellationToken cancellationToken)
        {
            if (rootScope.GetScopeById(request.ScopeId) == DHCPv6Scope.NotFound)
            {
                logger.LogInformation("unable to delete the scope {scopeId}. Scope not found");
                return false;
            }

            rootScope.DeleteScope(request.ScopeId, request.IncludeChildren);

            Boolean result = await storageEngine.Save(rootScope);
            if (result == false)
            {
                logger.LogError("unable to delete the scope {scopeId}. Saving changes failed", request.ScopeId);
            }
            return result;
        }
    }
}
