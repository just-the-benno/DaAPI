using DaAPI.App.Pages.DHCPv6Scopes;
using DaAPI.Core.Common.DHCPv6;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DaAPI.App.Validation
{
    public class TimeSpanInParentRangeAttribute : ValidationAttribute
    {
        private readonly Boolean _isPreferredLifetime;

        public TimeSpanInParentRangeAttribute(Boolean isPreferredLifetime) : base("is not in parent range")
        {
            _isPreferredLifetime = isPreferredLifetime;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            Boolean isValid;

            var vm = (ChildScopeAddressPropertiesViewModel)validationContext.ObjectInstance;
            if (value == null)
            {
                isValid = true;
            }
            else
            {
                TimeSpan currentValue = (TimeSpan)value;
                if(_isPreferredLifetime == true)
                {
                    TimeSpan upperValue = vm.ValidLifetime.HasValue == true ? vm.ValidLifetime.Value : vm.Properties.ValidLifetime.Value;
                    isValid = currentValue < upperValue;
                }
                else
                {
                    TimeSpan lowerValue = vm.PreferredLifetime.HasValue == true ? vm.PreferredLifetime.Value : vm.Properties.PreferedLifetime.Value;
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
