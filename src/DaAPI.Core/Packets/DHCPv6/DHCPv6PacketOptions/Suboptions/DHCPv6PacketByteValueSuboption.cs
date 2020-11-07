using DaAPI.Core.Common;
using DaAPI.Core.Helper;
using System;

namespace DaAPI.Core.Packets.DHCPv6
{
    public class DHCPv6PacketByteValueSuboption : DHCPv6PacketSuboption, IEquatable<DHCPv6PacketByteValueSuboption>
    {
        #region Properties

        #endregion

        #region Constructor

        public DHCPv6PacketByteValueSuboption(Byte[] data) : base(
            ByteHelper.ConvertToUInt16FromByte(data, 0), ByteHelper.CopyData(data, 4))
        {
        }

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"type: {Code} | content: {ByteHelper.ToString(Data)}";
        }

        public bool Equals(DHCPv6PacketByteValueSuboption other)
        {
            return base.Equals(other);
        }

        #endregion

    }
}
