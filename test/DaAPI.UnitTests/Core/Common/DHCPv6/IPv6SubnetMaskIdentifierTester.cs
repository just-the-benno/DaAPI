using DaAPI.Core.Common.DHCPv6;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace DaAPI.UnitTests.Core.Common.DHCPv6
{
    public class IPv6SubnetMaskIdentifierTester
    {
        [Theory]
        [InlineData(1)]
        [InlineData(20)]
        [InlineData(80)]
        [InlineData(128)]
        public void Constructor(Byte value)
        {
            IPv6SubnetMaskIdentifier identifier = new IPv6SubnetMaskIdentifier(value);
            Assert.Equal(value, identifier.Value);
            Assert.Equal(value, identifier);
        }

        [Theory]
        [InlineData(129)]
        [InlineData(250)]
        public void Constructor_Failed_OutOfRange(Byte value)
        {
            Assert.ThrowsAny<Exception>(() => new IPv6SubnetMaskIdentifier(value));
        }
    }
}
