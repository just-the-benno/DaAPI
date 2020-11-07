using DaAPI.Core.Common;
using DaAPI.Core.Common.DHCPv6;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using static DaAPI.Core.Listeners.DHCPv6ListenerEvents;

namespace DaAPI.Core.Listeners
{
    public class DHCPv6Listener : AggregateRootWithEvents
    {
        #region Properties

        public DHCPListenerName Name { get; private set; }
        public IPv6Address Address { get; private set; }
        public String PhysicalInterfaceId { get; private set; }
        public Byte[] PhysicalAddress { get; private set; }
        public NICInterfaceName Interfacename { get; private set; }

        public Boolean IsDeleted { get; private set; }

        #endregion

        #region Constructor

        public DHCPv6Listener() : base(Guid.Empty)
        {

        }

        public static DHCPv6Listener Create(
            String physicalInterfaceId, DHCPListenerName name, IPv6Address ipv6Address)
        {
            DHCPv6Listener listener = new DHCPv6Listener();
            listener.Apply(new DHCPv6ListenerCreatedEvent
            {
                InterfaceId = physicalInterfaceId,
                Name = name,
                IPv6Address = ipv6Address.ToString(),
                Id = Guid.NewGuid(),
            });

            return listener;
        }

        #endregion

        public void Delete()
        {
            base.Apply(new DHCPv6ListenerDeletedEvent { Id = base.Id });
        }

        protected override void When(DomainEvent domainEvent)
        {
            switch (domainEvent)
            {
                case DHCPv6ListenerCreatedEvent e:
                    Id = e.Id;
                    Name = new DHCPListenerName(e.Name);
                    PhysicalInterfaceId = e.InterfaceId;
                    Address = IPv6Address.FromString(e.IPv6Address);
                    break;
                case DHCPv6ListenerDeletedEvent _:
                    IsDeleted = true;
                    break;
                default:
                    break;
            }
        }

        public static DHCPv6Listener FromNIC(NetworkInterface nic, IPAddress address) =>
         new DHCPv6Listener()
         {
             PhysicalAddress = nic.GetPhysicalAddress().GetAddressBytes(),
             Interfacename = new NICInterfaceName(nic.Description),
             PhysicalInterfaceId = nic.Id,
             Address = IPv6Address.FromByteArray(address.GetAddressBytes()),
             Name = new DHCPListenerName(String.Empty),
         };
    }
}
