using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text;

namespace DaAPI.Shared.Validation
{
    public class MinAttribute : ValidationAttribute
    {
        private readonly Double minimum;
        public Boolean NullAreValid { get; set; }

        public MinAttribute(Double minimum)
        {
            this.minimum = minimum;
        }

        public override bool IsValid(object value)
        {
            if (value is null == true)
            {
                if (NullAreValid == false)
                {
                    return false;
                }

                return true;
            }

            Double numericValue = Convert.ToDouble(value);

            return numericValue >= minimum;
        }

        public override string FormatErrorMessage(string name) => String.Format(CultureInfo.CurrentCulture, ErrorMessageString, name, minimum);

    }
}
