using DaAPI.App.Resources;
using DaAPI.App.Resources.Pages.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using static DaAPI.Shared.Requests.LocalUserRequests.V1;

namespace DaAPI.App.Pages.Users
{
    public class ChangeLocalUserPasswordViewModel
    {
        [Required(ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.Required))]
        [MinLength(3, ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.MinLength))]
        [MaxLength(25, ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.MaxLength))]
        [PasswordPropertyText]
        [Display(Name = nameof(ChangeLocalUserPasswordViewModelDisplay.Password), ResourceType = typeof(ChangeLocalUserPasswordViewModelDisplay))]
        public String Password { get; set; }

        [Required(ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.Required))]
        [MinLength(3, ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.MinLength))]
        [MaxLength(25, ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.MaxLength))]
        [CompareProperty(nameof(Password), ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.PasswordMatch))]
        [PasswordPropertyText]
        [Display(Name = nameof(ChangeLocalUserPasswordViewModelDisplay.PasswordConfirmation), ResourceType = typeof(ChangeLocalUserPasswordViewModelDisplay))]
        public String PasswordConfirmation { get; set; }

        public ChangeLocalUserPasswordViewModel()
        {

        }

        public ResetPasswordRequest GetRequest() => new ResetPasswordRequest
        {
            Password = Password
        };
    }
}
