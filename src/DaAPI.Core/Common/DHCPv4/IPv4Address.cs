using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Common
{
    public class IPv4Address : IPAddress<IPv4Address>, IComparable<IPv4Address>, IEquatable<IPv4Address>
    {
        #region Fields

        private readonly Byte[] _addressBytes;

        private static readonly UInt32[] _conversations = new UInt32[4]
        {
            256 * 256 * 256,
            256 * 256,
            256,
            1
        };

        #endregion

        #region Constructor and Factories

        public static IPv4Address Empty => new IPv4Address(new Byte[] { 0, 0, 0, 0 });

        public override Byte[] GetBytes()
        {
            return new byte[]
            {
                _addressBytes[0],
                _addressBytes[1],
                _addressBytes[2],
                _addressBytes[3],
            };
        }

        public static IPv4Address Broadcast => new IPv4Address(new Byte[] { 255, 255, 255, 255 });

        private IPv4Address(UInt32 value)
        {
            UInt32 nextValue = value;
            Byte[] addressBytes = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                Byte byteValue = (Byte)(nextValue / _conversations[i]);
                nextValue -= (byteValue * _conversations[i]);
                addressBytes[i] = byteValue;
            }

            _addressBytes = addressBytes;

        }

        private IPv4Address(Byte[] address)
        {
            _addressBytes = address;
        }

        public static IPv4Address FromBytes(Byte firstOctet, Byte secondOcet, Byte thirdOctet, Byte fourhtOctet)
        {
            return FromByteArray(new Byte[4] { firstOctet, secondOcet, thirdOctet, fourhtOctet });
        }

        public static IPv4Address FromByteArray(Byte[] address)
        {
            if (address == null || address.Length != 4)
            {
                throw new ArgumentException(nameof(address));
            }

            return new IPv4Address(new byte[4]
            {
                address[0],
                address[1],
                address[2],
                address[3],
            });
        }

        public static IPv4Address FromByteArray(Byte[] stream, Int32 start)
        {
            if (stream == null || stream.Length < start + 4)
            {
                throw new ArgumentException(nameof(stream));
            }

            return new IPv4Address(new byte[] { stream[start], stream[start + 1], stream[start + 2], stream[start + 3] });
        }

        public static IPv4Address FromString(String address)
        {
            if (String.IsNullOrEmpty(address) == true)
            {
                throw new ArgumentNullException(nameof(address));
            }

            String[] parts = address.Trim().Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            if(parts.Length != 4)
            {
                throw new ArgumentException("invalid address", nameof(address));
            }

            Byte[] parsedAddress = new byte[4];
            for (int i = 0; i < parts.Length; i++)
            {
                String part = parts[i];
                try
                {
                    Byte addressByte = Convert.ToByte(part);
                    parsedAddress[i] = addressByte;
                }
                catch (Exception)
                {
                    throw new ArgumentException(nameof(address));
                }
            }

            return new IPv4Address(parsedAddress);
        }

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"{_addressBytes[0]}.{_addressBytes[1]}.{_addressBytes[2]}.{_addressBytes[3]}";
        }

        public override bool Equals(object other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            if (other is IPv4Address == false) { return false; }

            IPv4Address realOther = (IPv4Address)other;
            return Equals(realOther);
        }

        public bool Equals(IPv4Address other)
        {
            if (other == null) { return false; }

            for (int i = 0; i < 4; i++)
            {
                if (this._addressBytes[i] != other._addressBytes[i])
                {
                    return false;
                }
            }

            return true;
        }

        private UInt32 GetNumericValue()
        {
            UInt32 result =
                (UInt32)(_addressBytes[3] * _conversations[3]) +
                (UInt32)(_addressBytes[2] * _conversations[2]) +
                (UInt32)(_addressBytes[1] * _conversations[1]) +
                (UInt32)(_addressBytes[0] * _conversations[0]);

            return result;
        }

        public override int GetHashCode()
        {
            UInt32 numericValue = GetNumericValue();
            return (Int32)numericValue;
        }

        public int CompareTo(IPv4Address other)
        {
            UInt32 ownValue = GetNumericValue();
            UInt32 otherValue = other.GetNumericValue();

            Int64 diff = (Int64)ownValue - (Int64)otherValue;
            if (diff == 0)
            {
                return 0;
            }
            if (diff > 0)
            {
                if (diff > Int32.MaxValue)
                {
                    return Int32.MaxValue;
                }
                else
                {
                    return (Int32)diff;
                }
            }
            else
            {
                if (diff < Int32.MinValue)
                {
                    return Int32.MinValue;
                }
                else
                {
                    return (Int32)diff;
                }
            }
        }

        internal static IPv4Address FromAddress(IPv4Address address)
        {
            return new IPv4Address(new Byte[] {
                address._addressBytes[0],
                address._addressBytes[1],
                address._addressBytes[2],
                address._addressBytes[3]});
        }

        public override Boolean IsBetween(IPv4Address start, IPv4Address end)
        {
            UInt32 ownValue = GetNumericValue();
            UInt32 startValue = start.GetNumericValue();
            UInt32 endValue = end.GetNumericValue();

            return ownValue >= startValue && ownValue <= endValue;
        }

        public override Boolean IsGreaterThan(IPv4Address other) => this > other;

        #region Operators

        public static bool operator ==(IPv4Address addr1, IPv4Address addr2) => Equals(addr1, addr2);
        public static bool operator !=(IPv4Address addr1, IPv4Address addr2) => !Equals(addr1, addr2);

        public static Int64 operator -(IPv4Address addr1, IPv4Address addr2)
        {
            return (Int64)addr1.GetNumericValue() - (Int64)addr2.GetNumericValue();
        }

        public static bool operator <(IPv4Address addr1, IPv4Address addr2)
        {
            UInt32 addr1Value = addr1.GetNumericValue();
            UInt32 addr2Value = addr2.GetNumericValue();

            return addr1Value < addr2Value;
        }

        public static bool operator <=(IPv4Address addr1, IPv4Address addr2)
        {
            UInt32 addr1Value = addr1.GetNumericValue();
            UInt32 addr2Value = addr2.GetNumericValue();

            return addr1Value <= addr2Value;
        }

        public static bool operator >=(IPv4Address addr1, IPv4Address addr2)
        {
            UInt32 addr1Value = addr1.GetNumericValue();
            UInt32 addr2Value = addr2.GetNumericValue();

            return addr1Value >= addr2Value;
        }

        public static bool operator >(IPv4Address addr1, IPv4Address addr2)
        {
            UInt32 addr1Value = addr1.GetNumericValue();
            UInt32 addr2Value = addr2.GetNumericValue();

            return addr1Value > addr2Value;
        }

        public static IPv4Address operator +(IPv4Address addr, Int32 value)
        {
            UInt32 ownvalue = addr.GetNumericValue();
            if (value > ownvalue)
            {
                throw new InvalidOperationException();
            }

            UInt32 newValue = ownvalue + (UInt32)value;

            return new IPv4Address(newValue);
        }

        public static IPv4Address operator -(IPv4Address addr, Int32 value)
        {
            return addr + (-value);
        }

        #endregion

        #endregion

    }
}
