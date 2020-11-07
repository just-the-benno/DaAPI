using DaAPI.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Packets.DHCPv4
{
    public class DHCPv4PacketUInt16Option : DHCPv4PacketOption, IEquatable<DHCPv4PacketUInt16Option>
    {
        #region Fields

        private const Int32 _expectedDataLength = 2;

        #endregion

        #region Properties

        public UInt16 Value { get; private set; }

        #endregion

        #region constructor and factories

        public DHCPv4PacketUInt16Option(DHCPv4OptionTypes type, UInt16 value) : this((Byte)type, value)
        {

        }

        public DHCPv4PacketUInt16Option(Byte type, UInt16 value) : base(type,ByteHelper.GetBytes(value))
        {
            Value = value;
        }

        public static DHCPv4PacketUInt16Option FromByteArray(Byte[] data, Int32 offset)
        {
            if (data == null || data.Length < offset + 2 + _expectedDataLength)
            {
                throw new ArgumentException(nameof(data));
            }

            if (data[offset + 1] != _expectedDataLength)
            {
                throw new ArgumentException(nameof(data));
            }

            return new DHCPv4PacketUInt16Option(data[offset], ByteHelper.ConvertToUInt16FromByte(data, offset + 2));
        }

        #endregion

        #region Methods

        public bool Equals(DHCPv4PacketUInt16Option other)
        {
            return base.Equals(other);
        }

        public override string ToString()
        {
            return $"type: {OptionType} | value : {Value}";
        }


        #endregion

    }
}
