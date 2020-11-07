using DaAPI.Core.Scopes;
using DaAPI.TestHelper;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace DaAPI.UnitTests.Core.Scopes
{
    public class ScopeNameTester
    {
        [Theory]
        [InlineData("my best scope name")]
        [InlineData("#1 scope name")]
        [InlineData("a-long-name")]
        [InlineData("a very important scope")]
        public void ScopeNameTester_FromString(String input)
        {
            ScopeName description = ScopeName.FromString(input);

            String result = description;
            Assert.Equal(input, result);
        }

        [Fact]
        public void ScopeNameTester_StringEmpty()
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
                Assert.ThrowsAny<Exception>(() => ScopeName.FromString(invalidInput));
            }
        }

        [Fact]
        public void ScopeName_InvalidCharsEmpty()
        {
            List<String> inputs = new List<string> {
                "asfsfsfsfsfa`f`",
                "gdg45sfsfsf**~",
            };

            foreach (var invalidInput in inputs)
            {
                Assert.ThrowsAny<Exception>(() => ScopeName.FromString(invalidInput));
            }
        }

        [Fact]
        public void ScopeName_ToShort()
        {
            Random random = new Random();
            Int32 min = ScopeName.MinLenth;

            String inputToPass = random.GetAlphanumericString(min);
            String output = ScopeName.FromString(inputToPass);

            Assert.Equal(inputToPass, output);

            List<String> inputs = new List<string> {
                random.GetAlphanumericString(min-1),
                random.GetAlphanumericString(min-2),
                random.GetAlphanumericString(min - random.Next(1,min-1)),
            };

            foreach (var invalidInput in inputs)
            {
                Assert.ThrowsAny<Exception>(() => ScopeName.FromString(invalidInput));
            }
        }

        [Fact]
        public void ScopeName_ToLong()
        {
            Random random = new Random();
            Int32 max = ScopeName.MaxLength;

            String inputToPass = random.GetAlphanumericString(max);
            String output = ScopeName.FromString(inputToPass);

            Assert.Equal(inputToPass, output);

            List<String> inputs = new List<string> {
                random.GetAlphanumericString(max+1),
                random.GetAlphanumericString(max+2),
                random.GetAlphanumericString(max+random.Next(200,400)),
            };

            foreach (var invalidInput in inputs)
            {
                Assert.ThrowsAny<Exception>(() => ScopeName.FromString(invalidInput));
            }
        }
    }
}
