using DaAPI.Core.Common;
using DaAPI.Core.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Packets.DHCPv6
{
    public class DHCPv6PacketUInt16Option : DHCPv6PacketOption, IEquatable<DHCPv6PacketUInt16Option>
    {
        #region const

        private const UInt16 _expectedDataLength = 2;

        #endregion

        #region Properties

        public UInt16 Value { get; private set; }

        #endregion

        #region Constructor

        public DHCPv6PacketUInt16Option(UInt16 code, UInt16 value)
            : base(code, ByteHelper.GetBytes(value))
        {
            Value = value;
        }

        public DHCPv6PacketUInt16Option(DHCPv6PacketOptionTypes code, UInt16 value)
           : this((UInt16)code, value)
        {

        }

        public static DHCPv6PacketUInt16Option FromByteArray(Byte[] data, Int32 offset)
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

            UInt16 value = ByteHelper.ConvertToUInt16FromByte(data, offset + 4);

            return new DHCPv6PacketUInt16Option(code, value);
        }

        #endregion

        #region methods

        public override string ToString() => $"type: {Code} | value : {Value}";

        public bool Equals(DHCPv6PacketUInt16Option other) => base.Equals(other);

        #endregion
    }
}
