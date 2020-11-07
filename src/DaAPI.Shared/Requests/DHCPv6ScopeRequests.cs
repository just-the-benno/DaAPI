using DaAPI.Core.Scopes;
using DaAPI.Core.Scopes.DHCPv6.ScopeProperties;
using DaAPI.Shared.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading;
using static DaAPI.Shared.Responses.DHCPv6ScopeResponses.V1;

namespace DaAPI.Shared.Requests
{
    public static class DHCPv6ScopeRequests
    {
        public static class V1
        {
            public class CreateOrUpdateDHCPv6ScopeRequest
            {
                [Required]
                [StringLength(50, MinimumLength = 3)]
                public String Name { get; set; }

                [StringLength(100)]
                public String Description { get; set; }

                public Guid? ParentId { get; set; }

                [Required]
                public DHCPv6ScopeAddressPropertyReqest AddressProperties { get; set; }

                [Required]
                public CreateScopeResolverRequest Resolver { get; set; }

                public IEnumerable<DHCPv6ScopePropertyRequest> Properties { get; set; }
            }

            public class CreateScopeResolverRequest
            {
                [Required]
                [StringLength(150, MinimumLength = 3)]
                public String Typename { get; set; }

                [Required]
                [MaxLength(10)]
                public IDictionary<String, String> PropertiesAndValues { get; set; }
            }

            public class DHCPv6PrefixDelgationInfoRequest
            {
                [Display(Name = "Prefix")]
                [Required]
                [IPv6Address]
                [IsIPv6Subnet(nameof(PrefixLength))]
                public String Prefix { get; set; }

                [Display(Name = "Length")]
                [Required]
                [Max(128)]
                public Byte PrefixLength { get; set; }

                [Display(Name = "Length of assigned prefixes")]
                [Required]
                [Max(128)]
                [GreaterThan(nameof(PrefixLength))]
                public Byte AssingedPrefixLength { get; set; }

                public DHCPv6PrefixDelgationInfoRequest()
                {

                }

                public DHCPv6PrefixDelgationInfoRequest(DHCPv6PrefixDelgationInfoResponse response)
                {
                    Prefix = response.Prefix;
                    PrefixLength = response.PrefixLength;
                    AssingedPrefixLength = response.AssingedPrefixLength;
                }
            }

            public class DHCPv6ScopeAddressPropertyReqest
            {
                public enum AddressAllocationStrategies
                {
                    Random = 1,
                    Next = 2,
                }

                public Boolean? ReuseAddressIfPossible { get; set; }

                [Required]
                [StringLength(50, MinimumLength = 3)]
                [IPv6Address]
                public String Start { get; set; }

                [Required]
                [StringLength(50, MinimumLength = 3)]
                [IPv6Address]
                public String End { get; set; }

                [IPv6AddressList]
                public IEnumerable<String> ExcludedAddresses { get; set; }
                public TimeSpan? PreferredLifeTime { get; set; }
                public TimeSpan? ValidLifeTime { get; set; }

                //[Min(0.05),Max(0.95)]
                public Double? T1 { get; set; }

                //[Min(0.05), Max(0.95),GreaterThan(nameof(T1))]
                public Double? T2 { get; set; }

                public Boolean? SupportDirectUnicast { get; set; }
                public Boolean? AcceptDecline { get; set; }
                public Boolean? InformsAreAllowd { get; set; }
                public Boolean? RapitCommitEnabled { get; set; }
                public AddressAllocationStrategies? AddressAllocationStrategy { get; set; }

                public DHCPv6PrefixDelgationInfoRequest PrefixDelgationInfo { get; set; }
            }

            public abstract class DHCPv6ScopePropertyRequest
            {
                public UInt16 OptionCode { get; set; }
                public Boolean MarkAsRemovedInInheritance { get; set; }
                public DHCPv6ScopePropertyType Type { get; set; }
            }

            public class DHCPv6AddressListScopePropertyRequest : DHCPv6ScopePropertyRequest
            {
                [IPv6AddressList]
                public IEnumerable<String> Addresses { get; set; }
            }

            public class DHCPv6NumericScopePropertyRequest : DHCPv6ScopePropertyRequest
            {
                public Int64 Value { get; set; }
                public NumericScopePropertiesValueTypes NumericType { get; set; }
            }

            public class DHCPv6ScopeDeleteRequest
            { 
                [Required]
                public Guid Id { get; set; }
                public Boolean IncludeChildren { get; set; }
            }
        }
    }
}
