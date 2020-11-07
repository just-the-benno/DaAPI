using DaAPI.Infrastructure.ServiceBus.Messages;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DaAPI.Infrastructure.ServiceBus
{
    public class MediaRBasedServiceBus : IServiceBus
    {
        private readonly CustomMediator _mediator;

        public MediaRBasedServiceBus(ServiceFactory services)
        {
            _mediator = new CustomMediator(services, ParallelNoWait);
        }

        //see https://github.com/jbogard/MediatR/blob/master/samples/MediatR.Examples.PublishStrategies/Publisher.cs
        private Task ParallelNoWait(IEnumerable<Func<INotification, CancellationToken, Task>> handlers, INotification notification, CancellationToken cancellationToken)
        {
            foreach (var handler in handlers)
            {
                Task.Run(() => handler(notification, cancellationToken));
            }

            return Task.CompletedTask;
        }

        public async Task Publish(IMessage notification)
        {
            await this._mediator.Publish(notification);
        }
    }
}
