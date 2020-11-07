using DaAPI.App.Pages.Notifications;
using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Scopes.DHCPv6.ScopeProperties;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using static DaAPI.Shared.Responses.NotificationPipelineResponses.V1;

namespace DaAPI.App.Validation
{
    public class NotificationPipelineActorPropertyAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var objectInstance = (NotificationPipelineActorPropertyEntry)validationContext.ObjectInstance;

            bool isValid;
            if (objectInstance.Type == NotifcationActorDescription.ActorPropertyTypes.Endpoint)
            {
                String castedValue = (String)value;
                try
                {
                    var uri = new Uri(castedValue, UriKind.Absolute);
                    isValid = uri.Scheme == "http" || uri.Scheme == "https";
                }
                catch (Exception)
                {
                    isValid = false;
                }
            }
            else
            {
                isValid = true;
            }

            if (isValid == true)
            {
                return ValidationResult.Success;
            }
            else
            {
                return new ValidationResult(ErrorMessage, new[] { validationContext.MemberName });
            }
        }
    }
}
