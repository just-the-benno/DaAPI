using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Listeners;
using DaAPI.Infrastructure.InterfaceEngines;
using DaAPI.Infrastructure.StorageEngine.DHCPv4;
using DaAPI.Infrastructure.StorageEngine.DHCPv6;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DaAPI.Host.Application.Commands.DHCPv4Interfaces
{
    public class DeleteDHCPv4InterfaceListenerCommandHandler : IRequestHandler<DeleteDHCPv4InterfaceListenerCommand, Boolean>
    {
        private readonly IDHCPv4InterfaceEngine _interfaceEngine;
        private readonly IDHCPv4StorageEngine _storageEngine;
        private readonly ILogger<DeleteDHCPv4InterfaceListenerCommandHandler> _logger;

        public DeleteDHCPv4InterfaceListenerCommandHandler(
            IDHCPv4InterfaceEngine interfaceEngine,
            IDHCPv4StorageEngine storageEngine,
            ILogger<DeleteDHCPv4InterfaceListenerCommandHandler> logger)
        {
            this._interfaceEngine = interfaceEngine ?? throw new ArgumentNullException(nameof(interfaceEngine));
            this._storageEngine = storageEngine ?? throw new ArgumentNullException(nameof(storageEngine));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Boolean> Handle(DeleteDHCPv4InterfaceListenerCommand request, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Handle started");

            if (await _storageEngine.CheckIfAggrerootExists<DHCPv4Listener>(request.Id) == false)
            {
                return false;
            }

            var listener = await _storageEngine.GetAggregateRoot<DHCPv4Listener>(request.Id);
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
