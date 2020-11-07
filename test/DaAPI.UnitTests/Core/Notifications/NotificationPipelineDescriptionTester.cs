using DaAPI.Core.Notifications;
using DaAPI.TestHelper;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace DaAPI.UnitTests.Core.Notifications
{
    public class NotificationPipelineDescriptionTester
    {
        [Fact]
        public void FromString()
        {
            Random random = new Random();
            String value = random.GetAlphanumericString();

            NotificationPipelineDescription name = NotificationPipelineDescription.FromString(value);

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
                Assert.ThrowsAny<Exception>(() => NotificationPipelineDescription.FromString(input));
            }
        }

        [Theory]
        [InlineData(1, true)]
        [InlineData(2, true)]
        [InlineData(3, false)]
        [InlineData(200, false)]
        [InlineData(499, false)]
        [InlineData(500, false)]
        [InlineData(501, true)]
        [InlineData(1000, true)]
        public void FromString_MinMax(Int32 lenght, Boolean shouldThrowException)
        {
            Random random = new Random();
            String value = random.GetAlphanumericString(lenght);

            if (shouldThrowException == true)
            {
                Assert.ThrowsAny<Exception>(() => NotificationPipelineDescription.FromString(value));
            }
            else
            {
                NotificationPipelineDescription name = NotificationPipelineDescription.FromString(value);

                Assert.NotNull(name);
                Assert.Equal(value, name.Value);
                Assert.Equal(value, name);
            }
        }
    }
}
