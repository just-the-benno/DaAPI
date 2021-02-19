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
using System.Text;
using Xunit;
using static DaAPI.Core.Scopes.ScopeResolverPropertyDescription;

namespace DaAPI.UnitTests.Core.Scopes.DHCPv4.Resolvers
{
    public class DHCPv4RelayAgentSubnetResolverTester
    {
        [Fact]
        public void DHCPv4RelayAgentSubnetResolver_GetDescription()
        {
            List<ScopeResolverPropertyDescription> expectedDescriptions = new List<ScopeResolverPropertyDescription>
            {
                new ScopeResolverPropertyDescription("Mask",ScopeResolverPropertyValueTypes.IPv4Subnetmask),
                new ScopeResolverPropertyDescription("NetworkAddress", ScopeResolverPropertyValueTypes.IPv4Address),
            };

            DHCPv4RelayAgentSubnetResolver resolver = new DHCPv4RelayAgentSubnetResolver();

            ScopeResolverDescription description = resolver.GetDescription();
            Assert.NotNull(description);

            Assert.Equal("DHCPv4RelayAgentSubnetResolver", description.TypeName);

            Assert.NotNull(description.Properties);

            Assert.Equal(expectedDescriptions.Count, description.Properties.Count());

            foreach (var item in description.Properties)
            {
                ScopeResolverPropertyDescription expectedDescription = expectedDescriptions.FirstOrDefault(x => x.PropertyName == item.PropertyName);
                Assert.NotNull(expectedDescription);

                Assert.Equal(expectedDescription.PropertyValueType, item.PropertyValueType);
                Assert.Equal(expectedDescription.PropertyName, item.PropertyName);

                expectedDescriptions.Remove(expectedDescription);
            }

            Assert.Empty(expectedDescriptions);
        }

        [Fact]
        public void DHCPv4RelayAgentSubnetResolver_AreValuesValid_MissingKeys()
        {
            Random random = new Random();
            String validMaskValue = random.GetAlphanumericString(30);
            String validNetworkAddressValue = random.GetAlphanumericString(30);

            IPv4SubnetMask subnetMask = random.GetSubnetmask();
            IPv4Address address = (random.GetIPv4NetworkAddress(subnetMask)) - 1;

            var mock = new Mock<ISerializer>(MockBehavior.Strict);
            mock.Setup(x => x.Deserialze<IPv4SubnetMask>(validMaskValue)).Returns(subnetMask);
            mock.Setup(x => x.Deserialze<IPv4Address>(validNetworkAddressValue)).Returns(address);

            DHCPv4RelayAgentSubnetResolver resolver = new DHCPv4RelayAgentSubnetResolver();

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
                    { nameof(DHCPv4RelayAgentSubnetResolver.Mask), random.GetAlphanumericString(10)  },
                },
                new Dictionary<string, string>()
                {
                    { nameof(DHCPv4RelayAgentSubnetResolver.Mask), random.GetAlphanumericString(10)  },
                    { nameof(DHCPv4RelayAgentSubnetResolver.NetworkAddress), random.GetAlphanumericString(10)  },
                },
                 new Dictionary<string, string>()
                {
                    { nameof(DHCPv4RelayAgentSubnetResolver.Mask), validMaskValue  },
                    { nameof(DHCPv4RelayAgentSubnetResolver.NetworkAddress), random.GetAlphanumericString(10)  },
                },
                new Dictionary<string, string>()
                {
                    { nameof(DHCPv4RelayAgentSubnetResolver.Mask), random.GetAlphanumericString(10)  },
                    { nameof(DHCPv4RelayAgentSubnetResolver.NetworkAddress), validNetworkAddressValue  },
                },
                   new Dictionary<string, string>()
                {
                    { nameof(DHCPv4RelayAgentSubnetResolver.Mask), validMaskValue  },
                    { nameof(DHCPv4RelayAgentSubnetResolver.NetworkAddress), validNetworkAddressValue  },
                },
            };

