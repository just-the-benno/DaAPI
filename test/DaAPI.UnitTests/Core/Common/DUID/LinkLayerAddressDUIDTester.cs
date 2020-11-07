using DaAPI.Core.Common;
using DaAPI.TestHelper;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using static DaAPI.Core.Common.DUID;

namespace DaAPI.UnitTests.Core.Common.DUID
{
    public class LinkLayerAddressDUIDTester
    {
        [Fact]
        public void Constructor()
        {
            Random random = new Random();
            Byte[] macAddress = random.NextBytes(6);

            LinkLayerAddressDUID duid =
                new LinkLayerAddressDUID(LinkLayerAddressDUID.DUIDLinkLayerTypes.Ethernet, macAddress);

            Assert.Equal(LinkLayerAddressDUID.DUIDLinkLayerTypes.Ethernet, duid.AddressType);
            Assert.Equal(DUIDTypes.LinkLayer, duid.Type);
            Assert.Equal(macAddress, duid.LinkLayerAddress);
        }


        [Fact]
        public void FromByteArray()
        {
            String rawInput = "00:03:00:01:f8:7b:20:06:ab:70";
            Byte[] input = ByteConverter.FromString(rawInput, ':');
            Byte[] macAddress = ByteConverter.FromString("f8:7b:20:06:ab:70", ':');

            LinkLayerAddressDUID duid = LinkLayerAddressDUID.FromByteArray(input,0);

            Assert.Equal(LinkLayerAddressDUID.DUIDLinkLayerTypes.Ethernet, duid.AddressType);
            Assert.Equal(DUIDTypes.LinkLayer, duid.Type);
            Assert.Equal(macAddress, duid.LinkLayerAddress);

            Byte[] asByteStream = duid.GetAsByteStream();
            Assert.Equal(input, asByteStream);
        }

    }
}
