using DaAPI.App.Pages.DHCPv4Scopes;
using DaAPI.Core.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DaAPI.App.Validation
{
    public class IPv4AddressInParentRangeAttribute : ValidationAttribute
    {

        public IPv4AddressInParentRangeAttribute() : base("is not in parent range")
        {
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            bool isValid;
            var vm = (CreateDHCPv4ScopeViewModel)validationContext.ObjectInstance;
            if (vm.ChildAddressProperties == null)
            {
                isValid = true;
            }
            else
            {
                try
                {

                    var addressValue = IPv4Address.FromString(value as String);

                    isValid = addressValue.IsBetween(
                        IPv4Address.FromString(vm.ChildAddressProperties.Properties.Start),
                        IPv4Address.FromString(vm.ChildAddressProperties.Properties.End)
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
