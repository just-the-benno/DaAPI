using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DaAPI.TestHelper
{
    public class ByteConverter
    {
        public static Byte[] FromString(String input, Char seperation)
        {
            String[] parts = input.Split(seperation, StringSplitOptions.RemoveEmptyEntries);

            Byte[] result = parts.Select(x => Byte.Parse(x,System.Globalization.NumberStyles.HexNumber)).ToArray();
            return result;

        }

    }
}
