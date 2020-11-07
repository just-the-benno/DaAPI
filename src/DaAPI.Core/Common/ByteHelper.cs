using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DaAPI.Core.Common
{
    public static class ByteHelper
    {
        public static UInt16 ConvertToUInt16FromByte(Byte[] data, Int32 start)
        {
            return (UInt16)(data[start] << 8 | data[start + 1]);
        }

        public static UInt32 ConvertToUInt32FromByte(Byte[] data, Int32 start)
        {
            return (UInt32)(data[start] << 24 | data[start + 1] << 16 | data[start + 2] << 8 | data[start + 3]);
        }

        public static Int32 ConvertToInt32FromByte(Byte[] data, Int32 start)
        {
            return (Int32)(data[start] << 24 | data[start + 1] << 16 | data[start + 2] << 8 | data[start + 3]);
        }

        public static Byte[] CopyData(Byte[] input)
        {
            return CopyData(input, 0);
        }

        public static Byte[] CopyData(Byte[] input, Int32 start, Int32 length)
        {
            Byte[] result = new byte[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = input[i + start];
            }

            return result;
        }

        internal static byte[] GetBytesFromHexString(string rawByteValue) =>
            Enumerable.Range(0, rawByteValue.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(rawByteValue.Substring(x, 2), 16))
                             .ToArray();

        public static Byte[] CopyData(Byte[] input, Int32 start)
        {
            return CopyData(input, start, input.Length - start);
        }

        public static Boolean AreEqual(Byte[] input1, Byte[] input2) => AreEqual(input1, input2, 0);

        public static Boolean AreEqual(Byte[] input1, Byte[] input2, Int32 startIndex)
        {
            if (input1.Length != input2.Length) { return false; }

            for (int i = startIndex; i < input1.Length; i++)
            {
                if (input1[i] != input2[i]) { return false; }
            }

            return true;
        }

        public static byte[] ConcatBytes(IEnumerable<byte> byteSequence)
        {
            Byte[] result = new byte[byteSequence.Count()];
            Int32 index = 0;
            foreach (var item in byteSequence)
            {
                result[index++] = item;
            }

            return result;
        }

        public static byte[] ConcatBytes(params Byte[][] byteSequences)
        {
            return ConcatBytes(byteSequences.Select(x => x));
        }


        public static byte[] ConcatBytes(IEnumerable<byte[]> byteSequences)
        {
            Byte[] result = new Byte[byteSequences.Sum(x => x.Length)];

            Int32 byteIndex = 0;
            foreach (var item in byteSequences)
            {
                for (int i = 0; i < item.Length; i++, byteIndex++)
                {
                    result[byteIndex] = item[i];
                }
            }

            return result;
        }

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

        public static String ToString(byte[] data, Boolean withPrefix = true)
        {
            String result = withPrefix == true ? "0x" : String.Empty;

            for (int i = 0; i < data.Length; i++)
            {
                result += data[i].ToString("X2");
            }

            return result;
        }

        private static readonly List<Byte> evenBitValues = new List<byte>()
        {
            0b_1111_1111,
            0b_1111_1110,
            0b_1111_1100,
            0b_1111_1000,
            0b_1111_0000,
            0b_1110_0000,
            0b_1100_0000,
            0b_1000_0000,
        };

        internal static byte MakeEven(byte maskBit)
        {
            foreach (var item in evenBitValues)
            {
                if (maskBit >= item)
                {
                    return item;
                }
            }

            return 0;
        }

        //public static Byte[] GenerateMask(Int32 length, Int32 maxLength)
        //{
        //    Byte[] mask = new Byte[maxLength];
        //    Int32 currentBit = 0;
        //    for (int i = 0; i < mask.Length; i++)
        //    {
        //        if (currentBit + 8 < length)
        //        {
        //            mask[i] = 255;
        //        }
        //        else if (currentBit > length)
        //        {
        //            mask[i] = 0;
        //        }
        //        else
        //        {
        //            Int32 delta = length - currentBit;
        //            mask[i] = (Byte)(1 << delta);
        //        }
        //        currentBit += 8;

        //        if (length > (i + 1) * 8)
        //        {
        //            mask[i] = 255;
        //        }
        //    }

        //    return mask;
        //}

        public static Byte[] AndArray(Byte[] input1, Byte[] input2)
        {
            Byte[] result = new Byte[input1.Length];
            for (int i = 0; i < input1.Length; i++)
            {
                result[i] = (Byte)(input1[i] & input2[i]);
            }

            return result;
        }

        //ref https://stackoverflow.com/questions/3393717/c-sharp-converting-uint-to-byte
        public static byte[] GetBytes(Boolean value, Boolean littleEndian = false)
        {
            return ReverseAsNeeded(BitConverter.GetBytes(value), littleEndian);
        }

        public static byte[] GetBytes(Char value, Boolean littleEndian = false)
        {
            return ReverseAsNeeded(BitConverter.GetBytes(value), littleEndian);
        }
        public static byte[] GetBytes(Double value, Boolean littleEndian = false)
        {
            return ReverseAsNeeded(BitConverter.GetBytes(value), littleEndian);
        }

        public static byte[] GetBytes(Single value, Boolean littleEndian = false)
        {
            return ReverseAsNeeded(BitConverter.GetBytes(value), littleEndian);
        }
        public static byte[] GetBytes(Int32 value, Boolean littleEndian = false)
        {
            return ReverseAsNeeded(BitConverter.GetBytes(value), littleEndian);
        }
        public static byte[] GetBytes(Int64 value, Boolean littleEndian = false)
        {
            return ReverseAsNeeded(BitConverter.GetBytes(value), littleEndian);
        }
        public static byte[] GetBytes(Int16 value, Boolean littleEndian = false)
        {
            return ReverseAsNeeded(BitConverter.GetBytes(value), littleEndian);
        }
        public static byte[] GetBytes(UInt32 value, Boolean littleEndian = false)
        {
            return ReverseAsNeeded(BitConverter.GetBytes(value), littleEndian);
        }

        private static readonly List<Byte> _bitValues = new List<byte> { 1, 2, 4, 8, 16, 32, 64, 128 };

        public static IList<Byte> GetBits(Byte value)
        {
            List<Byte> result = new List<byte>();
            foreach (var item in _bitValues)
            {
                if ((value & item) == item)
                {
                    result.Add(item);
                }
            }

            return result;
        }

        public static void AddRandomBits(Random random, Byte[] value, Int32 index, Byte byteValue, Byte startValue)
        {
            IList<Byte> bits = ByteHelper.GetBits(byteValue);
            value[index] = startValue;
            if (bits.Count == 0) { return; }

            Int32 addAmount = random.Next(0, bits.Count + 1);
            for (int k = 0; k < addAmount; k++)
            {
                Int32 bitIndex = random.Next(0, bits.Count);
                value[index] += bits[bitIndex];

                bits.RemoveAt(bitIndex);
            }
        }

        //public static byte[] GetRandomBytesBetween(byte[] addressBytes1, byte[] addressBytes2, Random random)
        //{
        //    Byte[] result = new Byte[addressBytes1.Length];

        //    Boolean nextBytesRandom = false;
        //    for (int i = 0; i < addressBytes1.Length; i++)
        //    {
        //        if (nextBytesRandom == false)
        //        {
        //            Int32 delta = addressBytes2[i] - addressBytes1[i];
        //            if (delta == 0)
        //            {
        //                result[i] = addressBytes1[i];
        //            }
        //            else
        //            {
        //                Int32 actualDelta = random.Next(0, delta);
        //                result[i] = (Byte)(addressBytes1[i] + actualDelta);
        //                nextBytesRandom = true;
        //            }
        //        }
        //        else
        //        {
        //            Int32 start = 0;
        //            Int32 end = 256;
        //            if (result[i - 1] == addressBytes1[i - 1])
        //            {
        //                start = addressBytes1[i];
        //            }

        //            if (result[i - 1] == addressBytes2[i - 1])
        //            {
        //                end = addressBytes2[i] + 1;
        //            }

        //            result[i] = (Byte)random.Next(start, end);
        //        }
        //    }

        //    return result;
        //}

        public static byte[] GetBytes(UInt64 value, Boolean littleEndian = false)
        {
            return ReverseAsNeeded(BitConverter.GetBytes(value), littleEndian);
        }
        public static byte[] GetBytes(UInt16 value, Boolean littleEndian = false)
        {
            return ReverseAsNeeded(BitConverter.GetBytes(value), littleEndian);
        }

        private static byte[] ReverseAsNeeded(byte[] bytes, Boolean wantsLittleEndian)
        {
            if (wantsLittleEndian == BitConverter.IsLittleEndian)
            {
                return bytes;
            }
            else
            {
                return (byte[])bytes.Reverse().ToArray();
            }
        }
    }
}
