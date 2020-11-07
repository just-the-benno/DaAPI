using DaAPI.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Packets.DHCPv6
{
    public class DHCPv6PacketTrueOption : DHCPv6PacketOption, IEquatable<DHCPv6PacketTrueOption>
    {
        #region const

        private const UInt16 _expectedDataLength = 0;

        #endregion

        #region Properties

        #endregion

        #region Constructor

        public DHCPv6PacketTrueOption(DHCPv6PacketOptionTypes type)
           : this((UInt16)type)
        {

        }

        public DHCPv6PacketTrueOption(UInt16 type) : base(
            type,
            Array.Empty<Byte>())
        {
        }

        public static DHCPv6PacketTrueOption FromByteArray(Byte[] data, Int32 offset)
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

            return new DHCPv6PacketTrueOption(code);
        }

        #endregion

        #region methods

        public override string ToString()
        {
            return $"type: {Code}";
        }

        public bool Equals(DHCPv6PacketTrueOption other)
        {
            return base.Equals(other);
        }

        #endregion
    }
}
