using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Notifications.Triggers;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace DaAPI.UnitTests.Core.Notifications.Triggers
{
    public class PrefixBindingTester
    {
        [Theory]
        [InlineData("fe80::0", 64, "fe70::4", true)]
        [InlineData("fe80::1", 64, "fe70::4", false)]
        [InlineData("fe80::0", 64, "fe80::4", false)]
        public void Constructor(String prefix, Byte prefixLength, String host, Boolean expectedResult)
        {
            IPv6Address prefixAddress = IPv6Address.FromString(prefix);
            IPv6Address hostAddress = IPv6Address.FromString(host);
            IPv6SubnetMask mask = new IPv6SubnetMask(new IPv6SubnetMaskIdentifier(prefixLength));

            if (expectedResult == true)
            {
                var binding = new PrefixBinding(prefixAddress, mask, hostAddress);
                Assert.Equal(prefixAddress, binding.Prefix);
                Assert.Equal(hostAddress, binding.Host);
                Assert.Equal(mask, binding.Mask);
            }
            else
            {
                Assert.ThrowsAny<Exception>( () => new PrefixBinding(prefixAddress,mask,hostAddress) );
            }
        }

    }
}
