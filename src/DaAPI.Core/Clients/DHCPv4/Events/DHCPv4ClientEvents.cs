using DaAPI.Core.Common;
using DaAPI.Core.Packets.DHCPv4;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Clients.DHCPv4
{
    public static class DHCPv4ClientEvents
    {
        public class DHCPv4ClientCreatedEvent : EntityBasedDomainEvent
        {
            public DHCPv4ClientIdentifier Identifier { get; set; }

            public DHCPv4ClientCreatedEvent(
                Guid id,
                DHCPv4ClientIdentifier identifier
                ) : base(id)
            {
                Identifier = identifier;
            }

            public DHCPv4ClientCreatedEvent()
            {

            }
        }

        public class DHCPv4SuspiciousTransactionByClientDiscoverdEvent : EntityBasedDomainEvent
        {
            public enum SuspiciousReasons
            {
                TransactionFoundByExpectedANew = 1,
                MessageTypeSholdNotStartANewTransaction = 2,
                TransactionNotFound = 3,
                TransactionAlreadyClosed = 4,
            }

            public DHCPv4Packet IncomingPacket { get; set; }
            public SuspiciousReasons Reason { get; set; }

            public DHCPv4SuspiciousTransactionByClientDiscoverdEvent()
            {

            }

            public DHCPv4SuspiciousTransactionByClientDiscoverdEvent(
                Guid id, 
                DHCPv4Packet packet,
                SuspiciousReasons reason
                ) : base(id)
            {
                Reason = reason;
                IncomingPacket = packet;
            }
        }
    }
}
