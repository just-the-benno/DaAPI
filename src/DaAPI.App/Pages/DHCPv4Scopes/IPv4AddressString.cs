using DaAPI.App.Resources;
using DaAPI.App.Validation;
using DaAPI.Shared.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DaAPI.App.Pages.DHCPv4Scopes
{
    public class IPv4AddressString
    {
        [Required(ErrorMessageResourceName = nameof(ValidationErrorMessages.Required), ErrorMessageResourceType = typeof(ValidationErrorMessages))]
        [IPv4Address(ErrorMessageResourceName = nameof(ValidationErrorMessages.IPv6Address), ErrorMessageResourceType = typeof(ValidationErrorMessages))]
        [IPv4AdressInRange(nameof(Start), nameof(End), ErrorMessageResourceName = nameof(ValidationErrorMessages.IPv6AdressInRange), ErrorMessageResourceType = typeof(ValidationErrorMessages))]
        [IsUniqueInCollection(nameof(OtherItems), ErrorMessageResourceName = nameof(ValidationErrorMessages.IsUniqueInCollection), ErrorMessageResourceType = typeof(ValidationErrorMessages))]
        public String Value { get; set; }

        public String Start { get; set; }
        public String End { get; set; }

        public IEnumerable<IPv4AddressString> OtherItems { get; set; }
    }
}
