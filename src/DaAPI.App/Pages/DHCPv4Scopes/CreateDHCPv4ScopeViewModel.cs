using DaAPI.App.Pages.DHCPScopes;
using DaAPI.App.Resources;
using DaAPI.App.Resources.Pages.DHCPv4Scopes;
using DaAPI.App.Validation;
using DaAPI.Shared.Helper;
using DaAPI.Shared.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using static DaAPI.Core.Scopes.ScopeResolverPropertyDescription;
using static DaAPI.Shared.Requests.DHCPv4ScopeRequests.V1;
using static DaAPI.Shared.Requests.DHCPv4ScopeRequests.V1.DHCPv4ScopeAddressPropertyReqest;
using static DaAPI.Shared.Responses.DHCPv4ScopeResponses.V1;

namespace DaAPI.App.Pages.DHCPv4Scopes
{
    public class CreateDHCPv4ScopeViewModel
    {
        [Required(ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.Required))]
        [MinLength(3, ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.MinLength))]
        [MaxLength(50, ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.MaxLength))]
        [Display(Name = nameof(DHCPv4ScopeDisplay.Name), ResourceType = typeof(DHCPv4ScopeDisplay))]
        public String Name { get; set; }

        [MaxLength(500, ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.MaxLength))]
        [Display(Name = nameof(DHCPv4ScopeDisplay.Description), ResourceType = typeof(DHCPv4ScopeDisplay))]
        public String Description { get; set; }

        [Display(Name = nameof(DHCPv4ScopeDisplay.ParentId), ResourceType = typeof(DHCPv4ScopeDisplay))]
        public String ParentId { get; set; }

        private Boolean _hasParent;

        [Display(Name = nameof(DHCPv4ScopeDisplay.HasParent), ResourceType = typeof(DHCPv4ScopeDisplay))]
        public Boolean HasParent
        {
            get => _hasParent;
            set
            {
                _hasParent = value;
                if (value == true)
                {
                    RootAddressProperties = null;
                    ChildAddressProperties = new DHCPv4ChildScopeAddressPropertiesViewModel();
                }
                else
                {
                    RootAddressProperties = new DHCPv4RootScopeAddressPropertiesViewModel();
                    ChildAddressProperties = null;
                }
            }
        }

        private String _start;

        [Required(ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.Required))]
        [IPv4Address(ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.IPv6Address))]
        [IPv4AddressInParentRange(ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.IPv6AddressInParentRange))]
        [Display(Name = nameof(DHCPv4ScopeDisplay.AddressRangeStart), ResourceType = typeof(DHCPv4ScopeDisplay))]
        public String Start
        {
            get => _start;
            set
            {
                _start = value;
                foreach (var item in ExcludedAddresses)
                {
                    item.Start = value;
                }
            }
        }

        private String _end;

        [Required(ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.Required))]
        [IPv4Address(ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.IPv6Address))]
        [IPv4GreaterThan(nameof(Start), ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.GreaterThan))]
        [IPv4AddressInParentRange(ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.IPv6AddressInParentRange))]
        [Display(Name = nameof(DHCPv4ScopeDisplay.AddressRangeEnd), ResourceType = typeof(DHCPv4ScopeDisplay))]
        public String End
        {
            get => _end;
            set
            {
                _end = value;
                foreach (var item in ExcludedAddresses)
                {
                    item.End = value;
                }
            }
        }

        [Display(Name = nameof(DHCPv4ScopeDisplay.ExcludedAddresses), ResourceType = typeof(DHCPv4ScopeDisplay))]
        [ValidateComplexType]
        public IList<IPv4AddressString> ExcludedAddresses { get; set; }

        [ValidateComplexType]
        public DHCPv4RootScopeAddressPropertiesViewModel RootAddressProperties { get; private set; }

        [ValidateComplexType]
        public DHCPv4ChildScopeAddressPropertiesViewModel ChildAddressProperties { get; private set; }

        [Required(ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.Required))]
        [Display(Name = nameof(DHCPv4ScopeDisplay.ResolverTypeName), ResourceType = typeof(DHCPv4ScopeDisplay))]
        public String ResolverTypeName { get; set; }

        [Required(ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.Required))]
        [ValidateComplexType]
        public IList<DHCPScopeResolverValuesViewModel> ResolverProperties { get; set; }

        [Required(ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.Required))]
        [ValidateComplexType]
        public IList<DHCPv4ScopePropertyViewModel> ScopeProperties { get; set; }

        [ValidateComplexType]
        public IList<DHCPv4ScopePropertyViewModel> ParentScopeProperties { get; set; }

        public IList<DHCPv4ScopePropertyResponse> ParentPropertyResponses { get; private set; }

        private CreateDHCPv4ScopeViewModel(Boolean addDefaultProperties)
        {
            ExcludedAddresses = new List<IPv4AddressString>();
            ResolverProperties = new List<DHCPScopeResolverValuesViewModel>();
            RootAddressProperties = DHCPv4RootScopeAddressPropertiesViewModel.Default;
            ScopeProperties = new List<DHCPv4ScopePropertyViewModel>();
            if(addDefaultProperties == true)
            {
            }

            ParentScopeProperties = new List<DHCPv4ScopePropertyViewModel>();
        }

        public CreateDHCPv4ScopeViewModel() : this(true)
        {

        }

        public CreateDHCPv4ScopeViewModel(DHCPv4ScopePropertiesResponse response, IEnumerable<DHCPv4ScopeResolverDescription> descriptions) : this(false)
        {
            Name = response.Name;
            Description = response.Description;
            ParentId = response.ParentId.HasValue == false ? String.Empty : response.ParentId.Value.ToString();
            HasParent = response.ParentId.HasValue;

            Start = response.AddressRelated.Start;
            End = response.AddressRelated.End;

            foreach (var item in response.AddressRelated.ExcludedAddresses)
            {
                AddExcludedAddress(item);
            }

            if (HasParent == true)
            {
                ChildAddressProperties = new DHCPv4ChildScopeAddressPropertiesViewModel
                {
                    AcceptDecline = response.AddressRelated.AcceptDecline,
                    AddressAllocationStrategy = response.AddressRelated.AddressAllocationStrategy,
                    InformsAreAllowd = response.AddressRelated.InformsAreAllowd,
                    PreferredLifetime = response.AddressRelated.PreferredLifetime,
                    LeaseTime = response.AddressRelated.LeaseTime,
                    RenewalTime = response.AddressRelated.RenewalTime,
                    Subnetmask = response.AddressRelated.Mask,
                    ReuseAddressIfPossible = response.AddressRelated.ReuseAddressIfPossible,
                    SupportDirectUnicast = response.AddressRelated.SupportDirectUnicast,
                };
            }
            else
            {
                RootAddressProperties = new DHCPv4RootScopeAddressPropertiesViewModel
                {
                    AcceptDecline = response.AddressRelated.AcceptDecline.Value,
                    AddressAllocationStrategy = response.AddressRelated.AddressAllocationStrategy.Value,
                    InformsAreAllowd = response.AddressRelated.InformsAreAllowd.Value,
                    PreferredLifetime = response.AddressRelated.PreferredLifetime.Value,
                    RenewalTime = response.AddressRelated.RenewalTime.Value,
                    ReuseAddressIfPossible = response.AddressRelated.ReuseAddressIfPossible.Value,
                    SupportDirectUnicast = response.AddressRelated.SupportDirectUnicast.Value,
                    LeaseTime = response.AddressRelated.LeaseTime.Value,
                    Subnetmask = response.AddressRelated.Mask.Value,
                };
            }

            foreach (var item in response.Properties)
            {
                AddScopeProperty(item);
            }

            ResolverTypeName = response.Resolver.Typename;
            var normelizedPropertyMapper = DictionaryHelper.NormelizedProperties<String>(response.Resolver.PropertiesAndValues);
            var description = descriptions.First(x => x.TypeName == ResolverTypeName);
            foreach (var item in description.Properties)
            {
                AddScopeResolverProperty(item.PropertyName, item.PropertyValueType,
                    normelizedPropertyMapper[item.PropertyName]);
            }
        }

        public void AddEmptyExcludedAddress() => AddExcludedAddress(String.Empty);

        public void AddExcludedAddress(String address)
        {
            ExcludedAddresses.Add(new IPv4AddressString
            {
                Start = _start,
                End = _end,
                Value = address,
                OtherItems = ExcludedAddresses,
            });
        }

        public void ClearScopeResolverProperties() => ResolverProperties.Clear();

        public void AddScopeResolverProperty(String propertyName, ScopeResolverPropertyValueTypes valueType) =>
            ResolverProperties.Add(new DHCPScopeResolverValuesViewModel(propertyName, valueType, ResolverProperties));

        public void AddScopeResolverProperty(String propertyName, ScopeResolverPropertyValueTypes valueType,String value) =>
    ResolverProperties.Add(new DHCPScopeResolverValuesViewModel(propertyName, valueType, ResolverProperties, value));

        public void AddScopeProperty() => ScopeProperties.Add(new DHCPv4ScopePropertyViewModel());
        public void AddScopeProperty(DHCPv4ScopePropertyResponse response) => AddScopeProperty(new DHCPv4ScopePropertyViewModel(response));
        public void AddScopeProperty(DHCPv4ScopePropertyViewModel property) => ScopeProperties.Add(property);

        public void RemoveScopeProperty(Int32 index) => ScopeProperties.RemoveAt(index);

        public void SetParentProperties(IEnumerable<DHCPv4ScopePropertyResponse> parentProperties)
        {
            ParentPropertyResponses = parentProperties.ToList();
            foreach (var item in parentProperties)
            {
                var result = new DHCPv4ScopePropertyViewModel
                {
                    IsActive = false,
                    OptionCode = item.OptionCode.ToString(),
                    Type = item.Type,
                };

                switch (item)
                {
                    case DHCPv4AddressListScopePropertyResponse property:
                        foreach (var address in property.Addresses)
                        {
                            result.AddAddress(address);
                        }

                        break;
                    case DHCPv4NumericScopePropertyResponse property:
                        result.NumericValue = property.Value;

                        break;
                    default:
                        break;
                }

                ParentScopeProperties.Add(result);
            }
        }

        public CreateOrUpdateDHCPv4ScopeRequest GetRequest()
        {
            var request = new CreateOrUpdateDHCPv4ScopeRequest
            {
                Name = Name,
                Description = Description,
                ParentId = HasParent == false ? new Guid?() : Guid.Parse(ParentId),
                AddressProperties = new DHCPv4ScopeAddressPropertyReqest
                {
                    Start = Start,
                    End = End,
                    ExcludedAddresses = ExcludedAddresses.Select(x => x.Value).ToList(),
                },
                Resolver = new CreateScopeResolverRequest
                {
                    Typename = ResolverTypeName,
                    PropertiesAndValues = ResolverProperties.ToDictionary(x => x.Name,
                    x => x.GetSerializedValue()),
                },
                Properties = ScopeProperties.Union(ParentScopeProperties.Where(x => x.IsActive == true || x.MarkAsRemovedInInheritance == true)).Select(x => x.ToRequest()).ToList(),
            };

            if (HasParent == false)
            {
                request.AddressProperties.PreferredLifetime = RootAddressProperties.PreferredLifetime;
                request.AddressProperties.LeaseTime = RootAddressProperties.LeaseTime;
                request.AddressProperties.RenewalTime = RootAddressProperties.RenewalTime;
                request.AddressProperties.AcceptDecline = RootAddressProperties.AcceptDecline;
                request.AddressProperties.InformsAreAllowd = RootAddressProperties.InformsAreAllowd;
                request.AddressProperties.ReuseAddressIfPossible = RootAddressProperties.ReuseAddressIfPossible;
                request.AddressProperties.SupportDirectUnicast = RootAddressProperties.SupportDirectUnicast;
                request.AddressProperties.AddressAllocationStrategy = RootAddressProperties.AddressAllocationStrategy;
                request.AddressProperties.MaskLength = (Byte)RootAddressProperties.Subnetmask;
            }
            else
            {
                request.AddressProperties.PreferredLifetime = ChildAddressProperties.PreferredLifetime;
                request.AddressProperties.LeaseTime = ChildAddressProperties.LeaseTime;
                request.AddressProperties.AcceptDecline = ChildAddressProperties.AcceptDecline;
                request.AddressProperties.InformsAreAllowd = ChildAddressProperties.InformsAreAllowd;
                request.AddressProperties.ReuseAddressIfPossible = ChildAddressProperties.ReuseAddressIfPossible;
                request.AddressProperties.SupportDirectUnicast = ChildAddressProperties.SupportDirectUnicast;
                request.AddressProperties.AddressAllocationStrategy = ChildAddressProperties.AddressAllocationStrategy;
                request.AddressProperties.MaskLength = (Byte?)ChildAddressProperties.Subnetmask;
            }

            return request;
        }
    }
}
