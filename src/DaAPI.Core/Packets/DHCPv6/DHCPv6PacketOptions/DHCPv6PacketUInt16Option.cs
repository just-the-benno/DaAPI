using DaAPI.Core.Common;
using DaAPI.Core.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Packets.DHCPv6
{
    public class DHCPv6PacketUInt32Option : DHCPv6PacketOption, IEquatable<DHCPv6PacketUInt32Option>
    {
        #region const

        private const UInt16 _expectedDataLength = 4;

        #endregion

        #region Properties

        public UInt32 Value { get; private set; }

        #endregion

        #region Constructor

        public DHCPv6PacketUInt32Option(UInt16 code, UInt32 value)
            : base(code, ByteHelper.GetBytes(value))
        {
            Value = value;
        }

        public DHCPv6PacketUInt32Option(DHCPv6PacketOptionTypes code, UInt32 value)
           : this((UInt16)code, value)
        {

        }

        public static DHCPv6PacketUInt32Option FromByteArray(Byte[] data, Int32 offset)
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

            UInt32 value = ByteHelper.ConvertToUInt32FromByte(data, offset + 4);

            return new DHCPv6PacketUInt32Option(code, value);
        }

        #endregion

        #region methods

        public override string ToString()
        {
            return $"type: {Code} | value : {Value}";
        }

        public bool Equals(DHCPv6PacketUInt32Option other)
        {
            return base.Equals(other);
        }

        #endregion
    }
}
