using DaAPI.Core.Notifications;
using DaAPI.Core.Notifications.Actors;
using DaAPI.Core.Notifications.Conditions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DaAPI.Infrastructure.NotificationEngine
{
    public class ServiceProviderBasedNotificationConditionFactory : INotificationConditionFactory
    {
        private readonly IServiceProvider _serviceProvider;

        private static readonly Dictionary<String, Type> _typeResolver = new Dictionary<string, Type>
        {
            { nameof(DHCPv6ScopeIdNotificationCondition), typeof(DHCPv6ScopeIdNotificationCondition) },
        };

        public ServiceProviderBasedNotificationConditionFactory(IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public NotificationCondition Initilize(NotificationConditionCreateModel actorCreateInfo)
        {
            if(_typeResolver.ContainsKey(actorCreateInfo.Typename) == false)
            {
                return NotificationCondition.Invalid;
            }

            var condition = (NotificationCondition)_serviceProvider.GetService(_typeResolver[actorCreateInfo.Typename]);

            Boolean applied = condition.ApplyValues(actorCreateInfo.PropertiesAndValues);
            if(applied == false)
            {
                return NotificationCondition.Invalid;
            }

            return condition;
        }
    }
}
