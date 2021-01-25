using DaAPI.Core.Packets.DHCPv4;
using DaAPI.Core.Packets.DHCPv6;
using DaAPI.Infrastructure.LeaseEngines.DHCPv4;
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
    public class ValidDHCPv4PacketArrivedMessageHandler : INotificationHandler<ValidDHCPv4PacketArrivedMessage>
    {
        private readonly IServiceBus _serviceBus;
        private readonly IDHCPv4LeaseEngine _engine;
        private readonly ILogger<ValidDHCPv4PacketArrivedMessageHandler> _logger;

        public ValidDHCPv4PacketArrivedMessageHandler(
            IServiceBus serviceBus,
            IDHCPv4LeaseEngine engine,
            ILogger<ValidDHCPv4PacketArrivedMessageHandler> logger
            )
        {
            this._serviceBus = serviceBus ?? throw new ArgumentNullException(nameof(serviceBus));
            this._engine = engine ?? throw new ArgumentNullException(nameof(engine));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Handle(ValidDHCPv4PacketArrivedMessage notification, CancellationToken cancellationToken)
        {
            _logger.LogDebug("CURRENT STEP: Handling of packet");
            _logger.LogDebug("packet: {packetType}", notification.Packet.MessageType);

            DHCPv4Packet response = await _engine.HandlePacket(notification.Packet);
            if (response == DHCPv4Packet.Empty)
            {
                _logger.LogInformation("unable to get a repsonse for packet {packetType}",notification.Packet.MessageType);
                return;
            }

            _logger.LogDebug("STEP RESULT: Packet handled and response packet generated");
            _logger.LogDebug("NEXT STEP: Interface Engine");

            await _serviceBus.Publish(new DHCPv4PacketReadyToSendMessage(response));
        }
    }
}
