using DaAPI.Core.Common.DHCPv6;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text;

namespace DaAPI.Shared.Validation
{
    public class IPv6GreaterThanAttribute : ValidationAttribute
    {
        private readonly string _otherPropertyName;

        public String PropertyCaption { get; set; }

        public IPv6GreaterThanAttribute(String otherPropertyName) : base($"has to be greater than {otherPropertyName}")
        {
            this._otherPropertyName = otherPropertyName;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            Boolean isValid = false;

            var property = validationContext.ObjectType.GetProperty(_otherPropertyName);
            if (property != null)
            {
                String propertyRawValue = (String)property.GetValue(validationContext.ObjectInstance);

                try
                {
                    var adressValue = IPv6Address.FromString(propertyRawValue);
                    var currentAdress = IPv6Address.FromString(value as String);
                    if (currentAdress >= adressValue)
                    {
                        isValid = true;
                    }
                }
                catch
                {
                }
            }

            if (isValid == true)
            {
                return ValidationResult.Success;
            }

            return new ValidationResult(ErrorMessage, new[] { validationContext.MemberName });
        }

        public override string FormatErrorMessage(string name) => String.Format(CultureInfo.CurrentCulture, ErrorMessageString, name, PropertyCaption ?? _otherPropertyName);

    }
}
