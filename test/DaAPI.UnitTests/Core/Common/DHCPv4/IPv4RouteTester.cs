using DaAPI.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace DaAPI.UnitTests.Core.Common.DHCPv4
{
    public class IPv4RouteTester
    {
        [Theory]
        [InlineData("192.168.178.0", "255.255.255.0")]
        public void Constructor(String rawNetwork, String rawMask)
        {
            IPv4Address address = IPv4Address.FromString(rawNetwork);
            IPv4SubnetMask mask = IPv4SubnetMask.FromString(rawMask);

            IPv4Route route = new IPv4Route(address, mask);

            Assert.Equal(mask, route.SubnetMask);
            Assert.Equal(address, route.Network);
        }

        [Theory]
        [InlineData("192.168.178.45", "255.255.255.0")]
        public void Constructor_Failed_AddressNotNetwork(String rawNetwork, String rawMask)
        {
            IPv4Address address = IPv4Address.FromString(rawNetwork);
            IPv4SubnetMask mask = IPv4SubnetMask.FromString(rawMask);

            Assert.ThrowsAny<Exception>(() =>
               new IPv4Route(address, mask));
        }
    }
}
