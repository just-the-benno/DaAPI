using DaAPI.Core.Common;
using DaAPI.TestHelper;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace DaAPI.UnitTests.Core.Common.DUID
{
    public class DUIDFactoryTester
    {
        [Fact]
        public void GetDUID_LinkLayerAddressAndTimeDUID()
        {
            String rawInput = "00:01:00:01:23:c4:e7:39:00:15:5d:07:e1:12";
            Byte[] input = ByteConverter.FromString(rawInput, ':');
            Byte[] macAddress = ByteConverter.FromString("00:15:5d:07:e1:12", ':');
            DateTime time = new DateTime(2019, 1, 6, 16, 20, 09);

            var duid = DUIDFactory.GetDUID(input, 0);

            Assert.NotNull(duid);
            Assert.IsAssignableFrom<LinkLayerAddressAndTimeDUID>(duid);

            LinkLayerAddressAndTimeDUID castedDuid = (LinkLayerAddressAndTimeDUID)duid;
            Assert.Equal(macAddress, castedDuid.LinkLayerAddress);
            Assert.Equal(LinkLayerAddressDUID.DUIDLinkLayerTypes.Ethernet, castedDuid.LinkType);
            Assert.Equal(DaAPI.Core.Common.DUID.DUIDTypes.LinkLayerAndTime, duid.Type);
            Assert.Equal(time, castedDuid.Time);
        }

        [Fact]
        public void GetDUID_Unknown()
        {
            Random random = new Random();
            Byte[] value = random.NextBytes(20);
            Byte[] input = new byte[2 + value.Length];
            input[0] = 0;
            input[1] = 17;
            value.CopyTo(input, 2);

            var duid = DUIDFactory.GetDUID(input, 0);

            Assert.NotNull(duid);
            Assert.IsAssignableFrom<UnknownDUID>(duid);

            Assert.Equal(value, duid.Value);
        }

        private class FakeDUID : DaAPI.Core.Common.DUID
        {
        }

        [Fact]
        public void Add()
        {
            Byte newCode = 16;

            Random random = new Random();
            Byte[] value = random.NextBytes(20);
            Byte[] input = new byte[2 + value.Length];
            input[0] = 0;
            input[1] = newCode;
            value.CopyTo(input, 2);

            DUIDFactory.AddDUIDType(newCode, (input) => new FakeDUID(), false);

            var duid = DUIDFactory.GetDUID(input);
            Assert.NotNull(duid);
            Assert.IsAssignableFrom<FakeDUID>(duid);
        }

        [Fact]
        public void Add_AlreadyExists()
        {
            Assert.ThrowsAny<Exception>(() =>
           DUIDFactory.AddDUIDType(1, (input) => new FakeDUID(), false));
        }

        [Fact]
        public void Add_WithReplace()
        {
            Byte newCode = 2;

            Random random = new Random();
            Byte[] value = random.NextBytes(20);
            Byte[] input = new byte[2 + value.Length];
            input[0] = 0;
            input[1] = newCode;
            value.CopyTo(input, 2);

            DUIDFactory.AddDUIDType(newCode, (input) => new FakeDUID(), true);

            var duid = DUIDFactory.GetDUID(input);
            Assert.NotNull(duid);
            Assert.IsAssignableFrom<FakeDUID>(duid);
        }
    }
}
