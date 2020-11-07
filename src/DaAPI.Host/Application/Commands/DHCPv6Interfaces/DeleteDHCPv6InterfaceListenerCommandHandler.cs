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
    public class DeleteDHCPv6InterfaceListenerCommandHandler : IRequestHandler<DeleteDHCPv6InterfaceListenerCommand, Boolean>
    {
        private readonly IDHCPv6InterfaceEngine _interfaceEngine;
        private readonly IDHCPv6StorageEngine _storageEngine;
        private readonly ILogger<DeleteDHCPv6InterfaceListenerCommandHandler> _logger;

        public DeleteDHCPv6InterfaceListenerCommandHandler(
            IDHCPv6InterfaceEngine interfaceEngine,
            IDHCPv6StorageEngine storageEngine,
            ILogger<DeleteDHCPv6InterfaceListenerCommandHandler> logger)
        {
            this._interfaceEngine = interfaceEngine ?? throw new ArgumentNullException(nameof(interfaceEngine));
            this._storageEngine = storageEngine ?? throw new ArgumentNullException(nameof(storageEngine));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Boolean> Handle(DeleteDHCPv6InterfaceListenerCommand request, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Handle started");

            if (await _storageEngine.CheckIfAggrerootExists<DHCPv6Listener>(request.Id) == false)
            {
                return false;
            }

            DHCPv6Listener listener = await _storageEngine.GetAggregateRoot<DHCPv6Listener>(request.Id);
            listener.Delete();

            if (await _storageEngine.Save(listener) == false)
            {
                return false;
            }

             _interfaceEngine.CloseListener(listener);

            return true;
        }
    }
}
