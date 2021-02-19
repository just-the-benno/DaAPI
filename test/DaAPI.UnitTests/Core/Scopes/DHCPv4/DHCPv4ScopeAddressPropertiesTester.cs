using DaAPI.Core.Common;
using DaAPI.Core.Scopes.DHCPv4;
using DaAPI.TestHelper;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using static DaAPI.Core.Scopes.DHCPv4.DHCPv4ScopeAddressProperties;

namespace DaAPI.UnitTests.Core.Scopes.DHCPv4
{
    public class DHCPv4ScopeAddressPropertiesTester
    {

        [Fact]
        public void DHCPv4ScopeAddressProperties_FailedConstructor_NullInputs()
        {
            Random random = new Random();
            IPv4Address end = random.GetIPv4Address();
            IPv4Address start = random.GetIPv4AddressGreaterThan(end);

            Assert.ThrowsAny<Exception>(() => new DHCPv4ScopeAddressProperties(start, end, null));
            Assert.ThrowsAny<Exception>(() => new DHCPv4ScopeAddressProperties(null, end, Array.Empty<IPv4Address>()));
            Assert.ThrowsAny<Exception>(() => new DHCPv4ScopeAddressProperties(start, null, Array.Empty<IPv4Address>()));

            Assert.ThrowsAny<Exception>(() => new DHCPv4ScopeAddressProperties(start, null, null));
            Assert.ThrowsAny<Exception>(() => new DHCPv4ScopeAddressProperties(null, end, null));
            Assert.ThrowsAny<Exception>(() => new DHCPv4ScopeAddressProperties(null, null, Array.Empty<IPv4Address>()));

            Assert.ThrowsAny<Exception>(() => new DHCPv4ScopeAddressProperties(null, null, null));
        }

        [Fact]
        public void DHCPv4ScopeAddressProperties_FailedConstructor_StartGreaterThanEnd()
        {
            Random random = new Random();
            IPv4Address end = random.GetIPv4Address();
            IPv4Address start = random.GetIPv4AddressGreaterThan(end);

            Assert.ThrowsAny<Exception>(() => new DHCPv4ScopeAddressProperties(start, end, Array.Empty<IPv4Address>()));
        }

        [Fact]
        public void DHCPv4ScopeAddressProperties_FailedConstructor_ExcludedNotInRange()
        {
            Random random = new Random();
            IPv4Address start = random.GetIPv4Address();
            IPv4Address end = random.GetIPv4AddressGreaterThan(start);
            {
                List<IPv4Address> excluded = new List<IPv4Address>();
                excluded.AddRange(random.GetIPv4AddressesGreaterThan(end));

                Assert.ThrowsAny<Exception>(() => new DHCPv4ScopeAddressProperties(start, end, excluded));
            }
            {
                List<IPv4Address> excluded = new List<IPv4Address>();
                excluded.AddRange(random.GetIPv4AddressesSmallerThan(end));

                Assert.ThrowsAny<Exception>(() => new DHCPv4ScopeAddressProperties(start, end, excluded));
            }
        }

        [Fact]
        public void DHCPv4ScopeAddressProperties_FailedConstructor_AllAddressesExcluded()
        {
            Random random = new Random();
            IPv4Address start = random.GetIPv4Address();
            IPv4Address end = random.GetIPv4AddressGreaterThan(start);
            List<IPv4Address> excluded = new List<IPv4Address>();
            IPv4Address current = start;
            do
            {
                excluded.Add(current);
                current += 1;

            } while (current <= end);

            Assert.ThrowsAny<Exception>(() => new DHCPv4ScopeAddressProperties(start, end, excluded));
        }

