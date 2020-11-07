using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text;

namespace DaAPI.Shared.Validation
{
    public class MaxAttribute : ValidationAttribute
    {
        private readonly Double maximum;

        public Boolean NullAreValid { get; set; } = false;

        public MaxAttribute(Double maximum)
        {
            this.maximum = maximum;
        }

        public override bool IsValid(object value)
        {
            if(value is null == true)
            {
                if(NullAreValid == false)
                {
                    return false;
                }

                return true;
            }

            Double numericValue = Convert.ToDouble(value);

            return numericValue <= maximum;
        }

        public override string FormatErrorMessage(string name) => String.Format(CultureInfo.CurrentCulture, ErrorMessageString, name, maximum);
    }
}
