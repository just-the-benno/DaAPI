using DaAPI.Core.Common;
using DaAPI.Core.Helper;
using System;

namespace DaAPI.Core.Packets.DHCPv6
{
    public enum DHCPv6PacketSuboptionsType
    {
        IdentityAssociationAddress = 5,
        StatusCode = 13,
        IdentityAssociationPrefixDelegation = 26,
    }

    public abstract class DHCPv6PacketSuboption : Value,  IEquatable<DHCPv6PacketSuboption>
    {
        #region Properties

        public UInt16 Code { get; private set; }
        public Byte[] Data { get; private set; }

        #endregion

        #region Constructor

        protected DHCPv6PacketSuboption(UInt16 code, Byte[] data)
        {
            Code = code;
            Data = data;
        }

        #endregion

        #region Methods

        public Byte[] GetByteStream()
        {
            Byte[] stream = new byte[Data.Length + 4];
            InsertIntoStream(stream, 0);
            return stream;
        }

        public Int32 InsertIntoStream(Byte[] stream, Int32 offset)
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

        public bool Equals(DHCPv6PacketSuboption other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            return
                this.Code == other.Code &&
                ByteHelper.AreEqual(this.Data, other.Data) == true;
        }

        #endregion

    }
}
