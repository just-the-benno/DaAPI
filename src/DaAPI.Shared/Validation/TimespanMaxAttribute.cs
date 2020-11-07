using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DaAPI.Shared.Validation
{
    public class TimeSpanMaxAttribute : ValidationAttribute
    {
        private readonly TimeSpan _max;
        public Boolean NullAreValid { get; set; }

        public TimeSpanMaxAttribute(String maximum)
        {
            _max = TimeSpan.Parse(maximum);
        }

        public override bool IsValid(object value)
        {
            if(NullAreValid == true && value == null) { return true; }

            if (value is TimeSpan == false) { return false; }

            return ((TimeSpan)value) <= _max;

        }
    }
}