        [Theory]
        [InlineData("192.167.178.1", "192.167.178.1", "192.167.178.1", true)]
        [InlineData("192.167.178.1", "192.167.178.1", "192.167.178.2", false)]
        [InlineData("192.167.178.1", "192.167.178.255", "192.167.178.2", true)]
        [InlineData("10.10.0.0", "10.10.10.10", "172.27.16.0", false)]
        public void DHCPv4ScopeAddressProperties_IsAddressInRange(String startAddressRaw, String endAddressRaw, String targetAddressRaw, Boolean expectedResult)
        {
            IPv4Address start = IPv4Address.FromString(startAddressRaw);
            IPv4Address end = IPv4Address.FromString(endAddressRaw);
            IPv4Address target = IPv4Address.FromString(targetAddressRaw);

            DHCPv4ScopeAddressProperties properties = new DHCPv4ScopeAddressProperties(start, end, Array.Empty<IPv4Address>());

            Boolean actual = properties.IsAddressInRange(target);
            Assert.Equal(expectedResult, actual);
        }

        [Fact]
        public void DHCPv4ScopeAddressProperties_SingleAddressRange()
        {
            Random random = new Random();

            List<DHCPv4ScopeAddressProperties.AddressAllocationStrategies> strategies = new List<DHCPv4ScopeAddressProperties.AddressAllocationStrategies> {
                DHCPv4ScopeAddressProperties.AddressAllocationStrategies.Next,
                DHCPv4ScopeAddressProperties.AddressAllocationStrategies.Random,
            };

            IPv4Address start = random.GetIPv4Address();
            IPv4Address end = start;

            foreach (var item in strategies)
            {
                DHCPv4ScopeAddressProperties properties = new DHCPv4ScopeAddressProperties(start, end, Array.Empty<IPv4Address>(),
                    addressAllocationStrategy: item);

                IPv4Address next = properties.GetValidAddresses(Array.Empty<IPv4Address>());

                Assert.Equal(start, next);
            }
        }

        [Fact]
        public void DHCPv4ScopeAddressProperties_SingleAddressRange_NoAddressAvailable()
        {
            Random random = new Random();

            List<DHCPv4ScopeAddressProperties.AddressAllocationStrategies> strategies = new List<DHCPv4ScopeAddressProperties.AddressAllocationStrategies> {
                DHCPv4ScopeAddressProperties.AddressAllocationStrategies.Next,
                DHCPv4ScopeAddressProperties.AddressAllocationStrategies.Random,
            };

            foreach (var item in strategies)
            {
                {
                    IPv4Address start = random.GetIPv4Address();
                    IPv4Address end = start;

                    DHCPv4ScopeAddressProperties properties = new DHCPv4ScopeAddressProperties(start, end, Array.Empty<IPv4Address>(),
                        addressAllocationStrategy: item);

                    IPv4Address next = properties.GetValidAddresses(new IPv4Address[] { start });

                    Assert.Equal(IPv4Address.Empty, next);
                }
            }
        }

        [Fact]
        public void DHCPv4ScopeAddressProperties_NextAddress()
        {
            Random random = new Random();

            Int32 addressRange = random.Next(0, 255);

            IPv4Address start = random.GetIPv4Address();
            IPv4Address end = start + addressRange;

            Int32 usedAddreessAmount = random.Next(0, addressRange - 1);
            List<IPv4Address> usedAddress = new List<IPv4Address>();
            List<IPv4Address> excludedAddresses = new List<IPv4Address>();
            for (int i = 0; i < usedAddreessAmount; i++)
            {
                IPv4Address nextAddress = start + i;
                if (random.NextBoolean() == true)
                {
                    usedAddress.Add(nextAddress);
                }
                else
                {
                    excludedAddresses.Add(nextAddress);
                }
            }

            DHCPv4ScopeAddressProperties properties = new DHCPv4ScopeAddressProperties(start, end, excludedAddresses,
                addressAllocationStrategy: DHCPv4ScopeAddressProperties.AddressAllocationStrategies.Next);

            IPv4Address next = properties.GetValidAddresses(usedAddress);
            IPv4Address expectedAddress = start + usedAddreessAmount;
            Assert.Equal(expectedAddress, next);
        }

