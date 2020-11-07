using DaAPI.Core.Packets.DHCPv6;
using DaAPI.Infrastructure.LeaseEngines.DHCPv6;
using DaAPI.Infrastructure.ServiceBus.Messages;
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
    public class InvalidDHCPv6PacketArrivedMessageHandler : INotificationHandler<InvalidDHCPv6PacketArrivedMessage>
    {
        private readonly ILogger<InvalidDHCPv6PacketArrivedMessageHandler> _logger;
        private readonly IDHCPv6StorageEngine _storeEngine;

        public InvalidDHCPv6PacketArrivedMessageHandler(
            IDHCPv6StorageEngine storeEngine,
            ILogger<InvalidDHCPv6PacketArrivedMessageHandler> logger
            )
        {
            _storeEngine = storeEngine ?? throw new ArgumentNullException(nameof(storeEngine));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Handle(InvalidDHCPv6PacketArrivedMessage notification, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Invalid packet arrvied. Logging it into the store");
            await _storeEngine.LogInvalidDHCPv6Packet(notification.Packet);

            _logger.LogDebug("Invalid packet logged");
        }
    }
}
