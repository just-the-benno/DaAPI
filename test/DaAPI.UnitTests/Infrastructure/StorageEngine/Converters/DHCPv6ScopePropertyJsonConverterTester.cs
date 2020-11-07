using DaAPI.Core.Scopes;
using DaAPI.Core.Scopes.DHCPv6.ScopeProperties;
using DaAPI.Infrastructure.StorageEngine.Converters;
using DaAPI.TestHelper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace DaAPI.UnitTests.Infrastructure.StorageEngine.Converters
{
    public class DHCPv6ScopePropertyJsonConverterTester
    {


    [Fact]
        public void SerializeAndDeserialize()
        {
            Random random = new Random();

            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Converters.Add(new IPv6AddressJsonConverter());
            settings.Converters.Add(new DHCPv6ScopePropertyJsonConverter());
            settings.Converters.Add(new DHCPv6ScopePropertiesJsonConverter());

           var input = new DHCPv6ScopeProperties(
                            new DHCPv6AddressListScopeProperty(random.NextUInt16(), random.GetIPv6Addresses()),
                            new DHCPv6NumericValueScopeProperty(random.NextUInt16(), random.NextUInt32(), NumericScopePropertiesValueTypes.UInt32, DHCPv6ScopePropertyType.UInt32),
                            new DHCPv6NumericValueScopeProperty(random.NextUInt16(), random.NextUInt16(), NumericScopePropertiesValueTypes.UInt16, DHCPv6ScopePropertyType.UInt16),
                            new DHCPv6NumericValueScopeProperty(random.NextUInt16(), random.NextByte(), NumericScopePropertiesValueTypes.Byte, DHCPv6ScopePropertyType.Byte)
                            );

            input.RemoveFromInheritance(random.NextUInt16());
            input.RemoveFromInheritance(random.NextUInt16());
            input.RemoveFromInheritance(random.NextUInt16());

            String serialized = JsonConvert.SerializeObject(input, settings);
            var actual = JsonConvert.DeserializeObject<DHCPv6ScopeProperties>(serialized, settings);

            Assert.Equal(input, actual);
        }
    }
}
