using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace DaAPI.TestHelper
{
    public class NonStrictDictionaryComparer<TKey, TValue> : IEqualityComparer<IDictionary<TKey, TValue>>
    {
        public bool Equals([AllowNull] IDictionary<TKey, TValue> x, [AllowNull] IDictionary<TKey, TValue> y)
        {
            if (x.Count != y.Count) { return false; }

            foreach (TKey item in x.Keys)
            {
                if (y.ContainsKey(item) == false)
                {
                    return false;
                }

                TValue xValue = x[item];
                TValue yValue = y[item];

                if (xValue.Equals(yValue) == false)
                {
                    return false;
                }
            }

            return true;

        }

        public int GetHashCode([DisallowNull] IDictionary<TKey, TValue> obj)
        {
            return obj.GetHashCode();
        }
    }

   
}
