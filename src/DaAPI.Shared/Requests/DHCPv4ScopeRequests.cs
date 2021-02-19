using DaAPI.Core.Scopes;
using DaAPI.Core.Scopes.DHCPv4;
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
    public static class DHCPv4ScopeRequests
    {
        public static class V1
        {
            public class CreateOrUpdateDHCPv4ScopeRequest
            {
                [Required]
                [StringLength(50, MinimumLength = 3)]
                public String Name { get; set; }

                [StringLength(100)]
                public String Description { get; set; }

                public Guid? ParentId { get; set; }

                [Required]
                public DHCPv4ScopeAddressPropertyReqest AddressProperties { get; set; }

                [Required]
                public CreateScopeResolverRequest Resolver { get; set; }

                public IEnumerable<DHCPv4ScopePropertyRequest> Properties { get; set; }
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

            public class DHCPv4ScopeAddressPropertyReqest
            {
                public enum AddressAllocationStrategies
                {
                    Random = 1,
                    Next = 2,
                }

                public Boolean? ReuseAddressIfPossible { get; set; }

                [Required]
                [StringLength(50, MinimumLength = 3)]
                [IPv4Address]
                public String Start { get; set; }

                [Required]
                [StringLength(50, MinimumLength = 3)]
                [IPv4Address]
                public String End { get; set; }

                [IPv4AddressList]
                public IEnumerable<String> ExcludedAddresses { get; set; }
                public TimeSpan? PreferredLifetime { get; set; }
                public TimeSpan? LeaseTime { get; set; }
                public TimeSpan? RenewalTime { get; set; }

                [Range(0, 32)]
                public Byte? MaskLength { get; set; }

                public Boolean? SupportDirectUnicast { get; set; }
                public Boolean? AcceptDecline { get; set; }
                public Boolean? InformsAreAllowd { get; set; }
                public AddressAllocationStrategies? AddressAllocationStrategy { get; set; }

            }

            public abstract class DHCPv4ScopePropertyRequest
            {
                public Byte OptionCode { get; set; }
                public Boolean MarkAsRemovedInInheritance { get; set; }
                public DHCPv4ScopePropertyType Type { get; set; }
            }

            public class DHCPv4AddressListScopePropertyRequest : DHCPv4ScopePropertyRequest
            {
                [IPv4AddressList]
                public IEnumerable<String> Addresses { get; set; }
            }

            public class DHCPv4AddressScopePropertyRequest : DHCPv4ScopePropertyRequest
            {
                [IPv4Address]
                public String Address { get; set; }
            }

            public class DHCPv4BooleanScopePropertyRequest : DHCPv4ScopePropertyRequest
            {
                public Boolean Value { get; set; }
            }


            public class DHCPv4NumericScopePropertyRequest : DHCPv4ScopePropertyRequest
            {
                public Int64 Value { get; set; }
                public DHCPv4NumericValueTypes NumericType { get; set; }
            }

            public class DHCPv4TextScopePropertyRequest : DHCPv4ScopePropertyRequest
            {
                [Required]
                public String Value { get; set; }
            }

            public class DHCPv4TimeScopePropertyRequest : DHCPv4ScopePropertyRequest
            {
                public TimeSpan Value { get; set; }
            }

            public class DHCPv4ScopeDeleteRequest
            {
                [Required]
                public Guid Id { get; set; }
                public Boolean IncludeChildren { get; set; }
            }
        }
    }
}
