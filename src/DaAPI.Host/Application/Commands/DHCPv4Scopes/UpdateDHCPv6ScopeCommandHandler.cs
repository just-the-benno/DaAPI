using DaAPI.Core.Scopes;
using DaAPI.Core.Scopes.DHCPv4;
using DaAPI.Infrastructure.ServiceBus;
using DaAPI.Infrastructure.ServiceBus.Messages;
using DaAPI.Infrastructure.StorageEngine.DHCPv4;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using static DaAPI.Shared.Requests.DHCPv4ScopeRequests.V1;

namespace DaAPI.Host.Application.Commands.DHCPv4Scopes
{
    public class UpdateDHCPv4ScopeCommandHandler : ManipulateDHCPv4ScopeCommandHandler, IRequestHandler<UpdateDHCPv4ScopeCommand, Boolean>
    {
        private readonly IDHCPv4StorageEngine _store;
        private readonly IServiceBus _serviceBus;
        private readonly DHCPv4RootScope _rootScope;
        private readonly ILogger<UpdateDHCPv4ScopeCommandHandler> _logger;

        public UpdateDHCPv4ScopeCommandHandler(
            IDHCPv4StorageEngine store,
            IServiceBus serviceBus,
            DHCPv4RootScope rootScope,
            ILogger<UpdateDHCPv4ScopeCommandHandler> logger)
        {
            _store = store;
            this._serviceBus = serviceBus;
            _rootScope = rootScope;
            _logger = logger;
        }

        public async Task<Boolean> Handle(UpdateDHCPv4ScopeCommand request, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Handle started");

            var scope = _rootScope.GetScopeById(request.ScopeId);
            if (scope == DHCPv4Scope.NotFound)
            {
                return false;
            }

            Guid? parentId = scope.HasParentScope() == false ? new Guid?() : scope.ParentScope.Id;
            var properties = GetScopeProperties(request);
            var addressProperties = GetScopeAddressProperties(request);

            if (request.Name != scope.Name)
            {
                _rootScope.UpdateScopeName(request.ScopeId, ScopeName.FromString(request.Name));
            }
            if (request.Description != scope.Description)
            {
                _rootScope.UpdateScopeDescription(request.ScopeId, ScopeDescription.FromString(request.Description));
            }
            if (request.ParentId != parentId)
            {
                _rootScope.UpdateParent(request.ScopeId, request.ParentId);
            }

            _rootScope.UpdateScopeResolver(request.ScopeId, GetResolverInformation(request));

            if (addressProperties != scope.AddressRelatedProperties)
            {
                _rootScope.UpdateAddressProperties(request.ScopeId, addressProperties);
            }

            if (properties != scope.Properties)
            {
                _rootScope.UpdateScopeProperties(request.ScopeId, properties);
            }

            Boolean result = await _store.Save(_rootScope);

            if (result == true)
            {
                var triggers = _rootScope.GetTriggers();

                if (triggers.Any() == true)
                {
                    await _serviceBus.Publish(new NewTriggerHappendMessage(triggers));

                    _rootScope.ClearTriggers();
                }
            }

            return result;
        }
    }
}
