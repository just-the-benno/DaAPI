using Castle.Core.Logging;
using DaAPI.Core.Packets.DHCPv6;
using DaAPI.Infrastructure.FilterEngines.DHCPv6;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DaAPI.UnitTests.Infrastructure.FilterEngines.DHCPv6
{
    public class SimpleDHCPv6FilterEngineTester
    {
        [Fact]
        public void Constructor()
        {
            List<IDHCPv6PacketFilter> filters = new List<IDHCPv6PacketFilter>();
            for (int i = 0; i < 10; i++)
            {
                filters.Add(Mock.Of<IDHCPv6PacketFilter>(MockBehavior.Strict));
            }

            IDHCPv6PacketFilterEngine engine = new SimpleDHCPv6PacketFilterEngine(filters,
                Mock.Of<ILogger<SimpleDHCPv6PacketFilterEngine>>());

            Assert.Equal(filters.Count, engine.Filters.Count());

            for (int i = 0; i < filters.Count; i++)
            {
                Assert.Equal(filters[i], engine.Filters.ElementAt(i));
            }
        }

        [Fact]
        public void Constructor_Failed_ElementIsNull()
        {
            List<IDHCPv6PacketFilter> filters = new List<IDHCPv6PacketFilter>();
            for (int i = 0; i < 10; i++)
            {
                if (i == 4)
                {
                    filters.Add(null);
                }
                else
                {
                    filters.Add(Mock.Of<IDHCPv6PacketFilter>(MockBehavior.Strict));
                }
            }

            Assert.ThrowsAny<Exception>(() => new SimpleDHCPv6PacketFilterEngine(filters,
               Mock.Of<ILogger<SimpleDHCPv6PacketFilterEngine>>()));
        }

        [Fact]
        public void Constructor_Failed_Null()
        {
            Assert.ThrowsAny<Exception>(() => new SimpleDHCPv6PacketFilterEngine(null,
               Mock.Of<ILogger<SimpleDHCPv6PacketFilterEngine>>()));
        }

        [Fact]
        public void AddFilter()
        {
            IDHCPv6PacketFilterEngine engine = new SimpleDHCPv6PacketFilterEngine(
                Mock.Of<ILogger<SimpleDHCPv6PacketFilterEngine>>());

            IDHCPv6PacketFilter filter = Mock.Of<IDHCPv6PacketFilter>(MockBehavior.Strict);

            Assert.Empty(engine.Filters);
            engine.AddFilter(filter);
            Assert.Single(engine.Filters);

            Assert.Equal(filter, engine.Filters.First());
        }

        [Fact]
        public void AddFilter_Failed_Null()
        {
            IDHCPv6PacketFilterEngine engine = new SimpleDHCPv6PacketFilterEngine(
                Mock.Of<ILogger<SimpleDHCPv6PacketFilterEngine>>());

            Assert.ThrowsAny<ArgumentNullException>(() => engine.AddFilter(null));
        }

        public interface IMyFirstCustomerDHCPv6Filter : IDHCPv6PacketFilter { }
        public interface IMySecondCustomerDHCPv6Filter : IDHCPv6PacketFilter { }

        [Fact]
        public async Task ShouldPacketBeFilterd_False()
        {
            DHCPv6Packet packet = DHCPv6Packet.AsInner(1, DHCPv6PacketTypes.Solicit, new List<DHCPv6PacketOption>());

            List<Mock<IDHCPv6PacketFilter>> filterMocks = new List<Mock<IDHCPv6PacketFilter>>();
            for (int i = 0; i < 10; i++)
            {
                Mock<IDHCPv6PacketFilter> filterMock = new Mock<IDHCPv6PacketFilter>(MockBehavior.Strict);
                filterMock.Setup(x => x.ShouldPacketBeFiltered(packet)).ReturnsAsync(false).Verifiable();
                filterMocks.Add(filterMock);
            }

            IDHCPv6PacketFilterEngine engine = new SimpleDHCPv6PacketFilterEngine(filterMocks.Select(x => x.Object),
                Mock.Of<ILogger<SimpleDHCPv6PacketFilterEngine>>());

            (bool, string) result = await engine.ShouldPacketBeFilterd(packet);
            Assert.False(result.Item1);
            Assert.True(String.IsNullOrEmpty(result.Item2));

            foreach (var item in filterMocks)
            {
                item.Verify();
            }
        }

        [Theory]
        [InlineData(10,0)]
        [InlineData(10, 5)]
        [InlineData(10, 9)]
        public async Task ShouldPacketBeFilterd_True(Int32 filterAmount, Int32 index)
        {
            DHCPv6Packet packet = DHCPv6Packet.AsInner(1, DHCPv6PacketTypes.Solicit, new List<DHCPv6PacketOption>());

            List<Mock<IDHCPv6PacketFilter>> filterMocks = new List<Mock<IDHCPv6PacketFilter>>();
            for (int i = 0; i < filterAmount; i++)
            {
                Mock<IDHCPv6PacketFilter> filterMock = new Mock<IDHCPv6PacketFilter>(MockBehavior.Strict);
                filterMock.Setup(x => x.ShouldPacketBeFiltered(packet)).ReturnsAsync(i == index).Verifiable();
                filterMocks.Add(filterMock);
            }

            IDHCPv6PacketFilterEngine engine = new SimpleDHCPv6PacketFilterEngine(filterMocks.Select(x => x.Object),
                Mock.Of<ILogger<SimpleDHCPv6PacketFilterEngine>>());

            (bool, string) result = await engine.ShouldPacketBeFilterd(packet);
            Assert.True(result.Item1);
            Assert.False(String.IsNullOrEmpty(result.Item2));

            for (int i = 0; i < filterMocks.Count; i++)
            {
                var filterMock = filterMocks[i];
                if (i <= index)
                {
                    filterMock.Verify();
                }
                else
                {
                    filterMock.Verify(x => x.ShouldPacketBeFiltered(packet), Times.Never);
                }
            }
        }
    }
}
