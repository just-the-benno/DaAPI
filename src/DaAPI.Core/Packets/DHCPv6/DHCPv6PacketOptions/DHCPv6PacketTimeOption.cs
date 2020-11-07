using DaAPI.Core.Common;
using DaAPI.Core.Helper;
using System;

namespace DaAPI.Core.Packets.DHCPv6
{

    public class DHCPv6PacketTimeOption : DHCPv6PacketOption, IEquatable<DHCPv6PacketTimeOption>
    {
        public enum DHCPv6PacketTimeOptionUnits
        {
            Days,
            Hours,
            Minutes,
            Seconds,
            HundredsOfSeconds,
            Milliseconds,
        }

        #region const

        #endregion

        #region Properties

        public DHCPv6PacketTimeOptionUnits Unit { get; private set; }
        public TimeSpan Value { get; private set; }

        #endregion

        #region Constructor

        public DHCPv6PacketTimeOption(UInt16 code, UInt32 timeValue, DHCPv6PacketTimeOptionUnits unit)
            : base(code, ByteHelper.GetBytes(timeValue))
        {
            switch (unit)
            {
                case DHCPv6PacketTimeOptionUnits.Days:
                    Value = TimeSpan.FromDays(timeValue);
                    break;
                case DHCPv6PacketTimeOptionUnits.Hours:
                    Value = TimeSpan.FromHours(timeValue);
                    break;
                case DHCPv6PacketTimeOptionUnits.Minutes:
                    Value = TimeSpan.FromMinutes(timeValue);
                    break;
                case DHCPv6PacketTimeOptionUnits.Seconds:
                    Value = TimeSpan.FromSeconds(timeValue);
                    break;
                case DHCPv6PacketTimeOptionUnits.HundredsOfSeconds:
                    Value = TimeSpan.FromMilliseconds(timeValue * 10);
                    break;
                case DHCPv6PacketTimeOptionUnits.Milliseconds:
                    Value = TimeSpan.FromMilliseconds(timeValue);
                    break;
                default:
                    break;
            }
        }

        public DHCPv6PacketTimeOption(DHCPv6PacketOptionTypes code, UInt32 timeValue, DHCPv6PacketTimeOptionUnits unit)
            : this((UInt16)code, timeValue, unit)
        {

        }

        public static DHCPv6PacketTimeOption FromByteArray(Byte[] data, Int32 offset, DHCPv6PacketTimeOptionUnits unit)
        {
            if (data == null || data.Length < offset + 4 )
            {
                throw new ArgumentException(nameof(data));
            }

            UInt16 code = ByteHelper.ConvertToUInt16FromByte(data, offset);
            UInt16 length = ByteHelper.ConvertToUInt16FromByte(data, offset + 2);

            UInt32 value;
            if (length == 4)
            {
                value = ByteHelper.ConvertToUInt32FromByte(data, offset + 4);
            }
            else if(length == 2)
            {
                value = ByteHelper.ConvertToUInt16FromByte(data, offset + 4);
            }
            else
            {
                throw new ArgumentException("unexpected length");
            }

            return new DHCPv6PacketTimeOption(code, value, unit);

        }


        #endregion

        #region methods

        public override string ToString()
        {
            return $"type: {Code} | value : {Value}";
        }

        public bool Equals(DHCPv6PacketTimeOption other)
        {
            return base.Equals(other);
        }

        #endregion
    }
}
