using Humanizer;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text;

namespace DaAPI.Shared.Validation
{
    public class TimeSpanMinAttribute : ValidationAttribute
    {
        private readonly TimeSpan _min;
        public Boolean NullAreValid { get; set; }

        public TimeSpanMinAttribute(String minimum)
        {
            _min = TimeSpan.Parse(minimum);
        }

        public override bool IsValid(object value)
        {
            if (NullAreValid == true && value == null) { return true; }

            if (value is TimeSpan == false) { return false; }

            return ((TimeSpan)value) >= _min;

        }

        public override string FormatErrorMessage(string name) => String.Format(CultureInfo.CurrentCulture, ErrorMessageString, name, _min.Humanize(1));
    }
}
