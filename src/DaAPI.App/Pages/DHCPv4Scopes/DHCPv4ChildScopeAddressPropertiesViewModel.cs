using DaAPI.App.Resources;
using DaAPI.App.Resources.Pages.DHCPv4Scopes;
using DaAPI.App.Resources.Pages.DHCPv6Scopes;
using DaAPI.App.Validation;
using DaAPI.Shared.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using static DaAPI.Shared.Requests.DHCPv4ScopeRequests.V1.DHCPv4ScopeAddressPropertyReqest;
using static DaAPI.Shared.Responses.DHCPv4ScopeResponses.V1;

namespace DaAPI.App.Pages.DHCPv4Scopes
{
    public class DHCPv4ChildScopeAddressPropertiesViewModel
    {
        public DHCPv4ScopeAddressPropertiesResponse Properties { get; private set; }

        
        [TimeSpanMin("00.00:02:00", NullAreValid = true, ErrorMessageResourceName = nameof(ValidationErrorMessages.TimeSpanMin), ErrorMessageResourceType = typeof(ValidationErrorMessages))]
        [TimeSpanMax("20.00:00:00", NullAreValid = true, ErrorMessageResourceName = nameof(ValidationErrorMessages.TimeSpanMax), ErrorMessageResourceType = typeof(ValidationErrorMessages))]
        [DHCPv4TimeSpanInParentRange(DHCPv4TimeSpanInParentRangeAttribute.TimeTypes.RenewalTime, ErrorMessageResourceName = nameof(ValidationErrorMessages.TimeSpanInParentRange), ErrorMessageResourceType = typeof(ValidationErrorMessages))]
        [Display(Name = nameof(DHCPv4ScopeDisplay.RenewalTime), ResourceType = typeof(DHCPv4ScopeDisplay))]
        public TimeSpan? RenewalTime { get; set; }

        [TimeSpanMin("00.00:03:00", NullAreValid = true, ErrorMessageResourceName = nameof(ValidationErrorMessages.TimeSpanMin), ErrorMessageResourceType = typeof(ValidationErrorMessages))]
        [TimeSpanMax("20.00:00:00", NullAreValid = true, ErrorMessageResourceName = nameof(ValidationErrorMessages.TimeSpanMax), ErrorMessageResourceType = typeof(ValidationErrorMessages))]
        [DHCPv4TimeSpanInParentRange(DHCPv4TimeSpanInParentRangeAttribute.TimeTypes.PreferredLifetime, ErrorMessageResourceName = nameof(ValidationErrorMessages.TimeSpanInParentRange), ErrorMessageResourceType = typeof(ValidationErrorMessages))]
        [Display(Name = nameof(DHCPv4ScopeDisplay.PreferredLifetime), ResourceType = typeof(DHCPv4ScopeDisplay))]
        public TimeSpan? PreferredLifetime { get; set; }

        [TimeSpanMin("00.00:04:00", NullAreValid = true, ErrorMessageResourceName = nameof(ValidationErrorMessages.TimeSpanMin), ErrorMessageResourceType = typeof(ValidationErrorMessages))]
        [TimeSpanMax("40.00:00:00", NullAreValid = true, ErrorMessageResourceName = nameof(ValidationErrorMessages.TimeSpanMax), ErrorMessageResourceType = typeof(ValidationErrorMessages))]
        [DHCPv4TimeSpanInParentRange(DHCPv4TimeSpanInParentRangeAttribute.TimeTypes.LeaseTime, ErrorMessageResourceName = nameof(ValidationErrorMessages.TimeSpanInParentRange), ErrorMessageResourceType = typeof(ValidationErrorMessages))]
        [Display(Name = nameof(DHCPv4ScopeDisplay.Leasetime), ResourceType = typeof(DHCPv4ScopeDisplay))]
        public TimeSpan? LeaseTime { get; set; }

        [Display(Name = nameof(DHCPv4ScopeDisplay.SupportDirectUnicast), ResourceType = typeof(DHCPv4ScopeDisplay))]
        public Boolean? SupportDirectUnicast { get; set; }

        [Display(Name = nameof(DHCPv4ScopeDisplay.AcceptDecline), ResourceType = typeof(DHCPv4ScopeDisplay))]
        public Boolean? AcceptDecline { get; set; }

        [Display(Name = nameof(DHCPv4ScopeDisplay.InformsAreAllowd), ResourceType = typeof(DHCPv4ScopeDisplay))]
        public Boolean? InformsAreAllowd { get; set; }

        [Display(Name = nameof(DHCPv4ScopeDisplay.ReuseAddressIfPossible), ResourceType = typeof(DHCPv4ScopeDisplay))]
        public Boolean? ReuseAddressIfPossible { get; set; }

        [Display(Name = nameof(DHCPv4ScopeDisplay.AddressAllocationStrategy), ResourceType = typeof(DHCPv4ScopeDisplay))]
        public AddressAllocationStrategies? AddressAllocationStrategy { get; set; }

        [Max(32, ErrorMessageResourceName = nameof(ValidationErrorMessages.Max), ErrorMessageResourceType = typeof(ValidationErrorMessages))]
        [Min(1, ErrorMessageResourceName = nameof(ValidationErrorMessages.Min), ErrorMessageResourceType = typeof(ValidationErrorMessages))]
        [Display(Name = nameof(DHCPv4ScopeDisplay.SubnetmaskLength), ResourceType = typeof(DHCPv4ScopeDisplay))]
        public Int64? Subnetmask { get; set; }

        public void AddParentProperties(DHCPv4ScopeAddressPropertiesResponse parentProperties) => Properties = parentProperties;
    }
}
