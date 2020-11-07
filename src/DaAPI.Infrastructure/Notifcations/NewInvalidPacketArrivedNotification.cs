using DaAPI.Core.Packets.DHCPv4;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Infrastructure.Notifcations
{
    public class NewInvalidPacketArrivedNotification : INotification
    {
        public DHCPv4Packet Packet { get; private set; }

        public NewInvalidPacketArrivedNotification(DHCPv4Packet packet)
        {
            Packet = packet;
        }
    }
}
