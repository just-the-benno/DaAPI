using DaAPI.Core.Common.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Notifications
{
    public class NotificationPipelineName : LengthConstraintedStringValue<NotificationPipelineName>
    {
        private const Int32 _min = 3;
        private const Int32 _max = 30;

        internal NotificationPipelineName(String name) : base(name)
        {

        }

        public static NotificationPipelineName FromString(String input)
        {
            EnforeMinAndMaxLength(input, _min, _max);
            return new NotificationPipelineName(input);
        }

    }
}
