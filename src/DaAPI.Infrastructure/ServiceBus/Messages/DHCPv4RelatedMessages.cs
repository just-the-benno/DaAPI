using DaAPI.Core.Packets.DHCPv4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaAPI.Infrastructure.ServiceBus.Messages
{
    public record DHCPv4PacketArrivedMessage(DHCPv4Packet Packet) : IMessage;
    public record DHCPv4PacketFilteredMessage(DHCPv4Packet Packet, String FilterName) : IMessage;
    public record InvalidDHCPv4PacketArrivedMessage(DHCPv4Packet Packet) : IMessage;
    public record ValidDHCPv4PacketArrivedMessage(DHCPv4Packet Packet) : IMessage;
    public record DHCPv4PacketReadyToSendMessage(DHCPv4Packet Packet) : IMessage;


}
