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
    public class DHCPv6MilegateResolverTester
    {
        [Fact]
        public void HasUniqueIdentifier()
        {
            var resolver = new DHCPv6MilegateResolver();
            Assert.True(resolver.HasUniqueIdentifier);
        }

        private DHCPv6Packet GetPacket(Random random, Byte[] remoteId, Boolean includeRemoteIdOption = true)
        {
            IPv6HeaderInformation headerInformation =
new IPv6HeaderInformation(IPv6Address.FromString("fe80::1"), IPv6Address.FromString("fe80::2"));

            var packetOptions = new List<DHCPv6PacketOption>
                {
                    new DHCPv6PacketTrueOption(DHCPv6PacketOptionTypes.RapitCommit),
                };

            DHCPv6Packet innerPacket = DHCPv6Packet.AsInner(random.NextUInt16(),
                DHCPv6PacketTypes.Solicit, packetOptions);

            DHCPv6Packet innerRelayPacket = DHCPv6RelayPacket.AsInnerRelay(true, 1, random.GetIPv6Address(), random.GetIPv6Address(),
                new DHCPv6PacketOption[]
                {
                    new DHCPv6PacketByteArrayOption(DHCPv6PacketOptionTypes.InterfaceId, random.NextBytes(10)),
                    includeRemoteIdOption == true ? (DHCPv6PacketOption)( new DHCPv6PacketRemoteIdentifierOption(3561,remoteId)) : new DHCPv6PacketTrueOption(DHCPv6PacketOptionTypes.RapitCommit)
                }, innerPacket);

            DHCPv6Packet outerRelayPacket = DHCPv6RelayPacket.AsOuterRelay(headerInformation, true, 1, random.GetIPv6Address(), random.GetIPv6Address(),
                new DHCPv6PacketOption[]
                {
                                new DHCPv6PacketByteArrayOption(DHCPv6PacketOptionTypes.InterfaceId, random.NextBytes(10)),
                                new DHCPv6PacketRemoteIdentifierOption(9,random.NextBytes(12))
                }, innerRelayPacket);

            return outerRelayPacket;
        }

        [Fact]
        public void GetUniqueIdentifier()
        {
            Random random = new Random();
            var resolver = new DHCPv6MilegateResolver();

            Byte[] identifierValue = random.NextBytes(40);

            DHCPv6Packet packet = GetPacket(random, identifierValue);
            Assert.Equal(identifierValue, resolver.GetUniqueIdentifier(packet));
        }

        [Theory]
        [InlineData("My name", "2", "true", true)]
        [InlineData("My name", "0", "false", true)]
        [InlineData("", "2", "true", false)]
        [InlineData("My name", "-2", "true", false)]
        [InlineData("My name", "abc", "true", false)]
        [InlineData("My name", "0", "false23", false)]

        public void ArePropertiesAndValuesValid(String value, String index, String insensitveMatch, Boolean shouldBeValid)
        {
            Mock<ISerializer> serializerMock = new Mock<ISerializer>(MockBehavior.Strict);

            serializerMock.Setup(x => x.Deserialze<String>(value)).Returns(value).Verifiable();
            serializerMock.Setup(x => x.Deserialze<String>(index)).Returns(index).Verifiable();
            serializerMock.Setup(x => x.Deserialze<String>(insensitveMatch)).Returns(insensitveMatch).Verifiable();

            var resolver = new DHCPv6MilegateResolver();
            Boolean actual = resolver.ArePropertiesAndValuesValid(new Dictionary<String, String> {
               { "Value", value },
               { "Index", index },
               { "IsCaseSenstiveMatch", insensitveMatch },
            }, serializerMock.Object);

            Assert.Equal(shouldBeValid, actual);

            serializerMock.Verify(x => x.Deserialze<String>(value), Times.AtMostOnce());
            serializerMock.Verify(x => x.Deserialze<String>(index), Times.AtMostOnce());
            serializerMock.Verify(x => x.Deserialze<String>(insensitveMatch), Times.AtMostOnce());
        }

        [Fact]
        public void ArePropertiesAndValuesValid_KeyIsMissing()
        {
            var input = new[]{
                new  Dictionary<String,String>{
                    { "Value2", "something" },
                    { "Index", "0" },
                    { "IsCaseSenstiveMatch", "true" },

                },
                new  Dictionary<String,String>{
                    { "Value", "something" },
                    { "Index2", "0" },
                    { "IsCaseSenstiveMatch", "true" },
                },
                new  Dictionary<String,String>{
                    { "Value", "something" },
                    { "IsCaseSenstiveMatch", "true" },

                },
                new  Dictionary<String,String>{
                    { "Index", "0" },
                    { "IsCaseSenstiveMatch", "true" },
                },
                  new  Dictionary<String,String>{
                    { "Value", "something" },
                    { "Index", "0" },
                    { "IsCaseSenstiveMatch2", "true" },
                },
                };

            var resolver = new DHCPv6MilegateResolver();

            foreach (var item in input)
            {
                Boolean actual = resolver.ArePropertiesAndValuesValid(item, Mock.Of<ISerializer>(MockBehavior.Strict));
                Assert.False(actual);
            }
        }

        [Fact]
        public void ApplyValues()
        {
            Random random = new Random();

            String value = random.GetAlphanumericString();
            UInt16 index = (UInt16)random.Next(0, 10);
            Boolean caseInsensitveMatch = random.NextBoolean();

            Mock<ISerializer> serializerMock = new Mock<ISerializer>(MockBehavior.Strict);
            serializerMock.Setup(x => x.Deserialze<String>(value)).Returns(value).Verifiable();
            serializerMock.Setup(x => x.Deserialze<UInt16>(index.ToString())).Returns(index).Verifiable();
            serializerMock.Setup(x => x.Deserialze<Boolean>(caseInsensitveMatch == true ? "true" : "false")).Returns(caseInsensitveMatch).Verifiable();

            var resolver = new DHCPv6MilegateResolver();
            resolver.ApplyValues(new Dictionary<String, String> {
                   { "Value", value },
                    { "Index", index.ToString() },
                    { "IsCaseSenstiveMatch", caseInsensitveMatch == true ? "true" : "false" },
            }, serializerMock.Object);

            Assert.Equal(value, resolver.Value);
            Assert.Equal(index, resolver.Index);

            serializerMock.Verify();

            Dictionary<String, String> expectedValues = new Dictionary<string, string>
            {
                { "Value", value },
                { "IsCaseSenstiveMatch", caseInsensitveMatch == true ? "true" : "false" },
                { "Index", index.ToString() },
            };

            Assert.Equal(expectedValues.ToArray(), resolver.GetValues().ToArray());
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void PacketMeetsCondition_CaseSenstive(Boolean shouldMeetCondition)
        {
            Random random = new Random();

            String value = random.GetAlphanumericString();
            Byte[] remoteIdentifierAsBytes;
            if (shouldMeetCondition == true)
            {
                remoteIdentifierAsBytes = ASCIIEncoding.ASCII.GetBytes(value);
            }
            else
            {
                remoteIdentifierAsBytes = random.NextBytes(10);
            }

            DHCPv6Packet packet = GetPacket(random, remoteIdentifierAsBytes);

            Mock<ISerializer> serializerMock = new Mock<ISerializer>(MockBehavior.Strict);
            serializerMock.Setup(x => x.Deserialze<String>(value)).Returns(value).Verifiable();
            serializerMock.Setup(x => x.Deserialze<UInt16>("0")).Returns(0).Verifiable();
            serializerMock.Setup(x => x.Deserialze<Boolean>("true")).Returns(true).Verifiable();

            var resolver = new DHCPv6MilegateResolver();
            resolver.ApplyValues(new Dictionary<String, String> {
                   { "Value", value },
                   { "Index", "0" },
                   { "IsCaseSenstiveMatch", "true" },
            }, serializerMock.Object);

            Boolean result = resolver.PacketMeetsCondition(packet);
            Assert.Equal(shouldMeetCondition, result);

            serializerMock.Verify();
        }

      

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void PacketMeetsCondition_CaseInenstive(Boolean shouldMeetCondition)
        {
            Random random = new Random();

            String value = random.GetAlphanumericString();
            Byte[] remoteIdentifierAsBytes;
            if (shouldMeetCondition == true)
            {
                String valueToInsert = random.RandomizeUpperAndLowerChars(value);
                remoteIdentifierAsBytes = ASCIIEncoding.ASCII.GetBytes(valueToInsert);
            }
            else
            {
                remoteIdentifierAsBytes = random.NextBytes(10);
            }

            DHCPv6Packet packet = GetPacket(random, remoteIdentifierAsBytes);

            Mock<ISerializer> serializerMock = new Mock<ISerializer>(MockBehavior.Strict);
            serializerMock.Setup(x => x.Deserialze<String>(value)).Returns(value).Verifiable();
            serializerMock.Setup(x => x.Deserialze<UInt16>("0")).Returns(0).Verifiable();
            serializerMock.Setup(x => x.Deserialze<Boolean>("false")).Returns(false).Verifiable();

            var resolver = new DHCPv6MilegateResolver();
            resolver.ApplyValues(new Dictionary<String, String> {
                   { "Value", value },
                   { "Index", "0" },
                   { "IsCaseSenstiveMatch", "false" },
            }, serializerMock.Object);

            Boolean result = resolver.PacketMeetsCondition(packet);
            Assert.Equal(shouldMeetCondition, result);

            serializerMock.Verify();
        }
        [Fact]
        public void PacketMeetsCondition_False_NotRelay()
        {
            Random random = new Random();
            String value = random.GetAlphanumericString();

            IPv6HeaderInformation headerInformation =
                new IPv6HeaderInformation(IPv6Address.FromString("fe80::1"), IPv6Address.FromString("fe80::2"));

            var packetOptions = new List<DHCPv6PacketOption>
                {
                    new DHCPv6PacketTrueOption(DHCPv6PacketOptionTypes.RapitCommit),
                };

            DHCPv6Packet packet = DHCPv6Packet.AsOuter(headerInformation, random.NextUInt16(),
                DHCPv6PacketTypes.Solicit, packetOptions);

            Mock<ISerializer> serializerMock = new Mock<ISerializer>(MockBehavior.Strict);
            serializerMock.Setup(x => x.Deserialze<String>(value)).Returns(value).Verifiable();
            serializerMock.Setup(x => x.Deserialze<UInt16>("0")).Returns(0).Verifiable();
            serializerMock.Setup(x => x.Deserialze<Boolean>("true")).Returns(true).Verifiable();

            var resolver = new DHCPv6MilegateResolver();
            resolver.ApplyValues(new Dictionary<String, String> {
                   { "Value", value },
                    { "Index", "0" },
                   { "IsCaseSenstiveMatch", "true" },

            }, serializerMock.Object);

            Boolean result = resolver.PacketMeetsCondition(packet);
            Assert.False(result);

            serializerMock.Verify();
        }

        [Fact]
        public void PacketMeetsCondition_False_IndexNotPresented()
        {
            Random random = new Random();

            String value = random.GetAlphanumericString();
            Byte[] remoteIdentifierAsBytes = ASCIIEncoding.ASCII.GetBytes(value);

            DHCPv6Packet packet = GetPacket(random, remoteIdentifierAsBytes);

            Mock<ISerializer> serializerMock = new Mock<ISerializer>(MockBehavior.Strict);
            serializerMock.Setup(x => x.Deserialze<String>(value)).Returns(value).Verifiable();
            serializerMock.Setup(x => x.Deserialze<UInt16>("2")).Returns(2).Verifiable();
            serializerMock.Setup(x => x.Deserialze<Boolean>("true")).Returns(true).Verifiable();

            var resolver = new DHCPv6MilegateResolver();
            resolver.ApplyValues(new Dictionary<String, String> {
                   { "Value", value },
                    { "Index", "2" },
                   { "IsCaseSenstiveMatch", "true" },

            }, serializerMock.Object);

            Boolean result = resolver.PacketMeetsCondition(packet);
            Assert.False(result);

            serializerMock.Verify();
        }

        [Fact]
        public void PacketMeetsCondition_False_OptionNotPresented()
        {
            Random random = new Random();

            String value = random.GetAlphanumericString();
            Byte[] remoteIdentifierAsBytes = ASCIIEncoding.ASCII.GetBytes(value);

            DHCPv6Packet packet = GetPacket(random, remoteIdentifierAsBytes, false);

            Mock<ISerializer> serializerMock = new Mock<ISerializer>(MockBehavior.Strict);
            serializerMock.Setup(x => x.Deserialze<String>(value)).Returns(value).Verifiable();
            serializerMock.Setup(x => x.Deserialze<UInt16>("0")).Returns(0).Verifiable();
            serializerMock.Setup(x => x.Deserialze<Boolean>("true")).Returns(true).Verifiable();

            var resolver = new DHCPv6MilegateResolver();
            resolver.ApplyValues(new Dictionary<String, String> {
                   { "Value", value },
                    { "Index", "0" },
                   { "IsCaseSenstiveMatch", "true" },

            }, serializerMock.Object);

            Boolean result = resolver.PacketMeetsCondition(packet);
            Assert.False(result);

            serializerMock.Verify();
        }

        [Fact]
        public void GetDescription()
        {
            var expected = new ScopeResolverDescription("DHCPv6MilegateResolver", new[] {
            new ScopeResolverPropertyDescription("Value",ScopeResolverPropertyDescription.ScopeResolverPropertyValueTypes.String),
            new ScopeResolverPropertyDescription("Index",ScopeResolverPropertyDescription.ScopeResolverPropertyValueTypes.UInt32),
            new ScopeResolverPropertyDescription("IsCaseSenstiveMatch",ScopeResolverPropertyDescription.ScopeResolverPropertyValueTypes.Boolean),

            });

            var resolver = new DHCPv6MilegateResolver();
            var actual = resolver.GetDescription();

            Assert.Equal(expected, actual);
        }

    }
}
