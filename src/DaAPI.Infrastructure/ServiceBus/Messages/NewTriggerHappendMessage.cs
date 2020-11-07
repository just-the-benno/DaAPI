using DaAPI.Core.Notifications;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Infrastructure.ServiceBus.Messages
{
    public class NewTriggerHappendMessage : IMessage
    {
        public IEnumerable<NotifcationTrigger> Triggers { get; }

        public NewTriggerHappendMessage(IEnumerable<NotifcationTrigger> triggers)
        {
            Triggers = triggers;
        }
    }
}
