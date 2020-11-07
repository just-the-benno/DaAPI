using DaAPI.Core.Common.DHCPv6;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace DaAPI.UnitTests.Core.Common.DHCPv6
{
    public class IPv6SubnetMaskTester
    {
        [Theory]
        [InlineData(0, new Byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 })]
        [InlineData(128, new Byte[16] { 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255 })]
        [InlineData(16, new Byte[16] { 255, 255, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 })]
        [InlineData(17, new Byte[16] { 255, 255, 128, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 })]
        [InlineData(18, new Byte[16] { 255, 255, 192, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 })]
        [InlineData(19, new Byte[16] { 255, 255, 224, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 })]
        [InlineData(20, new Byte[16] { 255, 255, 240, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 })]
        [InlineData(21, new Byte[16] { 255, 255, 248, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 })]
        [InlineData(22, new Byte[16] { 255, 255, 252, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 })]
        [InlineData(23, new Byte[16] { 255, 255, 254, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 })]
        [InlineData(24, new Byte[16] { 255, 255, 255, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 })]
        [InlineData(25, new Byte[16] { 255, 255, 255, 128, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 })]
        public void GetMaskBytes(Byte identiifer, Byte[] expectedMask)
        {
            IPv6SubnetMask mask = new IPv6SubnetMask(new IPv6SubnetMaskIdentifier(identiifer));

            Assert.Equal(identiifer, mask.Identifier);
            Assert.Equal(expectedMask, mask.GetMaskBytes());
        }

        [Theory]
        [InlineData("fe80::0", 16, true)]
        [InlineData("fe80::1", 16, false)]
        [InlineData("2001:e68:5423:5ffd::0", 64, true)]
        [InlineData("2001:e68:5423:5ffd:716b:ef6e:b215:fb96", 64, false)]
        public void IsIPv6AdressANetworkAddress(String ipv6Adress, Byte identiifer, Boolean expectedResult)
        {
            IPv6SubnetMask mask = new IPv6SubnetMask(new IPv6SubnetMaskIdentifier(identiifer));

            Boolean actual = mask.IsIPv6AdressANetworkAddress(IPv6Address.FromString(ipv6Adress));
            Assert.Equal(expectedResult, actual);
        }

        [Theory]
        [InlineData("fe80::0", 64, "fe80::4", true)]
        [InlineData("fe80::0", 64, "fe70::4", false)]
        [InlineData("fe80::0", 128, "fe80::0", true)]
        [InlineData("fe80::0", 128, "fe80::1", false)]
        [InlineData("fe80::0", 127, "fe80::1", true)]
        [InlineData("fe80::0", 127, "fe80::2", false)]
        public void IsAddressInSubnet(String networkAddress, Byte length,String address, Boolean expectedResult)
        {
            IPv6Address parsedNetworkAddress = IPv6Address.FromString(networkAddress);
            IPv6SubnetMask mask = new IPv6SubnetMask(new IPv6SubnetMaskIdentifier(length));
            IPv6Address parsedAddress = IPv6Address.FromString(address);
            Boolean actual = mask.IsAddressInSubnet(parsedNetworkAddress, parsedAddress);
            Assert.Equal(expectedResult, actual);
        }
    }
}
