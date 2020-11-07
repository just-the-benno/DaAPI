using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Packets.DHCPv6
{
    public class DHCPv6PacketReconfigureOption : DHCPv6PacketByteOption
    {
        #region Properties

        public DHCPv6PacketTypes MessageType { get; private set; }

        #endregion

        #region Constructor

        public DHCPv6PacketReconfigureOption(DHCPv6PacketTypes type) : base(DHCPv6PacketOptionTypes.Reconfigure,(Byte)type)
        {
            MessageType = type;
        }

        public new static DHCPv6PacketReconfigureOption FromByteArray(Byte[] data, Int32 offset)
        {
            var byteOption = DHCPv6PacketByteOption.FromByteArray(data, offset);

            return new DHCPv6PacketReconfigureOption((DHCPv6PacketTypes)byteOption.Value);
        }

        #endregion
    }
}
