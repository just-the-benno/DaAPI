using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DaAPI.Infrastructure.Helper
{
    public static class IDictionaryExtentions
    {
        public static void Remove<TKey,TVale>(this IDictionary<TKey,TVale> dict, TVale value)
        {
            var elements = dict.Where(x => x.Value.Equals(value)).ToList();

            foreach (var item in elements)
            {
                dict.Remove(item.Key);
            }
        }

    }
}
