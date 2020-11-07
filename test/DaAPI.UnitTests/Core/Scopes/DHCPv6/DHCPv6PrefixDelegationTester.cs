using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Scopes.DHCPv6;
using DaAPI.TestHelper;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace DaAPI.UnitTests.Core.Scopes.DHCPv6
{
    public class DHCPv6PrefixDelegationTester
    {
        [Fact]
        public void None()
        {
            DHCPv6PrefixDelegation empty = DHCPv6PrefixDelegation.None;

            Assert.NotNull(empty);
            Assert.Equal(IPv6Address.Empty, empty.NetworkAddress);
            Assert.Equal(0, empty.Mask.Identifier);
            Assert.Equal((UInt32)0, empty.IdentityAssociation);
        }

        [Fact]
        public void FromValues()
        {
            Random random = new Random();

            IPv6Address address = IPv6Address.FromString("fe90::0");
            IPv6SubnetMask mask = new IPv6SubnetMask(new IPv6SubnetMaskIdentifier(64));
            UInt32 id = random.NextUInt32();

            DHCPv6PrefixDelegation value = DHCPv6PrefixDelegation.FromValues
                (address, mask, id);

            Assert.NotNull(value);
            Assert.Equal(address, value.NetworkAddress);
            Assert.Equal(64, value.Mask.Identifier);
            Assert.Equal(id, value.IdentityAssociation);
        }

        [Fact]
        public void FromValues_Failed_AddressNotMatch()
        {
            Random random = new Random();

            IPv6Address address = IPv6Address.FromString("2001:e68:5423:3a2b:716b:ef6e:b215:fb96");
            IPv6SubnetMask mask = new IPv6SubnetMask(new IPv6SubnetMaskIdentifier(64));
            UInt32 id = random.NextUInt32();

            Assert.ThrowsAny<Exception>(() => DHCPv6PrefixDelegation.FromValues
                (address, mask, id));
        }
    }
}
