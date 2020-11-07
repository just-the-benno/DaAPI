using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Listeners;
using DaAPI.Infrastructure.InterfaceEngines;
using DaAPI.Infrastructure.StorageEngine.DHCPv6;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DaAPI.Host.Application.Commands.DHCPv6Interfaces
{
    public class CreateDHCPv6InterfaceListenerCommandHandler : IRequestHandler<CreateDHCPv6InterfaceListenerCommand, Guid?>
    {
        private readonly IDHCPv6InterfaceEngine _interfaceEngine;
        private readonly IDHCPv6StorageEngine _storageEngine;
        private readonly ILogger<CreateDHCPv6InterfaceListenerCommandHandler> _logger;

        public CreateDHCPv6InterfaceListenerCommandHandler(
            IDHCPv6InterfaceEngine interfaceEngine,
            IDHCPv6StorageEngine storageEngine,
            ILogger<CreateDHCPv6InterfaceListenerCommandHandler> logger)
        {
            this._interfaceEngine = interfaceEngine ?? throw new ArgumentNullException(nameof(interfaceEngine));
            this._storageEngine = storageEngine ?? throw new ArgumentNullException(nameof(storageEngine));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Guid?> Handle(CreateDHCPv6InterfaceListenerCommand request, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Handle started");

            DHCPv6Listener listener = DHCPv6Listener.Create(
            request.NicId,
            DHCPListenerName.FromString(request.Name),
            IPv6Address.FromString(request.IPv6Addres));

            var possibleListeners = _interfaceEngine.GetPossibleListeners();
            if (possibleListeners.Count(x => x.Address == listener.Address && x.PhysicalInterfaceId == listener.PhysicalInterfaceId) == 0)
            {
                return null;
            }

            var activeListeners = await _interfaceEngine.GetActiveListeners();
            if(activeListeners.Count(x => x.Address == listener.Address && x.PhysicalInterfaceId == listener.PhysicalInterfaceId) > 0)
            {
                return null;
            }

            if(await _storageEngine.Save(listener) == false)
            {
                return null;
            }

            _interfaceEngine.OpenListener(listener);

            return listener.Id;
        }
    }
}
