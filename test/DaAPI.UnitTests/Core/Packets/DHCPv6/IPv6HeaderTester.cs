using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Packets.DHCPv6;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace DaAPI.UnitTests.Core.Packets.DHCPv6
{
    public class IPv6HeaderTester
    {
        [Fact]
        public void IsEqual()
        {
            IPv6HeaderInformation header1 = new IPv6HeaderInformation(
                IPv6Address.FromString("fe80::1"), IPv6Address.FromString("fe80::2")
                );

            IPv6HeaderInformation header2 = new IPv6HeaderInformation(
                IPv6Address.FromString("fe80::1"), IPv6Address.FromString("fe80::2")
                );

            IPv6HeaderInformation header3 = new IPv6HeaderInformation(
                IPv6Address.FromString("fe80::2"), IPv6Address.FromString("fe80::1")
                );

            Assert.True(header1.Equals(header1));
            Assert.True(header1.Equals(header2));
            Assert.True(header2.Equals(header1));
            Assert.True(header2.Equals(header2));

            Assert.Equal(header1, header2);
            Assert.Equal(header2, header1);
            Assert.Equal(header1, header1);
            Assert.Equal(header2, header2);

            Assert.False(header1.Equals(header3));
            Assert.False(header3.Equals(header1));
            Assert.False(header2.Equals(header3));
            Assert.False(header3.Equals(header2));

            Assert.NotEqual(header1, header3);
            Assert.NotEqual(header3, header1);
            Assert.NotEqual(header2, header3);
            Assert.NotEqual(header3, header2);
        }

        [Fact]
        public void AsReponse()
        {
            IPv6HeaderInformation requestHeader = new IPv6HeaderInformation(
                IPv6Address.FromString("fe80::1"), IPv6Address.FromString("fe80::2")
                );

            IPv6HeaderInformation responseHeader = IPv6HeaderInformation.AsResponse(requestHeader);
            Assert.Equal(requestHeader.Destionation, responseHeader.Source);
            Assert.Equal(requestHeader.Source, responseHeader.Destionation);
        }

    }
}
