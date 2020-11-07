using DaAPI.Core.Common;
using DaAPI.TestHelper;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace DaAPI.UnitTests.Core.Common
{
    public class DictionaryHelperTester
    {
        [Fact]
        public void ContainsKeys_True()
        {
            Random random = new Random();

            Int32 amounts = random.Next(30, 100);
            Dictionary<String, Int32> input = new Dictionary<string, int>();
            List<String> selectedKeys = new List<string>();
            for (int i = 0; i < amounts; i++)
            {
                String key = random.GetAlphanumericString(10);
                input.Add(key, random.Next());
                if (random.NextBoolean() == true)
                {
                    selectedKeys.Add(key);
                }
            }

            Boolean result = DictionaryHelper.ContainsKeys(input, selectedKeys);
            Assert.True(result);
        }

        [Fact]
        public void ContainsKeys_False()
        {
            Random random = new Random();

            Int32 amounts = random.Next(30, 100);
            Dictionary<String, Int32> input = new Dictionary<string, int>();
            List<String> selectedKeys = new List<string>();
            for (int i = 0; i < amounts; i++)
            {
                String key = random.GetAlphanumericString(10);
                input.Add(key, random.Next());
                if (random.NextBoolean() == true)
                {
                    selectedKeys.Add(key);
                }
            }

            selectedKeys.Add(random.GetAlphanumericString(3));
            
            Boolean result = DictionaryHelper.ContainsKeys(input, selectedKeys);
            Assert.False(result);
        }

    }
}
