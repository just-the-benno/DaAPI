using DaAPI.Core.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace DaAPI.TestHelper
{
    public class DummyEvent : DomainEvent
    {
    }

    public class DummyEventEqualityCompare : IEqualityComparer<DummyEvent>, IEqualityComparer<IEnumerable<DummyEvent>>
    {
        public bool Equals([AllowNull] DummyEvent x, [AllowNull] DummyEvent y)
        {
            return x == y;
        }

        public bool Equals([AllowNull] IEnumerable<DummyEvent> x, [AllowNull] IEnumerable<DummyEvent> y)
        {
            if (x.Count() != y.Count())
            {
                return false;
            }

            Int32 index = 0;
            for (int i = 0; i < x.Count(); i++)
            {
                if (Equals(x.ElementAt(index), y.ElementAt(index)) == false)
                {
                    return false;
                }

                index++;
            }

            return true;
        }

        public int GetHashCode([DisallowNull] DummyEvent obj) => obj.GetHashCode();
        public int GetHashCode([DisallowNull] IEnumerable<DummyEvent> obj) => obj.GetHashCode();
    }
}
