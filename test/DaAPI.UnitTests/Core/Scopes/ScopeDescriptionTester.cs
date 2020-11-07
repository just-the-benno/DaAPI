using DaAPI.Core.Scopes;
using DaAPI.TestHelper;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace DaAPI.UnitTests.Core.Scopes
{
    public class ScopeDescriptionTester
    {
        [Theory]
        [InlineData("my best scope description")]
        [InlineData("a very good description")]
        [InlineData("my first description")]
        [InlineData("#1 description")]
        [InlineData("a-long-description")]
        [InlineData("a-long-des?!cription")]
        [InlineData("Abd")]
        public void ScopeDescription_FromString(String input)
        {
            ScopeDescription description = ScopeDescription.FromString(input);

            String result = description;
            Assert.Equal(input, result);
        }

        [Fact]
        public void ScopeDescription_StringEmpty()
        {
            List<String> inputs = new List<string> {
                null,
                String.Empty,
                "",
                "    ",
                "\n\r\t",
            };

            foreach (var invalidInput in inputs)
            {
                Assert.ThrowsAny<Exception>(() => ScopeDescription.FromString(invalidInput));
            }
        }

        [Fact]
        public void ScopeDescription_InvalidCharsEmpty()
        {
            List<String> inputs = new List<string> {
                "asfa`f`",
                "**~",
            };

            foreach (var invalidInput in inputs)
            {
                Assert.ThrowsAny<Exception>(() => ScopeDescription.FromString(invalidInput));
            }
        }

        [Fact]
        public void ScopeDescription_ToLong()
        {
            Random random = new Random();
            Int32 max = ScopeDescription.MaxLength;

            String inputToPass = random.GetAlphanumericString(max);
            String output = ScopeDescription.FromString(inputToPass);

            Assert.Equal(inputToPass, output);

            List<String> inputs = new List<string> {
                random.GetAlphanumericString(max+1),
                random.GetAlphanumericString(max+2),
                random.GetAlphanumericString(max+random.Next(200,400)),
            };

            foreach (var invalidInput in inputs)
            {
                Assert.ThrowsAny<Exception>(() => ScopeDescription.FromString(invalidInput));
            }

        }

    }
}
