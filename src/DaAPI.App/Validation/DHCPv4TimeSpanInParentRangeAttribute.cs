using DaAPI.App.Pages.DHCPv4Scopes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DaAPI.App.Validation
{
    public class DHCPv4TimeSpanInParentRangeAttribute : ValidationAttribute
    {
        public enum TimeTypes
        {
            PreferredLifetime,
            LeaseTime,
            RenewalTime
        }

        private readonly TimeTypes _type;

        public DHCPv4TimeSpanInParentRangeAttribute(TimeTypes type) : base("is not in parent range")
        {
            _type = type;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            Boolean isValid;

            var vm = (DHCPv4ChildScopeAddressPropertiesViewModel)validationContext.ObjectInstance;
            if (value == null)
            {
                isValid = true;
            }
            else
            {
                TimeSpan currentValue = (TimeSpan)value;
                TimeSpan renewalTime = vm.RenewalTime.HasValue == true ? vm.RenewalTime.Value : vm.Properties.RenewalTime.Value;
                TimeSpan preferredLifetime = vm.PreferredLifetime.HasValue == true ? vm.PreferredLifetime.Value : vm.Properties.RenewalTime.Value;
                TimeSpan leaseTime = vm.LeaseTime.HasValue == true ? vm.LeaseTime.Value : vm.Properties.LeaseTime.Value;

                switch (_type)
                {
                    case TimeTypes.RenewalTime:
                        isValid = currentValue < preferredLifetime;
                        break;
                    case TimeTypes.PreferredLifetime:
                        isValid = currentValue > renewalTime && currentValue < leaseTime;
                        break;
                    case TimeTypes.LeaseTime:
                        isValid = currentValue > preferredLifetime;
                        break;
                    default:
                        isValid = false;
                        break;
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
