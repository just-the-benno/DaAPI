using DaAPI.Infrastructure.FilterEngines.DHCPv6;
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
    public class DHCPv6PacketArrivedMessageHandler : INotificationHandler<DHCPv6PacketArrivedMessage>
    {
        private readonly IServiceBus _serviceBus;
        private readonly IDHCPv6PacketFilterEngine _engine;
        private readonly ILogger<DHCPv6PacketArrivedMessageHandler> _logger;

        public DHCPv6PacketArrivedMessageHandler(
            IServiceBus serviceBus,
            IDHCPv6PacketFilterEngine engine,
            ILogger<DHCPv6PacketArrivedMessageHandler> logger
            )
        {
            this._serviceBus = serviceBus ?? throw new ArgumentNullException(nameof(serviceBus));
            this._engine = engine ?? throw new ArgumentNullException(nameof(engine));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Handle(DHCPv6PacketArrivedMessage notification, CancellationToken cancellationToken)
        {
            _logger.LogDebug("CURRENT STEP: Check if packet needs to be filtered");
            _logger.LogDebug("packet: {packetType}", notification.Packet.PacketType);
            var filterResult = await _engine.ShouldPacketBeFilterd(notification.Packet);

            if (filterResult.Item1 == true)
            {
                _logger.LogInformation("dhcp packet is filtered");
                _logger.LogDebug("STEP RESULT: Packet is invalid");
                _logger.LogDebug("NEXT STEP: drop the packet");

                await _serviceBus.Publish(new DHCPv6PacketFileteredMessage(notification.Packet, filterResult.Item2));
            }
            else
            {
                _logger.LogDebug("STEP RESULT: Packet can pass in the next step");
                _logger.LogDebug("NEXT STEP: Lease engine");

                await _serviceBus.Publish(new ValidDHCPv6PacketArrivedMessage(notification.Packet));
            }
        }
    }
}
