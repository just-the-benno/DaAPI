using DaAPI.Core.Common;
using DaAPI.TestHelper;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace DaAPI.UnitTests.Core.Common.DHCPv4
{
    public class IPv4AddressTester
    {
        [Theory]
        [InlineData("100.100.100.100", true, new Byte[] { 100, 100, 100, 100 })]
        [InlineData("10.11.44.255", true, new Byte[] { 10, 11, 44, 255 })]
        [InlineData("0.0.0.0", true, new Byte[] { 0, 0, 0, 0 })]
        [InlineData("255.255.255.255", true, new Byte[] { 255, 255, 255, 255 })]
        [InlineData("255.255.255", false, new Byte[0])]
        [InlineData("255.253.255.14.40", false, new Byte[0])]
        [InlineData("255.256.255.14", false, new Byte[0])]
        [InlineData("255.256.255.-14", false, new Byte[0])]
        [InlineData("255.256.255", false, new Byte[0])]
        [InlineData("", false, new Byte[0])]
        [InlineData(null, false, new Byte[0])]
        [InlineData("     ", false, new Byte[0])]
        public void IPv4Address_FromString(String input, Boolean parseable, Byte[] expectedByteSequence)
        {
            if (parseable == false)
            {
                Assert.ThrowsAny<Exception>(() => IPv4Address.FromString(input));
                return;
            }

            IPv4Address address = IPv4Address.FromString(input);
            Byte[] actual = address.GetBytes();
            Assert.Equal(expectedByteSequence, actual);
        }

        [Fact]
        public void IPv4Address_Equals()
        {
            Random random = new Random();
            Byte[] firstAddressBytes = random.NextBytes(4);
            Byte[] secondAddressBytes = new Byte[4];
            firstAddressBytes.CopyTo(secondAddressBytes, 0);

            IPv4Address firstAddress = IPv4Address.FromByteArray(firstAddressBytes);
            IPv4Address secondAddress = IPv4Address.FromByteArray(secondAddressBytes);

            Boolean result = firstAddress.Equals(secondAddress);
            Assert.True(result);

            Assert.Equal(firstAddress, secondAddress);

            Byte[] thirdAddressBytes = random.NextBytes(4);
            IPv4Address thirdAddress = IPv4Address.FromByteArray(thirdAddressBytes);

            Boolean nonEqualResult = firstAddress.Equals(thirdAddress);
            Assert.False(nonEqualResult);
            Assert.NotEqual(firstAddress, thirdAddress);

            Assert.True(firstAddress == secondAddress);
            Assert.False(firstAddress != secondAddress);

            Assert.False(firstAddress == thirdAddress);
            Assert.True(firstAddress != thirdAddress);
        }

        [Theory]
        [InlineData("194.22.0.0", "194.22.0.10", "195.20.0.20")]
        [InlineData("172.16.16.16", "172.16.16.17", "172.16.16.18")]
        [InlineData("0.0.0.0", "172.16.16.17", "255.255.255.255")]
        public void IPv4Adress_Compare(String small, String middle, String great)
        {
            IPv4Address smallestAddress = IPv4Address.FromString(small);
            IPv4Address middleAddress = IPv4Address.FromString(middle);
            IPv4Address greatestAddress = IPv4Address.FromString(great);

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
        [InlineData("194.22.0.0", "194.22.0.10", "195.20.0.20", true)]
        [InlineData("172.16.16.16", "172.16.16.17", "172.16.16.18", true)]
        [InlineData("0.0.0.0", "172.16.16.17", "255.255.255.255", true)]
        [InlineData("212.212.212.212", "212.212.212.212", "212.212.212.212", true)]
        [InlineData("212.212.212.212", "212.212.212.211", "212.212.212.212", false)]
        [InlineData("212.212.212.212", "212.212.212.200", "212.212.212.210", false)]
        [InlineData("212.212.212.212", "0.0.0.0", "212.212.212.215", false)]
        [InlineData("212.212.211.212", "255.255.255.255", "212.212.212.215", false)]
        public void IPv4Adress_IsInBetween(String start, String current, String end, Boolean expectedResult)
        {
            IPv4Address startAddress = IPv4Address.FromString(start);
            IPv4Address currentAddress = IPv4Address.FromString(current);
            IPv4Address endAddress = IPv4Address.FromString(end);

            Boolean actual = currentAddress.IsInBetween(startAddress, endAddress);

            Assert.Equal(expectedResult, actual);
        }

        [Theory]
        [InlineData("194.55.26.0", "194.55.26.0", 0)]
        [InlineData("194.55.26.0", "194.55.26.1", -1)]
        [InlineData("194.55.26.1", "194.55.26.0", 1)]
        [InlineData("194.55.27.0", "194.55.26.0", 256)]
        [InlineData("194.55.27.1", "194.55.26.0", 257)]
        [InlineData("194.55.28.1", "194.55.26.0", 256 + 1 + 256)]
        [InlineData("194.54.26.0", "194.55.26.0", -(256 * 256))]
        [InlineData("195.54.26.0", "194.54.26.0", (256 * 256 * 256))]
        [InlineData("255.255.255.255", "0.0.0.0", UInt32.MaxValue)]
        public void IPv4Address_SpaceBetween(String first, String second, Int64 expected)
        {
            IPv4Address firstAddress = IPv4Address.FromString(first);
            IPv4Address secondAddress = IPv4Address.FromString(second);

            Int64 diff = firstAddress - secondAddress;

            Assert.Equal(diff, expected);

            Int64 otherDiff = secondAddress - firstAddress;
            Assert.Equal(diff * -1, otherDiff);
        }

        [Theory]
        [InlineData("147.147.1.10", 10, "147.147.1.20")]
        [InlineData("147.147.0.0", 255, "147.147.0.255")]
        [InlineData("147.147.0.0", 256, "147.147.1.0")]
        [InlineData("147.147.0.0", 256 * 256, "147.148.0.0")]
        [InlineData("147.147.0.0", 256 * 256 * 256, "148.147.0.0")]

        [InlineData("147.147.1.10", -10, "147.147.1.0")]
        [InlineData("147.147.0.0", -255, "147.146.255.1")]
        [InlineData("147.147.0.0", -256, "147.146.255.0")]
        [InlineData("147.147.0.0", -(256 * 256), "147.146.0.0")]
        [InlineData("147.147.0.0", -(256 * 256 * 256), "146.147.0.0")]
        public void IPv4Address_Add(String start, Int32 diff, String expected)
        {
            IPv4Address startAddress = IPv4Address.FromString(start);

            IPv4Address endAddress = startAddress + diff;
            IPv4Address expectedEndAddress = IPv4Address.FromString(expected);

            Assert.Equal(expectedEndAddress, endAddress);

            IPv4Address otherStartAddress = endAddress - diff;

            Assert.Equal(startAddress, otherStartAddress);
        }
    }
}
