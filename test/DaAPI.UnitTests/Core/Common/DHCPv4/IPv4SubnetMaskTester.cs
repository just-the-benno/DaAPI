using DaAPI.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace DaAPI.UnitTests.Core.Common.DHCPv4
{
    public class IPv4SubnetMaskTester
    {
        [Theory]
        [InlineData(new Byte[] { 255, 0, 0, 0 }, true)]
        [InlineData(new Byte[] { 255, 128, 0, 0 }, true)]
        [InlineData(new Byte[] { 255, 192, 0, 0 }, true)]
        [InlineData(new Byte[] { 255, 191, 0, 0 }, false)]
        [InlineData(new Byte[] { 255, 192, 192, 0 }, false)]
        [InlineData(new Byte[] { 255, 0, 192, 0 }, false)]
        [InlineData(new Byte[] { 255, 255, 255, 254 }, true)]
        [InlineData(new Byte[] { 255, 255, 255, 255 }, true)]
        [InlineData(new Byte[] { 0, 0, 0, 0 }, true)]
        [InlineData(new Byte[] { 128, 0, 0, 0 }, true)]
        [InlineData(new Byte[] { 192, 0, 0, 0 }, true)]
        [InlineData(new Byte[] { 224, 0, 0, 0 }, true)]
        [InlineData(new Byte[] { 240, 0, 0, 0 }, true)]
        [InlineData(new Byte[] { 248, 0, 0, 0 }, true)]
        [InlineData(new Byte[] { 252, 0, 0, 0 }, true)]
        [InlineData(new Byte[] { 254, 0, 0, 0 }, true)]
        public void IPv4SubnetMask_FromByteArray(Byte[] input, Boolean parsable)
        {
            if (parsable == false)
            {
                Assert.ThrowsAny<Exception>(() => IPv4SubnetMask.FromByteArray(input));
                return;
            }

            IPv4SubnetMask mask = IPv4SubnetMask.FromByteArray(input);

            Byte[] actual = mask.GetBytes();
            Assert.Equal(input, actual);
        }

        [Theory]
        [InlineData("255.255.255.255", "192.158.55.20", true)]
        [InlineData("255.255.255.255", "192.158.55.24", true)]
        [InlineData("255.255.255.0", "192.158.178.0", true)]
        [InlineData("255.255.255.0", "192.158.178.1", false)]
        [InlineData("255.255.252.0", "172.16.16.0", true)]
        [InlineData("255.255.252.0", "172.16.16.1", false)]
        public void IPv4SubnetMask_IsIPAdressANetworkAddress(String subnetMaskInput, String addressInput, Boolean expectedResult)
        {
            IPv4SubnetMask mask = IPv4SubnetMask.FromString(subnetMaskInput);
            IPv4Address address = IPv4Address.FromString(addressInput);

            Boolean actual = mask.IsIPAdressANetworkAddress(address);

            Assert.Equal(expectedResult, actual);
        }

        [Fact]
        public void IPv4SubnetMask_FromIdentifier()
        {
            Dictionary<Int32, Byte> mapper = new Dictionary<int, byte>
            {
                { 0,0 },
                { 1,128 },
                { 2,192 },
                { 3,224 },
                { 4,240 },
                { 5,248 },
                { 6,252 },
                { 7,254 },
                { 8,255 },
            };

            for (int maskIdentifier = 0; maskIdentifier <= 32; maskIdentifier++)
            {
                IPv4SubnetMask mask = new IPv4SubnetMask(new IPv4SubnetMaskIdentifier(maskIdentifier));
                Byte[] result = mask.GetBytes();
                Int32 index = maskIdentifier % 8;
                Int32 arrayIndex = maskIdentifier / 8;

                if(maskIdentifier == 32)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        Assert.Equal(255, result[i]);
                    }

                    continue;
                }

                for (int j = 0; j < arrayIndex; j++)
                {
                    Assert.Equal(255, result[j]);
                }

                Assert.Equal(mapper[index], result[arrayIndex]);
            }

        }
    }
}
