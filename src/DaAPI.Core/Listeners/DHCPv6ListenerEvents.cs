using DaAPI.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Listeners
{
    public static class DHCPListenerEvents
    {
        public abstract class DHCPListenerCreatedEvent : DomainEvent
        {
            public Guid Id { get; set; }
            public String Name { get; set; }
            public String Address { get; set; }
            public String InterfaceId { get; set; }
        }
        
        public abstract class DHCPListenerDeletedEvent : DomainEvent
        {
            public Guid Id { get; set; }
        }

        public class DHCPv4ListenerCreatedEvent : DHCPListenerCreatedEvent
        {

        }

        public class DHCPv4ListenerDeletedEvent : DHCPListenerDeletedEvent
        {

        }

        public class DHCPv6ListenerCreatedEvent : DHCPListenerCreatedEvent
        {

        }

        public class DHCPv6ListenerDeletedEvent : DHCPListenerDeletedEvent
        {

        }
    }
}
