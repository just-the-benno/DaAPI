using DaAPI.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Packets.DHCPv4
{
    public class DHCPv4PacketTimeSpanOption : DHCPv4PacketOption, IEquatable<DHCPv4PacketUInt32Option>
    {
        #region Fields

        private const Int32 _expectedDataLength = 4;

        #endregion

        #region Properties

        public TimeSpan Value { get; private set; }

        #endregion

        #region constructor and factories

        public DHCPv4PacketTimeSpanOption(DHCPv4OptionTypes type, TimeSpan value, Boolean canBeNegative) : this((Byte)type, value, canBeNegative)
        {

        }

        public DHCPv4PacketTimeSpanOption(Byte type, TimeSpan value, Boolean canBeNegative) : base(
            type,
            canBeNegative == false ? ByteHelper.GetBytes((UInt32)value.TotalSeconds) : ByteHelper.GetBytes((Int32)value.TotalSeconds))
        {
            Value = value;

            if (canBeNegative == false && value.TotalSeconds < 0)
            {
                throw new ArgumentException();
            }
        }

        public static DHCPv4PacketTimeSpanOption FromByteArray(Byte[] data, Int32 offset, Boolean canBeNegative)
        {
            if (data == null || data.Length < offset + 2 + _expectedDataLength)
            {
                throw new ArgumentException(nameof(data));
            }

            if (data[offset + 1] != _expectedDataLength)
            {
                throw new ArgumentException(nameof(data));
            }

            TimeSpan seconds;
            if (canBeNegative == true)
            {
                seconds = TimeSpan.FromSeconds(ByteHelper.ConvertToInt32FromByte(data, offset + 2));
            }
            else
            {
                seconds = TimeSpan.FromSeconds(ByteHelper.ConvertToUInt32FromByte(data, offset + 2));
            }

            return new DHCPv4PacketTimeSpanOption(data[offset], seconds, canBeNegative);
        }

        #endregion

        #region Methods

        public bool Equals(DHCPv4PacketUInt32Option other)
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
