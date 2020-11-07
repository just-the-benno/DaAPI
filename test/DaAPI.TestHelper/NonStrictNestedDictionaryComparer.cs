using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.TestHelper
{
    public class NonStrictNestedDictionaryComparer<TOuterKey, TInnerKey, TInnerValue> : IEqualityComparer<IDictionary<TOuterKey, IDictionary<TInnerKey, TInnerValue>>>
    {
        private readonly NonStrictDictionaryComparer<TInnerKey, TInnerValue> _innerComparer = new NonStrictDictionaryComparer<TInnerKey, TInnerValue>();

        public NonStrictNestedDictionaryComparer()
        {

        }

        public bool Equals(IDictionary<TOuterKey, IDictionary<TInnerKey, TInnerValue>> x, IDictionary<TOuterKey, IDictionary<TInnerKey, TInnerValue>> y)
        {
            if (x.Count != y.Count) { return false; }

            foreach (TOuterKey item in x.Keys)
            {
                if (y.ContainsKey(item) == false)
                {
                    return false;
                }

                IDictionary<TInnerKey, TInnerValue> firstDict = x[item];
                IDictionary<TInnerKey, TInnerValue> otherDict = y[item];

                if (_innerComparer.Equals(firstDict, otherDict) == false)
                {
                    return false;
                }
            }

            return true;

        }

        public int GetHashCode(IDictionary<TOuterKey, IDictionary<TInnerKey, TInnerValue>> obj) => obj.GetHashCode();
    }
}
