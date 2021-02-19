using DaAPI.Core.Common;
using DaAPI.Core.Common.DHCPv6;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using static DaAPI.Core.Listeners.DHCPListenerEvents;

namespace DaAPI.Core.Listeners
{
    public abstract class DHCPListener<TAddress> : AggregateRootWithEvents
        where TAddress: IPAddress<TAddress>
    {
        #region Properties

        public String PhysicalInterfaceId { get; protected set; }
        public TAddress Address { get; protected set; }
        public DHCPListenerName Name { get; protected set; }
        public Boolean IsDeleted { get; protected set; }
        public Byte[] PhysicalAddress { get; protected set; }
        public NICInterfaceName Interfacename { get; protected set; }

        #endregion

        #region Constructor

        public DHCPListener() : base(Guid.Empty)
        {

        }


        public abstract void Delete();
        
        protected abstract TAddress GetAddressFromString(String address);

        protected override void When(DomainEvent domainEvent)
        {
            switch (domainEvent)
            {
                case DHCPListenerCreatedEvent e:
                    Id = e.Id;
                    Name = new DHCPListenerName(e.Name);
                    PhysicalInterfaceId = e.InterfaceId;
                    Address = GetAddressFromString(e.Address);
                    break;
                case DHCPListenerDeletedEvent _:
                    IsDeleted = true;
                    break;
                default:
                    break;
            }
        }

        #endregion
    }
}
