using DaAPI.Core.Common.DHCPv6;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace DaAPI.TestHelper
{
    public class IPv6AddressEquatableComparer : IEqualityComparer<IPv6Address>
    {
        public bool Equals([AllowNull] IPv6Address x, [AllowNull] IPv6Address y)
        {
            return x.Equals(y);
        }

        public int GetHashCode([DisallowNull] IPv6Address obj)
        {
            return obj.GetHashCode();
        }
    }
}
