using DaAPI.Core.Common;
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
    public class DHCPv6PacketServerIdentifierFilterTester
    {
        [Fact]
        public async Task Filter_ShouldHaveValue_ValueMatchesServerDUID()
        {
            Random random = new Random();
            UUIDDUID serverDuid = new UUIDDUID(random.NextGuid());

            DHCPv6PacketServerIdentifierFilter filter =
                new DHCPv6PacketServerIdentifierFilter(
                    serverDuid,
                    Mock.Of<ILogger<DHCPv6PacketServerIdentifierFilter>>());

            DHCPv6Packet packet = DHCPv6Packet.AsInner(
              1, DHCPv6PacketTypes.REQUEST, new List<DHCPv6PacketOption>
              {
                    new DHCPv6PacketIdentifierOption(DHCPv6PacketOptionTypes.ServerIdentifer,serverDuid),
              });

            Boolean result = await filter.ShouldPacketBeFiltered(packet);
            Assert.False(result);
        }

        [Fact]
        public async Task Filter_ShouldHaveValue_ValueNotMatchesServerDUID()
        {
            Random random = new Random();
            UUIDDUID serverDuid = new UUIDDUID(random.NextGuid());

            DHCPv6PacketServerIdentifierFilter filter =
                new DHCPv6PacketServerIdentifierFilter(
                    serverDuid,
                    Mock.Of<ILogger<DHCPv6PacketServerIdentifierFilter>>());

            DHCPv6Packet packet = DHCPv6Packet.AsInner(
              1, DHCPv6PacketTypes.REQUEST, new List<DHCPv6PacketOption>
              {
                    new DHCPv6PacketIdentifierOption(DHCPv6PacketOptionTypes.ServerIdentifer,new UUIDDUID(random.NextGuid())),
              });

            Boolean result = await filter.ShouldPacketBeFiltered(packet);
            Assert.True(result);
        }

        [Fact]
        public async Task Filter_CouldHaveValue_ValueMatchesServerDUID()
        {
            Random random = new Random();
            UUIDDUID serverDuid = new UUIDDUID(random.NextGuid());

            DHCPv6PacketServerIdentifierFilter filter =
                new DHCPv6PacketServerIdentifierFilter(
                    serverDuid,
                    Mock.Of<ILogger<DHCPv6PacketServerIdentifierFilter>>());

            DHCPv6Packet packet = DHCPv6Packet.AsInner(
              1, DHCPv6PacketTypes.INFORMATION_REQUEST, new List<DHCPv6PacketOption>
              {
                    new DHCPv6PacketIdentifierOption(DHCPv6PacketOptionTypes.ServerIdentifer,serverDuid),
              });

            Boolean result = await filter.ShouldPacketBeFiltered(packet);
            Assert.True(result);
        }

        [Fact]
        public async Task Filter_CouldHaveValue_ValueNotMatchesServerDUID()
        {
            Random random = new Random();
            UUIDDUID serverDuid = new UUIDDUID(random.NextGuid());

            DHCPv6PacketServerIdentifierFilter filter =
                new DHCPv6PacketServerIdentifierFilter(
                    serverDuid,
                    Mock.Of<ILogger<DHCPv6PacketServerIdentifierFilter>>());

            DHCPv6Packet packet = DHCPv6Packet.AsInner(
              1, DHCPv6PacketTypes.INFORMATION_REQUEST, new List<DHCPv6PacketOption>
              {
                    new DHCPv6PacketIdentifierOption(DHCPv6PacketOptionTypes.ServerIdentifer,new UUIDDUID(random.NextGuid())),
              });

            Boolean result = await filter.ShouldPacketBeFiltered(packet);
            Assert.True(result);
        }

        [Fact]
        public async Task Filter_CouldHaveValue_ValueNotPresented()
        {
            Random random = new Random();
            UUIDDUID serverDuid = new UUIDDUID(random.NextGuid());

            DHCPv6PacketServerIdentifierFilter filter =
                new DHCPv6PacketServerIdentifierFilter(
                    serverDuid,
                    Mock.Of<ILogger<DHCPv6PacketServerIdentifierFilter>>());

            DHCPv6Packet packet = DHCPv6Packet.AsInner(
              1, DHCPv6PacketTypes.INFORMATION_REQUEST, new List<DHCPv6PacketOption>
              {
                  //new DHCPv6PacketIdentifierOption(DHCPv6PacketOptionTypes.ServerIdentifer,new UUIDDUID(random.NextGuid())),
              });

            Boolean result = await filter.ShouldPacketBeFiltered(packet);
            Assert.False(result);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Filter_ShouldNotHaveValue(Boolean valueIsPresented)
        {
            Random random = new Random();
            UUIDDUID serverDuid = new UUIDDUID(random.NextGuid());

            DHCPv6PacketServerIdentifierFilter filter =
                new DHCPv6PacketServerIdentifierFilter(
                    serverDuid,
                    Mock.Of<ILogger<DHCPv6PacketServerIdentifierFilter>>());

            var options = new List<DHCPv6PacketOption>();
            if (valueIsPresented == true)
            {
                options.Add(new DHCPv6PacketIdentifierOption(DHCPv6PacketOptionTypes.ServerIdentifer, serverDuid));
            }

            DHCPv6Packet packet = DHCPv6Packet.AsInner(
              1, DHCPv6PacketTypes.Solicit, options);

            Boolean result = await filter.ShouldPacketBeFiltered(packet);
            Assert.False(result);
        }

    }
}