        [Fact]
        public void DHCPv4ScopeAddressProperties_RandomAddress()
        {
            Random random = new Random();

            Int32 addressRange = random.Next(0, 255);

            IPv4Address start = random.GetIPv4Address();
            IPv4Address end = start + addressRange;

            Int32 usedAddreessAmount = random.Next(0, addressRange - 1);
            List<IPv4Address> usedAddress = new List<IPv4Address>();
            List<IPv4Address> excludedAddresses = new List<IPv4Address>();
            for (int i = 0; i < usedAddreessAmount; i++)
            {
                IPv4Address nextAddress = start + i;
                if (random.NextBoolean() == true)
                {
                    usedAddress.Add(nextAddress);
                }
                else
                {
                    excludedAddresses.Add(nextAddress);
                }
            }

            DHCPv4ScopeAddressProperties properties = new DHCPv4ScopeAddressProperties(start, end, excludedAddresses,
                addressAllocationStrategy: DHCPv4ScopeAddressProperties.AddressAllocationStrategies.Random);

            Int32 checkAmount = random.Next(100, 200);

            for (int i = 0; i < checkAmount; i++)
            {

                IPv4Address next = properties.GetValidAddresses(usedAddress);

                Assert.DoesNotContain(next, usedAddress);
                Assert.DoesNotContain(next, excludedAddresses);
                Assert.True(start <= next);
                Assert.True(end >= next);
            }

        }

        [Fact]
        public void DHCPv4ScopeAddressProperties_PoolExhausted()
        {
            Random random = new Random();

            Int32 addressRange = random.Next(0, 255);

            IPv4Address start = random.GetIPv4Address();
            IPv4Address end = start + addressRange;

            List<IPv4Address> usedAddress = new List<IPv4Address>();
            List<IPv4Address> excludedAddresses = new List<IPv4Address>();
            for (int i = 0; i < addressRange + 1; i++)
            {
                IPv4Address nextAddress = start + i;
                if (random.NextBoolean() == true)
                {
                    usedAddress.Add(nextAddress);
                }
                else
                {
                    excludedAddresses.Add(nextAddress);
                }
            }

            List<DHCPv4ScopeAddressProperties.AddressAllocationStrategies> strategies = new List<DHCPv4ScopeAddressProperties.AddressAllocationStrategies> {
                DHCPv4ScopeAddressProperties.AddressAllocationStrategies.Next,
                DHCPv4ScopeAddressProperties.AddressAllocationStrategies.Random,
            };

            foreach (var item in strategies)
            {
                DHCPv4ScopeAddressProperties properties = new DHCPv4ScopeAddressProperties(start, end, excludedAddresses,
    addressAllocationStrategy: item);

                IPv4Address next = properties.GetValidAddresses(usedAddress);
                Assert.Equal(IPv4Address.Empty, next);
            }
        }

