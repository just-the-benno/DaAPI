using DaAPI.Core.Common;
using DaAPI.Core.Helper;
using System;

namespace DaAPI.Core.Packets.DHCPv6
{
    public abstract class DHCPv6PacketOption : Value, IEquatable<DHCPv6PacketOption>
    {
        #region Properties

        public UInt16 Code { get; private set; }
        public Byte[] Data { get; private set; }

        #endregion

        #region Constructor

        protected DHCPv6PacketOption(UInt16 code, Byte[] data)
        {
            Code = code;
            Data = data;
        }

        public Byte[] GetByteStream()
        {
            Byte[] data = new Byte[Data.Length + 4];
            AppendToStream(data, 0);
            return data;
        }

        public Int32 AppendToStream(Byte[] stream, Int32 offset)
        {
            Byte[] optionCode = ByteHelper.GetBytes(Code);
            Byte[] optionLength = ByteHelper.GetBytes((UInt16)Data.Length);

            stream[offset] = optionCode[0];
            stream[offset + 1] = optionCode[1];

            stream[offset + 2] = optionLength[0];
            stream[offset + 3] = optionLength[1];

            Data.CopyTo(stream, offset + 4);

            return 4 + Data.Length;
        }

        public bool Equals(DHCPv6PacketOption other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            return
                this.Code == other.Code &&
                ByteHelper.AreEqual(this.Data, other.Data) == true;
        }

        public override string ToString()
        {
            return $"type: {Code} | length: {Data.Length} | data : {ByteHelper.ToString(Data, ' ')}";
        }

        #endregion
    }
}
