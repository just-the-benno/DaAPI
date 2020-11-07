using Castle.Core.Logging;
using DaAPI.Core.Common.DHCPv6;
using DaAPI.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace DaAPI.UnitTests.Infrastructure.Services.JsonConverters
{
    public class IPv6AddressAsStringJsonConverterTester
    {
        [Theory]
        [InlineData("::1")]
        [InlineData("fe80::")]
        [InlineData("2001:e68:5423:3a2b:716b:ef6e:b215:fb96")]
        public void SerialzeAndDeserilize(String input)
        {
            IPv6Address address = IPv6Address.FromString(input);

            JSONBasedSerializer serializer = new JSONBasedSerializer();

            var serilizedValue =  serializer.Seralize(address);
            Assert.Equal(serilizedValue,  "\"" + input + "\"");
            IPv6Address actual = serializer.Deserialze<IPv6Address>(serilizedValue);

            Assert.Equal(address, actual);
        }

    }
}
