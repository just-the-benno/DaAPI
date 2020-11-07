using DaAPI.App.Resources;
using DaAPI.App.Resources.Pages.DHCPv6Interfaces;
using DaAPI.Shared.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DaAPI.App.Pages.DHCPv6Interfaces
{
    public class CreateDHCPv6ListenerViewModel
    {

        [Required(ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.Required))]
        [MinLength(3, ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.MinLength))]
        [MaxLength(100, ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.MaxLength))]
        [Display(Name = nameof(CreateDHCPv6ListenerDisplay.Name), ResourceType = typeof(CreateDHCPv6ListenerDisplay))]
        public String Name { get; set; }

        [Required(ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.Required))]
        [IPv6Address(ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.IPv6Address))]
        [Display(Name = nameof(CreateDHCPv6ListenerDisplay.IPv6Address), ResourceType = typeof(CreateDHCPv6ListenerDisplay))]
        public String IPv6Address { get; set; }

        [Required(ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.Required))]
        [MinLength(3, ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.MinLength))]
        [MaxLength(100, ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.MaxLength))]
        [Display(Name = nameof(CreateDHCPv6ListenerDisplay.InterfaceId), ResourceType = typeof(CreateDHCPv6ListenerDisplay))]
        public String InterfaceId { get; set; }
    }
}
