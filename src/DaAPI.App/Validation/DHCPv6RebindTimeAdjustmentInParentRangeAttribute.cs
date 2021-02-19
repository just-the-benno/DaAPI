using DaAPI.App.Pages.DHCPv6Scopes;
using DaAPI.Core.Common.DHCPv6;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DaAPI.App.Validation
{
    public class DHCPv6RebindTimeAdjustmentInParentRangeAttribute : ValidationAttribute
    {
        private readonly Boolean _isT1;

        public DHCPv6RebindTimeAdjustmentInParentRangeAttribute(Boolean isT1) : base("is not in parent range")
        {
            _isT1 = isT1;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            Boolean isValid;

            var vm = (DHCPv6ChildScopeAddressPropertiesViewModel)validationContext.ObjectInstance;
            if (value == null)
            {
                isValid = true;
            }
            else
            {
                Double currentValue = (Double)value;
                if(_isT1 == true)
                {
                    Double upperValue = vm.T2.HasValue == true ? vm.T2.Value : vm.Properties.T2.Value;
                    isValid = currentValue < upperValue;
                }
                else
                {
                    Double lowerValue = vm.T1.HasValue == true ? vm.T1.Value : vm.Properties.T1.Value;
                    isValid = currentValue > lowerValue;
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
