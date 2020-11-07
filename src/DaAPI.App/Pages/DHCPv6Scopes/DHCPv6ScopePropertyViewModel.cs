using DaAPI.App.Resources;
using DaAPI.App.Resources.Pages.DHCPv6Scopes;
using DaAPI.App.Validation;
using DaAPI.Core.Scopes.DHCPv6.ScopeProperties;
using DaAPI.Shared.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using static DaAPI.Shared.Requests.DHCPv6ScopeRequests.V1;
using static DaAPI.Shared.Responses.DHCPv6ScopeResponses.V1;

namespace DaAPI.App.Pages.DHCPv6Scopes
{
    public class DHCPv6ScopePropertyViewModel
    {
        public static IEnumerable<DHCPv6ScopePropertyType> Properties = new[] {
            DHCPv6ScopePropertyType.AddressList,
            DHCPv6ScopePropertyType.Byte,
            DHCPv6ScopePropertyType.Text,
            DHCPv6ScopePropertyType.UInt16,
            DHCPv6ScopePropertyType.UInt32
        };

        public static Dictionary<String, (String DisplayName, DHCPv6ScopePropertyType Type)> WellknowOptions
        { get; private set; } =
            new Dictionary<String, (string DisplayName, DHCPv6ScopePropertyType Type)>
        {
            { "23",  ("DNS-Server", DHCPv6ScopePropertyType.AddressList) },
            { "31",  ("SNTP-Server", DHCPv6ScopePropertyType.AddressList) },
            { "56",  ("NTP-Server", DHCPv6ScopePropertyType.AddressList) },
            { "7",   ("Preference", DHCPv6ScopePropertyType.Byte) },
        };

        private String _optionCode;

        [Display(Name = nameof(DHCPv6ScopeDisplay.ScopePropertyOptionCode), ResourceType = typeof(DHCPv6ScopeDisplay))]
        [Max(UInt16.MaxValue, ErrorMessageResourceName = nameof(ValidationErrorMessages.Max), ErrorMessageResourceType = typeof(ValidationErrorMessages))]
        public Int32 CustomOptionCode { get; set; }

        public Boolean MarkAsRemovedInInheritance { get; set; }

        [Display(Name = nameof(DHCPv6ScopeDisplay.ScopePropertyOptionCode), ResourceType = typeof(DHCPv6ScopeDisplay))]
        public String OptionCode
        {
            get => _optionCode;
            set
            {
                _optionCode = value;
                IsWellknownType = WellknowOptions.ContainsKey(value);
                if (IsWellknownType == true)
                {
                    Type = WellknowOptions[value].Type;
                    if (UInt16.TryParse(value, out UInt16 code) == true)
                    {
                        CustomOptionCode = code;
                    }
                }
            }
        }

        public String GetOptionCodeName() => WellknowOptions.ContainsKey(OptionCode) == true ? WellknowOptions[OptionCode].DisplayName : OptionCode;

        public Boolean IsWellknownType { get; private set; }

        [Display(Name = nameof(DHCPv6ScopeDisplay.ScopePropertyType), ResourceType = typeof(DHCPv6ScopeDisplay))]
        public DHCPv6ScopePropertyType Type { get; set; }

        [DHCPv6ScopePropertyValidation(ErrorMessageResourceName = nameof(ValidationErrorMessages.DHCPv6ScopePropertyValidation), ErrorMessageResourceType = typeof(ValidationErrorMessages))]
        public String TextValue { get; set; }

        [DHCPv6ScopePropertyValidation(ErrorMessageResourceName = nameof(ValidationErrorMessages.DHCPv6ScopePropertyValidation), ErrorMessageResourceType = typeof(ValidationErrorMessages))]
        public Int64 NumericValue { get; set; }

        public Boolean IsActive { get; set; }

        [DHCPv6ScopePropertyValidation(ErrorMessageResourceName = nameof(ValidationErrorMessages.DHCPv6ScopePropertyValidation), ErrorMessageResourceType = typeof(ValidationErrorMessages))]
        [ValidateComplexType]
        public IList<SimpleIPv6AddressString> Addresses { get; private set; }

        public DHCPv6ScopePropertyViewModel()
        {
            Addresses = new List<SimpleIPv6AddressString>();
        }

        public DHCPv6ScopePropertyViewModel(DHCPv6ScopePropertyResponse response) : this()
        {
            OptionCode = response.OptionCode.ToString();

            switch (response)
            {
                case DHCPv6AddressListScopePropertyResponse property:
                    foreach (var item in property.Addresses)
                    {
                        AddAddress(item);
                    }
                    break;
                case DHCPv6NumericScopePropertyResponse property:
                    NumericValue = property.Value;
                    break;
                default:
                    break;
            }
        }

        public void AddAddress() => AddAddress(String.Empty);
        public void AddAddress(String content) => Addresses.Add(new SimpleIPv6AddressString(Addresses) { Value = content });
        public void RemoveAddress(Int32 index) => Addresses.RemoveAt(index);

        public DHCPv6ScopePropertyRequest ToRequest()
        {
            DHCPv6ScopePropertyRequest request = Type switch
            {
                DHCPv6ScopePropertyType.AddressList => new DHCPv6AddressListScopePropertyRequest
                {
                    Addresses = Addresses.Select(x => x.Value).ToList(),
                },
                DHCPv6ScopePropertyType.Byte => new DHCPv6NumericScopePropertyRequest
                {
                    NumericType = Core.Scopes.NumericScopePropertiesValueTypes.Byte,
                    Value = NumericValue,
                },
                DHCPv6ScopePropertyType.UInt16 => new DHCPv6NumericScopePropertyRequest
                {
                    NumericType = Core.Scopes.NumericScopePropertiesValueTypes.UInt16,
                    Value = NumericValue,
                },
                DHCPv6ScopePropertyType.UInt32 => new DHCPv6NumericScopePropertyRequest
                {
                    NumericType = Core.Scopes.NumericScopePropertiesValueTypes.UInt32,
                    Value = NumericValue,

                },
                DHCPv6ScopePropertyType.Text => throw new NotImplementedException(),
                _ => throw new NotImplementedException(),
            };

            request.MarkAsRemovedInInheritance = MarkAsRemovedInInheritance;
            request.OptionCode = (UInt16)CustomOptionCode;
            request.Type = Type;

            return request;
        }
    }

}
