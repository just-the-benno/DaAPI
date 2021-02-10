using DaAPI.App.Pages.DHCPScopes;
using DaAPI.App.Resources;
using DaAPI.App.Resources.Pages.DHCPv6Scopes;
using DaAPI.App.Validation;
using DaAPI.Core.Scopes.DHCPv6.ScopeProperties;
using DaAPI.Shared.Helper;
using DaAPI.Shared.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using static DaAPI.Core.Scopes.ScopeResolverPropertyDescription;
using static DaAPI.Shared.Requests.DHCPv6ScopeRequests.V1;
using static DaAPI.Shared.Requests.DHCPv6ScopeRequests.V1.DHCPv6ScopeAddressPropertyReqest;
using static DaAPI.Shared.Responses.DHCPv6ScopeResponses.V1;

namespace DaAPI.App.Pages.DHCPv6Scopes
{
    public class CreateDHCPv6ScopeViewModel
    {
        [Required(ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.Required))]
        [MinLength(3, ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.MinLength))]
        [MaxLength(50, ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.MaxLength))]
        [Display(Name = nameof(DHCPv6ScopeDisplay.Name), ResourceType = typeof(DHCPv6ScopeDisplay))]
        public String Name { get; set; }

        [MaxLength(500, ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.MaxLength))]
        [Display(Name = nameof(DHCPv6ScopeDisplay.Description), ResourceType = typeof(DHCPv6ScopeDisplay))]
        public String Description { get; set; }

        [Display(Name = nameof(DHCPv6ScopeDisplay.ParentId), ResourceType = typeof(DHCPv6ScopeDisplay))]
        public String ParentId { get; set; }

        private Boolean _hasParent;

        [Display(Name = nameof(DHCPv6ScopeDisplay.HasParent), ResourceType = typeof(DHCPv6ScopeDisplay))]
        public Boolean HasParent
        {
            get => _hasParent;
            set
            {
                _hasParent = value;
                if (value == true)
                {
                    RootAddressProperties = null;
                    ChildAddressProperties = new DHCPv6ChildScopeAddressPropertiesViewModel();
                }
                else
                {
                    RootAddressProperties = new DHCPv6RootScopeAddressPropertiesViewModel();
                    ChildAddressProperties = null;
                }
            }
        }

        private String _start;

        [Required(ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.Required))]
        [IPv6Address(ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.IPv6Address))]
        [IPv6AddressInParentRange(ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.IPv6AddressInParentRange))]
        [Display(Name = nameof(DHCPv6ScopeDisplay.AddressRangeStart), ResourceType = typeof(DHCPv6ScopeDisplay))]
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
        [IPv6Address(ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.IPv6Address))]
        [IPv6GreaterThan(nameof(Start), ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.GreaterThan))]
        [IPv6AddressInParentRange(ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.IPv6AddressInParentRange))]
        [Display(Name = nameof(DHCPv6ScopeDisplay.AddressRangeEnd), ResourceType = typeof(DHCPv6ScopeDisplay))]
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

        [Display(Name = nameof(DHCPv6ScopeDisplay.ExcludedAddresses), ResourceType = typeof(DHCPv6ScopeDisplay))]
        [ValidateComplexType]
        public IList<IPv6AddressString> ExcludedAddresses { get; set; }

        private Boolean _hasPrefixInfo;

        [Display(Name = nameof(DHCPv6ScopeDisplay.HasPrefixInfo), ResourceType = typeof(DHCPv6ScopeDisplay))]
        public Boolean HasPrefixInfo
        {
            get => _hasPrefixInfo;
            set
            {
                _hasPrefixInfo = value;

                if (value == true)
                {
                    PrefixDelgationInfo = new DHCPv6PrefixDelgationViewModel();
                }
                else
                {
                    PrefixDelgationInfo = null;
                }
            }
        }

        public DHCPv6PrefixDelgationViewModel PrefixDelgationInfo { get; set; }

        [ValidateComplexType]
        public DHCPv6RootScopeAddressPropertiesViewModel RootAddressProperties { get; private set; }

        [ValidateComplexType]
        public DHCPv6ChildScopeAddressPropertiesViewModel ChildAddressProperties { get; private set; }

        [Required(ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.Required))]
        [Display(Name = nameof(DHCPv6ScopeDisplay.ResolverTypeName), ResourceType = typeof(DHCPv6ScopeDisplay))]
        public String ResolverTypeName { get; set; }

        [Required(ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.Required))]
        [ValidateComplexType]
        public IList<DHCPScopeResolverValuesViewModel> ResolverProperties { get; set; }

        [Required(ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.Required))]
        [ValidateComplexType]
        public IList<DHCPv6ScopePropertyViewModel> ScopeProperties { get; set; }

        [ValidateComplexType]
        public IList<DHCPv6ScopePropertyViewModel> ParentScopeProperties { get; set; }

        public IList<DHCPv6ScopePropertyResponse> ParentPropertyResponses { get; private set; }

        private CreateDHCPv6ScopeViewModel(Boolean addDefaultProperties)
        {
            ExcludedAddresses = new List<IPv6AddressString>();
            ResolverProperties = new List<DHCPScopeResolverValuesViewModel>();
            RootAddressProperties = DHCPv6RootScopeAddressPropertiesViewModel.Default;
            ScopeProperties = new List<DHCPv6ScopePropertyViewModel>();
            if(addDefaultProperties == true)
            {
                ScopeProperties.Add(new DHCPv6ScopePropertyViewModel { Type = DHCPv6ScopePropertyType.Byte, OptionCode = "7", NumericValue = 60 });
            }

            ParentScopeProperties = new List<DHCPv6ScopePropertyViewModel>();
        }

        public CreateDHCPv6ScopeViewModel() : this(true)
        {

        }

        public CreateDHCPv6ScopeViewModel(DHCPv6ScopePropertiesResponse response, IEnumerable<DHCPv6ScopeResolverDescription> descriptions) : this(false)
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
            HasPrefixInfo = response.AddressRelated.PrefixDelegationInfo != null;
            if (HasPrefixInfo == true)
            {
                PrefixDelgationInfo = new DHCPv6PrefixDelgationViewModel(response.AddressRelated.PrefixDelegationInfo);
            }

            if (HasParent == true)
            {
                ChildAddressProperties = new DHCPv6ChildScopeAddressPropertiesViewModel
                {
                    AcceptDecline = response.AddressRelated.AcceptDecline,
                    AddressAllocationStrategy = response.AddressRelated.AddressAllocationStrategy,
                    InformsAreAllowd = response.AddressRelated.InformsAreAllowd,
                    PreferredLifetime = response.AddressRelated.PreferedLifetime,
                    RapitCommitEnabled = response.AddressRelated.RapitCommitEnabled,
                    ReuseAddressIfPossible = response.AddressRelated.ReuseAddressIfPossible,
                    SupportDirectUnicast = response.AddressRelated.SupportDirectUnicast,
                    T1 = response.AddressRelated.T1,
                    T2 = response.AddressRelated.T2,
                    ValidLifetime = response.AddressRelated.ValidLifetime,
                };
            }
            else
            {
                RootAddressProperties = new DHCPv6RootScopeAddressPropertiesViewModel
                {
                    AcceptDecline = response.AddressRelated.AcceptDecline.Value,
                    AddressAllocationStrategy = response.AddressRelated.AddressAllocationStrategy.Value,
                    InformsAreAllowd = response.AddressRelated.InformsAreAllowd.Value,
                    PreferredLifetime = response.AddressRelated.PreferedLifetime.Value,
                    RapitCommitEnabled = response.AddressRelated.RapitCommitEnabled.Value,
                    ReuseAddressIfPossible = response.AddressRelated.ReuseAddressIfPossible.Value,
                    SupportDirectUnicast = response.AddressRelated.SupportDirectUnicast.Value,
                    T1 = response.AddressRelated.T1.Value,
                    T2 = response.AddressRelated.T2.Value,
                    ValidLifetime = response.AddressRelated.ValidLifetime.Value,
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
            ExcludedAddresses.Add(new IPv6AddressString
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

        public void AddScopeProperty() => ScopeProperties.Add(new DHCPv6ScopePropertyViewModel());
        public void AddScopeProperty(DHCPv6ScopePropertyResponse response) => AddScopeProperty(new DHCPv6ScopePropertyViewModel(response));
        public void AddScopeProperty(DHCPv6ScopePropertyViewModel property) => ScopeProperties.Add(property);

        public void RemoveScopeProperty(Int32 index) => ScopeProperties.RemoveAt(index);

        public void SetParentProperties(IEnumerable<DHCPv6ScopePropertyResponse> parentProperties)
        {
            ParentPropertyResponses = parentProperties.ToList();
            foreach (var item in parentProperties)
            {
                var result = new DHCPv6ScopePropertyViewModel
                {
                    IsActive = false,
                    OptionCode = item.OptionCode.ToString(),
                    Type = item.Type,
                };

                switch (item)
                {
                    case DHCPv6AddressListScopePropertyResponse property:
                        foreach (var address in property.Addresses)
                        {
                            result.AddAddress(address);
                        }

                        break;
                    case DHCPv6NumericScopePropertyResponse property:
                        result.NumericValue = property.Value;

                        break;
                    default:
                        break;
                }

                ParentScopeProperties.Add(result);
            }
        }

        public CreateOrUpdateDHCPv6ScopeRequest GetRequest()
        {
            var request = new CreateOrUpdateDHCPv6ScopeRequest
            {
                Name = Name,
                Description = Description,
                ParentId = HasParent == false ? new Guid?() : Guid.Parse(ParentId),
                AddressProperties = new DHCPv6ScopeAddressPropertyReqest
                {
                    Start = Start,
                    End = End,
                    ExcludedAddresses = ExcludedAddresses.Select(x => x.Value).ToList(),
                    PrefixDelgationInfo = HasPrefixInfo == true ? new DHCPv6PrefixDelgationInfoRequest
                    {
                        AssingedPrefixLength = PrefixDelgationInfo.AssingedPrefixLength,
                        Prefix = PrefixDelgationInfo.Prefix,
                        PrefixLength = PrefixDelgationInfo.PrefixLength,
                    } : null,
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
                request.AddressProperties.PreferredLifeTime = RootAddressProperties.PreferredLifetime;
                request.AddressProperties.ValidLifeTime = RootAddressProperties.ValidLifetime;
                request.AddressProperties.AcceptDecline = RootAddressProperties.AcceptDecline;
                request.AddressProperties.InformsAreAllowd = RootAddressProperties.InformsAreAllowd;
                request.AddressProperties.RapitCommitEnabled = RootAddressProperties.RapitCommitEnabled;
                request.AddressProperties.ReuseAddressIfPossible = RootAddressProperties.ReuseAddressIfPossible;
                request.AddressProperties.SupportDirectUnicast = RootAddressProperties.SupportDirectUnicast;
                request.AddressProperties.AddressAllocationStrategy = RootAddressProperties.AddressAllocationStrategy;
                request.AddressProperties.T1 = RootAddressProperties.T1;
                request.AddressProperties.T2 = RootAddressProperties.T2;

            }
            else
            {
                request.AddressProperties.PreferredLifeTime = ChildAddressProperties.PreferredLifetime;
                request.AddressProperties.ValidLifeTime = ChildAddressProperties.ValidLifetime;
                request.AddressProperties.AcceptDecline = ChildAddressProperties.AcceptDecline;
                request.AddressProperties.InformsAreAllowd = ChildAddressProperties.InformsAreAllowd;
                request.AddressProperties.RapitCommitEnabled = ChildAddressProperties.RapitCommitEnabled;
                request.AddressProperties.ReuseAddressIfPossible = ChildAddressProperties.ReuseAddressIfPossible;
                request.AddressProperties.SupportDirectUnicast = ChildAddressProperties.SupportDirectUnicast;
                request.AddressProperties.AddressAllocationStrategy = ChildAddressProperties.AddressAllocationStrategy;
                request.AddressProperties.T1 = ChildAddressProperties.T1;
                request.AddressProperties.T2 = ChildAddressProperties.T2;
            }

            return request;
        }
    }
}
