using DaAPI.Core.Packets.DHCPv6;
using DaAPI.Infrastructure.LeaseEngines.DHCPv6;
using DaAPI.Infrastructure.ServiceBus.Messages;
using DaAPI.Infrastructure.StorageEngine.DHCPv4;
using DaAPI.Infrastructure.StorageEngine.DHCPv6;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DaAPI.Infrastructure.ServiceBus.MessageHandler
{
    public class InvalidDHCPv4PacketArrivedMessageHandler : INotificationHandler<InvalidDHCPv4PacketArrivedMessage>
    {
        private readonly ILogger<InvalidDHCPv4PacketArrivedMessageHandler> _logger;
        private readonly IDHCPv4StorageEngine _storeEngine;

        public InvalidDHCPv4PacketArrivedMessageHandler(
            IDHCPv4StorageEngine storeEngine,
            ILogger<InvalidDHCPv4PacketArrivedMessageHandler> logger
            )
        {
            _storeEngine = storeEngine ?? throw new ArgumentNullException(nameof(storeEngine));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Handle(InvalidDHCPv4PacketArrivedMessage notification, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Invalid packet arrvied. Logging it into the store");
            await _storeEngine.LogInvalidDHCPv4Packet(notification.Packet);

            _logger.LogDebug("Invalid packet logged");
        }
    }
}
