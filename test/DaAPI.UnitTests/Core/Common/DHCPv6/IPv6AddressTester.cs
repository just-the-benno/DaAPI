using DaAPI.Core.Common;
using DaAPI.Core.Common.DHCPv6;
using DaAPI.TestHelper;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace DaAPI.UnitTests.Core.Common.DHCPv6
{
    public class IPv6AddressTester
    {
        [Theory]
        [InlineData("::1", true, new Byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 })]
        [InlineData("dfef:6dac:4879:AAAA:bd30:447d:2550:1223", true, new Byte[16] { 0xdf, 0xef, 0x6d, 0xac, 0x48, 0x79, 0xaa, 0xaa, 0xbd, 0x30, 0x44, 0x7d, 0x25, 0x50, 0x12, 0x23 })]
        [InlineData("6dac::1223", true, new Byte[16] { 0x6d, 0xac, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x12, 0x23 })]
        [InlineData("6dac:::1223", false, new Byte[0])]
        [InlineData("6dac::22::1223", false, new Byte[0])]
        [InlineData("", false, new Byte[0])]
        [InlineData(null, false, new Byte[0])]
        [InlineData("     ", false, new Byte[0])]
        public void IPv6Address_FromString(String input, Boolean parseable, Byte[] expectedByteSequence)
        {
            if (parseable == false)
            {
                Assert.ThrowsAny<Exception>(() => IPv6Address.FromString(input));
                return;
            }

            IPv6Address address = IPv6Address.FromString(input);
            Byte[] actual = address.GetBytes();
            Assert.Equal(expectedByteSequence, actual);
        }

        [Fact]
        public void IPv6Address_Equals()
        {
            Random random = new Random();
            Byte[] firstAddressBytes = random.NextBytes(16);
            Byte[] secondAddressBytes = new Byte[16];
            firstAddressBytes.CopyTo(secondAddressBytes, 0);

            IPv6Address firstAddress = IPv6Address.FromByteArray(firstAddressBytes);
            IPv6Address secondAddress = IPv6Address.FromByteArray(secondAddressBytes);

            Boolean result = firstAddress.Equals(secondAddress);
            Assert.True(result);

            Assert.Equal(firstAddress, secondAddress);

            Byte[] thirdAddressBytes = random.NextBytes(16);
            IPv6Address thirdAddress = IPv6Address.FromByteArray(thirdAddressBytes);

            Boolean nonEqualResult = firstAddress.Equals(thirdAddress);
            Assert.False(nonEqualResult);
            Assert.NotEqual(firstAddress, thirdAddress);

            Assert.True(firstAddress == secondAddress);
            Assert.False(firstAddress != secondAddress);

            Assert.False(firstAddress == thirdAddress);
            Assert.True(firstAddress != thirdAddress);
        }

        [Theory]
        [InlineData("20e8:7e43:888f:97cb:2350:a9dd:728a:8997", "20e8:7e43:888f:97cb:2350:a9dd:728a:8a71", true)]
        public void IPv6Adress_Greater(String operant1, String operant2, Boolean expected)
        {
            IPv6Address smallestAddress = IPv6Address.FromString(operant1);
            IPv6Address greaterAddress = IPv6Address.FromString(operant2);

            Boolean result = greaterAddress > smallestAddress;
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("fef8::1", "fef8::1000:1", "fef8::1000:1000:1")]
        [InlineData("fef8::1", "fef8::2", "fef8::3")]
        [InlineData("::0", "fef8::1", "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff")]
        public void IPv6Adress_Compare(String small, String middle, String great)
        {
            IPv6Address smallestAddress = IPv6Address.FromString(small);
            IPv6Address middleAddress = IPv6Address.FromString(middle);
            IPv6Address greatestAddress = IPv6Address.FromString(great);

            {
                Int32 firstResult = smallestAddress.CompareTo(smallestAddress);
                Int32 secondResult = smallestAddress.CompareTo(middleAddress);
                Int32 thirdResult = smallestAddress.CompareTo(greatestAddress);

                Assert.Equal(0, firstResult);
                Assert.True(secondResult < 0);
                Assert.True(thirdResult < 0);

                Assert.True(smallestAddress < middleAddress);
                Assert.True(smallestAddress < greatestAddress);

                Assert.False(smallestAddress > middleAddress);
                Assert.False(smallestAddress > greatestAddress);

#pragma warning disable CS1718 // Comparison made to same variable
                Assert.True(smallestAddress <= smallestAddress);
                Assert.True(smallestAddress >= smallestAddress);
#pragma warning restore CS1718 // Comparison made to same variable

            }

            {
                Int32 firstResult = middleAddress.CompareTo(smallestAddress);
                Int32 secondResult = middleAddress.CompareTo(middleAddress);
                Int32 thirdResult = middleAddress.CompareTo(greatestAddress);

                Assert.True(firstResult > 0);
                Assert.Equal(0, secondResult);
                Assert.True(thirdResult < 0);

                Assert.True(middleAddress > smallestAddress);
                Assert.True(middleAddress < greatestAddress);

                Assert.False(middleAddress > greatestAddress);
                Assert.False(middleAddress < smallestAddress);

#pragma warning disable CS1718 // Comparison made to same variable
                Assert.True(middleAddress <= middleAddress);
                Assert.True(middleAddress >= middleAddress);
#pragma warning restore CS1718 // Comparison made to same variable

            }

            {
                Int32 firstResult = greatestAddress.CompareTo(smallestAddress);
                Int32 secondResult = greatestAddress.CompareTo(middleAddress);
                Int32 thirdResult = greatestAddress.CompareTo(greatestAddress);

                Assert.True(firstResult > 0);
                Assert.True(secondResult > 0);
                Assert.Equal(0, thirdResult);

                Assert.True(greatestAddress > smallestAddress);
                Assert.True(greatestAddress > middleAddress);

                Assert.False(greatestAddress < smallestAddress);
                Assert.False(greatestAddress < middleAddress);



#pragma warning disable CS1718 // Comparison made to same variable
                Assert.True(greatestAddress <= greatestAddress);
                Assert.True(greatestAddress >= greatestAddress);
#pragma warning restore CS1718 // Comparison made to same variable

            }
        }

        [Theory]
        [InlineData("fe80::1", "fe80:ffff::1", "fe80:ffff:ffff::1", true)]
        [InlineData("fe80::1", "fe80::2", "fe80::3", true)]
        [InlineData("::0", "fe80::1", "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff", true)]
        [InlineData("fe80::1", "fe80::1", "fe80::1", true)]
        [InlineData("fe80::1", "fe80::0", "fe80::1", false)]
        [InlineData("fe80::4", "::1", "fe80::1", false)]
        [InlineData("fe80::1", "::0", "fe80::20", false)]
        [InlineData("fe80::1", "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff", "fe80::A", false)]
        public void IPv6Adress_IsInBetween(String start, String current, String end, Boolean expectedResult)
        {
            IPv6Address startAddress = IPv6Address.FromString(start);
            IPv6Address currentAddress = IPv6Address.FromString(current);
            IPv6Address endAddress = IPv6Address.FromString(end);

            Boolean actual = currentAddress.IsBetween(startAddress, endAddress);

            Assert.Equal(expectedResult, actual);
        }

        [Theory]
        [InlineData("dfef:6dac:4879:AAAA:bd30:447d:2550:1223", "dfef:6dac:4879:AAAA:bd30:447d:2550:1223", 0)]
        [InlineData("dfef:6dac:4879:AAAA:bd30:447d:2550:1222", "dfef:6dac:4879:AAAA:bd30:447d:2550:1223", -1)]
        [InlineData("dfef:6dac:4879:AAAA:bd30:447d:2550:1223", "dfef:6dac:4879:AAAA:bd30:447d:2550:1222", 1)]
        [InlineData("dfef:6dac:4879:AAAA:bd30:447d:2550:1300", "dfef:6dac:4879:AAAA:bd30:447d:2550:1200", 256)]
        [InlineData("dfef:6dac:4879:AAAA:bd30:447d:2550:1301", "dfef:6dac:4879:AAAA:bd30:447d:2550:1200", 257)]
        [InlineData("dfef:6dac:4879:AAAA:bd30:447d:2550:1401", "dfef:6dac:4879:AAAA:bd30:447d:2550:1200", 256 + 1 + 256)]
        [InlineData("dfef:6dac:4879:AAAA:bd30:447d:2550:1200", "dfef:6dac:4879:AAAA:bd30:447d:2551:1200", -(256 * 256))]
        [InlineData("dfef:6dac:4879:AAAA:bd30:447d:2650:1200", "dfef:6dac:4879:AAAA:bd30:447d:2550:1200", (256 * 256 * 256))]
        [InlineData("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff", "::0", 3.4028236692093846346337460743177e+38 - 1)]
        public void IPv6Address_SpaceBetween(String first, String second, Double expected)
        {
            IPv6Address firstAddress = IPv6Address.FromString(first);
            IPv6Address secondAddress = IPv6Address.FromString(second);

            Double diff = firstAddress - secondAddress;

            Assert.Equal(expected, diff);

            Double otherDiff = secondAddress - firstAddress;
            Assert.Equal(diff * -1, otherDiff);
        }

        [Theory]
        [InlineData("dfef:6dac:4879:AAAA:bd30:447d:2650:1200", 10, "dfef:6dac:4879:AAAA:bd30:447d:2650:120A")]
        [InlineData("dfef:6dac:4879:AAAA:bd30:447d:2650:1200", 255, "dfef:6dac:4879:AAAA:bd30:447d:2650:12FF")]
        [InlineData("dfef:6dac:4879:AAAA:bd30:447d:2650:1200", 256, "dfef:6dac:4879:AAAA:bd30:447d:2650:1300")]
        [InlineData("dfef:6dac:4879:AAAA:bd30:447d:2650:1200", 256 * 256, "dfef:6dac:4879:AAAA:bd30:447d:2651:1200")]
        [InlineData("dfef:6dac:4879:AAAA:bd30:447d:2650:1200", 256 * 256 * 256, "dfef:6dac:4879:AAAA:bd30:447d:2750:1200")]
        [InlineData("b520:b6a0:a0ea:3e4:d2dd:16a5:1ec0:68ae", 170, "b520:b6a0:a0ea:3e4:d2dd:16a5:1ec0:6958")]
        [InlineData("fe7f:ffff:ffff:ffff:ffff:ffff:ffff:ffff", 1, "fe80::0")]

        public void IPv6Address_Add(String start, UInt64 diff, String expected)
        {
            IPv6Address startAddress = IPv6Address.FromString(start);

            IPv6Address endAddress = startAddress + diff;
            IPv6Address expectedEndAddress = IPv6Address.FromString(expected);

            Assert.Equal(expectedEndAddress, endAddress);

            IPv6Address otherStartAddress = endAddress - diff;

            Assert.Equal(startAddress, otherStartAddress);
        }

        [Theory]
        [InlineData("dfef:6dac:4879:AAAA:bd30:447d:2650:120A", 10, "dfef:6dac:4879:AAAA:bd30:447d:2650:1200")]
        [InlineData("dfef:6dac:4879:AAAA:bd30:447d:2650:1200", 1, "dfef:6dac:4879:AAAA:bd30:447d:2650:11FF")]

        [InlineData("dfef:6dac:4879:AAAA:bd30:447d:2650:1200", 255, "dfef:6dac:4879:AAAA:bd30:447d:2650:1101")]
        [InlineData("dfef:6dac:4879:AAAA:bd30:447d:2650:1200", 256, "dfef:6dac:4879:AAAA:bd30:447d:2650:1100")]
        [InlineData("dfef:6dac:4879:AAAA:bd30:447d:2650:1200", (256 * 256), "dfef:6dac:4879:AAAA:bd30:447d:264F:1200")]
        [InlineData("dfef:6dac:4879:AAAA:bd30:447d:2650:1200", (256 * 256 * 256), "dfef:6dac:4879:AAAA:bd30:447d:2550:1200")]
        [InlineData("b520:b6a0:a0ea:3e4:d2dd:16a5:1ec0:6958", 170, "b520:b6a0:a0ea:3e4:d2dd:16a5:1ec0:68ae")]
        [InlineData("fe80::0", 1, "fe7F:ffff:ffff:ffff:ffff:ffff:ffff:ffff")]

        public void IPv6Address_Subtract(String start, UInt64 diff, String expected)
        {
            IPv6Address startAddress = IPv6Address.FromString(start);

            IPv6Address endAddress = startAddress - diff;
            IPv6Address expectedEndAddress = IPv6Address.FromString(expected);

            Assert.Equal(expectedEndAddress, endAddress);

            IPv6Address otherStartAddress = endAddress + diff;

            Assert.Equal(startAddress, otherStartAddress);
        }

        [Fact]
        public void Loopback()
        {
            IPv6Address loopback = IPv6Address.Loopback;
            Assert.Equal(IPv6Address.FromString("::1"), loopback);
        }

        [Theory]
        [InlineData("fe80::1", true)]
        [InlineData("2001:e68:5423:3a2b:716b:ef6e:b215:fb96", true)]
        [InlineData("3001:e68:5423:3a2b:716b:ef6e:b215:fb96", true)]
        [InlineData("4001:e68:5423:3a2b:716b:ef6e:b215:fb96", false)]
        [InlineData("ff00::2", false)]
        [InlineData("fc04::2", true)]
        [InlineData("::1", true)]
        public void IsUnicast(String address, Boolean shouldBeUnicast)
        {
            IPv6Address parsedAddress = IPv6Address.FromString(address);
            Boolean actual = parsedAddress.IsUnicast();

            Assert.Equal(shouldBeUnicast, actual);
        }
    }
}
