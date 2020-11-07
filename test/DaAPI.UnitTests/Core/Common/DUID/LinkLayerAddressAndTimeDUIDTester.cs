using DaAPI.Core.Common;
using DaAPI.TestHelper;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace DaAPI.UnitTests.Core.Common.DUID
{
    public class LinkLayerAddressAndTimeDUIDTester
    {
        [Fact]
        public void FromEthernet()
        {
            Random random = new Random();
            Byte[] macAddress = random.NextBytes(6);
            DateTime now = DateTime.Now;
            DateTime nowOnlySeconds =
                new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);

            LinkLayerAddressAndTimeDUID duid =  LinkLayerAddressAndTimeDUID.FromEthernet(
                macAddress,
                now
                );

            Assert.Equal(macAddress, duid.LinkLayerAddress);
            Assert.Equal(LinkLayerAddressDUID.DUIDLinkLayerTypes.Ethernet, duid.LinkType);
            Assert.Equal(nowOnlySeconds, duid.Time);
            Assert.Equal(DaAPI.Core.Common.DUID.DUIDTypes.LinkLayerAndTime, duid.Type);
        }

        [Fact]
        public void FromByteArray()
        {
           String rawInput = "00:01:00:01:23:c4:e7:39:00:15:5d:07:e1:12";
            Byte[] input = ByteConverter.FromString(rawInput, ':');
            Byte[] macAddress = ByteConverter.FromString("00:15:5d:07:e1:12", ':');
            DateTime time = new DateTime(2019, 1, 6, 16, 20, 09);

            LinkLayerAddressAndTimeDUID duid = LinkLayerAddressAndTimeDUID.FromByteArray(input, 0);

            Assert.Equal(macAddress, duid.LinkLayerAddress);
            Assert.Equal(LinkLayerAddressDUID.DUIDLinkLayerTypes.Ethernet, duid.LinkType);
            Assert.Equal(time, duid.Time);
            Assert.Equal(DaAPI.Core.Common.DUID.DUIDTypes.LinkLayerAndTime, duid.Type);

            Byte[] asByte = duid.GetAsByteStream();
            Assert.Equal(input, asByte);

        }
    }
}