            foreach (var item in invalidInputs)
            {
                Boolean result = resolver.ArePropertiesAndValuesValid(item, mock.Object);
                Assert.False(result);
            }
        }

        [Fact]
        public void DHCPv4RelayAgentSubnetResolver_AreValuesValid_Valid()
        {
            Random random = new Random();

            String validMaskValue = ((Byte)random.Next(10, 20)).ToString();

            IPv4SubnetMask subnetMask = new IPv4SubnetMask(new IPv4SubnetMaskIdentifier(Convert.ToInt32(validMaskValue)));
            IPv4Address address = random.GetIPv4NetworkAddress(subnetMask);

            var mock = new Mock<ISerializer>(MockBehavior.Strict);
            mock.Setup(x => x.Deserialze<String>(validMaskValue)).Returns(subnetMask.GetSlashNotation().ToString());
            mock.Setup(x => x.Deserialze<String>(address.ToString())).Returns(address.ToString());

            DHCPv4RelayAgentSubnetResolver resolver = new DHCPv4RelayAgentSubnetResolver();

            var input = new Dictionary<string, string>()
                {
                    { nameof(DHCPv4RelayAgentSubnetResolver.Mask), validMaskValue  },
                    { nameof(DHCPv4RelayAgentSubnetResolver.NetworkAddress), address.ToString()  },
                };

            Boolean result = resolver.ArePropertiesAndValuesValid(input, mock.Object);
            Assert.True(result);
        }

        [Fact]
        public void DHCPv4RelayAgentSubnetResolver_ApplyValues()
        {
            Random random = new Random();
            String validMaskValue = ((Byte)random.Next(1,32)).ToString();
            String validNetworkAddressValue = random.GetIPv4Address().ToString();

            IPv4SubnetMask subnetMask = new IPv4SubnetMask(new IPv4SubnetMaskIdentifier(Convert.ToInt32(validMaskValue)));
            IPv4Address address = IPv4Address.FromString(validNetworkAddressValue);

            var mock = new Mock<ISerializer>(MockBehavior.Strict);
            mock.Setup(x => x.Deserialze<String>(validMaskValue)).Returns(subnetMask.GetSlashNotation().ToString());
            mock.Setup(x => x.Deserialze<String>(validNetworkAddressValue)).Returns(address.ToString());

            DHCPv4RelayAgentSubnetResolver resolver = new DHCPv4RelayAgentSubnetResolver();

            var input = new Dictionary<string, string>()
                {
                    { nameof(DHCPv4RelayAgentSubnetResolver.Mask), validMaskValue  },
                    { nameof(DHCPv4RelayAgentSubnetResolver.NetworkAddress), validNetworkAddressValue  },
                };

            resolver.ApplyValues(input, mock.Object);

            Assert.Equal(subnetMask, resolver.Mask);
            Assert.Equal(address, resolver.NetworkAddress);
        }

        [Fact]
        public void DHCPv4RelayAgentSubnetResolver_PacketMeetsConditions()
        {
            Random random = new Random();

            Int32 maskIdentifier = random.Next(20, 24);

            IPv4SubnetMask mask = new IPv4SubnetMask(new IPv4SubnetMaskIdentifier(maskIdentifier));
            IPv4Address addresses = random.GetIPv4NetworkAddress(mask);

            Mock<ISerializer> serializer = new Mock<ISerializer>(MockBehavior.Strict);
            serializer.Setup(x => x.Deserialze<String>(mask.GetSlashNotation().ToString())).Returns(mask.GetSlashNotation().ToString());
            serializer.Setup(x => x.Deserialze<String>(addresses.ToString())).Returns(addresses.ToString());

            DHCPv4RelayAgentSubnetResolver resolver = new DHCPv4RelayAgentSubnetResolver();
            Dictionary<String, String> values = new Dictionary<String, String>() {
                { nameof(DHCPv4RelayAgentSubnetResolver.NetworkAddress), addresses.ToString() },
                { nameof(DHCPv4RelayAgentSubnetResolver.Mask), mask.GetSlashNotation().ToString() },
            };

            resolver.ApplyValues(values, serializer.Object);

            Int32 trys = random.Next(20, 30);

            for (int i = 0; i < trys; i++)
            {
                Boolean shouldPass = false;
                IPv4Address gwAddress = IPv4Address.Empty;

                if (random.NextDouble() > 0.5)
                {
                    gwAddress = random.GetIPv4AddressWithinSubnet(mask, addresses);
                    shouldPass = true;
                }
                else
                {
                    if (random.NextDouble() > 0.5)
                    {
                        gwAddress = random.GetIPv4AddressOutWithSubnet(mask, addresses);
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

        [Theory]
        [InlineData("192.168.178.0", "255.255.255.0", "192.168.178.1", true)]
        [InlineData("192.168.178.0", "255.255.255.0", "192.168.178.100", true)]
        [InlineData("192.168.178.0", "255.255.255.0", "192.168.178.212", true)]
        //[InlineData("192.168.178.0", "255.255.255.0", "192.168.178.0", false)]
        [InlineData("192.168.178.0", "255.255.255.0", "192.168.178.0", true)]
        //[InlineData("192.168.178.0", "255.255.255.0", "192.168.178.255", false)]
        [InlineData("192.168.178.0", "255.255.255.0", "192.168.178.255", true)]
        [InlineData("192.168.178.0", "255.255.255.0", "192.168.179.0", false)]
        [InlineData("192.168.178.0", "255.255.255.0", "192.168.177.255", false)]
        [InlineData("172.17.16.0", "255.255.252.0", "172.17.16.1", true)]
        [InlineData("172.17.16.0", "255.255.252.0", "172.17.17.1", true)]
        [InlineData("172.17.16.0", "255.255.252.0", "172.17.18.1", true)]
        [InlineData("172.17.16.0", "255.255.252.0", "172.17.19.1", true)]
        [InlineData("172.17.16.0", "255.255.252.0", "172.17.19.255", true)]
        [InlineData("172.17.16.0", "255.255.252.0", "172.17.20.0", false)]
        public void DHCPv4RelayAgentSubnetResolver_PacketMeetsConditions2(
            String networkAddressInput,
            String maskInput,
            String relayAgentAddressInput,
            Boolean shouldPass
            )
        {
            Random random = new Random();

            IPv4SubnetMask mask = IPv4SubnetMask.FromString(maskInput);
            IPv4Address networkAddress = IPv4Address.FromString(networkAddressInput);

            String inputAddressValue = random.GetAlphanumericString(10);
            String inputMaskValue = random.GetAlphanumericString(10);

            Mock<ISerializer> serializer = new Mock<ISerializer>(MockBehavior.Strict);
            serializer.Setup(x => x.Deserialze<String>(mask.GetSlashNotation().ToString())).Returns(mask.GetSlashNotation().ToString());
            serializer.Setup(x => x.Deserialze<String>(networkAddress.ToString())).Returns(networkAddress.ToString());

            DHCPv4RelayAgentSubnetResolver resolver = new DHCPv4RelayAgentSubnetResolver();
            Dictionary<String, String> values = new Dictionary<String, String>() {
                { nameof(DHCPv4RelayAgentSubnetResolver.NetworkAddress), networkAddress.ToString() },
                { nameof(DHCPv4RelayAgentSubnetResolver.Mask), mask.GetSlashNotation().ToString() },
            };

            resolver.ApplyValues(values, serializer.Object);

            IPv4Address gwAddress = IPv4Address.FromString(relayAgentAddressInput);
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
