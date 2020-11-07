using DaAPI.Core.Packets.DHCPv6;
using DaAPI.Infrastructure.LeaseEngines.DHCPv6;
using DaAPI.Infrastructure.ServiceBus.Messages;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DaAPI.Infrastructure.ServiceBus.MessageHandler
{
    public class ValidDHCPv6PacketArrivedMessageHandler : INotificationHandler<ValidDHCPv6PacketArrivedMessage>
    {
        private readonly IServiceBus _serviceBus;
        private readonly IDHCPv6LeaseEngine _engine;
        private readonly ILogger<ValidDHCPv6PacketArrivedMessageHandler> _logger;

        public ValidDHCPv6PacketArrivedMessageHandler(
            IServiceBus serviceBus,
            IDHCPv6LeaseEngine engine,
            ILogger<ValidDHCPv6PacketArrivedMessageHandler> logger
            )
        {
            this._serviceBus = serviceBus ?? throw new ArgumentNullException(nameof(serviceBus));
            this._engine = engine ?? throw new ArgumentNullException(nameof(engine));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Handle(ValidDHCPv6PacketArrivedMessage notification, CancellationToken cancellationToken)
        {
            _logger.LogDebug("CURRENT STEP: Handling of packet");
            _logger.LogDebug("packet: {packetType}", notification.Packet.PacketType);

            DHCPv6Packet response = await _engine.HandlePacket(notification.Packet);
            if (response == DHCPv6Packet.Empty)
            {
                _logger.LogInformation("unable to get a repsonse for packet {packetType}",notification.Packet.PacketType);
                return;
            }

            _logger.LogDebug("STEP RESULT: Packet handled and response packet generated");
            _logger.LogDebug("NEXT STEP: Interface Engine");

            await _serviceBus.Publish(new DHCPv6PacketReadyToSendMessage(response));
        }
    }
}
