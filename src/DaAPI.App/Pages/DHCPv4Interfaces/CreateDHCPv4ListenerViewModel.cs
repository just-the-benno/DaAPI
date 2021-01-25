using DaAPI.App.Resources;
using DaAPI.App.Resources.Pages.DHCPv4Interfaces;
using DaAPI.Shared.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DaAPI.App.Pages.DHCPv6Interfaces
{
    public class CreateDHCPv4ListenerViewModel
    {
        [Required(ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.Required))]
        [MinLength(3, ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.MinLength))]
        [MaxLength(100, ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.MaxLength))]
        [Display(Name = nameof(CreateDHCPv4ListenerDisplay.Name), ResourceType = typeof(CreateDHCPv4ListenerDisplay))]
        public String Name { get; set; }

        [Required(ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.Required))]
        [IPv4Address(ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.IPv4Address))]
        [Display(Name = nameof(CreateDHCPv4ListenerDisplay.IPv4Address), ResourceType = typeof(CreateDHCPv4ListenerDisplay))]
        public String IPv4Address { get; set; }

        [Required(ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.Required))]
        [MinLength(3, ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.MinLength))]
        [MaxLength(100, ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.MaxLength))]
        [Display(Name = nameof(CreateDHCPv4ListenerDisplay.InterfaceId), ResourceType = typeof(CreateDHCPv4ListenerDisplay))]
        public String InterfaceId { get; set; }
    }
}
