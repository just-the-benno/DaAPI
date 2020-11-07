using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Scopes.DHCPv6;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace DaAPI.UnitTests.Core.Scopes.DHCPv6
{
    public class DHCPv6PrefixDelgationInfoTester
    {
        [Fact]
        public void FromValues()
        {
            DHCPv6PrefixDelgationInfo info = DHCPv6PrefixDelgationInfo.FromValues(
                IPv6Address.FromString("2001:e68:5423:5ffd::0"), new IPv6SubnetMaskIdentifier(64), new IPv6SubnetMaskIdentifier(70));

            Assert.Equal(IPv6Address.FromString("2001:e68:5423:5ffd::0"),info.Prefix);
            Assert.Equal(64, info.PrefixLength);
            Assert.Equal(70,info.AssignedPrefixLength);
        }

        [Fact]
        public void FromValues_Failed_AssingPrefixGreaterThanPrefix()
        {
            Assert.ThrowsAny<Exception>(() => DHCPv6PrefixDelgationInfo.FromValues(
               IPv6Address.FromString("2001:e68:5423:5ffd::0"), new IPv6SubnetMaskIdentifier(70), new IPv6SubnetMaskIdentifier(64)));
        }

        [Fact]
        public void FromValues_Failed_PrefixIsNotANetworkAddress()
        {
            Assert.ThrowsAny<Exception>(() => DHCPv6PrefixDelgationInfo.FromValues(
               IPv6Address.FromString("2001:e68:5423:5ffd::1"), new IPv6SubnetMaskIdentifier(64), new IPv6SubnetMaskIdentifier(70)));
        }

    }
}
