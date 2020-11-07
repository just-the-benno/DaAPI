using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace DaAPI.TestHelper
{
    public class ByteArrayComparer : IEqualityComparer<Byte[]>
    {
        public bool Equals(byte[] x,  byte[] y)
        {
            if (x.Length != y.Length) { return false; }

            for (int i = 0; i < x.Length; i++)
            {
                if (x[i] != y[i]) { return false; }
            }

            return true;
        }

        public int GetHashCode(byte[] obj)
        {
            return obj.Length;
        }
    }
}