        [Fact]
        public void DHCPv4ScopeAddressProperties_ValueAreValidForRoot()
        {
            Random random = new Random();

            IPv4Address start = random.GetIPv4Address();
            IPv4Address end = random.GetIPv4AddressGreaterThan(start);

            TimeSpan renewalTime = TimeSpan.FromMinutes(random.Next(10, 100));
            TimeSpan preferredLifetime = renewalTime + TimeSpan.FromMinutes(random.Next(10, 100));
            TimeSpan validLifetime = preferredLifetime + TimeSpan.FromMinutes(random.Next(10, 100));

            DHCPv4ScopeAddressProperties validProperties = new DHCPv4ScopeAddressProperties(
                start, end, Array.Empty<IPv4Address>(),
                renewalTime, preferredLifetime, validLifetime,
                24,
                random.NextBoolean(),
                DHCPv4ScopeAddressProperties.AddressAllocationStrategies.Next,
                random.NextBoolean(), random.NextBoolean(), random.NextBoolean()
                );

            Boolean shouldBeValid = validProperties.ValueAreValidForRoot();
            Assert.True(shouldBeValid);

            List<DHCPv4ScopeAddressProperties> invalidProperties = new List<DHCPv4ScopeAddressProperties>
            {
                 new DHCPv4ScopeAddressProperties(
                start, end, Array.Empty<IPv4Address>(),
                null, preferredLifetime, validLifetime,
                24,
                random.NextBoolean(),
                DHCPv4ScopeAddressProperties.AddressAllocationStrategies.Next,
                random.NextBoolean(), random.NextBoolean(), random.NextBoolean()
                ),
                 new DHCPv4ScopeAddressProperties(
                start, end, Array.Empty<IPv4Address>(),
                renewalTime, null, validLifetime,
                24,
                random.NextBoolean(),
                DHCPv4ScopeAddressProperties.AddressAllocationStrategies.Next,
                random.NextBoolean(), random.NextBoolean(), random.NextBoolean()
                ),
                new DHCPv4ScopeAddressProperties(
                start, end, Array.Empty<IPv4Address>(),
                renewalTime, preferredLifetime, null,
                24,
                random.NextBoolean(),
                DHCPv4ScopeAddressProperties.AddressAllocationStrategies.Next,
                random.NextBoolean(), random.NextBoolean(), random.NextBoolean()
                ),
                new DHCPv4ScopeAddressProperties(
                start, end, Array.Empty<IPv4Address>(),
                renewalTime, preferredLifetime, validLifetime,
                0,
                random.NextBoolean(),
                DHCPv4ScopeAddressProperties.AddressAllocationStrategies.Next,
                random.NextBoolean(), random.NextBoolean(), random.NextBoolean()
                ),
                new DHCPv4ScopeAddressProperties(
                start, end, Array.Empty<IPv4Address>(),
                renewalTime, preferredLifetime, validLifetime,
                24,
                null,
                DHCPv4ScopeAddressProperties.AddressAllocationStrategies.Next,
                random.NextBoolean(), random.NextBoolean(), random.NextBoolean()
                ),
                new DHCPv4ScopeAddressProperties(
                start, end, Array.Empty<IPv4Address>(),
                renewalTime, preferredLifetime, validLifetime,
                24,
                random.NextBoolean(),
                null,
                random.NextBoolean(), random.NextBoolean(), random.NextBoolean()
                ),
                new DHCPv4ScopeAddressProperties(
                start, end, Array.Empty<IPv4Address>(),
                renewalTime, preferredLifetime, validLifetime,
                24,
                random.NextBoolean(),
                DHCPv4ScopeAddressProperties.AddressAllocationStrategies.Next,
                null, random.NextBoolean(), random.NextBoolean()
                ),
                new DHCPv4ScopeAddressProperties(
                start, end, Array.Empty<IPv4Address>(),
                renewalTime, preferredLifetime, validLifetime,
                24,
                random.NextBoolean(),
                DHCPv4ScopeAddressProperties.AddressAllocationStrategies.Next,
                random.NextBoolean(), null, random.NextBoolean()
                ),
                 new DHCPv4ScopeAddressProperties(
                start, end, Array.Empty<IPv4Address>(),
                renewalTime, preferredLifetime, validLifetime,
                24,
                random.NextBoolean(),
                DHCPv4ScopeAddressProperties.AddressAllocationStrategies.Next,
                random.NextBoolean(), random.NextBoolean(), null
                ),
            };

            foreach (var item in invalidProperties)
            {
                Boolean shouldBeInvalid = item.ValueAreValidForRoot();
                Assert.False(shouldBeInvalid);
            }
        }

