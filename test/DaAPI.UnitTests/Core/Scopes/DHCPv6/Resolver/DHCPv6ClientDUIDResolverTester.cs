using DaAPI.Core.Common;
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
    public class DHCPv6ClientDUIDResolverTester
    {
        [Fact]
        public void HasUniqueIdentifier()
        {
            var resolver = new DHCPv6ClientDUIDResolver();
            Assert.True(resolver.HasUniqueIdentifier);
        }

        [Fact]
        public void GetUniqueIdentifier()
        {
            String duidBytes = "000300015CA62DD98800";

            LinkLayerAddressDUID duid = new LinkLayerAddressDUID(
                LinkLayerAddressDUID.DUIDLinkLayerTypes.Ethernet,
                new Byte[] { 0x5c, 0xa6, 0x2d, 0xd9, 0x88, 0x00 });

            Mock<ISerializer> serializerMock = new Mock<ISerializer>(MockBehavior.Strict);
            serializerMock.Setup(x => x.Deserialze<String>(duidBytes)).Returns(duidBytes).Verifiable();

            var resolver = new DHCPv6ClientDUIDResolver();
            resolver.ApplyValues(new Dictionary<String, String> {
               { "ClientDuid", duidBytes },
            }, serializerMock.Object);

            Assert.Equal(duid.GetAsByteStream(), resolver.GetUniqueIdentifier(null));
        }

        [Theory]
        [InlineData("000300015CA62DD98800", true)]
        [InlineData("000500015CA62DD98800", false)]
        [InlineData("00G500015CA62DD98800", false)]
        [InlineData("sgsgsgsg--sgsg2", false)]
        public void ArePropertiesAndValuesValid(String duid, Boolean shouldBeValid)
        {
            Mock<ISerializer> serializerMock = new Mock<ISerializer>(MockBehavior.Strict);
            serializerMock.Setup(x => x.Deserialze<String>(duid)).Returns(duid);

            var resolver = new DHCPv6ClientDUIDResolver();
            Boolean actual = resolver.ArePropertiesAndValuesValid(new Dictionary<String, String> {
               { "ClientDuid", duid },
            }, serializerMock.Object);

            Assert.Equal(shouldBeValid, actual);

            serializerMock.Verify();
        }

        [Fact]
        public void ArePropertiesAndValuesValid_KeyIsMissing()
        {
            var input = new[]{
                new  Dictionary<String,String>{ { "ClientDuid2", "someVaue" } },
                };

            var resolver = new DHCPv6ClientDUIDResolver();

            foreach (var item in input)
            {
                Boolean actual = resolver.ArePropertiesAndValuesValid(item, Mock.Of<ISerializer>(MockBehavior.Strict));
                Assert.False(actual);
            }
        }

        [Fact]
        public void ApplyValues()
        {
            String duidBytes = "000300015CA62DD98800";

            LinkLayerAddressDUID duid = new LinkLayerAddressDUID(
                LinkLayerAddressDUID.DUIDLinkLayerTypes.Ethernet,
                new Byte[] { 0x5c, 0xa6, 0x2d, 0xd9, 0x88, 0x00 });

            Mock<ISerializer> serializerMock = new Mock<ISerializer>(MockBehavior.Strict);
            serializerMock.Setup(x => x.Deserialze<String>(duidBytes)).Returns(duidBytes).Verifiable();

            var resolver = new DHCPv6ClientDUIDResolver();
            resolver.ApplyValues(new Dictionary<String, String> {
               { "ClientDuid", duidBytes },
            }, serializerMock.Object);


            Assert.Equal(duid, resolver.ClientDuid);
            serializerMock.Verify();

            var values = resolver.GetValues();

            Dictionary<String, String> expectedValues = new Dictionary<string, string>
            {
                 { "ClientDuid", duidBytes }
            };

            Assert.Equal(expectedValues.ToArray(), values.ToArray());
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void PacketMeetsCondition(Boolean shouldMeetCondition)
        {
            Random random = new Random();

            String duidBytes = "000300015CA62DD98800";

            LinkLayerAddressDUID duid = new LinkLayerAddressDUID(
                LinkLayerAddressDUID.DUIDLinkLayerTypes.Ethernet,
                new Byte[] { 0x5c, 0xa6, 0x2d, 0xd9, 0x88, 0x00 });

            Mock<ISerializer> serializerMock = new Mock<ISerializer>(MockBehavior.Strict);
            serializerMock.Setup(x => x.Deserialze<String>(duidBytes)).Returns(duidBytes).Verifiable();

            var resolver = new DHCPv6ClientDUIDResolver();
            resolver.ApplyValues(new Dictionary<String, String> {
               { "ClientDuid", duidBytes },
            }, serializerMock.Object);

            var packet = DHCPv6RelayPacket.AsOuterRelay(new IPv6HeaderInformation(IPv6Address.FromString("fe80::1"), IPv6Address.FromString("fe80::2")),
                true, 1, random.GetIPv6Address(), random.GetIPv6Address(), Array.Empty<DHCPv6PacketOption>(), DHCPv6RelayPacket.AsInnerRelay(
             true, 0, IPv6Address.FromString("2004::1"), IPv6Address.FromString("fe80::1"), new DHCPv6PacketOption[]
            {
            }, DHCPv6Packet.AsInner(random.NextUInt16(), DHCPv6PacketTypes.Solicit, new[] { new DHCPv6PacketIdentifierOption(DHCPv6PacketOptionTypes.ClientIdentifier, shouldMeetCondition == true ? (DUID)duid : new UUIDDUID(random.NextGuid())) })));

            Boolean result = resolver.PacketMeetsCondition(packet);
            Assert.Equal(shouldMeetCondition, result);

            serializerMock.Verify();
        }

        [Fact]
        public void GetDescription()
        {
            var expected = new ScopeResolverDescription("DHCPv6ClientDUIDResolver", new[] {
            new ScopeResolverPropertyDescription("ClientDuid",ScopeResolverPropertyDescription.ScopeResolverPropertyValueTypes.ByteArray),
            });

            var resolver = new DHCPv6ClientDUIDResolver();
            var actual = resolver.GetDescription();

            Assert.Equal(expected, actual);
        }
    }
}
