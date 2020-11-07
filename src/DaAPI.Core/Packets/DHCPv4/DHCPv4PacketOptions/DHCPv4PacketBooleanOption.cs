using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Packets.DHCPv4
{
    public class DHCPv4PacketBooleanOption : DHCPv4PacketOption, IEquatable<DHCPv4PacketBooleanOption>
    {
        #region Fields

        private const Byte _expectedDataLength = 1;

        #endregion

        #region Properties

        public Boolean Value { get; private set; }

        #endregion

        #region constructor and factories

        public DHCPv4PacketBooleanOption(DHCPv4OptionTypes type, Boolean value)
            : this((Byte)type, value)
        {

        }

        public DHCPv4PacketBooleanOption(Byte type, Boolean value) : base(
            type,
            new byte[] { value == true ? (Byte)1 : (Byte)0 })
        {
            Value = value;
        }

        public static DHCPv4PacketBooleanOption FromByteArray(Byte[] data, Int32 offset)
        {
            if (data == null || data.Length < offset + 2 + _expectedDataLength)
            {
                throw new ArgumentException(nameof(data));
            }

            if (data[offset + 1] != _expectedDataLength)
            {
                throw new ArgumentException(nameof(data));
            }

            Byte rawValue = data[offset + 2];
            Boolean value = false;
            if (rawValue == 1)
            {
                value = true;
            }
            else if (rawValue != 0)
            {
                throw new ArgumentException(nameof(data));
            }

            return new DHCPv4PacketBooleanOption(data[offset], value);
        }

        #endregion

        #region methods

        public override string ToString()
        {
            return $"type: {OptionType} | value : {Value}";
        }

        public bool Equals(DHCPv4PacketBooleanOption other)
        {
            return base.Equals(other);
        }

        #endregion
    }
}
