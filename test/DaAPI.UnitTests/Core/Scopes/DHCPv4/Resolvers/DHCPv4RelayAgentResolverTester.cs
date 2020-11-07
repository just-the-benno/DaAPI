using DaAPI.Core.Common;
using DaAPI.Core.Packets.DHCPv4;
using DaAPI.Core.Scopes;
using DaAPI.Core.Scopes.DHCPv4;
using DaAPI.Core.Services;
using DaAPI.TestHelper;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using static DaAPI.Core.Scopes.ScopeResolverPropertyDescription;

namespace DaAPI.UnitTests.Core.Scopes.DHCPv4.Resolvers
{
    public class DHCPv4RelayAgentResolverTester
    {
        [Fact]
        public void DHCPv4RelayAgentResolver_GetDescription()
        {
            DHCPv4RelayAgentResolver resolver = new DHCPv4RelayAgentResolver(Mock.Of<ILogger<DHCPv4RelayAgentResolver>>(), Mock.Of<ISerializer>());

            ScopeResolverDescription description = resolver.GetDescription();
            Assert.NotNull(description);

            Assert.Equal(nameof(DHCPv4RelayAgentResolver), description.TypeName);

            Assert.NotNull(description.Properties);
            Assert.Single(description.Properties);

            ScopeResolverPropertyDescription propertyDescription = description.Properties.First();
            Assert.Equal(nameof(DHCPv4RelayAgentResolver.AgentAddresses), propertyDescription.PropertyName);
            Assert.Equal(ScopeResolverPropertyValueTypes.IPv4AddressList, propertyDescription.PropertyValueType);
        }

        [Fact]
        public void DHCPv4RelayAgentResolver_AreValuesValid_MissingKeys()
        {
            Random random = new Random();
            String emptyListValue = random.GetAlphanumericString(30);

            var mock = new Mock<ISerializer>(MockBehavior.Strict);
            mock.Setup(x => x.Deserialze<IEnumerable<IPv4Address>>(emptyListValue)).Returns(new List<IPv4Address>());

            DHCPv4RelayAgentResolver resolver = new DHCPv4RelayAgentResolver(Mock.Of<ILogger<DHCPv4RelayAgentResolver>>(), mock.Object);

            List<Dictionary<String, String>> invalidInputs = new List<Dictionary<string, string>>
            {
                null,
                new Dictionary<string, string>(),
                new Dictionary<string, string>()
                {
                    { random.GetAlphanumericString(10), random.GetAlphanumericString(10)   }
                },
                new Dictionary<string, string>()
                {
                    { nameof(DHCPv4RelayAgentResolver.AgentAddresses), "" },
                },
                new Dictionary<string, string>()
                {
                    { nameof(DHCPv4RelayAgentResolver.AgentAddresses), emptyListValue },
                }
            };

            foreach (var item in invalidInputs)
            {
                Boolean result = resolver.ArePropertiesAndValuesValid(item);
                Assert.False(result);
            }
        }

        [Fact]
        public void DHCPv4RelayAgentResolver_AreValuesValid_Valid()
        {
            Random random = new Random();
            String value = random.GetAlphanumericString(30);

            var mock = new Mock<ISerializer>(MockBehavior.Strict);
            mock.Setup(x => x.Deserialze<IEnumerable<IPv4Address>>(value)).Returns(random.GetIPv4Addresses());

            DHCPv4RelayAgentResolver resolver = new DHCPv4RelayAgentResolver(Mock.Of<ILogger<DHCPv4RelayAgentResolver>>(), mock.Object);

            var input = new Dictionary<string, string>()
                {
                    { nameof(DHCPv4RelayAgentResolver.AgentAddresses), value },
                };

            Boolean result = resolver.ArePropertiesAndValuesValid(input);
            Assert.True(result);
        }

        [Fact]
        public void DHCPv4RelayAgentResolver_ApplyValues()
        {
            Random random = new Random();
            String value = random.GetAlphanumericString(30);
            List<IPv4Address> agentAddresses = random.GetIPv4Addresses();

            var mock = new Mock<ISerializer>(MockBehavior.Strict);
            mock.Setup(x => x.Deserialze<IEnumerable<IPv4Address>>(value)).Returns(agentAddresses);

            DHCPv4RelayAgentResolver resolver = new DHCPv4RelayAgentResolver(Mock.Of<ILogger<DHCPv4RelayAgentResolver>>(), mock.Object);

            var input = new Dictionary<string, string>()
                {
                    { nameof(DHCPv4RelayAgentResolver.AgentAddresses), value },
                };

            resolver.ApplyValues(input);
            Assert.Equal(agentAddresses, resolver.AgentAddresses);
        }

        [Fact]
        public void DHCPv4RelayAgentResolver_PacketMeetsConditions()
        {
            Random random = new Random();
            List<IPv4Address> addresses = random.GetIPv4Addresses();

            String inputValue = random.GetAlphanumericString(20);

            Mock<ISerializer> serializer = new Mock<ISerializer>(MockBehavior.Strict);
            serializer.Setup(x => x.Deserialze<IEnumerable<IPv4Address>>(inputValue)).Returns(addresses);

            DHCPv4RelayAgentResolver resolver = new DHCPv4RelayAgentResolver(Mock.Of<ILogger<DHCPv4RelayAgentResolver>>(), serializer.Object);
            Dictionary<String, String> values = new Dictionary<String, String>() { { nameof(DHCPv4RelayAgentResolver.AgentAddresses), inputValue } };
            resolver.ApplyValues(values);

            foreach (IPv4Address item in addresses)
            {
                Boolean shouldPass = false;

                IPv4Address gwAddress = IPv4Address.Empty;
                if (random.NextDouble() > 0.5)
                {
                    gwAddress = item;
                    shouldPass = true;
                }
                else
                {
                    if (random.NextDouble() > 0.5)
                    {
                        gwAddress = random.GetIPv4Address();
                    }
                }

                DHCPv4Packet packet = new DHCPv4Packet(
                  new IPv4HeaderInformation(random.GetIPv4Address(), random.GetIPv4Address()),
                  random.NextBytes(6),
                  (UInt32)random.Next(),
                  IPv4Address.Empty,
                  gwAddress,
                  IPv4Address.Empty
                  );

                Boolean actual = resolver.PacketMeetsCondition(packet);
                Assert.Equal(shouldPass, actual);
            }
        }
    }
}
