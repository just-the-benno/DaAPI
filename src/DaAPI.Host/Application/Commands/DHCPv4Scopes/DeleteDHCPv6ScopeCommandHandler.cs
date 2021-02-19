using DaAPI.Core.Scopes;
using DaAPI.Core.Scopes.DHCPv4;
using DaAPI.Infrastructure.StorageEngine.DHCPv4;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DaAPI.Host.Application.Commands.DHCPv4Scopes
{
    public class DeleteDHCPv4ScopeCommandHandler : IRequestHandler<DeleteDHCPv4ScopeCommand, Boolean>
    {
        private readonly DHCPv4RootScope rootScope;
        private readonly IDHCPv4StorageEngine storageEngine;
        private readonly ILogger<DeleteDHCPv4ScopeCommandHandler> logger;

        public DeleteDHCPv4ScopeCommandHandler(
            IDHCPv4StorageEngine storageEngine,
            DHCPv4RootScope rootScope,
            ILogger<DeleteDHCPv4ScopeCommandHandler> logger
            )
        {
            this.rootScope = rootScope;
            this.storageEngine = storageEngine;
            this.logger = logger;
        }

        public async Task<Boolean> Handle(DeleteDHCPv4ScopeCommand request, CancellationToken cancellationToken)
        {
            if (rootScope.GetScopeById(request.ScopeId) == DHCPv4Scope.NotFound)
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
