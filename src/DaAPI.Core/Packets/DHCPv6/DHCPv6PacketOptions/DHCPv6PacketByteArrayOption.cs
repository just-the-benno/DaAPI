using DaAPI.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Packets.DHCPv6
{
    public class DHCPv6PacketByteArrayOption : DHCPv6PacketOption
    {
        public DHCPv6PacketByteArrayOption(UInt16 code, Byte[] content) : base(code, content)
        {
        }

        public DHCPv6PacketByteArrayOption(DHCPv6PacketOptionTypes type, Byte[] content) : this((UInt16)type, content)
        {
        }

        public DHCPv6PacketByteArrayOption(DHCPv6PacketOptionTypes type, Byte[] content, Int32 contentLength)
            : this(type, ByteHelper.CopyData(content, 0, contentLength))
        {

        }

        public static DHCPv6PacketByteArrayOption FromByteArray(byte[] data, int offset)
        {
            UInt16 code = ByteHelper.ConvertToUInt16FromByte(data, offset);
            UInt16 length = ByteHelper.ConvertToUInt16FromByte(data, offset + 2);

            Byte[] content = ByteHelper.CopyData(data, offset + 4, length);
         
            return new DHCPv6PacketByteArrayOption(code, content);
        }
    }
}
