using DaAPI.Core.Common;
using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Packets.DHCPv6;
using DaAPI.Infrastructure.FilterEngines.DHCPv6;
using DaAPI.TestHelper;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DaAPI.UnitTests.Infrastructure.FilterEngines.DHCPv6
{
    public class DHCPv6PacketConsistencyFilterTester
    {
        [Fact]
        public async Task Filter_IsClientRequest_IsConsistent()
        {
            Random random = new Random();

            DHCPv6PacketConsistencyFilter filter =
                new DHCPv6PacketConsistencyFilter(
                    Mock.Of<ILogger<DHCPv6PacketConsistencyFilter>>());

            DHCPv6Packet packet = DHCPv6Packet.AsInner(
              1, DHCPv6PacketTypes.Solicit, new List<DHCPv6PacketOption>
              {
                    new DHCPv6PacketIdentifierOption(DHCPv6PacketOptionTypes.ClientIdentifier,new UUIDDUID(random.NextGuid())),
              });

            Boolean result = await filter.ShouldPacketBeFiltered(packet);
            Assert.False(result);
        }

        [Fact]
        public async Task Filter_IsClientRequest_IsNotConsistent()
        {
            Random random = new Random();

            DHCPv6PacketConsistencyFilter filter =
                new DHCPv6PacketConsistencyFilter(
                    Mock.Of<ILogger<DHCPv6PacketConsistencyFilter>>());

            DHCPv6Packet packet = DHCPv6Packet.AsInner(
              1, DHCPv6PacketTypes.Solicit, new List<DHCPv6PacketOption>
              {
                    new DHCPv6PacketIdentifierOption(DHCPv6PacketOptionTypes.ClientIdentifier,new UUIDDUID(random.NextGuid())),
                    new DHCPv6PacketIdentifierOption(DHCPv6PacketOptionTypes.ServerIdentifer,new UUIDDUID(random.NextGuid())),
              });

            Boolean result = await filter.ShouldPacketBeFiltered(packet);
            Assert.True(result);
        }

        [Fact]
        public async Task Filter_IsNotClientRequest()
        {
            Random random = new Random();

            DHCPv6PacketConsistencyFilter filter =
                new DHCPv6PacketConsistencyFilter(
                    Mock.Of<ILogger<DHCPv6PacketConsistencyFilter>>());

            DHCPv6Packet packet = DHCPv6Packet.AsInner(
              1, DHCPv6PacketTypes.ADVERTISE, new List<DHCPv6PacketOption>
              {
                    new DHCPv6PacketIdentifierOption(DHCPv6PacketOptionTypes.ClientIdentifier,new UUIDDUID(random.NextGuid())),
                    new DHCPv6PacketIdentifierOption(DHCPv6PacketOptionTypes.ServerIdentifer,new UUIDDUID(random.NextGuid())),
              });

            Boolean result = await filter.ShouldPacketBeFiltered(packet);
            Assert.True(result);
        }

        [Fact]
        public async Task Filter_IsNotClientRequest_WithinRelayedPacket()
        {
            Random random = new Random();

            DHCPv6PacketConsistencyFilter filter =
                new DHCPv6PacketConsistencyFilter(
                    Mock.Of<ILogger<DHCPv6PacketConsistencyFilter>>());

            DHCPv6Packet packet = DHCPv6RelayPacket.AsInnerRelay(true, 1, IPv6Address.FromString("fe80::1"), IPv6Address.FromString("fe80::2"), Array.Empty<DHCPv6PacketOption>(), DHCPv6Packet.AsInner(
              1, DHCPv6PacketTypes.ADVERTISE, new List<DHCPv6PacketOption>
              {
                    new DHCPv6PacketIdentifierOption(DHCPv6PacketOptionTypes.ClientIdentifier,new UUIDDUID(random.NextGuid())),
                    new DHCPv6PacketIdentifierOption(DHCPv6PacketOptionTypes.ServerIdentifer,new UUIDDUID(random.NextGuid())),
              }));

            Boolean result = await filter.ShouldPacketBeFiltered(packet);
            Assert.True(result);
        }
    }
}
