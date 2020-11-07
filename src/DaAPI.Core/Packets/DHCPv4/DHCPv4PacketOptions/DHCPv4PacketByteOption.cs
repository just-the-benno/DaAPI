using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Packets.DHCPv4
{
    public class DHCPv4PacketByteOption : DHCPv4PacketOption, IEquatable<DHCPv4PacketByteOption>
    {
        #region Fields

        private const Byte _expectedDataLength = 1;

        #endregion

        #region Properties

        public Byte Value { get; private set; }

        #endregion

        #region constructor and factories

        public DHCPv4PacketByteOption(DHCPv4OptionTypes type, Byte value) : this((Byte)type, value)
        {

        }

        public DHCPv4PacketByteOption(Byte type, Byte value) : base(type, new byte[] { value })
        {
            Value = value;
        }

        public static DHCPv4PacketByteOption FromByteArray(Byte[] data, Int32 offset)
        {
            if (data == null || data.Length < offset + 2 + _expectedDataLength)
            {
                throw new ArgumentException(nameof(data));
            }

            if (data[offset + 1] != 1)
            {
                throw new ArgumentException(nameof(data));
            }

            return new DHCPv4PacketByteOption(data[offset], data[offset + 2]);
        }

        #endregion

        #region Methods

        public bool Equals(DHCPv4PacketByteOption other)
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
