using DaAPI.Core.Common.DHCPv6;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text;

namespace DaAPI.Shared.Validation
{
    public class TimespanGreaterThanAttribute : ValidationAttribute
    {
        private readonly string _otherPropertyName;

        public TimespanGreaterThanAttribute(String otherPropertyName)
        {
            this._otherPropertyName = otherPropertyName;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            Boolean isValid = false;

            var property = validationContext.ObjectType.GetProperty(_otherPropertyName);
            if (property != null)
            {
                TimeSpan propertyValue = (TimeSpan)property.GetValue(validationContext.ObjectInstance);

                try
                {
                    var currentAdress = (TimeSpan)value;
                    isValid = currentAdress >= propertyValue;
                }
                catch (Exception)
                {
                }
            }

            if (isValid == true)
            {
                return ValidationResult.Success;
            }

            return new ValidationResult(ErrorMessage, new[] { validationContext.MemberName });
        }

        public override string FormatErrorMessage(string name) => String.Format(CultureInfo.CurrentCulture, ErrorMessageString, name, _otherPropertyName);

    }
}
