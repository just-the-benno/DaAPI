using DaAPI.App.Pages.DHCPScopes;
using DaAPI.App.Pages.DHCPv6Scopes;
using DaAPI.Core.Common.DHCPv6;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DaAPI.App.Validation
{
    public class ScopeResolverValuesViewModelValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var objectInstance = (DHCPScopeResolverValuesViewModel)validationContext.ObjectInstance;

            Boolean isValid = false;

            if (objectInstance.IsListValue == true && value is IEnumerable<String>)
            {
                isValid = true;
            }
            else if (objectInstance.IsTextValue == true && value is String castedValue)
            {
                switch (objectInstance.ValueType)
                {
                    case Core.Scopes.ScopeResolverPropertyDescription.ScopeResolverPropertyValueTypes.IPv6NetworkAddress:
                        try
                        {
                            var address = IPv6Address.FromString(castedValue);
                            var maskVm = objectInstance.OtherItems.FirstOrDefault(x => x.ValueType == Core.Scopes.ScopeResolverPropertyDescription.ScopeResolverPropertyValueTypes.IPv6Subnet);
                            if (maskVm != null)
                            {
                                if (Byte.TryParse(maskVm.SingleValue, out Byte maskLength) == true)
                                {
                                    if (maskLength <= 128)
                                    {
                                        IPv6SubnetMask mask = new IPv6SubnetMask(new IPv6SubnetMaskIdentifier(maskLength));
                                        isValid = mask.IsIPv6AdressANetworkAddress(address);
                                    }
                                }
                                else
                                {
                                    isValid = true;
                                }
                            }
                            else
                            {
                                isValid = true;
                            }
                        }
                        catch (Exception)
                        {
                            isValid = false;
                        }
                        break;
                    case Core.Scopes.ScopeResolverPropertyDescription.ScopeResolverPropertyValueTypes.IPv6Subnet:
                        {
                            if (Byte.TryParse(castedValue, out Byte mask) == true)
                            {
                                isValid = mask <= 128;
                            }
                            else
                            {
                                isValid = false;
                            }
                        }
                        break;
                    case Core.Scopes.ScopeResolverPropertyDescription.ScopeResolverPropertyValueTypes.IPv6Address:
                        try
                        {
                            IPv6Address.FromString(castedValue);
                            isValid = true;
                        }
                        catch (Exception)
                        {
                            isValid = false;
                        }
                        break;
                    case Core.Scopes.ScopeResolverPropertyDescription.ScopeResolverPropertyValueTypes.ByteArray:
                        isValid = Regex.IsMatch(castedValue, @"^[0-9a-fA-F]+$");
                        
                        break;
                    default:
                        isValid = true;
                        break;
                }
            }
            else if (objectInstance.IsNullableNumericValue == true && value is Int64?)
            {
                Int64? castedNumericValue = (Int64?)value;
                if (castedNumericValue.HasValue == true)
                {
                    isValid = objectInstance.ValueType switch
                    {
                        Core.Scopes.ScopeResolverPropertyDescription.ScopeResolverPropertyValueTypes.NullableUInt32 => castedNumericValue.Value >= 0 && castedNumericValue.Value <= UInt32.MaxValue,
                        _ => false,
                    };
                }
                else
                {
                    isValid = true;
                }

            }
            else if (objectInstance.IsNumericValue == true && value is Int64 castedNumericValue2)
            {

                isValid = objectInstance.ValueType switch
                {
                    Core.Scopes.ScopeResolverPropertyDescription.ScopeResolverPropertyValueTypes.UInt32 => castedNumericValue2 >= 0 && castedNumericValue2 <= UInt32.MaxValue,
                    _ => false,
                };
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
