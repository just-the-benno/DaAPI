using DaAPI.Core.Scopes;
using DaAPI.Core.Scopes.DHCPv4;
using System;
using System.Collections.Generic;
using System.Text;
using static DaAPI.Core.Scopes.ScopeResolverPropertyDescription;
using static DaAPI.Shared.Requests.DHCPv4ScopeRequests.V1.DHCPv4ScopeAddressPropertyReqest;

namespace DaAPI.Shared.Responses
{
    public static class DHCPv4ScopeResponses
    {
        public static class V1
        {
            public abstract class DHCPv4ScopePropertyResponse
            {
                public Byte OptionCode { get; set; }
                public DHCPv4ScopePropertyType Type { get; set; }
            }

            public class DHCPv4AddressListScopePropertyResponse : DHCPv4ScopePropertyResponse
            {
                public IEnumerable<String> Addresses { get; set; }
            }

            public class DHCPv4NumericScopePropertyResponse : DHCPv4ScopePropertyResponse
            {
                public Int64 Value { get; set; }
                public DHCPv4NumericValueTypes NumericType { get; set; }
            }

            public class DHCPv4AddressScopePropertyResponse : DHCPv4ScopePropertyResponse
            {
                public String Value { get; set; }
            }

            public class DHCPv4BooleanScopePropertyResponse : DHCPv4ScopePropertyResponse
            {
                public Boolean Value { get; set; }
            }

            public class DHCPv4TextScopePropertyResponse : DHCPv4ScopePropertyResponse
            {
                public String Value { get; set; }
            }

            public class DHCPv4TimeScopePropertyResponse : DHCPv4ScopePropertyResponse
            {
                public TimeSpan Value { get; set; }
            }

            public class DHCPv4ScopePropertiesResponse
            {
                public String Name { get; set; }
                public String Description { get; set; }
                public Guid? ParentId { get; set; }
                public ScopeResolverResponse Resolver { get; set; }
                public DHCPv4ScopeAddressPropertiesResponse AddressRelated { get; set; }
                public IEnumerable<DHCPv4ScopePropertyResponse> Properties { get; set; }
            }

            public class DHCPv4ScopeAddressPropertiesResponse
            {
                public String Start { get; set; }
                public String End { get; set; }
                public IEnumerable<String> ExcludedAddresses { get; set; }
                public Boolean? ReuseAddressIfPossible { get; set; }
                public AddressAllocationStrategies? AddressAllocationStrategy { get; set; }


                public Boolean? SupportDirectUnicast { get; set; }
                public Boolean? AcceptDecline { get; set; }
                public Boolean? InformsAreAllowd { get; set; }

                public TimeSpan? LeaseTime { get;  set; }
                public TimeSpan? RenewalTime { get;  set; }
                public TimeSpan? PreferredLifetime { get;  set; }

                public Byte? Mask { get;  set; }
            }

            public class DHCPv4ScopeResolverDescription
            {
                public String TypeName { get; set; }
                public IEnumerable<DHCPv4ScopeResolverPropertyDescription> Properties { get; set; }
            }

            public class DHCPv4ScopeResolverPropertyDescription
            {
                public String PropertyName { get; set; }
                public ScopeResolverPropertyValueTypes PropertyValueType { get; set; }
            }

            public class DHCPv4ScopeItem
            {
                public String Name { get; set; }
                public Guid Id { get; set; }
                public String StartAddress { get; set; }
                public String EndAddress { get; set; }
            }

            public class DHCPv4ScopeTreeViewItem : DHCPv4ScopeItem
            {
                public IEnumerable<DHCPv4ScopeTreeViewItem> ChildScopes { get; set; }
            }

            public class ScopeResolverResponse
            {
                public String Typename { get; set; }
                public IDictionary<String, String> PropertiesAndValues { get; set; }
            }
        }
    }
}
