using DaAPI.Core.Notifications;
using DaAPI.TestHelper;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace DaAPI.UnitTests.Core.Notifications
{
    public class NotificationPipelineNameTester
    {
        [Fact]
        public void FromString()
        {
            Random random = new Random();
            String value = random.GetAlphanumericString();

            NotificationPipelineName name = NotificationPipelineName.FromString(value);

            Assert.NotNull(name);
            Assert.Equal(value, name.Value);
            Assert.Equal(value, name);
        }

        [Fact]
        public void FromString_Failed_Empty()
        {
            IEnumerable<String> emptyInputs = InputHelper.GetEmptyStringInputs();

            foreach (String input in emptyInputs)
            {
                Assert.ThrowsAny<Exception>(() => NotificationPipelineName.FromString(input));
            }
        }

        [Theory]
        [InlineData(1, true)]
        [InlineData(2, true)]
        [InlineData(3, false)]
        [InlineData(29, false)]
        [InlineData(30, false)]
        [InlineData(31, true)]
        [InlineData(100, true)]
        public void FromString_MinMax(Int32 lenght, Boolean shouldThrowException)
        {
            Random random = new Random();
            String value = random.GetAlphanumericString(lenght);

            if (shouldThrowException == true)
            {
                Assert.ThrowsAny<Exception>(() => NotificationPipelineName.FromString(value));
            }
            else
            {
                NotificationPipelineName name = NotificationPipelineName.FromString(value);

                Assert.NotNull(name);
                Assert.Equal(value, name.Value);
                Assert.Equal(value, name);
            }
        }
    }
}
