using DaAPI.App.Resources;
using DaAPI.App.Resources.Pages.DHCPv4Scopes;
using DaAPI.App.Validation;
using DaAPI.Core.Scopes.DHCPv4;
using DaAPI.Shared.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using static DaAPI.Shared.Requests.DHCPv4ScopeRequests.V1;
using static DaAPI.Shared.Responses.DHCPv4ScopeResponses.V1;

namespace DaAPI.App.Pages.DHCPv4Scopes
{
    public class DHCPv4ScopePropertyViewModel
    {
        public static IEnumerable<DHCPv4ScopePropertyType> Properties = new[] {
            DHCPv4ScopePropertyType.AddressList,
            DHCPv4ScopePropertyType.Address,
            DHCPv4ScopePropertyType.Boolean,
            DHCPv4ScopePropertyType.Byte,
            DHCPv4ScopePropertyType.UInt16,
            DHCPv4ScopePropertyType.UInt32,
            DHCPv4ScopePropertyType.Text,
            DHCPv4ScopePropertyType.Time,
        };

        public static Dictionary<String, (String DisplayName, DHCPv4ScopePropertyType Type)> WellknowOptions
        { get; private set; } =
            new Dictionary<String, (string DisplayName, DHCPv4ScopePropertyType Type)>
        {
            { "23",  ("DNS-Server", DHCPv4ScopePropertyType.AddressList) },
            { "31",  ("SNTP-Server", DHCPv4ScopePropertyType.AddressList) },
            { "56",  ("NTP-Server", DHCPv4ScopePropertyType.AddressList) },
            { "7",   ("Preference", DHCPv4ScopePropertyType.Byte) },
        };

        private String _optionCode;

        [Display(Name = nameof(DHCPv4ScopeDisplay.ScopePropertyOptionCode), ResourceType = typeof(DHCPv4ScopeDisplay))]
        [Max(UInt16.MaxValue, ErrorMessageResourceName = nameof(ValidationErrorMessages.Max), ErrorMessageResourceType = typeof(ValidationErrorMessages))]
        public Int32 CustomOptionCode { get; set; }

        public Boolean MarkAsRemovedInInheritance { get; set; }

        [Display(Name = nameof(DHCPv4ScopeDisplay.ScopePropertyOptionCode), ResourceType = typeof(DHCPv4ScopeDisplay))]
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

        [Display(Name = nameof(DHCPv4ScopeDisplay.ScopePropertyType), ResourceType = typeof(DHCPv4ScopeDisplay))]
        public DHCPv4ScopePropertyType Type { get; set; }

        [DHCPv4ScopePropertyValidation(ErrorMessageResourceName = nameof(ValidationErrorMessages.DHCPv4ScopePropertyValidation), ErrorMessageResourceType = typeof(ValidationErrorMessages))]
        public String TextValue { get; set; }

        [DHCPv4ScopePropertyValidation(ErrorMessageResourceName = nameof(ValidationErrorMessages.DHCPv4ScopePropertyValidation), ErrorMessageResourceType = typeof(ValidationErrorMessages))]
        public Int64 NumericValue { get; set; }

        [DHCPv4ScopePropertyValidation(ErrorMessageResourceName = nameof(ValidationErrorMessages.DHCPv4ScopePropertyValidation), ErrorMessageResourceType = typeof(ValidationErrorMessages))]
        public Boolean BooleanValue { get; set; }

        [DHCPv4ScopePropertyValidation(ErrorMessageResourceName = nameof(ValidationErrorMessages.DHCPv4ScopePropertyValidation), ErrorMessageResourceType = typeof(ValidationErrorMessages))]
        public TimeSpan TimeValue { get; set; }

        public Boolean IsActive { get; set; }

        [DHCPv4ScopePropertyValidation(ErrorMessageResourceName = nameof(ValidationErrorMessages.DHCPv4ScopePropertyValidation), ErrorMessageResourceType = typeof(ValidationErrorMessages))]
        [ValidateComplexType]
        public IList<SimpleIPv4AddressString> Addresses { get; private set; }

        [DHCPv4ScopePropertyValidation(ErrorMessageResourceName = nameof(ValidationErrorMessages.DHCPv4ScopePropertyValidation), ErrorMessageResourceType = typeof(ValidationErrorMessages))]
        public SimpleIPv4AddressString Address { get;  set; }

        public DHCPv4ScopePropertyViewModel()
        {
            Addresses = new List<SimpleIPv4AddressString>();
        }

        public DHCPv4ScopePropertyViewModel(DHCPv4ScopePropertyResponse response) : this()
        {
            OptionCode = response.OptionCode.ToString();

            switch (response)
            {
                case DHCPv4AddressListScopePropertyResponse property:
                    foreach (var item in property.Addresses)
                    {
                        AddAddress(item);
                    }
                    break;
                case DHCPv4AddressScopePropertyResponse property:
                    Address = new SimpleIPv4AddressString(property.Value);
                    break;
                case DHCPv4BooleanScopePropertyResponse property:
                    BooleanValue = property.Value;
                    break;
                case DHCPv4NumericScopePropertyResponse property:
                    NumericValue = property.Value;
                    break;
                case DHCPv4TextScopePropertyResponse property:
                    TextValue = property.Value;
                    break;
                case DHCPv4TimeScopePropertyResponse property:
                    TimeValue = property.Value;
                    break;
                default:
                    break;
            }
        }

        public void AddAddress() => AddAddress(String.Empty);
        public void AddAddress(String content) => Addresses.Add(new SimpleIPv4AddressString(Addresses) { Value = content });
        public void RemoveAddress(Int32 index) => Addresses.RemoveAt(index);

        public DHCPv4ScopePropertyRequest ToRequest()
        {
            DHCPv4ScopePropertyRequest request = Type switch
            {
                DHCPv4ScopePropertyType.AddressList => new DHCPv4AddressListScopePropertyRequest
                {
                    Addresses = Addresses.Select(x => x.Value).ToList(),
                },
                DHCPv4ScopePropertyType.Address => new DHCPv4AddressScopePropertyRequest
                {
                    Address = Address.Value,
                },
                DHCPv4ScopePropertyType.Byte => new DHCPv4NumericScopePropertyRequest
                {
                    NumericType = DHCPv4NumericValueTypes.Byte,
                    Value = NumericValue,
                },
                DHCPv4ScopePropertyType.UInt16 => new DHCPv4NumericScopePropertyRequest
                {
                    NumericType = DHCPv4NumericValueTypes.UInt16,
                    Value = NumericValue,
                },
                DHCPv4ScopePropertyType.UInt32 => new DHCPv4NumericScopePropertyRequest
                {
                    NumericType = DHCPv4NumericValueTypes.UInt32,
                    Value = NumericValue,
                },
                DHCPv4ScopePropertyType.Boolean => new DHCPv4BooleanScopePropertyRequest
                {
                    Value = BooleanValue,
                },
                DHCPv4ScopePropertyType.Time => new DHCPv4TimeScopePropertyRequest
                {
                    Value = TimeValue,
                },
                DHCPv4ScopePropertyType.Text => new DHCPv4TextScopePropertyRequest
                {
                    Value = TextValue,
                },
                _ => throw new NotImplementedException(),
            };

            request.MarkAsRemovedInInheritance = MarkAsRemovedInInheritance;
            request.OptionCode = (Byte)CustomOptionCode;
            request.Type = Type;

            return request;
        }
    }
}
