using DaAPI.App.Resources;
using DaAPI.App.Resources.Pages.DHCPv6Scopes;
using DaAPI.Shared.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using static DaAPI.Shared.Responses.DHCPv6ScopeResponses.V1;

namespace DaAPI.App.Pages.DHCPv6Scopes
{
    public class DHCPv6PrefixDelgationViewModel
    {
        [Display(Name = nameof(DHCPv6ScopeDisplay.Prefix), ResourceType = typeof(DHCPv6ScopeDisplay))]
        [Required(ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.Required))]
        [IPv6Address(ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.IPv6Address))]
        [IsIPv6Subnet(nameof(PrefixLength), ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.IsIPv6Subnet))]
        public String Prefix { get; set; }

        [Display(Name = nameof(DHCPv6ScopeDisplay.PrefixLength), ResourceType = typeof(DHCPv6ScopeDisplay))]
        [Required(ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.Required))]
        [Max(128, ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.Max))]
        public Byte PrefixLength { get; set; }

        [Display(Name = nameof(DHCPv6ScopeDisplay.AssingedPrefixLength), ResourceType = typeof(DHCPv6ScopeDisplay))]
        [Required(ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.Required))]
        [Max(128, ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.Max))]
        [GreaterThan(nameof(PrefixLength), ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.GreaterThan))]
        public Byte AssingedPrefixLength { get; set; }

        public DHCPv6PrefixDelgationViewModel()
        {

        }

        public DHCPv6PrefixDelgationViewModel(DHCPv6PrefixDelgationInfoResponse response)
        {
            Prefix = response.Prefix;
            PrefixLength = response.PrefixLength;
            AssingedPrefixLength = response.AssingedPrefixLength;
        }
    }
}
