using DaAPI.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Scopes
{
    public class ScopeResolverPropertyDescription : Value<ScopeResolverPropertyDescription>
    {
        public enum ScopeResolverPropertyValueTypes
        {
            NotSpecified = 0,
            String = 1,
            Numeric = 2,
            IPv4Address = 3,
            IPv4AddressList = 4,
            IPv4Subnetmask = 5,
            ByteArray = 6,
            Resolvers = 7,
            UInt32 = 8,
            NullableUInt32 = 9,
            IPv6NetworkAddress = 10,
            IPv6Subnet = 11,
            IPv6Address = 12,
            Boolean = 13,
        }

        #region Properties

        public String PropertyName { get; private set; }
        public ScopeResolverPropertyValueTypes PropertyValueType { get; private set; }

        #endregion

        #region Properties

        public ScopeResolverPropertyDescription(String name, ScopeResolverPropertyValueTypes type)
        {
            PropertyName = name;
            PropertyValueType = type;
        }

        #endregion
    }
}
