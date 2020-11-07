using System;
using System.Collections.Generic;
using System.Text;
using static DaAPI.Core.Packets.DHCPv4.DHCPv4Packet;

namespace DaAPI.Core.Packets.DHCPv4
{
    public class DHCPv4PacketMessageTypeOption : DHCPv4PacketOption, IEquatable<DHCPv4PacketMessageTypeOption>
    {
        #region Fields

        private const Byte _expectedDataLength = 1;

        #endregion

        #region Properties

        public DHCPv4MessagesTypes Value { get; private set; }

        #endregion

        #region constructor and factories

        public DHCPv4PacketMessageTypeOption(DHCPv4MessagesTypes value) : base(
            (Byte)DHCPv4OptionTypes.MessageType,
            new byte[] { (Byte)value })
        {
            Value = value;
        }

        public static DHCPv4PacketMessageTypeOption FromByteArray(Byte[] data, Int32 offset)
        {
            if (data == null || data.Length < offset + 2 + _expectedDataLength)
            {
                throw new ArgumentException(nameof(data));
            }

            if (data[offset] != (Byte)DHCPv4OptionTypes.MessageType)
            {
                throw new ArgumentException(nameof(data));
            }

            if (data[offset + 1] != 1)
            {
                throw new ArgumentException(nameof(data));
            }


            return new DHCPv4PacketMessageTypeOption((DHCPv4MessagesTypes)data[offset + 2]);
        }

        #endregion

        #region Methods

        public bool Equals(DHCPv4PacketMessageTypeOption other)
        {
            return base.Equals(other);
        }

        public override string ToString()
        {
            return $"message type {Value}";
        }


        #endregion

    }
}
