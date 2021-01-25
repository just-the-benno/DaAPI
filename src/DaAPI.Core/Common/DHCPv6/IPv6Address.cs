using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace DaAPI.Core.Common.DHCPv6
{
    public class IPv6Address : IPAddress<IPv6Address>, IComparable<IPv6Address>, IEquatable<IPv6Address>
    {
        #region Fields

        private static readonly Double[] _conversations;
        private readonly Byte[] _addressBytes;

        #endregion

        #region Constructor and Factories

        static IPv6Address()
        {
            _conversations = new Double[16];


            Double value = 1, scaledValue = 1;
            for (int i = 0; i < 16; i++)
            {
                _conversations[15 - i] = value;


                value *= 256;
                scaledValue *= 2.56;
            }
        }

        public static IPv6Address Empty => new IPv6Address(new Byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, false);

        public static IPv6Address Loopback => new IPv6Address(new Byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 }, false);


        public override Byte[] GetBytes() => ByteHelper.CopyData(_addressBytes);

        private IPv6Address(Byte[] address, Boolean copy)
        {
            if (copy == true)
            {
                _addressBytes = ByteHelper.CopyData(address);

            }
            else
            {
                _addressBytes = address;
            }
        }

        public static IPv6Address FromByteArray(Byte[] address)
        {
            if (address == null || address.Length != 16)
            {
                throw new ArgumentException(nameof(address));
            }

            return new IPv6Address(address, true);
        }

        public static IPv6Address FromByteArray(Byte[] stream, Int32 start)
        {
            if (stream == null || stream.Length < start + 16)
            {
                throw new ArgumentException(nameof(stream));
            }

            return new IPv6Address(ByteHelper.CopyData(stream, start, 16), false);
        }

        public static IPv6Address FromString(String address)
        {
            if (String.IsNullOrEmpty(address) == true)
            {
                throw new ArgumentNullException(nameof(address));
            }

            if (IPAddress.TryParse(address, out IPAddress castedAddress) == false)
            {
                throw new ArgumentException("invalid address", nameof(address));
            }

            return new IPv6Address(castedAddress.GetAddressBytes(), false);
        }

        #endregion

        #region Methods

        public override string ToString()
        {
            return new IPAddress(_addressBytes).ToString();
        }

        public override bool Equals(object other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            if (other is IPv6Address == false) { return false; }

            IPv6Address realOther = (IPv6Address)other;
            return Equals(realOther);
        }

        public bool Equals(IPv6Address other)
        {
            if (other == null) { return false; }

            for (int i = 0; i < 16; i++)
            {
                if (this._addressBytes[i] != other._addressBytes[i])
                {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode() => _addressBytes.Sum(x => x);

        private Boolean? _isUnicast;

        public bool IsUnicast()
        {
            if (_isUnicast.HasValue == false)
            {

                IPv6Address globalUnicastNetworkAddress = IPv6Address.FromString("2000::0");
                IPv6SubnetMask globalUnicastNetworkMask = new IPv6SubnetMask(new IPv6SubnetMaskIdentifier(3));

                IPv6Address uniqueLocalUnicastNetworkAddress = IPv6Address.FromString("fc00::0");
                IPv6SubnetMask uniqueLocalUnicastNetworkMask = new IPv6SubnetMask(new IPv6SubnetMaskIdentifier(7));

                IPv6Address linkLocalUnicastNetworkAddress = IPv6Address.FromString("fe80::0");
                IPv6SubnetMask linkLocalUnicastNetworkMask = new IPv6SubnetMask(new IPv6SubnetMaskIdentifier(10));

                _isUnicast =
                    globalUnicastNetworkMask.IsAddressInSubnet(globalUnicastNetworkAddress, this) ||
                    uniqueLocalUnicastNetworkMask.IsAddressInSubnet(uniqueLocalUnicastNetworkAddress, this) ||
                    linkLocalUnicastNetworkMask.IsAddressInSubnet(linkLocalUnicastNetworkAddress, this) ||
                    this == Loopback
                    ;
            }

            return _isUnicast.Value;
        }

        public int CompareTo(IPv6Address other)
        {
            for (int i = 0; i < 16; i++)
            {
                if (this._addressBytes[i] == other._addressBytes[i])
                {
                    continue;
                }

                return this._addressBytes[i] > other._addressBytes[i] ? 1 : -1;
            }

            return 0;
        }

        internal static IPv6Address FromAddress(IPv6Address address)
        {
            return new IPv6Address(address._addressBytes, true);
        }

        public override Boolean IsBetween(IPv6Address start, IPv6Address end) => this >= start && this <= end;

        public override Boolean IsGreaterThan(IPv6Address other) => this > other;


        #region Operators

        public static bool operator ==(IPv6Address addr1, IPv6Address addr2) => Equals(addr1, addr2);
        public static bool operator !=(IPv6Address addr1, IPv6Address addr2) => !Equals(addr1, addr2);

        public static Double operator -(IPv6Address addr1, IPv6Address addr2)
        {
            Byte[] rawResult = new Byte[16];

            Int16 multiplier = 1;
            if (addr2 > addr1)
            {
                var temp = addr2;
                addr2 = addr1;
                addr1 = temp;
                multiplier = -1;
            }

            Byte carry = 0;
            for (int i = 16 - 1; i >= 0; i--)
            {
                Int16 addResult = (Int16)(addr1._addressBytes[i] - addr2._addressBytes[i] - carry);

                Byte digit;
                if (addResult < 0)
                {
                    carry = 1;
                    digit = (Byte)(256 + addResult);
                }
                else
                {
                    carry = 0;
                    digit = (Byte)addResult;
                }

                rawResult[i] = digit;
            }

            Double value = 0.0;

            for (int i = 0; i < 16; i++)
            {
                value += rawResult[i] * _conversations[i];
            }

            value *= multiplier;

            return value;
        }

        public static IPv6Address operator -(IPv6Address address, Byte[] addr2)
        {
            Byte[] result = new Byte[16];
            Byte[] addr1 = address.GetBytes();

            Byte carry = 0;
            for (int i = 16 - 1; i >= 0; i--)
            {
                Int16 addResult = (Int16)(addr1[i] - addr2[i] - carry);

                Byte digit;
                if (addResult < 0)
                {
                    carry = 1;
                    digit = (Byte)(256 + addResult);
                }
                else
                {
                    carry = 0;
                    digit = (Byte)addResult;
                }

                result[i] = digit;
            }

            return new IPv6Address(result, false);
        }

        public static bool operator <(IPv6Address addr1, IPv6Address addr2)
        {
            for (int i = 0; i < 16; i++)
            {
                if (addr1._addressBytes[i] == addr2._addressBytes[i])
                {
                    continue;
                }

                return addr1._addressBytes[i] < addr2._addressBytes[i];
            }

            return false;
        }

        public static bool operator <=(IPv6Address addr1, IPv6Address addr2)
        {
            for (int i = 0; i < 16; i++)
            {
                if (addr1._addressBytes[i] == addr2._addressBytes[i])
                {
                    continue;
                }

                return addr1._addressBytes[i] < addr2._addressBytes[i];
            }

            return true;
        }

        public static bool operator >=(IPv6Address addr1, IPv6Address addr2)
        {
            for (int i = 0; i < 16; i++)
            {
                if (addr1._addressBytes[i] == addr2._addressBytes[i])
                {
                    continue;
                }

                return addr1._addressBytes[i] > addr2._addressBytes[i];
            }

            return true;
        }

        public static bool operator >(IPv6Address addr1, IPv6Address addr2)
        {
            for (int i = 0; i < 16; i++)
            {
                if (addr1._addressBytes[i] == addr2._addressBytes[i])
                {
                    continue;
                }

                return addr1._addressBytes[i] > addr2._addressBytes[i];
            }

            return false;
        }

        public static IPv6Address operator +(IPv6Address addr, UInt64 operant)
        {
            Byte[] result = new Byte[16];
            for (int i = 0; i < 7; i++)
            {
                result[i] = addr._addressBytes[i];
            }

            var temp = ByteHelper.GetBytes(operant);

            Byte[] operantAsByte = new byte[16];
            operantAsByte[8] = temp[0];
            operantAsByte[9] = temp[1];
            operantAsByte[10] = temp[2];
            operantAsByte[11] = temp[3];
            operantAsByte[12] = temp[4];
            operantAsByte[13] = temp[5];
            operantAsByte[14] = temp[6];
            operantAsByte[15] = temp[7];

            AddBytes(result, addr._addressBytes, operantAsByte);

            return new IPv6Address(result, false);
        }


        public static IPv6Address operator +(IPv6Address addr1, IPv6Address addr2)
        {
            Byte[] result = new Byte[16];


            Byte[] firstAddress = addr1.GetBytes();
            Byte[] secondAddress = addr2.GetBytes();

            AddBytes(result, firstAddress, secondAddress);

            return new IPv6Address(result, false);
        }

        private static void AddBytes(byte[] result, byte[] firstAddress, byte[] secondAddress)
        {
            Byte carry = 0;
            for (int i = 16 - 1; i >= 0; i--)
            {
                UInt16 addResult = (UInt16)(carry + firstAddress[i] + secondAddress[i]);
                Byte digit;
                if (addResult > 255)
                {
                    carry = 1;
                    digit = (Byte)(addResult - 256);
                }
                else
                {
                    carry = 0;
                    digit = (Byte)addResult;
                }

                result[i] = digit;
            }
        }

        public static IPv6Address operator -(IPv6Address addr, UInt64 operant)
        {
            Byte[] result = new Byte[16];

            var temp = ByteHelper.GetBytes(operant);

            Byte[] operantAsByte = new byte[16];
            operantAsByte[8] = temp[0];
            operantAsByte[9] = temp[1];
            operantAsByte[10] = temp[2];
            operantAsByte[11] = temp[3];
            operantAsByte[12] = temp[4];
            operantAsByte[13] = temp[5];
            operantAsByte[14] = temp[6];
            operantAsByte[15] = temp[7];

            Int16 carry = 0;
            for (int i = 16 - 1; i >= 0; i--)
            {
                Int16 addResult = (Int16)(addr._addressBytes[i] - operantAsByte[i] - carry);
                Byte digit;
                if (addResult < 0)
                {
                    carry = 1;
                    digit = (Byte)(256 + addResult);
                }
                else
                {
                    carry = 0;
                    digit = (Byte)addResult;
                }

                result[i] = digit;
            }

            return new IPv6Address(result, false);
        }

        #endregion

        #endregion

    }
}
