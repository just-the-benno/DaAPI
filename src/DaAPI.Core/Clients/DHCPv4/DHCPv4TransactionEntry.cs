using DaAPI.Core.Common;
using DaAPI.Core.Packets.DHCPv4;
using System;
using System.Collections.Generic;
using System.Text;
using static DaAPI.Core.Clients.DHCPv4.DHCPv4TransactionEntryEvents;

namespace DaAPI.Core.Clients.DHCPv4
{
    public class DHCPv4TransactionEntry : Entity
    {
        public DHCPv4Packet Request { get; private set; }
        public DHCPv4Packet Response { get; private set; }


        public DHCPv4TransactionEntry(Guid id, Action<DomainEvent> addtionalApplier) : base(id, addtionalApplier)
        {

        }

        protected override void When(DomainEvent domainEvent)
        {
            switch (domainEvent)
            {
                case DHCPv4TransactionEntryCreatedEvent e:
                    Request = e.Request;
                    Response = e.Response;
                    break;
                default:
                    break;
            }
        }

    }
}
