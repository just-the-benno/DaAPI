using DaAPI.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Packets.DHCPv6
{
    public class DHCPv6PacketBooleanOption : DHCPv6PacketOption, IEquatable<DHCPv6PacketBooleanOption>
    {
        #region const

        private const UInt16 _expectedDataLength = 1;

        #endregion

        #region Properties

        public Boolean Value { get; private set; }

        #endregion

        #region Constructor

        public DHCPv6PacketBooleanOption(DHCPv6PacketOptionTypes type, Boolean value)
           : this((UInt16)type, value)
        {

        }

        public DHCPv6PacketBooleanOption(UInt16 type, Boolean value) : base(
            type,
            new byte[] { value == true ? (Byte)1 : (Byte)0 })
        {
            Value = value;
        }

        public static DHCPv6PacketBooleanOption FromByteArray(Byte[] data, Int32 offset)
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

            Byte rawValue = data[offset + 4];
            Boolean value = false;
            if (rawValue == 1)
            {
                value = true;
            }
            else if (rawValue != 0)
            {
                throw new ArgumentException(nameof(data));
            }

            return new DHCPv6PacketBooleanOption(code, value);
        }

        #endregion

        #region methods

        public override string ToString()
        {
            return $"type: {Code} | value : {Value}";
        }

        public bool Equals(DHCPv6PacketBooleanOption other)
        {
            return base.Equals(other);
        }

        #endregion
    }
}
