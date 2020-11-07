using DaAPI.Core.Helper;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace DaAPI.UnitTests.Core.Helper
{
    public class SimpleByteToStringConverterTester
    {
        [Theory]
        [InlineData(new Byte[] { 0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15 }, "000102030405060708090A0B0C0D0E0F")]
        [InlineData(new Byte[] { 16,255, 150 }, "10FF96")]
        public void Convert(Byte[] input, String expected)
        {
            SimpleByteToStringConverter converter = new SimpleByteToStringConverter();
            String output = converter.Convert(input);
            Assert.Equal(expected, output);
        }

    }
}
