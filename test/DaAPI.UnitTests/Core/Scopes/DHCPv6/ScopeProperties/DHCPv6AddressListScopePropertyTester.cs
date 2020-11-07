using DaAPI.Core.Scopes.DHCPv6.ScopeProperties;
using DaAPI.TestHelper;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace DaAPI.UnitTests.Core.Scopes.DHCPv6.ScopeProperties
{
    public class DHCPv6AddressListScopePropertyTester
    {
        [Fact]
        public void Constructor()
        {
            Random random = new Random();
            var addresses = random.GetIPv6Addresses();

            var property = new DHCPv6AddressListScopeProperty(22, addresses);

            Assert.Equal(DHCPv6ScopePropertyType.AddressList, property.ValueType);
            Assert.Equal(22, property.OptionIdentifier);
            Assert.Equal(addresses, property.Addresses,new IPv6AddressEquatableComparer());
        }

    }
}
