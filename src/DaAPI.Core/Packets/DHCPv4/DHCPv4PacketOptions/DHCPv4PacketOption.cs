using DaAPI.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Packets.DHCPv4
{
    public abstract class DHCPv4PacketOption : Value, IEquatable<DHCPv4PacketOption>
    {
        #region Properties

        public Byte OptionType { get; private set; }
        public Byte[] OptionData { get; private set; }

        #endregion

        #region Constructor 

        private DHCPv4PacketOption()
        {

        }

        protected DHCPv4PacketOption(Byte type,  Byte[] data)
        {
            OptionType = type;
            OptionData = ByteHelper.CopyData(data);
        }

        public static DHCPv4PacketOption NotPresented => null;

        #endregion

        #region Methods

        public Byte[] GetByteStream()
        {
            Byte[] data = new Byte[OptionData.Length+2];
            data[0] = OptionType;
            data[1] = (Byte)OptionData.Length;

            for (int i = 0; i < OptionData.Length; i++)
            {
                data[i + 2] = OptionData[i];
            }

            return data;
        }

        public Int32 AppendToStream(Byte[] stream, Int32 offset)
        {
            stream[offset] = OptionType;
            stream[offset + 1] = (Byte)OptionData.Length;

            for (int i = 0; i < OptionData.Length; i++)
            {
                stream[offset + 2 + i] = OptionData[i];
            }

            return 2 + OptionData.Length;
        }

        public bool Equals(DHCPv4PacketOption other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            return
                this.OptionType == other.OptionType &&
                ByteHelper.AreEqual(this.OptionData, other.OptionData) == true;
        }

        public override string ToString()
        {
            return $"type: {OptionType} | length: {OptionData.Length} | data : {ByteHelper.ToString(OptionData, ' ')}";
        }

        #endregion
    }
}
