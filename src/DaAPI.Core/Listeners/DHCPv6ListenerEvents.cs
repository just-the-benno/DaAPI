using DaAPI.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Listeners
{
    public static class DHCPv6ListenerEvents
    {
        public class DHCPv6ListenerCreatedEvent : DomainEvent
        {
            public Guid Id { get; set; }
            public String Name { get; set; }
            public String IPv6Address { get; set; }
            public String InterfaceId { get; set; }
        }
        
        public class DHCPv6ListenerDeletedEvent : DomainEvent
        {
            public Guid Id { get; set; }
        }
    }
}
