using DaAPI.App.Pages.DHCPv6Scopes;
using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Scopes.DHCPv6.ScopeProperties;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DaAPI.App.Validation
{
    public class DHCPv6ScopePropertyValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var objectInstance = (DHCPv6ScopePropertyViewModel)validationContext.ObjectInstance;

            Boolean isValid = true;

            if (objectInstance.Type == DHCPv6ScopePropertyType.AddressList && value is IEnumerable<SimpleIPv6AddressString> addresses)
            {
                foreach (var item in addresses)
                {
                    try
                    {
                        IPv6Address.FromString(item.Value);
                    }
                    catch (Exception)
                    {
                        isValid = false;
                        break;
                    }
                }

                isValid = true;
            }
            else if (objectInstance.Type == DHCPv6ScopePropertyType.Text && value is String)
            {
                isValid = true;
            }
            else if(value is Int64 numericValue1)
            {
                if (objectInstance.Type == DHCPv6ScopePropertyType.Byte == true)
                {
                    isValid = numericValue1 >= Byte.MinValue && numericValue1 <= Byte.MaxValue;
                }
                else if (objectInstance.Type == DHCPv6ScopePropertyType.UInt16 == true)
                {
                    isValid = numericValue1 >= UInt16.MinValue && numericValue1 <= UInt16.MaxValue;
                }
                else if (objectInstance.Type == DHCPv6ScopePropertyType.UInt32 == true)
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
