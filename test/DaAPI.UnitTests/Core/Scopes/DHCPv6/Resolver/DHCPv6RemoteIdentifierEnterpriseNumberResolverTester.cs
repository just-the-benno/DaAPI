using Castle.Core.Logging;
using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Packets.DHCPv6;
using DaAPI.Core.Scopes.DHCPv6.Resolvers;
using DaAPI.Core.Services;
using DaAPI.TestHelper;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace DaAPI.UnitTests.Core.Scopes.DHCPv6.Resolvers
{
    public class DHCPv6RemoteIdentifierEnterpriseNumberResolverTester
    {
        [Theory]
        [InlineData(-2)]
        [InlineData(null)]
        [InlineData(23)]
        public void ApplyValues(Int32? agentIndex)
        {
            Random random = new Random();

            UInt32 enterpriseNumber = random.NextUInt32();

            DHCPv6RemoteIdentifierEnterpriseNumberResolver resolver = new DHCPv6RemoteIdentifierEnterpriseNumberResolver(
                Mock.Of<ILogger<DHCPv6RemoteIdentifierEnterpriseNumberResolver>>());

            Mock<ISerializer> serializerMock = new Mock<ISerializer>(MockBehavior.Strict);
            serializerMock.Setup(x => x.Deserialze<UInt32>(enterpriseNumber.ToString())).Returns(enterpriseNumber).Verifiable();
            serializerMock.Setup(x => x.Deserialze<Int32?>(agentIndex.ToString())).Returns(agentIndex).Verifiable();

            resolver.ApplyValues(new Dictionary<String, String>
            {
                { "EnterpriseNumber", enterpriseNumber.ToString() },
                { "RelayAgentIndex", agentIndex.ToString() },
            }, serializerMock.Object);

            Assert.Equal(enterpriseNumber, resolver.EnterpriseNumber);
            Assert.Equal(agentIndex, resolver.RelayAgentIndex);

            serializerMock.Verify();

            var expectedValues = new Dictionary<String, String>
            {
                { "EnterpriseNumber", enterpriseNumber.ToString() },
                { "RelayAgentIndex", agentIndex.ToString() },
            };

            Assert.Equal(expectedValues.ToArray(), resolver.GetValues().ToArray());
        }

        [Fact]
        public void HasUniqueIdentiifer()
        {
            DHCPv6RemoteIdentifierEnterpriseNumberResolver resolver = new DHCPv6RemoteIdentifierEnterpriseNumberResolver(
    Mock.Of<ILogger<DHCPv6RemoteIdentifierEnterpriseNumberResolver>>());

            Assert.False(resolver.HasUniqueIdentifier);
        }

        [Fact]
        public void GetUniqueIdentifier()
        {
            DHCPv6RemoteIdentifierEnterpriseNumberResolver resolver = new DHCPv6RemoteIdentifierEnterpriseNumberResolver(
    Mock.Of<ILogger<DHCPv6RemoteIdentifierEnterpriseNumberResolver>>());

            Assert.ThrowsAny<Exception>(() => resolver.GetUniqueIdentifier(null));
        }

        [Theory]
        [InlineData(-2)]
        [InlineData(null)]
        [InlineData(23)]
        public void ArePropertiesAndValuesValid(Int32? agentIndex)
        {
            Random random = new Random();

            UInt32 enterpriseNumber = random.NextUInt32();

            DHCPv6RemoteIdentifierEnterpriseNumberResolver resolver = new DHCPv6RemoteIdentifierEnterpriseNumberResolver(
                Mock.Of<ILogger<DHCPv6RemoteIdentifierEnterpriseNumberResolver>>());

            Mock<ISerializer> serializerMock = new Mock<ISerializer>(MockBehavior.Strict);
            serializerMock.Setup(x => x.Deserialze<UInt32>(enterpriseNumber.ToString())).Returns(enterpriseNumber).Verifiable();
            serializerMock.Setup(x => x.Deserialze<Int32?>(agentIndex.ToString())).Returns(agentIndex).Verifiable();

            Boolean actual = resolver.ArePropertiesAndValuesValid(new Dictionary<String, String>
            {
                { "EnterpriseNumber", enterpriseNumber.ToString() },
                { "RelayAgentIndex", agentIndex.ToString() },
            }, serializerMock.Object);

            Assert.True(actual);

            serializerMock.Verify();

        }

        [Fact]
        public void ArePropertiesAndValuesValid_Failed_PropertiesMissing()
        {
            Random random = new Random();

            UInt32 enterpriseNumber = random.NextUInt32();

            DHCPv6RemoteIdentifierEnterpriseNumberResolver resolver = new DHCPv6RemoteIdentifierEnterpriseNumberResolver(
                Mock.Of<ILogger<DHCPv6RemoteIdentifierEnterpriseNumberResolver>>());

            List<Dictionary<String, String>> inputs = new List<Dictionary<string, string>>
            {
                new Dictionary<String, String>
                {
                    { "EnterpriseNumber2", enterpriseNumber.ToString() },
                    { "RelayAgentIndex",4.ToString() },
                },
                new Dictionary<String, String>
                {
                    { "EnterpriseNumber", enterpriseNumber.ToString() },
                    { "RelayAgentIndex2", 4.ToString() },
                },
                null
            };

            foreach (var item in inputs)
            {
                Boolean result = resolver.ArePropertiesAndValuesValid(item, Mock.Of<ISerializer>(MockBehavior.Strict));
                Assert.False(result);
            }
        }

        [Fact]
        public void ArePropertiesAndValuesValid_Failed_PropertiesAreNotSeralizble()
        {
            Random random = new Random();

            UInt32 enterpriseNumber = random.NextUInt32();

            DHCPv6RemoteIdentifierEnterpriseNumberResolver resolver = new DHCPv6RemoteIdentifierEnterpriseNumberResolver(
                Mock.Of<ILogger<DHCPv6RemoteIdentifierEnterpriseNumberResolver>>());

            List<Dictionary<String, String>> inputs = new List<Dictionary<string, string>>
            {
                new Dictionary<String, String>
                {
                    { "EnterpriseNumber", enterpriseNumber.ToString() },
                    { "RelayAgentIndex",4.ToString() },
                },
                new Dictionary<String, String>
                {
                    { "EnterpriseNumber", enterpriseNumber.ToString() },
                    { "RelayAgentIndex", 4.ToString() },
                }
            };

            foreach (var item in inputs)
            {
                Boolean result = resolver.ArePropertiesAndValuesValid(item, Mock.Of<ISerializer>(MockBehavior.Strict));
                Assert.False(result);
            }
        }

        private static DHCPv6RelayPacket GeneratePacket(Random random, uint enterpriseNumber)
        {
            return DHCPv6RelayPacket.AsOuterRelay(
                new IPv6HeaderInformation(IPv6Address.FromString("fe80::1"), IPv6Address.FromString("fe80::2")), true, 0,
                IPv6Address.FromString("fe80::1"), IPv6Address.FromString("fe80::1"), new DHCPv6PacketOption[]
                {
                    new DHCPv6PacketRemoteIdentifierOption(enterpriseNumber,random.NextBytes(20)),
                }, DHCPv6Packet.AsInner(random.NextUInt16(), DHCPv6PacketTypes.Solicit, Array.Empty<DHCPv6PacketOption>()));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(null)]
        public void PacketMeetsCondition(Int32? agentIndex)
        {
            Random random = new Random();

            UInt32 enterpriseNumber = random.NextUInt32();

            DHCPv6RemoteIdentifierEnterpriseNumberResolver resolver = new DHCPv6RemoteIdentifierEnterpriseNumberResolver(
                Mock.Of<ILogger<DHCPv6RemoteIdentifierEnterpriseNumberResolver>>());

            Mock<ISerializer> serializerMock = new Mock<ISerializer>(MockBehavior.Strict);
            serializerMock.Setup(x => x.Deserialze<UInt32>(enterpriseNumber.ToString())).Returns(enterpriseNumber).Verifiable();
            serializerMock.Setup(x => x.Deserialze<Int32?>(agentIndex.ToString())).Returns(agentIndex).Verifiable();

            resolver.ApplyValues(new Dictionary<String, String>
            {
                { "EnterpriseNumber", enterpriseNumber.ToString() },
                { "RelayAgentIndex", agentIndex.ToString() },
            }, serializerMock.Object);
            DHCPv6RelayPacket packet = GeneratePacket(random, enterpriseNumber);

            Boolean result = resolver.PacketMeetsCondition(packet);
            Assert.True(result);

            serializerMock.Verify();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(null)]
        public void PacketMeetsCondition_DifferentEnterpriseId(Int32? agentIndex)
        {
            Random random = new Random();

            UInt32 enterpriseNumber = random.NextUInt32();
            UInt32 differenEnterpriseNumber = random.NextUInt32();

            DHCPv6RemoteIdentifierEnterpriseNumberResolver resolver = new DHCPv6RemoteIdentifierEnterpriseNumberResolver(
                Mock.Of<ILogger<DHCPv6RemoteIdentifierEnterpriseNumberResolver>>());

            Mock<ISerializer> serializerMock = new Mock<ISerializer>(MockBehavior.Strict);
            serializerMock.Setup(x => x.Deserialze<UInt32>(differenEnterpriseNumber.ToString())).Returns(differenEnterpriseNumber).Verifiable();
            serializerMock.Setup(x => x.Deserialze<Int32?>(agentIndex.ToString())).Returns(agentIndex).Verifiable();

            resolver.ApplyValues(new Dictionary<String, String>
            {
                { "EnterpriseNumber", differenEnterpriseNumber.ToString() },
                { "RelayAgentIndex", agentIndex.ToString() },
            }, serializerMock.Object);
            DHCPv6RelayPacket packet = GeneratePacket(random, enterpriseNumber);

            Boolean result = resolver.PacketMeetsCondition(packet);
            Assert.False(result);

            serializerMock.Verify();
        }

        [Fact]
        public void PacketMeetsCondition_False_AgentIndexNotFound()
        {
            Random random = new Random();

            UInt32 enterpriseNumber = random.NextUInt32();
            Int32 agentIndex = 2;

            DHCPv6RemoteIdentifierEnterpriseNumberResolver resolver = new DHCPv6RemoteIdentifierEnterpriseNumberResolver(
                Mock.Of<ILogger<DHCPv6RemoteIdentifierEnterpriseNumberResolver>>());

            Mock<ISerializer> serializerMock = new Mock<ISerializer>(MockBehavior.Strict);
            serializerMock.Setup(x => x.Deserialze<UInt32>(enterpriseNumber.ToString())).Returns(enterpriseNumber).Verifiable();
            serializerMock.Setup(x => x.Deserialze<Int32?>(agentIndex.ToString())).Returns(agentIndex).Verifiable();

            resolver.ApplyValues(new Dictionary<String, String>
            {
                { "EnterpriseNumber", enterpriseNumber.ToString() },
                { "RelayAgentIndex", agentIndex.ToString() },
            }, serializerMock.Object);
            DHCPv6RelayPacket packet = GeneratePacket(random, enterpriseNumber);

            Boolean result = resolver.PacketMeetsCondition(packet);
            Assert.False(result);

            serializerMock.Verify();
        }

        [Fact]
        public void PacketMeetsCondition_False_WrongPacketType()
        {
            Random random = new Random();

            UInt32 enterpriseNumber = random.NextUInt32();
            Int32 agentIndex = 2;

            DHCPv6RemoteIdentifierEnterpriseNumberResolver resolver = new DHCPv6RemoteIdentifierEnterpriseNumberResolver(
                Mock.Of<ILogger<DHCPv6RemoteIdentifierEnterpriseNumberResolver>>());

            Mock<ISerializer> serializerMock = new Mock<ISerializer>(MockBehavior.Strict);
            serializerMock.Setup(x => x.Deserialze<UInt32>(enterpriseNumber.ToString())).Returns(enterpriseNumber).Verifiable();
            serializerMock.Setup(x => x.Deserialze<Int32?>(agentIndex.ToString())).Returns(agentIndex).Verifiable();

            resolver.ApplyValues(new Dictionary<String, String>
            {
                { "EnterpriseNumber", enterpriseNumber.ToString() },
                { "RelayAgentIndex", agentIndex.ToString() },
            }, serializerMock.Object);
            DHCPv6Packet packet = DHCPv6Packet.AsInner(random.NextUInt16(), DHCPv6PacketTypes.Solicit, Array.Empty<DHCPv6PacketOption>());

            Boolean result = resolver.PacketMeetsCondition(packet);
            Assert.False(result);

            serializerMock.Verify();
        }

    }
}
