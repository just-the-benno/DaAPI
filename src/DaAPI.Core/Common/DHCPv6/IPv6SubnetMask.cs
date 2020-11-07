using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DaAPI.Core.Common.DHCPv6
{
    public class IPv6SubnetMaskIdentifier : Value<IPv6SubnetMaskIdentifier>
    {
        public Byte Value { get; private set; }

        public IPv6SubnetMaskIdentifier(Byte value)
        {
            if (value > 128) { throw new ArgumentOutOfRangeException(nameof(value)); }

            Value = value;
        }

        public static implicit operator Byte(IPv6SubnetMaskIdentifier identifier) => identifier.Value;
    }

    public class IPv6SubnetMask : Value<IPv6SubnetMask>
    {
        private readonly Byte[] _mask;
        private readonly static Byte[] _incompleteByteValues = new byte[]
        {
            0,
            128,
            192,
            224,
            240,
            248,
            252,
            254,
            255
        };

        public IPv6SubnetMaskIdentifier Identifier { get; }
        public Byte[] GetMaskBytes() => _mask.Select(x => x).ToArray();

        public IPv6SubnetMask(IPv6SubnetMaskIdentifier identifier)
        {
            Identifier = identifier;

            _mask = new byte[16];

            Int32 fullIndicies = identifier.Value / 8;
            for (int i = 0; i < fullIndicies; i++)
            {
                _mask[i] = 255;
            }

            if(fullIndicies < 16)
            {
                Int32 overhead = identifier.Value - (fullIndicies * 8);

                _mask[fullIndicies] = _incompleteByteValues[overhead];
            }
        }

        public static IPv6SubnetMask Empty => new IPv6SubnetMask(new IPv6SubnetMaskIdentifier(0));

        public Boolean IsIPv6AdressANetworkAddress(IPv6Address address)
        {
            Byte[] andResult = ByteHelper.AndArray(_mask, address.GetBytes());
            Boolean equalResult = ByteHelper.AreEqual(andResult, address.GetBytes());

            return equalResult;
        }

        public Boolean IsAddressInSubnet(IPv6Address networkAddress, IPv6Address address)
        {
            Byte[] and = ByteHelper.AndArray(address.GetBytes(), _mask);
            return ByteHelper.AreEqual(networkAddress.GetBytes(), and);
        }
    }
}
