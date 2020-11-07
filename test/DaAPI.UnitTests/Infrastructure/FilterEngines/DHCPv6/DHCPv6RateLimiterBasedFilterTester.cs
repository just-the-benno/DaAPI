using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.FilterEngines.DHCPv6;
using DaAPI.Core.Packets.DHCPv6;
using DaAPI.Infrastructure.FilterEngines.DHCPv4;
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
    public class DHCPv6RateLimiterBasedFilterTester
    {
        private readonly Random _rand = new Random();

        [Fact]
        public async Task ShouldPacketBeFilterd_No()
        {
            DHCPv6RateLimitBasedFilter filter = new DHCPv6RateLimitBasedFilter(
                Mock.Of<ILogger<DHCPv6RateLimitBasedFilter>>());

            Int32 packetAmount = _rand.Next(1000, 10000);
            IPv6Address serverAddress = IPv6Address.FromByteArray(_rand.NextBytes(16));

            for (int i = 0; i < packetAmount; i++)
            {
                IPv6Address address = IPv6Address.FromByteArray(_rand.NextBytes(16));
                IPv6HeaderInformation header = new IPv6HeaderInformation(address, serverAddress);
                DHCPv6Packet packet = new DHCPv6Packet(header, 1, DHCPv6PacketTypes.Solicit, Array.Empty<DHCPv6PacketOption>());

                Boolean result = await filter.ShouldPacketBeFiltered(packet);
                Assert.False(result);
            }
        }

        [Fact]
        public async Task ShouldPacketBeFilterd_SameAdressBelowLimit()
        {
            DHCPv6RateLimitBasedFilter filter = new DHCPv6RateLimitBasedFilter(
                Mock.Of<ILogger<DHCPv6RateLimitBasedFilter>>());

            UInt16 packetsPerSeconds = (UInt16)_rand.Next(30, 100);
            filter.PacketsPerSecons = packetsPerSeconds;

            TimeSpan timePerPacket = TimeSpan.FromSeconds(packetsPerSeconds / 1000.0);

            Int32 packetAmount = _rand.Next(packetsPerSeconds * 2, packetsPerSeconds * 4);
            IPv6Address serverAddress = IPv6Address.FromByteArray(_rand.NextBytes(16));

            IPv6Address address = IPv6Address.FromByteArray(_rand.NextBytes(16));

            for (int i = 0; i < packetAmount; i++)
            {
                DateTime start = DateTime.Now;
                IPv6HeaderInformation header = new IPv6HeaderInformation(address, serverAddress);
                DHCPv6Packet packet = new DHCPv6Packet(header, 1, DHCPv6PacketTypes.Solicit, Array.Empty<DHCPv6PacketOption>());

                Boolean result = await filter.ShouldPacketBeFiltered(packet);
                Assert.False(result);

                DateTime end = DateTime.Now;
                TimeSpan diff = end - start;
                TimeSpan timeToWait = timePerPacket - diff;

                if (timeToWait.TotalMilliseconds > 0)
                {
                    await Task.Delay(timeToWait);
                }
            }
        }

        [Fact]
        public async Task ShouldPacketBeFilterd_ToManyPackets()
        {
            IPv6Address serverAddress = IPv6Address.FromByteArray(_rand.NextBytes(16));

            DHCPv6RateLimitBasedFilter filter = new DHCPv6RateLimitBasedFilter(
                          Mock.Of<ILogger<DHCPv6RateLimitBasedFilter>>());

            UInt16 packetsPerSeconds = (UInt16)_rand.Next(4, 10);
            filter.PacketsPerSecons = packetsPerSeconds;
            TimeSpan timePerPacket = TimeSpan.FromSeconds(packetsPerSeconds / 1000.0);

            IPv6Address address = IPv6Address.FromByteArray(_rand.NextBytes(16));

            Int32 durationInSecods = _rand.Next(3, 10);

            for (int i = 0; i < durationInSecods; i++)
            {
                DateTime tempNow = DateTime.Now;
                await Task.Delay(1000 - tempNow.Millisecond);

                Int32 packetAmount = _rand.Next(packetsPerSeconds * 2, packetsPerSeconds * 4);
                Boolean limitShouldBeExceeded = true;
                if (_rand.NextDouble() > 0.5)
                {
                    packetAmount = _rand.Next(1, packetsPerSeconds);
                    limitShouldBeExceeded = false;
                }

                for (int j = 0; j < packetAmount; j++)
                {
                    DateTime start = DateTime.Now;

                    IPv6HeaderInformation header = new IPv6HeaderInformation(address, serverAddress);
                    DHCPv6Packet packet = new DHCPv6Packet(header, 1, DHCPv6PacketTypes.Solicit, Array.Empty<DHCPv6PacketOption>());

                    Boolean result = await filter.ShouldPacketBeFiltered(packet);

                    if (j > filter.PacketsPerSecons)
                    {
                        if (limitShouldBeExceeded == true)
                        {
                            Assert.True(result);
                        }
                        else
                        {
                            Assert.False(result);
                        }
                    }
                    else
                    {
                        Assert.False(result);
                    }

                    DateTime end = DateTime.Now;
                    TimeSpan diff = end - start;
                    TimeSpan timeToWait = (timePerPacket - diff) - TimeSpan.FromMilliseconds(10);

                    if (timeToWait.TotalMilliseconds > 0)
                    {
                        await Task.Delay(timeToWait);
                    }
                }
            }
        }
    }
}
