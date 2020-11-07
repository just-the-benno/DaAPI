using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Packets.DHCPv4
{
    public class DHCPv4PacketTextOption : DHCPv4PacketOption, IEquatable<DHCPv4PacketTextOption>
    {
        #region Properties

        public String Value { get; private set; }

        #endregion

        #region constructor and factories

        public DHCPv4PacketTextOption(DHCPv4OptionTypes type, String value) : this((Byte)type, value)
        {

        }

        public DHCPv4PacketTextOption(Byte type, String value) : base(type, ASCIIEncoding.ASCII.GetBytes(value))
        {
            Value = value;
        }

        public static DHCPv4PacketTextOption FromByteArray(Byte[] data, Int32 offset)
        {
            if (data == null || data.Length < offset + 2 + 1)
            {
                throw new ArgumentException(nameof(data));
            }

            Int32 length = data[offset + 1];
            Byte[] raw = new byte[length];

            for (int i = 0; i < length; i++)
            {
                raw[i] = data[offset + 2 + i];
            }

            String text = ASCIIEncoding.ASCII.GetString(data,offset+2,length);
            return new DHCPv4PacketTextOption(data[offset], text);
        }

        #endregion

        #region Methods

        public bool Equals(DHCPv4PacketTextOption other)
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
