using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Packets.DHCPv6;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace DaAPI.UnitTests.Core.Packets.DHCPv6
{
    public class DHCPv6RelayPacketTester
    {
        [Fact]
        public void GetInnerPacket_SingleEncapsulated()
        {
            IPv6HeaderInformation header = new IPv6HeaderInformation(IPv6Address.FromString("fe80::1"), IPv6Address.FromString("fe80::2"));

            DHCPv6Packet innerPacket = DHCPv6Packet.AsInner(1, DHCPv6PacketTypes.REPLY, Array.Empty<DHCPv6PacketOption>());
            DHCPv6RelayPacket outerPacket = DHCPv6RelayPacket.AsOuterRelay(
                header,
                true, 1,
                IPv6Address.FromString("fe80::1"), IPv6Address.FromString("fe80::2"),
                Array.Empty<DHCPv6PacketOption>(),
                innerPacket);

            DHCPv6Packet actualInnerPacket = outerPacket.GetInnerPacket();

            Assert.Equal(innerPacket, actualInnerPacket);
        }

        [Fact]
        public void GetInnerPacket_MultipleEncapsulated()
        {
            IPv6HeaderInformation header = new IPv6HeaderInformation(IPv6Address.FromString("fe80::1"), IPv6Address.FromString("fe80::2"));

            Int32 depth = 10;

            DHCPv6Packet expectedInnerPacket = DHCPv6Packet.AsInner(1, DHCPv6PacketTypes.REPLY, Array.Empty<DHCPv6PacketOption>());
            DHCPv6Packet innerPacket = expectedInnerPacket;

            for (int i = 0; i < depth; i++)
            {
                DHCPv6RelayPacket outerPacket = DHCPv6RelayPacket.AsInnerRelay(
                      true, 1,
                      IPv6Address.FromString("fe80::1"), IPv6Address.FromString("fe80::2"),
                      Array.Empty<DHCPv6PacketOption>(),
                      innerPacket);

                innerPacket = outerPacket;
            }

            DHCPv6RelayPacket inputPacket = DHCPv6RelayPacket.AsOuterRelay(
                 header,
                   true, 1,
                   IPv6Address.FromString("fe80::1"), IPv6Address.FromString("fe80::2"),
                   Array.Empty<DHCPv6PacketOption>(),
                   innerPacket);

            DHCPv6Packet actualInnerPacket = inputPacket.GetInnerPacket();
            Assert.Equal(expectedInnerPacket, actualInnerPacket);
        }

        private DHCPv6RelayPacket GetInnerRelayPacket(DHCPv6RelayPacket packet, Int32 depth)
        {
            if (depth == 0)
            {
                return packet;
            }

            return GetInnerRelayPacket((DHCPv6RelayPacket)(packet.InnerPacket), depth - 1);
        }

        [Fact]
        public void ConstructPacket_MultipleRelayPackets()
        {
            IPv6HeaderInformation header = new IPv6HeaderInformation(IPv6Address.FromString("fe80::1"), IPv6Address.FromString("fe80::2"));

            Int32 depth = 10;

            DHCPv6Packet receivedInnerPacket = DHCPv6Packet.AsInner(1, DHCPv6PacketTypes.Solicit, Array.Empty<DHCPv6PacketOption>());
            DHCPv6Packet innerPacket = receivedInnerPacket;

            List<IPv6Address> expectedLinkAddresses = new List<IPv6Address>();

            for (int i = 0; i < depth; i++)
            {
                IPv6Address linkAddress = IPv6Address.FromString($"fe{i}::1");

                DHCPv6RelayPacket outerPacket = DHCPv6RelayPacket.AsInnerRelay(
                      true, 1,
                      linkAddress, IPv6Address.FromString($"fe{i}::2"),
                      Array.Empty<DHCPv6PacketOption>(),
                      innerPacket);

                innerPacket = outerPacket;
                expectedLinkAddresses.Insert(0, linkAddress);
            }

            DHCPv6RelayPacket inputPacket = DHCPv6RelayPacket.AsOuterRelay(
                 header,
                   true, 1,
                   IPv6Address.FromString("ff70::1"), IPv6Address.FromString("fe80::2"),
                   Array.Empty<DHCPv6PacketOption>(),
                   innerPacket);

            expectedLinkAddresses.Insert(0, IPv6Address.FromString("ff70::1"));


            DHCPv6Packet sendInnerPacket = DHCPv6Packet.AsInner(1, DHCPv6PacketTypes.ADVERTISE, Array.Empty<DHCPv6PacketOption>());

            DHCPv6Packet packet = DHCPv6Packet.ConstructPacket(inputPacket, sendInnerPacket);
            Assert.IsAssignableFrom<DHCPv6RelayPacket>(packet);

            DHCPv6RelayPacket relayPacket = (DHCPv6RelayPacket)packet;

            DHCPv6Packet innerUsedPacket = relayPacket.GetInnerPacket();
            Assert.Equal(sendInnerPacket, innerUsedPacket);

            for (int i = 0; i < expectedLinkAddresses.Count; i++)
            {
                DHCPv6RelayPacket innerRelayPacket = GetInnerRelayPacket(relayPacket, i);
                Assert.Equal(innerRelayPacket.LinkAddress, expectedLinkAddresses[i]);
                Assert.Equal((Byte)(expectedLinkAddresses.Count - i - 1), innerRelayPacket.HopCount);
            }
        }


        [Fact]
        public void GetRelayPacketChain()
        {
            IPv6HeaderInformation header = new IPv6HeaderInformation(IPv6Address.FromString("fe80::1"), IPv6Address.FromString("fe80::2"));

            Int32 depth = 10;

            DHCPv6Packet innerPacket = DHCPv6Packet.AsInner(1, DHCPv6PacketTypes.REPLY, Array.Empty<DHCPv6PacketOption>()); ;

            for (int i = 0; i < depth; i++)
            {
                DHCPv6RelayPacket outerPacket = DHCPv6RelayPacket.AsInnerRelay(
                      true, 1,
                      IPv6Address.FromString("fe80::1"), IPv6Address.FromString("fe80::2"),
                      Array.Empty<DHCPv6PacketOption>(),
                      innerPacket);

                innerPacket = outerPacket;
            }

            DHCPv6RelayPacket inputPacket = DHCPv6RelayPacket.AsOuterRelay(
                 header,
                   true, 1,
                   IPv6Address.FromString("fe80::1"), IPv6Address.FromString("fe80::2"),
                   Array.Empty<DHCPv6PacketOption>(),
                   innerPacket);

            var chain = inputPacket.GetRelayPacketChain();
            Assert.NotEmpty(chain);
            Assert.Equal(depth + 1, chain.Count);

            Assert.NotEqual(DHCPv6PacketTypes.RELAY_FORW, chain[0].InnerPacket.PacketType);
            Assert.NotEqual(DHCPv6PacketTypes.RELAY_REPL, chain[0].InnerPacket.PacketType);

            for (int i = 1; i < chain.Count; i++)
            {
                Assert.Equal(chain[i].InnerPacket, chain[i - 1]);
            }

            Assert.Equal(inputPacket, chain[depth]);
        }
    }
}

