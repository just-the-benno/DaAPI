using DaAPI.App.Resources;
using DaAPI.App.Resources.Pages.FirstSteps;
using DaAPI.Shared.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DaAPI.App.Pages.FirstSteps
{
    public class InitizeServerViewModel
    {
        [Required(ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.Required))]
        [MinLength(3, ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.MinLength))]
        [MaxLength(50, ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.MaxLength))]
        [PasswordPropertyText]
        [Display(Name = nameof(InitizeServerViewModelDisplay.Username), ResourceType = typeof(InitizeServerViewModelDisplay))]
        public String Username { get; set; }

        [Required(ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.Required))]
        [MinLength(3, ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.MinLength))]
        [MaxLength(25, ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.MaxLength))]
        [PasswordPropertyText]
        [Display(Name = nameof(InitizeServerViewModelDisplay.Password), ResourceType = typeof(InitizeServerViewModelDisplay))]
        public String Password { get; set; }

        [Required(ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.Required))]
        [MinLength(3, ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.MinLength))]
        [MaxLength(25, ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.MaxLength))]
        [CompareProperty(nameof(Password), ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.PasswordMatch))]
        [PasswordPropertyText]
        [Display(Name = nameof(InitizeServerViewModelDisplay.PasswordConfirmation), ResourceType = typeof(InitizeServerViewModelDisplay))]
        public String PasswordConfirmation { get; set; }

        public InitilizeServeRequest GetRequest() => new InitilizeServeRequest { Password = Password, UserName = Username };
    }
}
