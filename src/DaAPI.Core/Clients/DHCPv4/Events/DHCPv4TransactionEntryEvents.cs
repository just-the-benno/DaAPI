using DaAPI.Core.Common;
using DaAPI.Core.Packets.DHCPv4;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Clients.DHCPv4
{
    public static class DHCPv4TransactionEntryEvents
    {
        public class DHCPv4TransactionEntryCreatedEvent : EntityBasedDomainEvent
        {
            public DHCPv4Packet Request { get; set; }
            public DHCPv4Packet Response { get; set; }
            public Guid TransactionId { get; set; }

            public DHCPv4TransactionEntryCreatedEvent()
            {

            }

            public DHCPv4TransactionEntryCreatedEvent(
                Guid id,
                Guid transactionId,
                DHCPv4Packet request,
                DHCPv4Packet response
                ) : base(id)
            {
                TransactionId = transactionId;
                Request = request;
                Response = response;
            }

        }
    }
}
