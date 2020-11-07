using DaAPI.App.Resources;
using DaAPI.App.Resources.Pages.DHCPv6Scopes;
using DaAPI.Shared.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using static DaAPI.Shared.Requests.DHCPv6ScopeRequests.V1.DHCPv6ScopeAddressPropertyReqest;

namespace DaAPI.App.Pages.DHCPv6Scopes
{
    public class RootScopeAddressPropertiesViewModel
    {
        [Display(Name = nameof(DHCPv6ScopeDisplay.T1), ResourceType = typeof(DHCPv6ScopeDisplay))]
        [Max(0.95, ErrorMessageResourceName = nameof(ValidationErrorMessages.Max), ErrorMessageResourceType = typeof(ValidationErrorMessages))]
        [Min(0.1, ErrorMessageResourceName = nameof(ValidationErrorMessages.Min), ErrorMessageResourceType = typeof(ValidationErrorMessages))]
        public Double T1 { get; set; }

        [Display(Name = nameof(DHCPv6ScopeDisplay.T2), ResourceType = typeof(DHCPv6ScopeDisplay))]
        [Max(0.95, ErrorMessageResourceName = nameof(ValidationErrorMessages.Max), ErrorMessageResourceType = typeof(ValidationErrorMessages))]
        [Min(0.1, ErrorMessageResourceName = nameof(ValidationErrorMessages.Min), ErrorMessageResourceType = typeof(ValidationErrorMessages))]
        [GreaterThan(nameof(T1), PropertyCaption = "T1",  ErrorMessageResourceName = nameof(ValidationErrorMessages.GreaterThan), ErrorMessageResourceType = typeof(ValidationErrorMessages))]
        public Double T2 { get; set; }

        [Display(Name = nameof(DHCPv6ScopeDisplay.PreferredLifetime), ResourceType = typeof(DHCPv6ScopeDisplay))]
        [TimeSpanMin("00.00:02:00", ErrorMessageResourceName = nameof(ValidationErrorMessages.TimeSpanMin), ErrorMessageResourceType = typeof(ValidationErrorMessages))]
        [TimeSpanMax("20.00:00:00", ErrorMessageResourceName = nameof(ValidationErrorMessages.TimeSpanMax), ErrorMessageResourceType = typeof(ValidationErrorMessages))]
        public TimeSpan PreferredLifetime { get; set; }

        [Display(Name = nameof(DHCPv6ScopeDisplay.ValidLifetime), ResourceType = typeof(DHCPv6ScopeDisplay))]
        [TimeSpanMin("00.00:04:00", ErrorMessageResourceName = nameof(ValidationErrorMessages.TimeSpanMin), ErrorMessageResourceType = typeof(ValidationErrorMessages))]
        [TimeSpanMax("40.00:00:00", ErrorMessageResourceName = nameof(ValidationErrorMessages.TimeSpanMax), ErrorMessageResourceType = typeof(ValidationErrorMessages))]
        [TimespanGreaterThan(nameof(PreferredLifetime),  ErrorMessageResourceName = nameof(ValidationErrorMessages.GreaterThan), ErrorMessageResourceType = typeof(ValidationErrorMessages))]
        public TimeSpan ValidLifetime { get; set; }

        [Display(Name = nameof(DHCPv6ScopeDisplay.SupportDirectUnicast), ResourceType = typeof(DHCPv6ScopeDisplay))]
        public Boolean SupportDirectUnicast { get; set; }

        [Display(Name = nameof(DHCPv6ScopeDisplay.AcceptDecline), ResourceType = typeof(DHCPv6ScopeDisplay))]
        public Boolean AcceptDecline { get; set; }

        [Display(Name = nameof(DHCPv6ScopeDisplay.InformsAreAllowd), ResourceType = typeof(DHCPv6ScopeDisplay))]
        public Boolean InformsAreAllowd { get; set; }
        
        [Display(Name = nameof(DHCPv6ScopeDisplay.RapitCommitEnabled), ResourceType = typeof(DHCPv6ScopeDisplay))]
        public Boolean RapitCommitEnabled { get; set; }

        [Display(Name = nameof(DHCPv6ScopeDisplay.ReuseAddressIfPossible), ResourceType = typeof(DHCPv6ScopeDisplay))]
        public Boolean ReuseAddressIfPossible { get; set; }

        [Display(Name = nameof(DHCPv6ScopeDisplay.AddressAllocationStrategy), ResourceType = typeof(DHCPv6ScopeDisplay))]
        public AddressAllocationStrategies AddressAllocationStrategy { get; set; }

        public RootScopeAddressPropertiesViewModel()
        {
        }

        public static RootScopeAddressPropertiesViewModel Default => new RootScopeAddressPropertiesViewModel
        {
            PreferredLifetime = TimeSpan.FromHours(12),
            ValidLifetime = TimeSpan.FromHours(24),
            T1 = 0.5,
            T2 = 0.75,
            SupportDirectUnicast = true,
            InformsAreAllowd = true,
            RapitCommitEnabled = true,
            AddressAllocationStrategy = AddressAllocationStrategies.Random,
        };
    }
}
