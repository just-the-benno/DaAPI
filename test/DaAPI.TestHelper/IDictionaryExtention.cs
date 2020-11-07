using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.TestHelper
{
    public static class IDictionaryExtention
    {
        public static void AddIfNotExisting<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue value)
        {
            if(dict.ContainsKey(key) == false)
            {
                dict.Add(key, value);
            }
        }

    }
}
