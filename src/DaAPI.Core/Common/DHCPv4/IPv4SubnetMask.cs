using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DaAPI.Core.Common
{
    public class IPv4SubnetMaskIdentifier : Value
    {
        public Int32 Value { get; private set; }

        public IPv4SubnetMaskIdentifier(Int32 value)
        {
            if (value < 0) { throw new ArgumentOutOfRangeException(nameof(value)); }
            if (value > 32) { throw new ArgumentOutOfRangeException(nameof(value)); }

            Value = value;
        }
    }

    public class IPv4SubnetMask : Value, IEquatable<IPv4SubnetMask>
    {
        private static readonly Dictionary<Int32, Byte[]> _possibleSubnetMasks = new Dictionary<int, byte[]>
        {
            { 0, new byte[4] {0,0,0,0} },
            { 1, new byte[4] {128,0,0,0} },
            { 2, new byte[4] {192,0,0,0} },
            { 3, new byte[4] {224,0,0,0} },
            { 4, new byte[4] {240,0,0,0} },
            { 5, new byte[4] {248,0,0,0} },
            { 6, new byte[4] {252,0,0,0} },
            { 7, new byte[4] {254,0,0,0} },
            { 8, new byte[4] {255,0,0,0} },

            { 9, new byte[4] { 255,128, 0,0} },
            { 10, new byte[4] { 255,192, 0,0} },
            { 11, new byte[4] { 255,224, 0,0} },
            { 12, new byte[4] { 255,240, 0,0} },
            { 13, new byte[4] { 255,248, 0,0} },
            { 14, new byte[4] { 255,252, 0,0} },
            { 15, new byte[4] { 255,254, 0,0} },
            { 16, new byte[4] { 255,255, 0,0} },

            { 17, new byte[4] { 255, 255,128, 0} },
            { 18, new byte[4] { 255, 255,192, 0} },
            { 19, new byte[4] { 255, 255,224, 0} },
            { 20, new byte[4] { 255, 255,240, 0} },
            { 21, new byte[4] { 255, 255,248, 0} },
            { 22, new byte[4] { 255, 255,252, 0} },
            { 23, new byte[4] { 255, 255,254, 0} },
            { 24, new byte[4] { 255, 255,255, 0} },

            { 25, new byte[4] { 255, 255, 255,128} },
            { 26, new byte[4] { 255, 255, 255,192} },
            { 27, new byte[4] { 255, 255, 255,224} },
            { 28, new byte[4] { 255, 255, 255,240} },
            { 29, new byte[4] { 255, 255, 255,248} },
            { 30, new byte[4] { 255, 255, 255,252} },
            { 31, new byte[4] { 255, 255, 255,254} },
            { 32, new byte[4] { 255, 255, 255,255} },
        };

        #region Fields

        private readonly Byte[] _maskAsByte;

        public static IPv4SubnetMask AllZero => new IPv4SubnetMask(new IPv4SubnetMaskIdentifier(0));



        #endregion

        #region constructor and factories

        private IPv4SubnetMask(Byte[] data)
        {
            _maskAsByte = data;
        }

        public IPv4SubnetMask(IPv4SubnetMaskIdentifier identifier)
        {
            _maskAsByte = _possibleSubnetMasks[identifier.Value];
        }

        public static IPv4SubnetMask FromByteArray(Byte[] data)
        {
            return FromByteArray(data, 0);
        }

        public static IPv4SubnetMask FromByteArray(Byte[] stream, Int32 offset)
        {
            if (stream == null || stream.Length < offset + 4)
            {
                throw new ArgumentException(nameof(stream));
            }

            Byte[] mask = new byte[4] { stream[offset], stream[offset + 1], stream[offset + 2], stream[offset + 3] };

            List<Byte> validValues = new List<Byte> { 255, 254, 252, 248, 240, 224, 192, 128, 0 };
            Boolean lastByteWasFull = true;
            for (int i = 0; i < 4; i++)
            {
                Byte currentValue = mask[i];

                if (lastByteWasFull == false)
                {
                    if (currentValue != 0)
                    {
                        throw new ArgumentException();
                    }
                }
                else
                {
                    if (validValues.Contains(currentValue) == false)
                    {
                        throw new ArgumentException();
                    }

                    lastByteWasFull = currentValue == 255;
                }

            }

            return new IPv4SubnetMask(mask);
        }

        public static IPv4SubnetMask FromString(string rawValue)
        {
            IPv4Address pseudoAddress = IPv4Address.FromString(rawValue);
            return FromByteArray(pseudoAddress.GetBytes());
        }

        #endregion

        #region Methods

        internal Int32 GetSlashNotation()
        {
            foreach (var item in _possibleSubnetMasks)
            {
                if (ByteHelper.AreEqual(item.Value, _maskAsByte) == false) { continue; }

                return item.Key;
            }

            return -1;
        }

        public Boolean IsIPAdressANetworkAddress(IPv4Address address)
        {
            Byte[] andResult = ByteHelper.AndArray(_maskAsByte, address.GetBytes());
            Boolean equalResult = ByteHelper.AreEqual(andResult, address.GetBytes());

            return equalResult;
        }

        public byte[] GetBytes()
        {
            return new byte[4]
            {
                _maskAsByte[0],
                _maskAsByte[1],
                _maskAsByte[2],
                _maskAsByte[3]
            };
        }

        public int GetAmountOfPossibleAddresses()
        {
            Int32 slashNotation = 32 - GetSlashNotation();

            Int32 result = (Int32)Math.Pow(2, slashNotation);
            return result;
        }

        #region basics and operators

        public bool Equals(IPv4SubnetMask other)
        {
            if (other is null) { return false; }
            if (ReferenceEquals(this, other)) { return true; }

            return ByteHelper.AreEqual(this._maskAsByte, other._maskAsByte);
        }

        public override bool Equals(object other)
        {
            if (other is null) { return false; }
            if (ReferenceEquals(this, other)) { return true; }

            if (other is IPv4SubnetMask mask)
            {
                return Equals(mask);
            }
            else
            {
                return base.Equals(other);
            }
        }

        public override int GetHashCode()
        {
            return _maskAsByte.AsEnumerable().Sum(x => x);
        }

        public override string ToString()
        {
            return $"{_maskAsByte[0]}.{_maskAsByte[1]}.{_maskAsByte[2]}.{_maskAsByte[3]} (/{GetSlashNotation()})";
        }

        public static bool operator ==(IPv4SubnetMask left, IPv4SubnetMask right) => Equals(left, right);
        public static bool operator !=(IPv4SubnetMask left, IPv4SubnetMask right) => !Equals(left, right);

        #endregion

        #endregion
    }
}
