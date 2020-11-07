using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Scopes;
using DaAPI.Core.Scopes.DHCPv6;
using DaAPI.Core.Scopes.DHCPv6.ScopeProperties;
using DaAPI.Infrastructure.StorageEngine.DHCPv6;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;
using static DaAPI.Shared.Requests.DHCPv6ScopeRequests.V1;

namespace DaAPI.Host.Application.Commands.DHCPv6Scopes
{
    public class CreateDHCPv6ScopeCommandHandler : ManipulateDHCPv6ScopeCommandHandler, IRequestHandler<CreateDHCPv6ScopeCommand, Guid?>
    {
        private readonly IDHCPv6StorageEngine _store;
        private readonly DHCPv6RootScope _rootScope;
        private readonly ILogger<CreateDHCPv6ScopeCommandHandler> _logger;

        public CreateDHCPv6ScopeCommandHandler(
            IDHCPv6StorageEngine store, DHCPv6RootScope rootScope,
            ILogger<CreateDHCPv6ScopeCommandHandler> logger)
        {
            _store = store;
            _rootScope = rootScope;
            _logger = logger;
        }

        public async Task<Guid?> Handle(CreateDHCPv6ScopeCommand request, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Handle started");

            Guid id = Guid.NewGuid();

            DHCPv6ScopeCreateInstruction instruction = new DHCPv6ScopeCreateInstruction
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
