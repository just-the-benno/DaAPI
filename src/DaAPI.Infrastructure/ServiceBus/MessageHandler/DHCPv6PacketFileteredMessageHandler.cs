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
    public class DHCPv6PacketFileteredMessageHandler : INotificationHandler<DHCPv6PacketFileteredMessage>
    {
        private readonly ILogger<DHCPv6PacketFileteredMessageHandler> _logger;
        private readonly IDHCPv6StorageEngine _storeEngine;

        public DHCPv6PacketFileteredMessageHandler(
            IDHCPv6StorageEngine storeEngine,
            ILogger<DHCPv6PacketFileteredMessageHandler> logger
            )
        {
            _storeEngine = storeEngine ?? throw new ArgumentNullException(nameof(storeEngine));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Handle(DHCPv6PacketFileteredMessage notification, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Invalid packet arrvied. Logging it into the store");
            await _storeEngine.LogFilteredDHCPv6Packet(notification.Packet, notification.FilterName);

            _logger.LogDebug("Invalid packet logged");
        }
    }
}
