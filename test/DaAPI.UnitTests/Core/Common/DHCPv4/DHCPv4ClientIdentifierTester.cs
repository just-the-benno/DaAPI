using DaAPI.Core.Common;
using DaAPI.Core.Packets.DHCPv4;
using DaAPI.TestHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace DaAPI.UnitTests.Core.Common.DHCPv4
{
    public class DHCPv4ClientIdentifierTester
    {
        [Fact]
        public void FromOptionData_HWAddress()
        {
            Random random = new Random();
            Byte[] identifierValue = new Byte[7];
            random.NextBytes(identifierValue);
            identifierValue[0] =
                (Byte)DHCPv4Packet.DHCPv4PacketHardwareAddressTypes.Ethernet;

            DHCPv4ClientIdentifier identifier = DHCPv4ClientIdentifier.FromOptionData(identifierValue);

            Assert.Equal(
                identifierValue.Skip(1).ToArray(),
                identifier.HwAddress);

            Assert.Equal(DaAPI.Core.Common.DUID.Empty, identifier.DUID);
        }

        [Fact]
        public void FromOptionData_FromDUID()
        {
            Random random = new Random();
            Guid duidValue = random.NextGuid();

            UUIDDUID duid = new UUIDDUID(duidValue);

            Byte[] identifierValue = duid.GetAsByteStream();

            DHCPv4ClientIdentifier identifier = DHCPv4ClientIdentifier.FromOptionData(identifierValue);

            Assert.Empty(identifier.HwAddress);
            Assert.Equal(duid, identifier.DUID);
        }

        [Fact]
        public void Equals_WithoutDuid()
        {
            Random random = new Random();
            Byte[] firstHWAddress = random.NextBytes(12);
            Byte[] secondHWAddress = random.NextBytes(12);

            DHCPv4ClientIdentifier firstIdentifier = DHCPv4ClientIdentifier.FromHwAddress(firstHWAddress);
            DHCPv4ClientIdentifier secondIdentifier = DHCPv4ClientIdentifier.FromHwAddress(firstHWAddress);
            DHCPv4ClientIdentifier thirdIdentifier = DHCPv4ClientIdentifier.FromHwAddress(secondHWAddress);

            Assert.Equal(firstIdentifier, secondIdentifier);
            Assert.NotEqual(firstIdentifier, thirdIdentifier);
        }

        [Fact]
        public void Equals_BasedOnDuid()
        {
            Guid firstGuid = Guid.NewGuid();
            Guid secondGuid = Guid.NewGuid();

            DHCPv4ClientIdentifier firstIdentifier = DHCPv4ClientIdentifier.FromDuid(new UUIDDUID(firstGuid));
            DHCPv4ClientIdentifier secondIdentifier = DHCPv4ClientIdentifier.FromDuid(new UUIDDUID(firstGuid));
            DHCPv4ClientIdentifier thirdIdentifier = DHCPv4ClientIdentifier.FromDuid(new UUIDDUID(secondGuid));

            Assert.Equal(firstIdentifier, secondIdentifier);
            Assert.NotEqual(firstIdentifier, thirdIdentifier);
        }

        [Fact]
        public void Equals_BasedOnDuid_ButWithDifferentHWAddresses()
        {
            Guid firstGuid = Guid.NewGuid();
            Guid secondGuid = Guid.NewGuid();

            Random random = new Random();
            Byte[] firstHWAddress = random.NextBytes(12);
            Byte[] secondHWAddress = random.NextBytes(12);
            Byte[] thirdHWAddress = random.NextBytes(12);

            DHCPv4ClientIdentifier firstIdentifier = DHCPv4ClientIdentifier.FromDuid(new UUIDDUID(firstGuid), firstHWAddress);
            DHCPv4ClientIdentifier secondIdentifier = DHCPv4ClientIdentifier.FromDuid(new UUIDDUID(firstGuid), secondHWAddress);
            DHCPv4ClientIdentifier thirdIdentifier = DHCPv4ClientIdentifier.FromDuid(new UUIDDUID(secondGuid), thirdHWAddress);

            Assert.Equal(firstIdentifier, secondIdentifier);
            Assert.NotEqual(firstIdentifier, thirdIdentifier);
        }

        [Fact]
        public void AddHWAdress()
        {
            UUIDDUID duid = new UUIDDUID(Guid.NewGuid());

            Random random = new Random();
            Byte[] hwAddress = random.NextBytes(12);

            DHCPv4ClientIdentifier firstIdentifier = DHCPv4ClientIdentifier.FromDuid(duid);
            DHCPv4ClientIdentifier secondIdentifier = firstIdentifier.AddHardwareAddress(hwAddress);

            Assert.Equal(hwAddress, secondIdentifier.HwAddress);
            Assert.Equal(duid, secondIdentifier.DUID);
        }

        [Fact]
        public void AddHWAdress_HwAlreadySet()
        {
            UUIDDUID duid = new UUIDDUID(Guid.NewGuid());

            Random random = new Random();
            Byte[] hwAddress = random.NextBytes(12);
            Byte[] packetHwAddress = random.NextBytes(12);

            DHCPv4ClientIdentifier firstIdentifier = DHCPv4ClientIdentifier.FromDuid(duid, hwAddress);
            DHCPv4ClientIdentifier secondIdentifier = firstIdentifier.AddHardwareAddress(packetHwAddress);

            Assert.NotEqual(packetHwAddress, secondIdentifier.HwAddress);
            Assert.Equal(hwAddress, secondIdentifier.HwAddress);
            Assert.Equal(duid, secondIdentifier.DUID);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void HasHardwareAddress(Boolean isSet)
        {
            UUIDDUID duid = new UUIDDUID(Guid.NewGuid());

            Random random = new Random();
            Byte[] hwAddress = random.NextBytes(12);

            DHCPv4ClientIdentifier identifier;
            if(isSet == true)
            {
                identifier = DHCPv4ClientIdentifier.FromDuid(duid, hwAddress);
            }
            else
            {
                identifier = DHCPv4ClientIdentifier.FromDuid(duid);
            }

            Boolean actual = identifier.HasHardwareAddress();
            Assert.Equal(isSet, actual);
        }
    }
}
