using DaAPI.Core.Packets.DHCPv6;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Infrastructure.ServiceBus.Messages
{
    public class DHCPv6PacketArrivedMessage : IMessage
    {
        public DHCPv6Packet Packet { get; private set; }

        public DHCPv6PacketArrivedMessage(DHCPv6Packet packet)
        {
            Packet = packet;
        }
    }
}
