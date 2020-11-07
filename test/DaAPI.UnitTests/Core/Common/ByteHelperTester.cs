using DaAPI.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace DaAPI.UnitTests.Core.Common
{
    public class ByteHelperTester
    {
        [Theory]
        [InlineData(new Byte[] { 42, 255, 255 }, UInt16.MaxValue)]
        [InlineData(new Byte[] { 37, 0, 255 }, 255)]
        [InlineData(new Byte[] { 0, 1, 0 }, 256)]
        [InlineData(new Byte[] { 4, 1, 1 }, 257)]
        public void ConvertToUInt16FromByte(Byte[] input, UInt16 expected)
        {
            UInt16 actual = ByteHelper.ConvertToUInt16FromByte(input, 1);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(new Byte[] { 42, 255, 255, 255, 255 }, UInt32.MaxValue)]
        [InlineData(new Byte[] { 37, 0, 1, 0, 1 }, 65537)]
        [InlineData(new Byte[] { 0, 0, 0, 1, 0 }, 256)]
        [InlineData(new Byte[] { 0, 0, 0, 0, 140 }, 140)]
        public void ConvertToUInt32FromByte(Byte[] input, UInt32 expected)
        {
            UInt32 actual = ByteHelper.ConvertToUInt32FromByte(input, 1);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(new Byte[] { 42, 128, 0, 0, 0 }, Int32.MinValue)]
        [InlineData(new Byte[] { 42, 127, 255, 255, 255 }, Int32.MaxValue)]
        [InlineData(new Byte[] { 42, 255, 255, 255, 255 }, -1)]
        [InlineData(new Byte[] { 0, 254, 255, 255, 255 }, -16777217)]
        [InlineData(new Byte[] { 0, 0, 0, 1, 0 }, 256)]
        [InlineData(new Byte[] { 0, 255, 255, 254, 255 }, -257)]
        [InlineData(new Byte[] { 0, 0, 0, 0, 157 }, 157)]
        public void ConvertToInt32FromByte(Byte[] input, Int32 expected)
        {
            Int32 actual = ByteHelper.ConvertToInt32FromByte(input, 1);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CopyData()
        {
            Random random = new Random();
            Byte[] input = new Byte[1024];
            random.NextBytes(input);

            Byte[] actual = ByteHelper.CopyData(input);

            Assert.Equal(input, actual);
        }

        [Fact]
        public void CopyData_WithStartIndx()
        {
            Random random = new Random();
            Byte[] input = new Byte[1024];

            Int32 skipAmount = random.Next(10, 100); ;

            Byte[] actual = ByteHelper.CopyData(input, skipAmount);

            Assert.Equal(input.Skip(skipAmount).ToArray(), actual);
        }

        [Fact]
        public void CopyData_WithStartIndxAndAmount()
        {
            Random random = new Random();
            Byte[] input = new Byte[1024];

            Int32 skipAmount = random.Next(10, 100);
            Int32 takeAmount = random.Next(10, 100);

            Byte[] actual = ByteHelper.CopyData(input, skipAmount, takeAmount);

            Assert.Equal(input.Skip(skipAmount).Take(takeAmount).ToArray(), actual);
        }

        [Fact]
        public void AreEqual_True()
        {
            Random random = new Random();
            Byte[] input = new Byte[1024];
            Byte[] copy = new Byte[1024];
            random.NextBytes(input);

            for (int i = 0; i < input.Length; i++)
            {
                copy[i] = input[i];
            }

            Boolean result = ByteHelper.AreEqual(input, copy);
            Assert.True(result);
        }

        [Fact]
        public void AreEqual_False()
        {
            Random random = new Random();
            Byte[] input = new Byte[1024];
            random.NextBytes(input);

            Byte[] copy = new Byte[1024];

            for (int i = 0; i < input.Length - 1; i++)
            {
                copy[i] = input[i];
            }

            Boolean result = ByteHelper.AreEqual(input, copy);
            Assert.False(result);
        }

        [Fact]
        public void ConcatBytes()
        {
            Random random = new Random();
            Byte[] input1 = new Byte[1024];
            Byte[] input2 = new Byte[1024];
            Byte[] input3 = new Byte[1024];

            random.NextBytes(input1);
            random.NextBytes(input2);
            random.NextBytes(input3);

            var inputs = new List<Byte[]> {
                input1,
                input2,
                input3
            };

            Byte[] result = ByteHelper.ConcatBytes(inputs);

            Int32 resultIndex = 0;
            foreach (var item in inputs)
            {
                for (int i = 0; i < item.Length; i++)
                {
                    Assert.Equal(
                        result[resultIndex++], item[i]);
                }
            }
        }

        [Fact]
        public void ConcatBytes_Bytes()
        {
            Random random = new Random();

            Int32 amount = random.Next(30, 1000);
            List<Byte> input = new List<byte>(amount);
            for (int i = 0; i < amount; i++)
            {
                input.Add((Byte)random.Next(0, 256));
            }

            Byte[] result = ByteHelper.ConcatBytes(input);
            Assert.Equal(input.ToArray(), result);
        }

        [Theory]
        [InlineData(new Byte[] { 255, 255, 255 }, "FF_FF_FF")]
        [InlineData(new Byte[] { 10, 0x4F, 0xC7 }, "0A_4F_C7")]
        public void ToStringRepresentation(Byte[] input, String expectedResult)
        {
            String result = ByteHelper.ToString(input, '_');
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData(new Byte[] { 255, 255 }, new Byte[] { 154, 00 }, new Byte[] { 154, 00 })]
        [InlineData(new Byte[] { 0, 255 }, new Byte[] { 154, 80 }, new Byte[] { 0, 80 })]
        [InlineData(new Byte[] { 128, 255 }, new Byte[] { 154, 80 }, new Byte[] { 128, 80 })]
        public void AndArray(Byte[] ipAdd, Byte[] mask, Byte[] expectedResult)
        {
            Byte[] result = ByteHelper.AndArray(ipAdd, mask);
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData(255, new Byte[] { 1, 2, 4, 8, 16, 32, 64, 128 })]
        [InlineData(0, new Byte[] { })]
        [InlineData(154, new Byte[] { 128, 16, 8, 2 })]
        [InlineData(224, new Byte[] { 128, 64, 32 })]
        public void GetBits(Byte value, Byte[] expectedBits)
        {
            IList<Byte> result = ByteHelper.GetBits(value);

            Assert.Equal(expectedBits.OrderBy(x => x), result.OrderBy(x => x));
        }
    }
}
