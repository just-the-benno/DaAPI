using DaAPI.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DaAPI.Core.Notifications
{
    public abstract class NotificationActor : Value
    {
        public static NotificationActor Invalid => null;

        internal protected abstract Task<Boolean> Handle(NotifcationTrigger trigger);

        public abstract NotificationActorCreateModel ToCreateModel();

        protected String GetQuotedString(String value) => $"\"{value}\"";
        protected String GetValueWithoutQuota(String value) => value.StartsWith('\"') == false ? value : value[1..^1];

        public abstract Boolean ApplyValues(IDictionary<String, String> propertiesAndValues);
    }
}
