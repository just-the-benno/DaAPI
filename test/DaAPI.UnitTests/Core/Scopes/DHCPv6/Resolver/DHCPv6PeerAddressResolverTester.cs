using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Packets.DHCPv6;
using DaAPI.Core.Scopes;
using DaAPI.Core.Scopes.DHCPv6.Resolvers;
using DaAPI.Core.Services;
using DaAPI.Infrastructure.Services.JsonConverters;
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
    public class DHCPv6PeerAddressResolverTester
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]

        public void HasUniqueIdentifier(Boolean shouldHave)
        {
            Random random = new Random();

            IPv6Address address = random.GetIPv6Address();
            String booleanValue = shouldHave == true ? "true" : "false";

            Mock<ISerializer> serializerMock = new Mock<ISerializer>(MockBehavior.Strict);
            serializerMock.Setup(x => x.Deserialze<IPv6Address>(address.ToString())).Returns(address).Verifiable();
            serializerMock.Setup(x => x.Deserialze<Boolean>(booleanValue)).Returns(shouldHave).Verifiable();

            var resolver = new DHCPv6PeerAddressResolver();
            resolver.ApplyValues(new Dictionary<String, String> {
               { "PeerAddress", address.ToString() },
               { "IsUnique", booleanValue },

            }, serializerMock.Object);

            Assert.Equal(shouldHave,resolver.HasUniqueIdentifier);
        }

        [Fact]
        public void GetUniqueIdentifier()
        {
            IPv6Address address = IPv6Address.FromString("fced::1");

            Mock<ISerializer> serializerMock = new Mock<ISerializer>(MockBehavior.Strict);
            serializerMock.Setup(x => x.Deserialze<IPv6Address>(address.ToString())).Returns(address).Verifiable();
            serializerMock.Setup(x => x.Deserialze<Boolean>("true")).Returns(true).Verifiable();

            var resolver = new DHCPv6PeerAddressResolver();
            resolver.ApplyValues(new Dictionary<String, String> {
               { "PeerAddress", address.ToString() },
               { "IsUnique", "true" },

            }, serializerMock.Object);

            Assert.Equal(address.GetBytes(), resolver.GetUniqueIdentifier(null));
        }

        [Theory]
        [InlineData("fe80::1",true)]
        [InlineData("fe80::2::1", false)]
        public void ArePropertiesAndValuesValid(String ipAddress,  Boolean shouldBeValid)
        {
            Mock<ISerializer> serializerMock = new Mock<ISerializer>(MockBehavior.Strict);
            serializerMock.Setup(x => x.Deserialze<String>("true")).Returns("true").Verifiable();

            if (shouldBeValid == true)
            { 
            serializerMock.Setup(x => x.Deserialze<IPv6Address>(ipAddress)).Returns(IPv6Address.FromString(ipAddress)).Verifiable();
            }
            else
            {
                serializerMock.Setup(x => x.Deserialze<IPv6Address>(ipAddress)).Throws(new Exception()).Verifiable();
            }

            var resolver = new DHCPv6PeerAddressResolver();
            Boolean actual = resolver.ArePropertiesAndValuesValid(new Dictionary<String, String> {
               { "PeerAddress", ipAddress },
               { "IsUnique", "true" },

            }, serializerMock.Object);

            Assert.Equal(shouldBeValid, actual);

            serializerMock.Verify(x => x.Deserialze<String>("true"), Times.AtMostOnce());
            serializerMock.Verify(x => x.Deserialze<IPv6Address>(ipAddress), Times.Once());
        }

        [Fact]
        public void ArePropertiesAndValuesValid_KeyIsMissing()
        {
            var input = new[]{
                new  Dictionary<String,String>{ 
                    { "RelayAgentAddress2", "someVaue" },
                    { "IsUnique", "true" }
                },
                new  Dictionary<String,String>{
                    { "RelayAgentAddress", "someVaue" },
                    { "IsUnique2", "true" }
                },
                };

            var resolver = new DHCPv6PeerAddressResolver();

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
            serializerMock.Setup(x => x.Deserialze<Boolean>("true")).Returns(true).Verifiable();

            var resolver = new DHCPv6PeerAddressResolver();
            resolver.ApplyValues(new Dictionary<String, String> {
               { "PeerAddress", ipAddress },
               { "IsUnique", "true" },

            }, serializerMock.Object);

            Assert.Equal(address, resolver.PeerAddress);

            serializerMock.Verify();

            Dictionary<String, String> expectedValues = new Dictionary<String, String> {
               { "IsUnique", "true" },
                { "PeerAddress", ipAddress },
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
            serializerMock.Setup(x => x.Deserialze<Boolean>("true")).Returns(true).Verifiable();

            var resolver = new DHCPv6PeerAddressResolver();

            resolver.ApplyValues(new Dictionary<String, String> {
               { "PeerAddress", ipAddress },
                  { "IsUnique", "true" },
            }, serializerMock.Object);

            var packet = DHCPv6RelayPacket.AsOuterRelay(new IPv6HeaderInformation(IPv6Address.FromString("fe80::1"), IPv6Address.FromString("fe80::2")),
                true,1,random.GetIPv6Address(), random.GetIPv6Address(), Array.Empty<DHCPv6PacketOption>(), DHCPv6RelayPacket.AsInnerRelay(
             true, 0, IPv6Address.FromString("fe80::1"), shouldMeetCondition == true ? IPv6Address.FromString(ipAddress) : IPv6Address.FromString("2004::1"), new DHCPv6PacketOption[]
            {
            }, DHCPv6Packet.AsInner(random.NextUInt16(), DHCPv6PacketTypes.Solicit, Array.Empty<DHCPv6PacketOption>())));

            Boolean result = resolver.PacketMeetsCondition(packet);
            Assert.Equal(shouldMeetCondition, result);

            serializerMock.Verify();
        }

        [Fact]
        public void GetDescription()
        {
            var expected = new ScopeResolverDescription("DHCPv6PeerAddressResolver", new[] {
                new ScopeResolverPropertyDescription("IsUnique",ScopeResolverPropertyDescription.ScopeResolverPropertyValueTypes.Boolean),
                new ScopeResolverPropertyDescription("PeerAddress",ScopeResolverPropertyDescription.ScopeResolverPropertyValueTypes.IPv6Address),
            });

            var resolver = new DHCPv6PeerAddressResolver();
            var actual = resolver.GetDescription();

            Assert.Equal(expected, actual);
        }

    }
}
