using DaAPI.Infrastructure.NotificationEngine;
using DaAPI.Infrastructure.ServiceBus.Messages;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DaAPI.Infrastructure.ServiceBus.MessageHandler
{
    public class NewTriggerHappendMessageHandler : INotificationHandler<NewTriggerHappendMessage>
    {
        private readonly INotificationEngine _engine;
        private readonly ILogger<NewTriggerHappendMessageHandler> _logger;

        public NewTriggerHappendMessageHandler(INotificationEngine engine,
            ILogger<NewTriggerHappendMessageHandler> logger)
        {
            this._engine = engine;
            this._logger = logger;
        }

        public async Task Handle(NewTriggerHappendMessage notification, CancellationToken cancellationToken)
        {
            _logger.LogDebug("{amount} triggers occured. Handling them now", notification.Triggers.Count());
            foreach (var item in notification.Triggers)
            {
                _logger.LogDebug("start handling trigger {triggerType}", item.GetTypeIdentifier());
                await _engine.HandleTrigger(item);
            }
        }
    }
}
