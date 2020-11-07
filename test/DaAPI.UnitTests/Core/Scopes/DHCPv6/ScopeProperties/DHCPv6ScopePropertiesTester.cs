using DaAPI.Core.Scopes.DHCPv6.ScopeProperties;
using DaAPI.TestHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace DaAPI.UnitTests.Core.Scopes.DHCPv6.ScopeProperties
{
    public class DHCPv6ScopePropertiesTester
    {
        [Fact]
        public void Empty()
        {
            var properties =  DHCPv6ScopeProperties.Empty;

            Assert.NotNull(properties);
            Assert.Empty(properties.Properties);
        }

        [Fact]
        public void Contructor()
        {
            Random random = new Random();

            List<DHCPv6ScopeProperty> propertiesToAdd = new List<DHCPv6ScopeProperty>();
            for (UInt16 i = 160; i < 15; i++)
            {
                DHCPv6AddressListScopeProperty property = new DHCPv6AddressListScopeProperty(i, random.GetIPv6Addresses());
                propertiesToAdd.Add(property);
            }

            var properties = new DHCPv6ScopeProperties(propertiesToAdd);

            Assert.NotNull(properties);
            Assert.Equal(propertiesToAdd.Count, properties.Properties.Count());
        }
    }
}
