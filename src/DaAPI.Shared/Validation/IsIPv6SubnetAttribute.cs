using DaAPI.Core.Common.DHCPv6;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DaAPI.Shared.Validation
{
    public class IsIPv6SubnetAttribute : ValidationAttribute
    {
        private readonly string _otherPropertyName;

        public IsIPv6SubnetAttribute(String otherPropertyName) : base($"is not a valid subnet address")
        {
            this._otherPropertyName = otherPropertyName;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            Boolean isValid = false;

            var property = validationContext.ObjectType.GetProperty(_otherPropertyName);
            if (property != null)
            {
                Byte subnetLength = (Byte)property.GetValue(validationContext.ObjectInstance);

                try
                {
                    var address = IPv6Address.FromString(value as String);
                    IPv6SubnetMask mask = new IPv6SubnetMask(new IPv6SubnetMaskIdentifier(subnetLength));
                    isValid = mask.IsIPv6AdressANetworkAddress(address);
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
    }
}

