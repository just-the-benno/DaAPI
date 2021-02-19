using DaAPI.App.Resources;
using DaAPI.Shared.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DaAPI.App.Pages.DHCPv4Scopes
{
    public class SimpleIPv4AddressString
    {
        [Required]
        [IPv4Address(ErrorMessageResourceName = nameof(ValidationErrorMessages.IPv4Address), ErrorMessageResourceType = typeof(ValidationErrorMessages))]
        [IsUniqueInCollection(nameof(OtherItems), ErrorMessageResourceName = nameof(ValidationErrorMessages.IsUniqueInCollection), ErrorMessageResourceType = typeof(ValidationErrorMessages))]
        public String Value { get; set; }

        public IEnumerable<SimpleIPv4AddressString> OtherItems { get; }

        public SimpleIPv4AddressString(String content) : this(Array.Empty<SimpleIPv4AddressString>())
        {
            Value = content;
        }

        public SimpleIPv4AddressString(IEnumerable<SimpleIPv4AddressString> otherItems)
        {
            OtherItems = otherItems;
        }
    }
}
