using DaAPI.Core.Common;
using DaAPI.Core.Listeners;
using DaAPI.Infrastructure.InterfaceEngines;
using DaAPI.Infrastructure.StorageEngine.DHCPv4;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DaAPI.Host.Application.Commands.DHCPv4Interfaces
{
    public class CreateDHCPv4InterfaceListenerCommandHandler : IRequestHandler<CreateDHCPv4InterfaceListenerCommand, Guid?>
    {
        private readonly IDHCPv4InterfaceEngine _interfaceEngine;
        private readonly IDHCPv4StorageEngine _storageEngine;
        private readonly ILogger<CreateDHCPv4InterfaceListenerCommandHandler> _logger;

        public CreateDHCPv4InterfaceListenerCommandHandler(
            IDHCPv4InterfaceEngine interfaceEngine,
            IDHCPv4StorageEngine storageEngine,
            ILogger<CreateDHCPv4InterfaceListenerCommandHandler> logger)
        {
            this._interfaceEngine = interfaceEngine ?? throw new ArgumentNullException(nameof(interfaceEngine));
            this._storageEngine = storageEngine ?? throw new ArgumentNullException(nameof(storageEngine));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Guid?> Handle(CreateDHCPv4InterfaceListenerCommand request, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Handle started");

            var listener = DHCPv4Listener.Create(
            request.NicId,
            DHCPListenerName.FromString(request.Name),
            IPv4Address.FromString(request.IPv4Addres));

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
