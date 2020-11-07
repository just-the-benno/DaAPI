using DaAPI.Core.Common.DHCPv6;
using DaAPI.Infrastructure.StorageEngine.Converters;
using DaAPI.TestHelper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace DaAPI.UnitTests.Infrastructure.StorageEngine.Converters
{
    public class IPv6AddressJsonConverterTester
    {
        [Fact]
        public void SerializeAndDeserialize()
        {
            Random random = new Random();
            var input = random.GetIPv6Addresses();

            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Converters.Add(new IPv6AddressJsonConverter());

            String serialized = JsonConvert.SerializeObject(input, settings);
            var actual = JsonConvert.DeserializeObject<List<IPv6Address>>(serialized, settings);

            for (int i = 0; i < input.Count; i++)
            {
                Assert.Equal(input[i], actual[i]);
            }
        }

        [Fact]
        public void SerializeAndDeserialize_Null()
        {
            IPv6Address input = null;

            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Converters.Add(new IPv6AddressJsonConverter());

            String serialized = JsonConvert.SerializeObject(input, settings);
            var actual = JsonConvert.DeserializeObject<IPv6Address>(serialized, settings);

            Assert.Null(actual);
        }

    }
}
