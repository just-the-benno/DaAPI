using DaAPI.Infrastructure.ServiceBus.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DaAPI.Infrastructure.ServiceBus
{
    public interface IServiceBus
    {
        public Task Publish(IMessage notification);
    }
}
