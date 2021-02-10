using DaAPI.Core.Common;
using DaAPI.Core.Common.DHCPv6;
using System;
using System.ComponentModel.DataAnnotations;

namespace DaAPI.App.Validation
{
    public class IPv4AdressInRangeAttribute : ValidationAttribute
    {
        private readonly string _startPropertyName;
        private readonly string _endPropertyName;

        public IPv4AdressInRangeAttribute(String startPropertyName, String endPropertyName)
        {
            this._startPropertyName = startPropertyName;
            this._endPropertyName = endPropertyName;
        }

        private IPv4Address GetIPv4Address(String propertyName, ValidationContext validationContext)
        {
            var property = validationContext.ObjectType.GetProperty(propertyName);
            if (property != null)
            {
                String propertyRawValue = (String)property.GetValue(validationContext.ObjectInstance);
                try
                {
                    var adressValue = IPv4Address.FromString(propertyRawValue);
                    return adressValue;
                }
                catch
                {
                    return null;
                }
            }

            return null;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            Boolean isValid = false;

            var startAddress = GetIPv4Address(_startPropertyName, validationContext);
            var endAddress = GetIPv4Address(_endPropertyName, validationContext);

            if (endAddress != null && startAddress != null)
            {
                try
                {
                    var currentAddress = IPv4Address.FromString(value as String);
                    isValid = currentAddress.IsBetween(startAddress, endAddress);
                }
                catch (Exception)
                {
                }
            }
            
            if(isValid == true)
            {
                return ValidationResult.Success;
            }

            return new ValidationResult(ErrorMessage, new[] { validationContext.MemberName });
        }
    }
}
