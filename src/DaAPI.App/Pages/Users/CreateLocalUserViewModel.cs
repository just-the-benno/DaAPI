using DaAPI.App.Resources;
using DaAPI.App.Resources.Pages.Users;
using DaAPI.Shared.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using static DaAPI.Shared.Requests.LocalUserRequests.V1;

namespace DaAPI.App.Pages.Users
{
    public class CreateLocalUserViewModel
    {
        [Required(ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.Required))]
        [MinLength(3, ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.MinLength))]
        [MaxLength(50, ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.MaxLength))]
        [Display(Name = nameof(CreateLocalUserViewModelDisplay.Username), ResourceType = typeof(CreateLocalUserViewModelDisplay))]
        public String Username { get; set; }

        [Required(ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.Required))]
        [MinLength(3, ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.MinLength))]
        [MaxLength(25, ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.MaxLength))]
        [PasswordPropertyText]
        [Display(Name = nameof(CreateLocalUserViewModelDisplay.Password), ResourceType = typeof(CreateLocalUserViewModelDisplay))]
        public String Password { get; set; }

        [Required(ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.Required))]
        [MinLength(3, ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.MinLength))]
        [MaxLength(25, ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.MaxLength))]
        [CompareProperty(nameof(Password), ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.PasswordMatch))]
        [PasswordPropertyText]
        [Display(Name = nameof(CreateLocalUserViewModelDisplay.PasswordConfirmation), ResourceType = typeof(CreateLocalUserViewModelDisplay))]
        public String PasswordConfirmation { get; set; }

        public CreateUserRequest GetRequest() => new CreateUserRequest { Password = Password, Username = Username };
    }
}
