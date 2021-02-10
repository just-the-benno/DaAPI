using DaAPI.App.Pages.DHCPv4Scopes;
using DaAPI.App.Pages.DHCPv6Scopes;
using DaAPI.Core.Common;
using DaAPI.Core.Scopes.DHCPv4;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DaAPI.App.Validation
{
    public class DHCPv4ScopePropertyValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var objectInstance = (DHCPv4ScopePropertyViewModel)validationContext.ObjectInstance;

            Boolean isValid = true;

            if (objectInstance.Type == DHCPv4ScopePropertyType.AddressList && value is IEnumerable<SimpleIPv4AddressString> addresses)
            {
                foreach (var item in addresses)
                {
                    try
                    {
                        IPv4Address.FromString(item.Value);
                    }
                    catch (Exception)
                    {
                        isValid = false;
                        break;
                    }
                }

                isValid = true;
            }
            if (objectInstance.Type == DHCPv4ScopePropertyType.Address && value is SimpleIPv4AddressString address)
            {
                try
                {
                    IPv4Address.FromString(address.Value);
                    isValid = true;
                }
                catch (Exception)
                {
                    isValid = false;
                }
            }
            else if (objectInstance.Type == DHCPv4ScopePropertyType.Text && value is String)
            {
                isValid = true;
            }
            else if (objectInstance.Type == DHCPv4ScopePropertyType.Boolean && value is Boolean)
            {
                isValid = true;
            }
            else if (objectInstance.Type == DHCPv4ScopePropertyType.Time && value is TimeSpan timespan)
            {
                isValid = timespan.TotalSeconds < UInt32.MaxValue;
            }
            else if (value is Int64 numericValue1)
            {
                if (objectInstance.Type == DHCPv4ScopePropertyType.Byte == true)
                {
                    isValid = numericValue1 >= Byte.MinValue && numericValue1 <= Byte.MaxValue;
                }
                else if (objectInstance.Type == DHCPv4ScopePropertyType.UInt16 == true)
                {
                    isValid = numericValue1 >= UInt16.MinValue && numericValue1 <= UInt16.MaxValue;
                }
                else if (objectInstance.Type == DHCPv4ScopePropertyType.UInt32 == true)
                {
                    isValid = numericValue1 >= UInt32.MinValue && numericValue1 <= UInt32.MaxValue;
                }
            }
            else
            {
                isValid = true;
            }

            if (isValid == true)
            {
                return ValidationResult.Success;
            }
            else
            {
                return new ValidationResult(ErrorMessage, new[] { validationContext.MemberName });
            }
        }
    }
}
