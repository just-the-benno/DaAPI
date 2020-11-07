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
    public class IPv6SubnetMaskJsonConverterTester
    {
        [Fact]
        public void SerialzeAndDeserilize()
        {
            JSONBasedSerializer serializer = new JSONBasedSerializer();

            for (Byte i = 0; i <= 128; i++)
            {
                IPv6SubnetMask mask = new IPv6SubnetMask(new IPv6SubnetMaskIdentifier(i));

                var serilizedValue = serializer.Seralize(mask);
                Assert.Equal($"\"{i}\"", serilizedValue);
                IPv6SubnetMask actual = serializer.Deserialze<IPv6SubnetMask>(serilizedValue);

                Assert.Equal(mask, actual);
            }
        }
    }
}
