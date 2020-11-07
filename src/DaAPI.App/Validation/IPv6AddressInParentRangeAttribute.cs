using DaAPI.App.Pages.DHCPv6Scopes;
using DaAPI.Core.Common.DHCPv6;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DaAPI.App.Validation
{
    public class IPv6AddressInParentRangeAttribute : ValidationAttribute
    {

        public IPv6AddressInParentRangeAttribute() : base("is not in parent range")
        {
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            bool isValid;
            var vm = (CreateDHCPv6ScopeViewModel)validationContext.ObjectInstance;
            if (vm.ChildAddressProperties == null)
            {
                isValid = true;
            }
            else
            {
                try
                {

                    var addressValue = IPv6Address.FromString(value as String);

                    isValid = addressValue.IsBetween(
                        IPv6Address.FromString(vm.ChildAddressProperties.Properties.Start),
                        IPv6Address.FromString(vm.ChildAddressProperties.Properties.End)
                        );
                }
                catch
                {
                    return null;
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
