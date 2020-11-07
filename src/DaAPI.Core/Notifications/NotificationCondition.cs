using DaAPI.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DaAPI.Core.Notifications
{
    public abstract class NotificationCondition : Value
    {
        private class NotificationTrueCondition : NotificationCondition
        {
            public override Task<Boolean> IsValid(NotifcationTrigger trigger) => Task.FromResult(true);

            public override NotificationConditionCreateModel ToCreateModel() => new NotificationConditionCreateModel
            {
                Typename = nameof(NotificationTrueCondition),
                PropertiesAndValues = new Dictionary<String, String>(),
            };

            public override bool ApplyValues(IDictionary<string, string> propertiesAndValues) => true;

        }

        public static NotificationCondition True => new NotificationTrueCondition();


        public abstract Task<Boolean> IsValid(NotifcationTrigger trigger);
        public abstract NotificationConditionCreateModel ToCreateModel();

        public abstract Boolean ApplyValues(IDictionary<String, String> propertiesAndValues);


        public static NotificationCondition Invalid = null;
    }
}
