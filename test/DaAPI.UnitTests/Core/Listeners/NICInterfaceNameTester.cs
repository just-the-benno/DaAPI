using DaAPI.Core.Listeners;
using DaAPI.TestHelper;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace DaAPI.UnitTests.Core.Listeners
{
    public class NICInterfaceNameTester
    {
        [Fact]
        public void FromString()
        {
            Random random = new Random();
            String value = random.GetAlphanumericString(20);

            NICInterfaceName name = NICInterfaceName.FromString(value);

            Assert.NotNull(name);
            Assert.Equal(value, name.Value);
            Assert.Equal(value, name);
        }

        [Fact]
        public void FromString_Failed_Empty()
        {
            List<String> emptyInputs = new List<string> { String.Empty, null, " ", "\n" };

            foreach (String input in emptyInputs)
            {
                Assert.ThrowsAny<Exception>(() => NICInterfaceName.FromString(input));
            }
        }

        [Theory]
        [InlineData(0, true)]
        [InlineData(1, false)]
        [InlineData(254, false)]
        [InlineData(255, false)]
        [InlineData(256, true)]
        [InlineData(500, true)]
        public void FromString_MinMax(Int32 lenght, Boolean shouldThrowException)
        {
            Random random = new Random();
            String value = random.GetAlphanumericString(lenght);

            if (shouldThrowException == true)
            {
                Assert.ThrowsAny<Exception>(() => NICInterfaceName.FromString(value));
            }
            else
            {
                NICInterfaceName name = NICInterfaceName.FromString(value);

                Assert.NotNull(name);
                Assert.Equal(value, name.Value);
                Assert.Equal(value, name);
            }
        }
    }
}

