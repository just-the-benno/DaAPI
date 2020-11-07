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
    public class DHCPv6RelayAgentResolverTester
    {
        [Fact]
        public void HasUniqueIdentifier()
        {
            var resolver = new DHCPv6RelayAgentResolver();
            Assert.False(resolver.HasUniqueIdentifier);
        }

        [Fact]
        public void GetUniqueIdentifier()
        {
            var resolver = new DHCPv6RelayAgentResolver();
            Assert.ThrowsAny<Exception>(() => resolver.GetUniqueIdentifier(null));
        }

        [Theory]
        [InlineData("fe80::1",true)]
        [InlineData("fe80::2::1", false)]
        public void ArePropertiesAndValuesValid(String ipAddress,  Boolean shouldBeValid)
        {
            Mock<ISerializer> serializerMock = new Mock<ISerializer>(MockBehavior.Strict);
            if(shouldBeValid == true)
            { 
            serializerMock.Setup(x => x.Deserialze<IPv6Address>(ipAddress)).Returns(IPv6Address.FromString(ipAddress)).Verifiable();
            }
            else
            {
                serializerMock.Setup(x => x.Deserialze<IPv6Address>(ipAddress)).Throws(new Exception()).Verifiable();
            }

            var resolver = new DHCPv6RelayAgentResolver();
            Boolean actual = resolver.ArePropertiesAndValuesValid(new Dictionary<String, String> {
               { "RelayAgentAddress", ipAddress },
            },serializerMock.Object);

            Assert.Equal(shouldBeValid, actual);

            serializerMock.Verify();
        }

        [Fact]
        public void ArePropertiesAndValuesValid_KeyIsMissing()
        {
            var input = new[]{
                new  Dictionary<String,String>{ { "RelayAgentAddress2", "someVaue" } },
                };

            var resolver = new DHCPv6RelayAgentResolver();

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

            IPv6Address address = IPv6Address.FromString(ipAddress);

            Mock<ISerializer> serializerMock = new Mock<ISerializer>(MockBehavior.Strict);
            serializerMock.Setup(x => x.Deserialze<IPv6Address>(ipAddress)).Returns(address).Verifiable();

            var resolver = new DHCPv6RelayAgentResolver();
            resolver.ApplyValues(new Dictionary<String, String> {
               { "RelayAgentAddress", ipAddress },
            }, serializerMock.Object);

            Assert.Equal(address, resolver.RelayAgentAddress);

            serializerMock.Verify();

            Dictionary<String, String> expectedValues = new Dictionary<String, String>
            {
               { "RelayAgentAddress", ipAddress },
            };

            Assert.Equal(expectedValues.ToArray(), resolver.GetValues().ToArray());
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void PacketMeetsCondition(Boolean shouldMeetCondition)
        {
            Random random = new Random();

            String ipAddress = "fe80::1";

            IPv6Address address = IPv6Address.FromString(ipAddress);

            Mock<ISerializer> serializerMock = new Mock<ISerializer>(MockBehavior.Strict);
            serializerMock.Setup(x => x.Deserialze<IPv6Address>(ipAddress)).Returns(address).Verifiable();

            var resolver = new DHCPv6RelayAgentResolver();

            resolver.ApplyValues(new Dictionary<String, String> {
               { "RelayAgentAddress", ipAddress },
            }, serializerMock.Object);

            var packet = DHCPv6RelayPacket.AsOuterRelay(new IPv6HeaderInformation(IPv6Address.FromString("fe80::1"), IPv6Address.FromString("fe80::2")),
                true,1,random.GetIPv6Address(), random.GetIPv6Address(), Array.Empty<DHCPv6PacketOption>(), DHCPv6RelayPacket.AsInnerRelay(
             true, 0, shouldMeetCondition == true ? IPv6Address.FromString(ipAddress) : IPv6Address.FromString("2004::1"), IPv6Address.FromString("fe80::1"), new DHCPv6PacketOption[]
            {
            }, DHCPv6Packet.AsInner(random.NextUInt16(), DHCPv6PacketTypes.Solicit, Array.Empty<DHCPv6PacketOption>())));

            Boolean result = resolver.PacketMeetsCondition(packet);
            Assert.Equal(shouldMeetCondition, result);

            serializerMock.Verify();
        }

        [Fact]
        public void GetDescription()
        {
            var expected = new ScopeResolverDescription("DHCPv6RelayAgentResolver", new[] {
            new ScopeResolverPropertyDescription("RelayAgentAddress",ScopeResolverPropertyDescription.ScopeResolverPropertyValueTypes.IPv6Address),
            });

            var resolver = new DHCPv6RelayAgentResolver();
            var actual = resolver.GetDescription();

            Assert.Equal(expected, actual);
        }

    }
}
