using DaAPI.Core.Packets;
using DaAPI.Core.Packets.DHCPv6;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Infrastructure.ServiceBus.Messages
{
    public class DHCPv6PacketFileteredMessage : IMessage
    {
        public DHCPv6Packet Packet { get; private set; }
        public String FilterName { get; private set; }

        public DHCPv6PacketFileteredMessage(DHCPv6Packet packet, String filterName)
        {
            Packet = packet;
            FilterName = filterName;
        }
    }
}
