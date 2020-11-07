using DaAPI.Core.Common;
using DaAPI.Core.Packets.DHCPv4;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Clients.DHCPv4
{
    public static class DHCPv4TranscationEvents
    {
        public class DHCPv4TransactionCreatedEvent : EntityBasedDomainEvent
        {
            public DHCPv4Packet.DHCPv4MessagesTypes MessageType { get; set; }
            public UInt32 TransactionId { get; set; }

            public DHCPv4TransactionCreatedEvent()
            {

            }

            public DHCPv4TransactionCreatedEvent(
                Guid id,
                DHCPv4Packet.DHCPv4MessagesTypes type,
                UInt32 transactionId
                ) : base(id)
            {
                MessageType = type;
                TransactionId = transactionId;
            }

        }

        public class DHCPv4SuspiciousTransactionDiscoverdEvent : EntityBasedDomainEvent
        {
            public enum SuspiciousReasons
            {
                NonExpectedState = 1
            }

            public DHCPv4Packet IncomingPacket { get; set; }
            public SuspiciousReasons Reason { get; set; }

            public DHCPv4SuspiciousTransactionDiscoverdEvent()
            {

            }

            public DHCPv4SuspiciousTransactionDiscoverdEvent(
                Guid id, 
                DHCPv4Packet packet,
                SuspiciousReasons reason
                ) : base(id)
            {
                Reason = reason;
                IncomingPacket = packet;
            }
        }

        public class DHCPv4TransactionStateChangedEvent : EntityBasedDomainEvent
        {
            public DHCPv4Transaction.DHCPv4TransactionStates State { get; set; }

            public DHCPv4TransactionStateChangedEvent()
            {

            }

            public DHCPv4TransactionStateChangedEvent(Guid id, DHCPv4Transaction.DHCPv4TransactionStates state) : base(id)
            {
                State = state;
            }
        }

        public class DHCPv4TransactionCompletedEvent : EntityBasedDomainEvent
        {
            public Boolean CompletedSuccessfully { get; set; }

            public DHCPv4TransactionCompletedEvent()
            {

            }

            public DHCPv4TransactionCompletedEvent(Guid id, Boolean comletedSuccessfully) : base(id)
            {
                CompletedSuccessfully = comletedSuccessfully;
            }
        }


    }
}
