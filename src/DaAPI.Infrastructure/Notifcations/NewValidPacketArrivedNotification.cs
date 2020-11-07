using DaAPI.Core.Packets.DHCPv4;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Infrastructure.Notifcations
{
    public class NewValidPacketArrivedNotification : INotification
    {
        public DHCPv4Packet Packet { get; private set; }

        public NewValidPacketArrivedNotification(DHCPv4Packet packet)
        {
            Packet = packet;
        }
    }
}
