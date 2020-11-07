using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Packets.DHCPv6;
using DaAPI.Core.Scopes;
using DaAPI.Core.Scopes.DHCPv6.Resolvers;
using DaAPI.Core.Services;
using DaAPI.TestHelper;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Xunit;

namespace DaAPI.UnitTests.Core.Scopes.DHCPv6.Resolvers
{
    public class DHCPv6RelayAgentSubnetResolverTester
    {
        [Fact]
        public void HasUniqueIdentifier()
        {
            var resolver = new DHCPv6RelayAgentSubnetResolver();
            Assert.False(resolver.HasUniqueIdentifier);
        }

        [Fact]
        public void GetUniqueIdentifier()
        {
            var resolver = new DHCPv6RelayAgentSubnetResolver();
            Assert.ThrowsAny<Exception>(() => resolver.GetUniqueIdentifier(null));
        }

        [Theory]
        [InlineData("fe80::0",64,true)]
        [InlineData("fe80::1", 64, false)]
        public void ArePropertiesAndValuesValid(String ipAddress, Byte subnetmaskLength, Boolean shouldBeValid)
        {
            IPv6Address address = IPv6Address.FromString(ipAddress);
            IPv6SubnetMask mask = new IPv6SubnetMask(new IPv6SubnetMaskIdentifier(subnetmaskLength));

            Mock<ISerializer> serializerMock = new Mock<ISerializer>(MockBehavior.Strict);
            serializerMock.Setup(x => x.Deserialze<IPv6Address>(ipAddress)).Returns(address).Verifiable();
            serializerMock.Setup(x => x.Deserialze<IPv6SubnetMask>(subnetmaskLength.ToString())).Returns(mask).Verifiable();

            var resolver = new DHCPv6RelayAgentSubnetResolver();
            Boolean actual = resolver.ArePropertiesAndValuesValid(new Dictionary<String, String> {
               { "NetworkAddress", ipAddress },
               { "SubnetMask", subnetmaskLength.ToString() },
            },serializerMock.Object);

            Assert.Equal(shouldBeValid, actual);

            serializerMock.Verify();

         
        }

        [Fact]
        public void ArePropertiesAndValuesValid_KeyIsMissing()
        {
            var input = new[]{
                new  Dictionary<String,String>{ { "NetworkAddress", "someVaue" }, { "SubnetMask2", "ohter" } },
                new  Dictionary<String,String>{ { "SubnetMask", "someVaue" }, { "NetworkAddress2", "ohter" } },
                };

            var resolver = new DHCPv6RelayAgentSubnetResolver();

            foreach (var item in input)
            {
                Boolean actual = resolver.ArePropertiesAndValuesValid(item, Mock.Of<ISerializer>(MockBehavior.Strict));
                Assert.False(actual);
            }
        }

        [Fact]
        public void ApplyValues()
        {
            String ipAddress = "fe80::";
            Byte subnetmaskLength = 32;

            IPv6Address address = IPv6Address.FromString(ipAddress);
            IPv6SubnetMask mask = new IPv6SubnetMask(new IPv6SubnetMaskIdentifier(subnetmaskLength));

            Mock<ISerializer> serializerMock = new Mock<ISerializer>(MockBehavior.Strict);
            serializerMock.Setup(x => x.Deserialze<IPv6Address>(ipAddress)).Returns(address).Verifiable();
            serializerMock.Setup(x => x.Deserialze<IPv6SubnetMask>(subnetmaskLength.ToString())).Returns(mask).Verifiable();

            var resolver = new DHCPv6RelayAgentSubnetResolver();
            resolver.ApplyValues(new Dictionary<String, String> {
               { "NetworkAddress", ipAddress },
               { "SubnetMask", subnetmaskLength.ToString() },
            }, serializerMock.Object);

            Assert.Equal(address, resolver.NetworkAddress);
            Assert.Equal(mask, resolver.SubnetMask);

            serializerMock.Verify();

            var expectedValues = new Dictionary<String, String>
            {
               { "NetworkAddress", ipAddress },
               { "SubnetMask", subnetmaskLength.ToString() },
            };

            Assert.Equal(expectedValues.ToArray(), resolver.GetValues().ToArray());
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void PacketMeetsCondition(Boolean isInSubnet)
        {
            Random random = new Random();

            String ipAddress = "fe80::0";
            Byte subnetmaskLength = 32;

            IPv6Address address = IPv6Address.FromString(ipAddress);
            IPv6SubnetMask mask = new IPv6SubnetMask(new IPv6SubnetMaskIdentifier(subnetmaskLength));

            Mock<ISerializer> serializerMock = new Mock<ISerializer>(MockBehavior.Strict);
            serializerMock.Setup(x => x.Deserialze<IPv6Address>(ipAddress)).Returns(address).Verifiable();
            serializerMock.Setup(x => x.Deserialze<IPv6SubnetMask>(subnetmaskLength.ToString())).Returns(mask).Verifiable();

            DHCPv6RelayAgentSubnetResolver resolver = new DHCPv6RelayAgentSubnetResolver();

            resolver.ApplyValues(new Dictionary<String, String> {
               { "NetworkAddress", ipAddress },
               { "SubnetMask", subnetmaskLength.ToString() },
            }, serializerMock.Object);

            var packet = DHCPv6RelayPacket.AsOuterRelay(new IPv6HeaderInformation(IPv6Address.FromString("fe80::1"), IPv6Address.FromString("fe80::2")),
                true,1,random.GetIPv6Address(), random.GetIPv6Address(), Array.Empty<DHCPv6PacketOption>(), DHCPv6RelayPacket.AsInnerRelay(
             true, 0,  isInSubnet == true ? IPv6Address.FromString("fe80::1") : IPv6Address.FromString("2004::1"), IPv6Address.FromString("fe80::1"), new DHCPv6PacketOption[]
            {
            }, DHCPv6Packet.AsInner(random.NextUInt16(), DHCPv6PacketTypes.Solicit, Array.Empty<DHCPv6PacketOption>())));

            Boolean result = resolver.PacketMeetsCondition(packet);
            Assert.Equal(isInSubnet, result);

            serializerMock.Verify();
        }

        [Fact]
        public void GetDescription()
        {
            var expected = new ScopeResolverDescription("DHCPv6RelayAgentSubnetResolver", new[] {
            new ScopeResolverPropertyDescription("NetworkAddress",ScopeResolverPropertyDescription.ScopeResolverPropertyValueTypes.IPv6NetworkAddress),
            new ScopeResolverPropertyDescription("SubnetMask",ScopeResolverPropertyDescription.ScopeResolverPropertyValueTypes.IPv6Subnet),
            });

            var resolver = new DHCPv6RelayAgentSubnetResolver();
            var actual = resolver.GetDescription();

            Assert.Equal(expected, actual);
        }

    }
}
