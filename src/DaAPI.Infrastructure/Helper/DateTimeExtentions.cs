using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Infrastructure.Helper
{
    public static class DateTimeExtentions
    {
        public static DateTime GetFirstWeekDay(this DateTime input)
        {
            Int32 diff = (Int32)input.DayOfWeek - 1;
            if (diff < 0)
            {
                diff = 6;
            }

            return input.Date.AddDays(-diff);
        }
    }
}
