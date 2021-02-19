using DaAPI.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Packets
{
    public abstract class DHCPPacket<TPacket, TAddress> : Value
        where TPacket : DHCPPacket<TPacket, TAddress>
        where TAddress : IPAddress<TAddress>
    {
        public static TPacket Empty => null;

        public IPHeader<TAddress> Header { get; protected set; }
        public abstract Boolean IsValid { get; }

        public abstract Byte[] GetAsStream();
    }
}
