using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DaAPI.App.Helper
{
    public static class ByteArrayFormater
    {
        public static String ToString(byte[] data, char seperationChar)
        {
            String result = String.Empty;

            for (int i = 0; i < data.Length; i++)
            {
                result += data[i].ToString("X2");
                if (i < data.Length - 1)
                {
                    result += seperationChar;
                }
            }

            return result;
        }

        public static String PrintAsMacAddress(this Byte[] input) => ToString(input, ':');
    }
}
