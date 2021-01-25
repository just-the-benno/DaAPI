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
    public class DHCPv4PacketFileteredMessageHandler : INotificationHandler<DHCPv4PacketFilteredMessage>
    {
        private readonly ILogger<DHCPv4PacketFileteredMessageHandler> _logger;
        private readonly IDHCPv4StorageEngine _storeEngine;

        public DHCPv4PacketFileteredMessageHandler(
            IDHCPv4StorageEngine storeEngine,
            ILogger<DHCPv4PacketFileteredMessageHandler> logger
            )
        {
            _storeEngine = storeEngine ?? throw new ArgumentNullException(nameof(storeEngine));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Handle(DHCPv4PacketFilteredMessage notification, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Invalid packet arrvied. Logging it into the store");
            await _storeEngine.LogFilteredDHCPv4Packet(notification.Packet, notification.FilterName);

            _logger.LogDebug("Invalid packet logged");
        }
    }
}
