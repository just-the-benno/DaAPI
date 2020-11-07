using DaAPI.Core.Common;
using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Packets.DHCPv6;
using DaAPI.Infrastructure.StorageEngine.Converters;
using DaAPI.TestHelper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace DaAPI.UnitTests.Infrastructure.StorageEngine.Converters
{
    public class DHCPv6PacketConverterTester
    {
        [Fact]
        public void SerializeAndDeserialize()
        {
            Random random = new Random();

            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Converters.Add(new IPv6AddressJsonConverter());
            settings.Converters.Add(new DHCPv6PacketJsonConverter());
            settings.Converters.Add(new IPv6HeaderInformationJsonConverter());

            IPv6HeaderInformation header = new IPv6HeaderInformation(
                random.GetIPv6Address(), random.GetIPv6Address());

            DHCPv6Packet input = new DHCPv6Packet(header, random.NextUInt16(), DHCPv6PacketTypes.Solicit, new List<DHCPv6PacketOption>
            {
                new DHCPv6PacketIdentifierOption(DHCPv6PacketOptionTypes.ClientIdentifier,new  UUIDDUID(Guid.NewGuid())),
                new DHCPv6PacketIdentifierOption(DHCPv6PacketOptionTypes.ServerIdentifer,new  UUIDDUID(Guid.NewGuid())),
            }
            );

            String serialized = JsonConvert.SerializeObject(input, settings);
            var actual = JsonConvert.DeserializeObject<DHCPv6Packet>(serialized, settings);

            Assert.Equal(input, actual);
        }

        [Fact]
        public void SerializeAndDeserialize_Null()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Converters.Add(new IPv6AddressJsonConverter());
            settings.Converters.Add(new DHCPv6PacketJsonConverter());
            settings.Converters.Add(new IPv6HeaderInformationJsonConverter());

            String serialized = JsonConvert.SerializeObject(DHCPv6Packet.Empty, settings);
            var actual = JsonConvert.DeserializeObject<DHCPv6Packet>(serialized, settings);

            Assert.Equal(DHCPv6Packet.Empty, actual);
        }

    }
}
