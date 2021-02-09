using DaAPI.Core.Scopes;
using DaAPI.Core.Scopes.DHCPv4;
using DaAPI.Infrastructure.StorageEngine.DHCPv4;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;
using static DaAPI.Shared.Requests.DHCPv6ScopeRequests.V1;

namespace DaAPI.Host.Application.Commands.DHCPv4Scopes
{
    public class CreateDHCPv4ScopeCommandHandler : ManipulateDHCPv4ScopeCommandHandler, IRequestHandler<CreateDHCPv4ScopeCommand, Guid?>
    {
        private readonly IDHCPv4StorageEngine _store;
        private readonly DHCPv4RootScope _rootScope;
        private readonly ILogger<CreateDHCPv4ScopeCommandHandler> _logger;

        public CreateDHCPv4ScopeCommandHandler(
            IDHCPv4StorageEngine store, DHCPv4RootScope rootScope,
            ILogger<CreateDHCPv4ScopeCommandHandler> logger)
        {
            _store = store;
            _rootScope = rootScope;
            _logger = logger;
        }

        public async Task<Guid?> Handle(CreateDHCPv4ScopeCommand request, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Handle started");

            Guid id = Guid.NewGuid();

            DHCPv4ScopeCreateInstruction instruction = new DHCPv4ScopeCreateInstruction
            {
                Id = id,
                Name = request.Name,
                Description = request.Description,
                ParentId = request.ParentId,
                AddressProperties = GetScopeAddressProperties(request),
                ResolverInformation = GetResolverInformation(request),
                ScopeProperties = GetScopeProperties(request),
            };

            _rootScope.AddScope(instruction);

            await _store.Save(_rootScope);

            return id;
        }
    }
}
