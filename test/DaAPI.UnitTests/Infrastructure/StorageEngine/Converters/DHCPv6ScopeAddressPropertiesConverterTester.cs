using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Scopes.DHCPv6;
using DaAPI.Infrastructure.StorageEngine.Converters;
using DaAPI.TestHelper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace DaAPI.UnitTests.Infrastructure.StorageEngine.Converters
{
    public class DHCPv6ScopeAddressPropertiesConverterTester
    {
        [Fact]
        public void SerializeAndDeserialize()
        {
            Random random = new Random();

            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Converters.Add(new IPv6AddressJsonConverter());
            settings.Converters.Add(new DHCPv6ScopeAddressPropertiesConverter());
            settings.Converters.Add(new DHCPv6PrefixDelgationInfoJsonConverter());

            var input = new DHCPv6ScopeAddressProperties(
                       IPv6Address.FromString("fe80::1"), IPv6Address.FromString("fe80::A"),
                       new List<IPv6Address> { IPv6Address.FromString("fe80::4"), IPv6Address.FromString("fe80::5") },
                       DHCPv6TimeScale.FromDouble(0.5), DHCPv6TimeScale.FromDouble(0.7),
                       TimeSpan.FromMinutes(random.Next(10, 20)), TimeSpan.FromMinutes(random.Next(40, 60)),
                       random.NextBoolean(), DHCPv6ScopeAddressProperties.AddressAllocationStrategies.Next, random.NextBoolean(),
                       random.NextBoolean(), random.NextBoolean(), random.NextBoolean(),
                       DHCPv6PrefixDelgationInfo.FromValues(IPv6Address.FromString("2001:e68:5423:5ffd::0"),new IPv6SubnetMaskIdentifier(64),new IPv6SubnetMaskIdentifier(70)));
            
            String serialized = JsonConvert.SerializeObject(input, settings);
            var actual = JsonConvert.DeserializeObject<DHCPv6ScopeAddressProperties>(serialized, settings);

            Assert.Equal(input, actual);
        }
    }
}
