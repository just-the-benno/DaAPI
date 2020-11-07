using DaAPI.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Packets.DHCPv4
{
    public class DHCPv4PacketRawByteOption : DHCPv4PacketOption, IEquatable<DHCPv4PacketRawByteOption>
    {
        #region constructor and factories

        private DHCPv4PacketRawByteOption(Byte type, Byte[] data) : base(type, data)
        {

        }


        public DHCPv4PacketRawByteOption FromByteArray(Byte[] data)
        {
            return FromByteArray(data, 0);
        }

        public static DHCPv4PacketRawByteOption FromByteArray(Byte[] data, Int32 offset)
        {
            if (data == null || data.Length < offset + 2)
            {
                throw new ArgumentException(nameof(data));
            }

            Int32 lenght = data[offset + 1];

            return new DHCPv4PacketRawByteOption(data[offset + 0], ByteHelper.CopyData(data, offset + 2, lenght));
        }

        #endregion

        #region Methods

        public bool Equals(DHCPv4PacketRawByteOption other)
        {
            return base.Equals(other);
        }

        #endregion

    }
}
