using DaAPI.Core.Packets;
using DaAPI.Core.Packets.DHCPv6;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Infrastructure.ServiceBus.Messages
{
    public class DHCPv6PacketReadyToSendMessage : IMessage
    {
        public DHCPv6Packet Packet { get; private set; }

        public DHCPv6PacketReadyToSendMessage(DHCPv6Packet packet)
        {
            Packet = packet;
        }
    }
}
