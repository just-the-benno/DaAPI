using DaAPI.Core.Common;
using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Packets.DHCPv6;
using DaAPI.TestHelper;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace DaAPI.UnitTests.Core.Packets.DHCPv6
{
    public class DHCPv6RelayPacketTester_FromByteArray
    {
        private void CheckByteRepresentation(DHCPv6RelayPacket input, IPv6HeaderInformation header)
        {
            Byte[] rawStream = new Byte[1800];
            Int32 writtenBytes = input.GetAsStream(rawStream);
            Byte[] stream = ByteHelper.CopyData(rawStream, 0, writtenBytes);

            DHCPv6RelayPacket secondPacket = DHCPv6Packet.FromByteArray(stream, header) as DHCPv6RelayPacket;
            Assert.Equal(input, secondPacket);
        }

        [Fact]
        public void FromByteArray_RelayPacket_SingleRelayPacket()
        {
            Random random = new Random();
            UInt32 transactionId = (UInt32)random.Next(0, 256 * 256 * 256);
            IPv6HeaderInformation header = new IPv6HeaderInformation(IPv6Address.FromString("fe80::1"), IPv6Address.FromString("fe80::2"));

            DHCPv6Packet innerPacket = DHCPv6Packet.AsInner(transactionId, DHCPv6PacketTypes.Solicit, new List<DHCPv6PacketOption>
            {
                new DHCPv6PacketTrueOption(DHCPv6PacketOptionTypes.RapitCommit),
                new DHCPv6PacketTrueOption(DHCPv6PacketOptionTypes.ReconfigureAccepte),
            });

            DHCPv6RelayPacket relayPacket = DHCPv6RelayPacket.AsOuterRelay(
                header,
                true,
                random.NextByte(),
                IPv6Address.FromString("fe80::acde"), IPv6Address.FromString("fe80::acdf"),
                new List<DHCPv6PacketOption>
                {
                    new DHCPv6PacketByteArrayOption(DHCPv6PacketOptionTypes.InterfaceId,random.NextBytes(23)),
                    new DHCPv6PacketRemoteIdentifierOption(random.NextUInt32(),random.NextBytes(15))
                },
                innerPacket);

            CheckByteRepresentation(relayPacket, header);
        }

        [Fact]
        public void FromByteArray_RelayPacket_MultipleRelayPacket()
        {
            Random random = new Random();
            UInt32 transactionId = (UInt32)random.Next(0, 256 * 256 * 256);
            IPv6HeaderInformation header = new IPv6HeaderInformation(IPv6Address.FromString("fe80::1"), IPv6Address.FromString("fe80::2"));

            DHCPv6Packet innerPacket = DHCPv6Packet.AsInner(transactionId, DHCPv6PacketTypes.Solicit, new List<DHCPv6PacketOption>
            {
                new DHCPv6PacketTrueOption(DHCPv6PacketOptionTypes.RapitCommit),
                new DHCPv6PacketTrueOption(DHCPv6PacketOptionTypes.ReconfigureAccepte),
            });


            DHCPv6RelayPacket innerRelayPacket = DHCPv6RelayPacket.AsInnerRelay(
                true,
                random.NextByte(),
                IPv6Address.FromString("fe80::acde"), IPv6Address.FromString("fe80::acdf"),
                new List<DHCPv6PacketOption>
                {
                    new DHCPv6PacketByteArrayOption(DHCPv6PacketOptionTypes.InterfaceId,random.NextBytes(40)),
                    new DHCPv6PacketRemoteIdentifierOption(random.NextUInt32(),random.NextBytes(50))
                },
                innerPacket);

            DHCPv6RelayPacket relayPacket = DHCPv6RelayPacket.AsOuterRelay(
                header,
                true,
                random.NextByte(),
                IPv6Address.FromString("fe80::acde"), IPv6Address.FromString("fe80::acdf"),
                new List<DHCPv6PacketOption>
                {
                    new DHCPv6PacketByteArrayOption(DHCPv6PacketOptionTypes.InterfaceId,random.NextBytes(23)),
                    new DHCPv6PacketRemoteIdentifierOption(random.NextUInt32(),random.NextBytes(15))
                },
                innerRelayPacket);

            CheckByteRepresentation(relayPacket, header);
        }

    }
}
