using DaAPI.Core.Notifications;
using DaAPI.Core.Notifications.Actors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DaAPI.Infrastructure.NotificationEngine
{
    public class ServiceProviderBasedNotificationActorFactory : INotificationActorFactory
    {
        private readonly IServiceProvider _serviceProvider;

        private static readonly Dictionary<String, Type> _typeResolver = new Dictionary<string, Type>
        {
            { nameof(NxOsStaticRouteUpdaterNotificationActor), typeof(NxOsStaticRouteUpdaterNotificationActor) },
        };

        public ServiceProviderBasedNotificationActorFactory(IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public NotificationActor Initilize(NotificationActorCreateModel actorCreateInfo)
        {
            if(_typeResolver.ContainsKey(actorCreateInfo.Typename) == false)
            {
                return NotificationActor.Invalid;
            }

            var actor = (NotificationActor)_serviceProvider.GetService(_typeResolver[actorCreateInfo.Typename]);

            Boolean applied = actor.ApplyValues(actorCreateInfo.PropertiesAndValues);
            if(applied == false)
            {
                return NotificationActor.Invalid;
            }

            return actor;
        }
    }
}
