using DaAPI.Core.Common;
using DaAPI.Core.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Packets.DHCPv6
{
    public class DHCPv6PacketByteOption : DHCPv6PacketOption, IEquatable<DHCPv6PacketByteOption>
    {
        #region const

        private const UInt16 _expectedDataLength = 1;

        #endregion

        #region Properties

        public Byte Value { get; private set; }

        #endregion

        #region Constructor

        public DHCPv6PacketByteOption(UInt16 code, Byte value)
            : base(code, new Byte[] { value })
        {
            Value = value;
        }

        public DHCPv6PacketByteOption(DHCPv6PacketOptionTypes code, Byte value)
           : this((UInt16)code, value)
        {

        }

        public static DHCPv6PacketByteOption FromByteArray(Byte[] data, Int32 offset)
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

            Byte value = data[offset + 4];

            return new DHCPv6PacketByteOption(code, value);
        }

        #endregion

        #region methods

        public override string ToString()
        {
            return $"type: {Code} | value : {Value}";
        }

        public bool Equals(DHCPv6PacketByteOption other)
        {
            return base.Equals(other);
        }

        #endregion
    }
}
