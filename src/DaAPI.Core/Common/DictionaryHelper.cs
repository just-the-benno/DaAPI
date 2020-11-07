using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DaAPI.Core.Common
{
    public static class DictionaryHelper
    {
        public static Boolean ContainsKeys<TKey, TValue>(this IDictionary<TKey, TValue> dict, IEnumerable<TKey> keys)
        {
            Int32 keyCount = keys.Count();
            Int32 unionCount = dict.Select(x => x.Key).Intersect(keys).Count();

            return keyCount == unionCount;
        }
    }
}
