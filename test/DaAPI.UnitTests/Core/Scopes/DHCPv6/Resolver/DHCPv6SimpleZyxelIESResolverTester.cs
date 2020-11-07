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
    public class DHCPv6SimpleZyxelIESResolverTester
    {
        [Fact]
        public void HasUniqueIdentifier()
        {
            var resolver = new DHCPv6SimpleZyxelIESResolver();
            Assert.True(resolver.HasUniqueIdentifier);
        }

        private Byte[] GetExpectedByteSequence(Int32 slotId, Int32 portId) => (new ASCIIEncoding()).GetBytes($"{slotId}/{portId}");

        private DHCPv6Packet GetPacket(Random random, Byte[] remoteId, Int32 slotId, Int32 portId, Int32 enterpriseId = 0, Boolean includeRelevantOptions = true)
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
                    new DHCPv6PacketIdentifierOption(DHCPv6PacketOptionTypes.ClientIdentifier, new UUIDDUID(random.NextGuid())),
                    includeRelevantOptions == true ? (DHCPv6PacketOption)( new DHCPv6PacketRemoteIdentifierOption((UInt32)enterpriseId,remoteId)) : new DHCPv6PacketTrueOption(DHCPv6PacketOptionTypes.RapitCommit),
                    includeRelevantOptions == true ? (DHCPv6PacketOption)( new DHCPv6PacketByteArrayOption(DHCPv6PacketOptionTypes.InterfaceId, GetExpectedByteSequence(slotId,portId))) : new DHCPv6PacketTimeOption(DHCPv6PacketOptionTypes.ElapsedTime,10,DHCPv6PacketTimeOption.DHCPv6PacketTimeOptionUnits.Minutes)

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
            var resolver = new DHCPv6SimpleZyxelIESResolver();

            Byte[] macAddress = random.NextBytes(6);
            Int32 slotId = random.NextByte();
            Int32 portId = random.NextByte();

            DHCPv6Packet packet = GetPacket(random, macAddress, slotId, portId);

            Byte[] actual = resolver.GetUniqueIdentifier(packet);

            for (int i = 0; i < 4; i++)
            {
                Assert.Equal(0, actual[i]);

            }

            for (int i = 0; i < macAddress.Length; i++)
            {
                Assert.Equal(macAddress[i], actual[i + 4]);
            }

            Byte[] expectedInterfaceValue = GetExpectedByteSequence(slotId, portId);
            for (int i = 0; i < expectedInterfaceValue.Length; i++)
            {
                Assert.Equal(expectedInterfaceValue[i], actual[i + macAddress.Length + 4]);
            }
        }

        [Theory]
        [InlineData("2", "1", "1", "54833a9d3407", true)]
        [InlineData("0", "1", "1", "54833a9d3407", true)]
        [InlineData("0", "4", "2", "54833a9d3407", true)]
        [InlineData("-1", "0", "1", "54833a9d3407", false)]
        [InlineData("2", "0", "1", "54833a9d3407", false)]
        [InlineData("2", "1", "0", "54833a9d3407", false)]
        [InlineData("2", "-1", "1", "54833a9d3407", false)]
        [InlineData("2", "1", "-1", "54833a9d3407", false)]
        [InlineData("2", "1", "1", "54833a9d340Q", false)]
        [InlineData("2", "1", "1", "54833a9d34", false)]
        [InlineData("2", "1", "1", "", false)]
        public void ArePropertiesAndValuesValid(String index, String slotId, String portId, String deviceMacAddress, Boolean shouldBeValid)
        {
            Mock<ISerializer> serializerMock = new Mock<ISerializer>(MockBehavior.Strict);

            serializerMock.Setup(x => x.Deserialze<String>(index)).Returns(index).Verifiable();
            serializerMock.Setup(x => x.Deserialze<String>(slotId)).Returns(slotId).Verifiable();
            serializerMock.Setup(x => x.Deserialze<String>(portId)).Returns(portId).Verifiable();
            serializerMock.Setup(x => x.Deserialze<String>(deviceMacAddress)).Returns(deviceMacAddress).Verifiable();

            var resolver = new DHCPv6SimpleZyxelIESResolver();
            Boolean actual = resolver.ArePropertiesAndValuesValid(new Dictionary<String, String> {
               { "Index", index },
               { "SlotId", slotId },
               { "PortId", portId },
               { "DeviceMacAddress", deviceMacAddress },
            }, serializerMock.Object);

            Assert.Equal(shouldBeValid, actual);
        }

        [Fact]
        public void ArePropertiesAndValuesValid_KeyIsMissing()
        {
            var input = new[]{
                new  Dictionary<String,String>{
                    { "Index1", "0" },
                    { "SlotId", "0" },
                    { "PortId", "0" },
                    { "DeviceMacAddress", "0" },
                },
                new  Dictionary<String,String>{
                    { "Index", "0" },
                    { "SlotId1", "0" },
                    { "PortId", "0" },
                    { "DeviceMacAddress", "0" },
                },
                new  Dictionary<String,String>{
                    { "Index", "0" },
                    { "SlotId", "0" },
                    { "PortId1", "0" },
                    { "DeviceMacAddress", "0" },
                },
               new  Dictionary<String,String>{
                    { "Index", "0" },
                    { "SlotId", "0" },
                    { "PortId", "0" },
                    { "DeviceMacAddress1", "0" },
                },
                new  Dictionary<String,String>{
                    { "SlotId", "0" },
                    { "PortId", "0" },
                    { "DeviceMacAddress", "0" },
                },
                 new  Dictionary<String,String>{
                    { "Index", "0" },
                    { "PortId", "0" },
                    { "DeviceMacAddress", "0" },
                },
                new  Dictionary<String,String>{
                    { "Index", "0" },
                    { "SlotId", "0" },
                    { "DeviceMacAddress", "0" },
                },
                new  Dictionary<String,String>{
                    { "Index", "0" },
                    { "SlotId", "0" },
                    { "PortId", "0" },
                },
                };

            var resolver = new DHCPv6SimpleZyxelIESResolver();

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

            UInt16 index = (UInt16)random.Next(0, 10);
            UInt16 slotId = (UInt16)random.Next(0, 10);
            UInt16 portId = (UInt16)random.Next(0, 10);

            Byte[] macAddress = random.NextBytes(6);

            String macAddressInput = ByteHelper.ToString(macAddress, false);

            Mock<ISerializer> serializerMock = new Mock<ISerializer>(MockBehavior.Strict);
            serializerMock.Setup(x => x.Deserialze<UInt16>(index.ToString())).Returns(index).Verifiable();
            serializerMock.Setup(x => x.Deserialze<UInt16>(slotId.ToString())).Returns(slotId).Verifiable();
            serializerMock.Setup(x => x.Deserialze<UInt16>(portId.ToString())).Returns(portId).Verifiable();
            serializerMock.Setup(x => x.Deserialze<String>(macAddressInput)).Returns(macAddressInput).Verifiable();

            var resolver = new DHCPv6SimpleZyxelIESResolver();
            resolver.ApplyValues(new Dictionary<String, String> {
                    { "Index", index.ToString() },
                    { "SlotId", slotId.ToString() },
                    { "PortId", portId.ToString() },
                    { "DeviceMacAddress", macAddressInput },
            }, serializerMock.Object);

            Assert.Equal(index, resolver.Index);
            Assert.Equal(slotId, resolver.SlotId);
            Assert.Equal(portId, resolver.PortId);
            Assert.Equal(macAddress, resolver.DeviceMacAddress);

            serializerMock.Verify();

            Dictionary<String, String> expectedValues = new Dictionary<string, string>
            {
                { "Index", index.ToString() },
                { "SlotId", slotId.ToString() },
                { "PortId", portId.ToString() },
                { "DeviceMacAddress", macAddressInput },
            };

            Assert.Equal(expectedValues.ToArray(), resolver.GetValues().ToArray());
            Assert.Equal(expectedValues, resolver.GetValues(), new NonStrictDictionaryComparer<String, String>());

        }

        private static (UInt16 slotId, UInt16 portId) GetValidResolver(Random random, UInt16 index, out byte[] macAddress, out Mock<ISerializer> serializerMock, out DHCPv6SimpleZyxelIESResolver resolver)
        {
            UInt16 slotId = (UInt16)random.Next(0, 10);
            UInt16 portId = (UInt16)random.Next(0, 10);
            macAddress = random.NextBytes(6);
            String macAddressInput = ByteHelper.ToString(macAddress, false);

            String value = random.GetAlphanumericString();

            serializerMock = new Mock<ISerializer>(MockBehavior.Strict);
            serializerMock.Setup(x => x.Deserialze<UInt16>(index.ToString())).Returns(index).Verifiable();
            serializerMock.Setup(x => x.Deserialze<UInt16>(slotId.ToString())).Returns(slotId).Verifiable();
            serializerMock.Setup(x => x.Deserialze<UInt16>(portId.ToString())).Returns(portId).Verifiable();
            serializerMock.Setup(x => x.Deserialze<String>(macAddressInput)).Returns(macAddressInput).Verifiable();

            resolver = new DHCPv6SimpleZyxelIESResolver();
            resolver.ApplyValues(new Dictionary<String, String> {
                    { "Index", index.ToString() },
                    { "SlotId", slotId.ToString() },
                    { "PortId", portId.ToString() },
                    { "DeviceMacAddress", macAddressInput },
            }, serializerMock.Object);

            serializerMock.Verify();

            return (slotId, portId);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void PacketMeetsCondition(Boolean shouldMeetCondition)
        {
            Random random = new Random();

            var (slotId, portId) = GetValidResolver(random, 0, out Byte[] macAddress, out Mock<ISerializer> serializerMock, out DHCPv6SimpleZyxelIESResolver resolver);

            Byte[] remoteIdentifierAsBytes;
            if (shouldMeetCondition == true)
            {
                remoteIdentifierAsBytes = macAddress;
            }
            else
            {
                remoteIdentifierAsBytes = random.NextBytes(6);
            }

            DHCPv6Packet packet = GetPacket(random, remoteIdentifierAsBytes, slotId, portId);

            Boolean result = resolver.PacketMeetsCondition(packet);
            Assert.Equal(shouldMeetCondition, result);

            serializerMock.Verify();
        }

        [Fact]
        public void PacketMeetsCondition_DifferentEnterpriseNumber()
        {
            Random random = new Random();

            var (slotId, portId) = GetValidResolver(random, 0, out byte[] macAddress, out Mock<ISerializer> serializerMock, out DHCPv6SimpleZyxelIESResolver resolver);

            Byte[] remoteIdentifierAsBytes = macAddress;

            DHCPv6Packet packet = GetPacket(random, remoteIdentifierAsBytes, slotId, portId,random.Next());

            Boolean result = resolver.PacketMeetsCondition(packet);
            Assert.True(result);

            serializerMock.Verify();
        }

        [Fact]
        public void PacketMeetsCondition_False_NotRelay()
        {
            Random random = new Random();

            Mock<ISerializer> serializerMock = new Mock<ISerializer>(MockBehavior.Strict);
            DHCPv6SimpleZyxelIESResolver resolver = new DHCPv6SimpleZyxelIESResolver();

            IPv6HeaderInformation headerInformation =
                new IPv6HeaderInformation(IPv6Address.FromString("fe80::1"), IPv6Address.FromString("fe80::2"));

            var packetOptions = new List<DHCPv6PacketOption>
                {
                    new DHCPv6PacketTrueOption(DHCPv6PacketOptionTypes.RapitCommit),
                };

            DHCPv6Packet packet = DHCPv6Packet.AsOuter(headerInformation, random.NextUInt16(),
                DHCPv6PacketTypes.Solicit, packetOptions);

            Boolean result = resolver.PacketMeetsCondition(packet);
            Assert.False(result);

            serializerMock.Verify();
        }

        [Fact]
        public void PacketMeetsCondition_False_IndexNotPresented()
        {
            Random random = new Random();

            var (slotId, portId) = GetValidResolver(random, 2, out byte[] macAddress, out Mock<ISerializer> serializerMock, out DHCPv6SimpleZyxelIESResolver resolver);

            DHCPv6Packet packet = GetPacket(random, macAddress, slotId, portId);

            Boolean result = resolver.PacketMeetsCondition(packet);
            Assert.False(result);

            serializerMock.Verify();
        }

        [Fact]
        public void PacketMeetsCondition_False_OptionNotPresented()
        {
            Random random = new Random();

            var (slotId, portId) = GetValidResolver(random, 0, out byte[] macAddress, out _, out DHCPv6SimpleZyxelIESResolver resolver);

            DHCPv6Packet packet = GetPacket(random, macAddress, slotId, portId,0, false);

            Boolean result = resolver.PacketMeetsCondition(packet);
            Assert.False(result);
        }

        [Fact]
        public void GetDescription()
        {
            var expected = new ScopeResolverDescription("DHCPv6SimpleZyxelIESResolver", new[] {
            new ScopeResolverPropertyDescription("Index",ScopeResolverPropertyDescription.ScopeResolverPropertyValueTypes.UInt32),
            new ScopeResolverPropertyDescription("SlotId",ScopeResolverPropertyDescription.ScopeResolverPropertyValueTypes.UInt32),
            new ScopeResolverPropertyDescription("PortId",ScopeResolverPropertyDescription.ScopeResolverPropertyValueTypes.UInt32),
            new ScopeResolverPropertyDescription("DeviceMacAddress",ScopeResolverPropertyDescription.ScopeResolverPropertyValueTypes.ByteArray),
            });

            var resolver = new DHCPv6SimpleZyxelIESResolver();
            var actual = resolver.GetDescription();

            Assert.Equal(expected, actual);
        }

    }
}
