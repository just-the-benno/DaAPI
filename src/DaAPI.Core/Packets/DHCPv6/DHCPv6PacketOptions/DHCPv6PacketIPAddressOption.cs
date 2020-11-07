using DaAPI.Core.Common;
using DaAPI.Core.Common.DHCPv6;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Packets.DHCPv6
{
    public class DHCPv6PacketIPAddressOption : DHCPv6PacketOption, IEquatable<DHCPv6PacketIPAddressOption>
    {
        #region const

        private const UInt16 _expectedDataLength = 16;

        #endregion

        #region Properties

        public IPv6Address Address { get; private set; }

        #endregion

        #region Cosntructor

        public DHCPv6PacketIPAddressOption(UInt16 code, IPv6Address address) :
            base(code, address.GetBytes())
        {
            Address = address;
        }

        public DHCPv6PacketIPAddressOption(DHCPv6PacketOptionTypes code, IPv6Address address) :
            this((UInt16)code, address)
        {

        }

        public static DHCPv6PacketIPAddressOption FromByteArray(Byte[] data, Int32 offset)
        {
            if (data == null || data.Length < offset + 4 + _expectedDataLength)
            {
                throw new ArgumentException(nameof(data));
            }

            UInt16 code = ByteHelper.ConvertToUInt16FromByte(data, offset);
            UInt16 length = ByteHelper.ConvertToUInt16FromByte(data, offset + 2);

            if (length != _expectedDataLength)
            {
                throw new ArgumentException(nameof(data));
            }

            IPv6Address address = IPv6Address.FromByteArray(data, offset + 4);

            return new DHCPv6PacketIPAddressOption(code, address);
        }

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"type: {Code} | value : {Address}";
        }

        public bool Equals(DHCPv6PacketIPAddressOption other)
        {
            return base.Equals(other);
        }

        #endregion
    }
}
