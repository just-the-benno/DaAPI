using DaAPI.Core.Packets.DHCPv6;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaAPI.Infrastructure.ServiceBus.Messages
{
    public record DHCPv6PacketArrivedMessage(DHCPv6Packet Packet) : IMessage;
    public record DHCPv6PacketFilteredMessage(DHCPv6Packet Packet, String FilterName) : IMessage;
    public record DHCPv6PacketReadyToSendMessage(DHCPv6Packet Packet) : IMessage;
    public record InvalidDHCPv6PacketArrivedMessage(DHCPv6Packet Packet) : IMessage;
    public record ValidDHCPv6PacketArrivedMessage(DHCPv6Packet Packet) : IMessage;
}
