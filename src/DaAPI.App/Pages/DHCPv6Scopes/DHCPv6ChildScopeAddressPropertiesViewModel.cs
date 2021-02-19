using DaAPI.App.Resources;
using DaAPI.App.Resources.Pages.DHCPv6Scopes;
using DaAPI.App.Validation;
using DaAPI.Shared.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using static DaAPI.Shared.Requests.DHCPv6ScopeRequests.V1.DHCPv6ScopeAddressPropertyReqest;
using static DaAPI.Shared.Responses.DHCPv6ScopeResponses.V1;

namespace DaAPI.App.Pages.DHCPv6Scopes
{
    public class DHCPv6ChildScopeAddressPropertiesViewModel
    {
        public DHCPv6ScopeAddressPropertiesResponse Properties { get; private set; }

        [Max(0.95, NullAreValid = true, ErrorMessageResourceName = nameof(ValidationErrorMessages.Max), ErrorMessageResourceType = typeof(ValidationErrorMessages))]
        [Min(0.1, NullAreValid = true, ErrorMessageResourceName = nameof(ValidationErrorMessages.Min), ErrorMessageResourceType = typeof(ValidationErrorMessages))]
        [DHCPv6RebindTimeAdjustmentInParentRange(true, ErrorMessageResourceName = nameof(ValidationErrorMessages.RebindTimeAdjustmentInParentRange), ErrorMessageResourceType = typeof(ValidationErrorMessages))]
        [Display(Name = nameof(DHCPv6ScopeDisplay.T1), ResourceType = typeof(DHCPv6ScopeDisplay))]
        public Double? T1 { get; set; }

        [Max(0.95, NullAreValid = true, ErrorMessageResourceName = nameof(ValidationErrorMessages.Max), ErrorMessageResourceType = typeof(ValidationErrorMessages))]
        [Min(0.1, NullAreValid = true, ErrorMessageResourceName = nameof(ValidationErrorMessages.Min), ErrorMessageResourceType = typeof(ValidationErrorMessages))]
        [DHCPv6RebindTimeAdjustmentInParentRange(false, ErrorMessageResourceName = nameof(ValidationErrorMessages.RebindTimeAdjustmentInParentRange), ErrorMessageResourceType = typeof(ValidationErrorMessages))]
        [Display(Name = nameof(DHCPv6ScopeDisplay.T2), ResourceType = typeof(DHCPv6ScopeDisplay))]
        public Double? T2 { get; set; }

        [TimeSpanMin("00.00:02:00", NullAreValid = true, ErrorMessageResourceName = nameof(ValidationErrorMessages.TimeSpanMin), ErrorMessageResourceType = typeof(ValidationErrorMessages))]
        [TimeSpanMax("20.00:00:00", NullAreValid = true, ErrorMessageResourceName = nameof(ValidationErrorMessages.TimeSpanMax), ErrorMessageResourceType = typeof(ValidationErrorMessages))]
        [DHCPv6TimeSpanInParentRange(true, ErrorMessageResourceName = nameof(ValidationErrorMessages.TimeSpanInParentRange), ErrorMessageResourceType = typeof(ValidationErrorMessages))]
        [Display(Name = nameof(DHCPv6ScopeDisplay.PreferredLifetime), ResourceType = typeof(DHCPv6ScopeDisplay))]
        public TimeSpan? PreferredLifetime { get; set; }

        [TimeSpanMin("00.00:04:00", NullAreValid = true, ErrorMessageResourceName = nameof(ValidationErrorMessages.TimeSpanMin), ErrorMessageResourceType = typeof(ValidationErrorMessages))]
        [TimeSpanMax("40.00:00:00", NullAreValid = true, ErrorMessageResourceName = nameof(ValidationErrorMessages.TimeSpanMax), ErrorMessageResourceType = typeof(ValidationErrorMessages))]
        [DHCPv6TimeSpanInParentRange(false, ErrorMessageResourceName = nameof(ValidationErrorMessages.TimeSpanInParentRange), ErrorMessageResourceType = typeof(ValidationErrorMessages))]
        [Display(Name = nameof(DHCPv6ScopeDisplay.ValidLifetime), ResourceType = typeof(DHCPv6ScopeDisplay))]
        public TimeSpan? ValidLifetime { get; set; }

        [Display(Name = nameof(DHCPv6ScopeDisplay.SupportDirectUnicast), ResourceType = typeof(DHCPv6ScopeDisplay))]
        public Boolean? SupportDirectUnicast { get; set; }

        [Display(Name = nameof(DHCPv6ScopeDisplay.AcceptDecline), ResourceType = typeof(DHCPv6ScopeDisplay))]
        public Boolean? AcceptDecline { get; set; }

        [Display(Name = nameof(DHCPv6ScopeDisplay.InformsAreAllowd), ResourceType = typeof(DHCPv6ScopeDisplay))]
        public Boolean? InformsAreAllowd { get; set; }

        [Display(Name = nameof(DHCPv6ScopeDisplay.RapitCommitEnabled), ResourceType = typeof(DHCPv6ScopeDisplay))]
        public Boolean? RapitCommitEnabled { get; set; }

        [Display(Name = nameof(DHCPv6ScopeDisplay.ReuseAddressIfPossible), ResourceType = typeof(DHCPv6ScopeDisplay))]
        public Boolean? ReuseAddressIfPossible { get; set; }

        [Display(Name = nameof(DHCPv6ScopeDisplay.AddressAllocationStrategy), ResourceType = typeof(DHCPv6ScopeDisplay))]
        public AddressAllocationStrategies? AddressAllocationStrategy { get; set; }

        public void AddParentProperties(DHCPv6ScopeAddressPropertiesResponse parentProperties) => Properties = parentProperties;
    }
}
