using DaAPI.Core.Common.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Notifications
{
    public class NotificationPipelineDescription : LengthConstraintedStringValue<NotificationPipelineName>
    {
        private const Int32 _min = 3;
        private const Int32 _max = 500;

        internal NotificationPipelineDescription(String name) : base(name)
        {

        }

        public static NotificationPipelineDescription FromString(String input)
        {
            EnforeMinAndMaxLength(input, _min, _max);
            return new NotificationPipelineDescription(input);
        }

        public static NotificationPipelineDescription Empty => new NotificationPipelineDescription(String.Empty);

    }
}
