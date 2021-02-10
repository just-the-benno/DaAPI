using DaAPI.App.Resources;
using DaAPI.App.Resources.Pages.DHCPv4Scopes;
using DaAPI.Shared.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using static DaAPI.Shared.Requests.DHCPv4ScopeRequests.V1.DHCPv4ScopeAddressPropertyReqest;

namespace DaAPI.App.Pages.DHCPv4Scopes
{
    public class DHCPv4RootScopeAddressPropertiesViewModel
    {

        [Display(Name = nameof(DHCPv4ScopeDisplay.RenewalTime), ResourceType = typeof(DHCPv4ScopeDisplay))]
        [TimeSpanMin("00.00:04:00", ErrorMessageResourceName = nameof(ValidationErrorMessages.TimeSpanMin), ErrorMessageResourceType = typeof(ValidationErrorMessages))]
        [TimeSpanMax("40.00:00:00", ErrorMessageResourceName = nameof(ValidationErrorMessages.TimeSpanMax), ErrorMessageResourceType = typeof(ValidationErrorMessages))]
        public TimeSpan RenewalTime { get; set; }

        [Display(Name = nameof(DHCPv4ScopeDisplay.PreferredLifetime), ResourceType = typeof(DHCPv4ScopeDisplay))]
        [TimeSpanMin("00.00:02:00", ErrorMessageResourceName = nameof(ValidationErrorMessages.TimeSpanMin), ErrorMessageResourceType = typeof(ValidationErrorMessages))]
        [TimeSpanMax("20.00:00:00", ErrorMessageResourceName = nameof(ValidationErrorMessages.TimeSpanMax), ErrorMessageResourceType = typeof(ValidationErrorMessages))]
        [TimespanGreaterThan(nameof(RenewalTime), ErrorMessageResourceName = nameof(ValidationErrorMessages.GreaterThan), ErrorMessageResourceType = typeof(ValidationErrorMessages))]
        public TimeSpan PreferredLifetime { get; set; }

        [Display(Name = nameof(DHCPv4ScopeDisplay.Leasetime), ResourceType = typeof(DHCPv4ScopeDisplay))]
        [TimeSpanMin("00.00:04:00", ErrorMessageResourceName = nameof(ValidationErrorMessages.TimeSpanMin), ErrorMessageResourceType = typeof(ValidationErrorMessages))]
        [TimeSpanMax("40.00:00:00", ErrorMessageResourceName = nameof(ValidationErrorMessages.TimeSpanMax), ErrorMessageResourceType = typeof(ValidationErrorMessages))]
        [TimespanGreaterThan(nameof(PreferredLifetime), ErrorMessageResourceName = nameof(ValidationErrorMessages.GreaterThan), ErrorMessageResourceType = typeof(ValidationErrorMessages))]
        public TimeSpan LeaseTime { get; set; }

        [Display(Name = nameof(DHCPv4ScopeDisplay.SupportDirectUnicast), ResourceType = typeof(DHCPv4ScopeDisplay))]
        public Boolean SupportDirectUnicast { get; set; }

        [Display(Name = nameof(DHCPv4ScopeDisplay.AcceptDecline), ResourceType = typeof(DHCPv4ScopeDisplay))]
        public Boolean AcceptDecline { get; set; }

        [Display(Name = nameof(DHCPv4ScopeDisplay.InformsAreAllowd), ResourceType = typeof(DHCPv4ScopeDisplay))]
        public Boolean InformsAreAllowd { get; set; }
        

        [Display(Name = nameof(DHCPv4ScopeDisplay.ReuseAddressIfPossible), ResourceType = typeof(DHCPv4ScopeDisplay))]
        public Boolean ReuseAddressIfPossible { get; set; }

        [Display(Name = nameof(DHCPv4ScopeDisplay.AddressAllocationStrategy), ResourceType = typeof(DHCPv4ScopeDisplay))]
        public AddressAllocationStrategies AddressAllocationStrategy { get; set; }

        [Max(32, ErrorMessageResourceName = nameof(ValidationErrorMessages.Max), ErrorMessageResourceType = typeof(ValidationErrorMessages))]
        [Min(1, ErrorMessageResourceName = nameof(ValidationErrorMessages.Min), ErrorMessageResourceType = typeof(ValidationErrorMessages))]
        [Display(Name = nameof(DHCPv4ScopeDisplay.SubnetmaskLength), ResourceType = typeof(DHCPv4ScopeDisplay))]
        public Int64 Subnetmask { get; set; }

        public DHCPv4RootScopeAddressPropertiesViewModel()
        {
        }

        public static DHCPv4RootScopeAddressPropertiesViewModel Default => new DHCPv4RootScopeAddressPropertiesViewModel
        {
            PreferredLifetime = TimeSpan.FromHours(12),
            RenewalTime = TimeSpan.FromHours(18),
            LeaseTime = TimeSpan.FromHours(24),
            SupportDirectUnicast = true,
            InformsAreAllowd = true,

            AddressAllocationStrategy = AddressAllocationStrategies.Random,
        };
    }
}