        [Fact]
        public void DHCPv4ScopeAddressProperties_AreTimeValueValid()
        {
            Random random = new Random();

            IPv4Address start = random.GetIPv4Address();
            IPv4Address end = random.GetIPv4AddressGreaterThan(start);

            TimeSpan renewalTime = TimeSpan.FromMinutes(random.Next(10, 100));
            TimeSpan preferredLifetime = renewalTime + TimeSpan.FromMinutes(random.Next(10, 100));
            TimeSpan validLifetime = preferredLifetime + TimeSpan.FromMinutes(random.Next(10, 100));

            DHCPv4ScopeAddressProperties validInput =
                new DHCPv4ScopeAddressProperties(start, end, Array.Empty<IPv4Address>(), renewalTime: renewalTime, preferredLifetime: preferredLifetime, leaseTime: validLifetime);

            Boolean shouldBeTrue = validInput.AreTimeValueValid();
            Assert.True(shouldBeTrue);

            List<DHCPv4ScopeAddressProperties> invalidInputs = new List<DHCPv4ScopeAddressProperties>
            {
                new DHCPv4ScopeAddressProperties(start,end,Array.Empty<IPv4Address>(),renewalTime: null,preferredLifetime:null,leaseTime: null),
                new DHCPv4ScopeAddressProperties(start,end,Array.Empty<IPv4Address>(),renewalTime: renewalTime,preferredLifetime:null,leaseTime: null),
                new DHCPv4ScopeAddressProperties(start,end,Array.Empty<IPv4Address>(),renewalTime: null,preferredLifetime:preferredLifetime,leaseTime: null),
                new DHCPv4ScopeAddressProperties(start,end,Array.Empty<IPv4Address>(),renewalTime: null,preferredLifetime:null,leaseTime: validLifetime),
                new DHCPv4ScopeAddressProperties(start,end,Array.Empty<IPv4Address>(),renewalTime: renewalTime,preferredLifetime:preferredLifetime,leaseTime: null),
                new DHCPv4ScopeAddressProperties(start,end,Array.Empty<IPv4Address>(),renewalTime: renewalTime,preferredLifetime:null,leaseTime: validLifetime),
                new DHCPv4ScopeAddressProperties(start,end,Array.Empty<IPv4Address>(),renewalTime: null,preferredLifetime:preferredLifetime,leaseTime: validLifetime),

                new DHCPv4ScopeAddressProperties(start,end,Array.Empty<IPv4Address>(),renewalTime: renewalTime,preferredLifetime:validLifetime,leaseTime: preferredLifetime),
                new DHCPv4ScopeAddressProperties(start,end,Array.Empty<IPv4Address>(),renewalTime: preferredLifetime,preferredLifetime:renewalTime,leaseTime: validLifetime),
                new DHCPv4ScopeAddressProperties(start,end,Array.Empty<IPv4Address>(),renewalTime: validLifetime,preferredLifetime:preferredLifetime,leaseTime:renewalTime ),
            };

            foreach (var item in invalidInputs)
            {
                Boolean shouldBeFalse = item.AreTimeValueValid();
                Assert.False(shouldBeFalse);
            }

        }

        [Theory]
        [InlineData("10.10.10.0", "10.10.10.255", "10.10.10.100", "10.10.10.120", true)]
        [InlineData("10.10.10.0", "10.10.10.255", "10.10.10.0", "10.10.10.255", true)]
        [InlineData("10.10.10.0", "10.10.10.255", "10.10.10.0", "10.10.10.254", true)]
        [InlineData("10.10.10.0", "10.10.10.255", "10.10.10.100", "10.10.10.255", true)]

        [InlineData("10.10.10.0", "10.10.10.255", "10.10.9.0", "10.10.9.255", false)]
        [InlineData("10.10.10.0", "10.10.10.255", "10.10.11.0", "10.10.11.1", false)]
        public void DHCPv4ScopeAddressProperties_IsAddressRangeBetween(
            String startOwnRangeRaw, String endOwnRangeRaw,
            String startOtherRangeRaw, String endOtherRangeRaw,
            Boolean expectedResult
            )
        {
            IPv4Address startOwn = IPv4Address.FromString(startOwnRangeRaw);
            IPv4Address endOwn = IPv4Address.FromString(endOwnRangeRaw);

            DHCPv4ScopeAddressProperties own = new DHCPv4ScopeAddressProperties(startOwn, endOwn, Array.Empty<IPv4Address>());

            IPv4Address startOther = IPv4Address.FromString(startOtherRangeRaw);
            IPv4Address endOther = IPv4Address.FromString(endOtherRangeRaw);

            DHCPv4ScopeAddressProperties other = new DHCPv4ScopeAddressProperties(startOther, endOther, Array.Empty<IPv4Address>());

            Boolean actual = own.IsAddressRangeBetween(other);

            Assert.Equal(expectedResult, actual);

        }
    }
}
