using DaAPI.Core.Listeners;
using DaAPI.TestHelper;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace DaAPI.UnitTests.Core.Listeners
{
    public class DHCPListenerNameTester
    {
        [Fact]
        public void FromString()
        {
            Random random = new Random();
            String value = random.GetAlphanumericString(20);

            DHCPListenerName name = DHCPListenerName.FromString(value);

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
                Assert.ThrowsAny<Exception>(() => DHCPListenerName.FromString(input));
            }
        }

        [Theory]
        [InlineData(1, true)]
        [InlineData(2, true)]
        [InlineData(3, false)]
        [InlineData(149, false)]
        [InlineData(150, false)]
        [InlineData(151, true)]
        [InlineData(300, true)]
        public void FromString_MinMax(Int32 lenght, Boolean shouldThrowException)
        {
            Random random = new Random();
            String value = random.GetAlphanumericString(lenght);

            if (shouldThrowException == true)
            {
                Assert.ThrowsAny<Exception>(() => DHCPListenerName.FromString(value));
            }
            else
            {
                DHCPListenerName name = DHCPListenerName.FromString(value);

                Assert.NotNull(name);
                Assert.Equal(value, name.Value);
                Assert.Equal(value, name);
            }
        }
    }
}

