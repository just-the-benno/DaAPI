using DaAPI.Core.Common;
using DaAPI.TestHelper;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace DaAPI.UnitTests.Core.Common.DUID
{
    public class UnknownDUIDTester
    {

        [Fact]
        public void Constructor()
        {
            Random random = new Random();
            Byte[] duidValue = random.NextBytes(20);

            UnknownDUID duid = new UnknownDUID(duidValue);

            Assert.Equal(duidValue, duid.Value);
        }

        [Fact]
        public void FromByteArray()
        {
            Random random = new Random();
            Byte[] value = random.NextBytes(20);
            Byte[] input = new Byte[value.Length + 2];
            input[0] = 0;
            input[1] = 0;
            value.CopyTo(input, 2);

            UnknownDUID duid = UnknownDUID.FromByteArray(input, 0);  new UnknownDUID(input);
            Byte[] asByteStream = duid.GetAsByteStream();

            Assert.Equal(value, duid.Value);
            Assert.Equal(input, asByteStream);
        }
    }
}
