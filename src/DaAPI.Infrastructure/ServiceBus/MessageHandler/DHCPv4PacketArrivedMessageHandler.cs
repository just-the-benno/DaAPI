using DaAPI.Infrastructure.FilterEngines.DHCPv4;
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
    public class DHCPv4PacketArrivedMessageHandler : INotificationHandler<DHCPv4PacketArrivedMessage>
    {
        private readonly IServiceBus _serviceBus;
        private readonly IDHCPv4PacketFilterEngine _engine;
        private readonly ILogger<DHCPv4PacketArrivedMessageHandler> _logger;

        public DHCPv4PacketArrivedMessageHandler(
            IServiceBus serviceBus,
            IDHCPv4PacketFilterEngine engine,
            ILogger<DHCPv4PacketArrivedMessageHandler> logger
            )
        {
            this._serviceBus = serviceBus ?? throw new ArgumentNullException(nameof(serviceBus));
            this._engine = engine ?? throw new ArgumentNullException(nameof(engine));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Handle(DHCPv4PacketArrivedMessage notification, CancellationToken cancellationToken)
        {
            (Boolean result, String matchedFilter) = await _engine.ShouldPacketBeFilterd(notification.Packet);

            if (result == true)
            {
                await _serviceBus.Publish(new DHCPv4PacketFilteredMessage(notification.Packet, matchedFilter));
            }
            else
            {
                await _serviceBus.Publish(new ValidDHCPv4PacketArrivedMessage(notification.Packet));
            }
        }
    }
}
