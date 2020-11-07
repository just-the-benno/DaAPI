using DaAPI.Core.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Infrastructure.Notifcations
{
    public class DHCPv4PacketHandledNotifaction : INotification
    {
        public IEnumerable<DomainEvent> ScopeChanges { get; private set; }
        public IEnumerable<DomainEvent> ClientChanges { get; private set; }

        public DHCPv4PacketHandledNotifaction(IEnumerable<DomainEvent> scopeChanges, IEnumerable<DomainEvent> clientChanges)
        {
            ScopeChanges = scopeChanges;
            ClientChanges = clientChanges;
        }
    }
}
