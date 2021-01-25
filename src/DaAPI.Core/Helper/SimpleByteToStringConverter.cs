using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Helper
{
    public static class SimpleByteToStringConverter
    {
        private static readonly Dictionary<Int32, Char> _hexMapper = new Dictionary<int, char>
        {
            { 0, '0' },
            { 1, '1' },
            { 2, '2' },
            { 3, '3' },
            { 4, '4' },
            { 5, '5' },
            { 6, '6' },
            { 7, '7' },
            { 8, '8' },
            { 9, '9' },
            { 10, 'A' },
            { 11, 'B' },
            { 12, 'C' },
            { 13, 'D' },
            { 14, 'E' },
            { 15, 'F' },
        };

        public static string Convert(byte[] input)
        {
            Char[] result = new char[input.Length * 2];
            Int32 index = 0;
            foreach (Byte item in input)
            {
                Int32 first = item / 16;
                Int32 second = item - (first * 16);

                result[index++] = _hexMapper[first];
                result[index++] = _hexMapper[second];
            }

            return new String(result);
        }
    }
}
