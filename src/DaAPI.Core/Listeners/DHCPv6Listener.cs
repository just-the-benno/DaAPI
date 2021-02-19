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
    public class DHCPv6Listener : DHCPListener<IPv6Address>
    {
        #region Properties

        #endregion

        #region Constructor

        public DHCPv6Listener() : base()
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
                Address = ipv6Address.ToString(),
                Id = Guid.NewGuid(),
            });

            return listener;
        }

        #endregion

        public override void Delete() => base.Apply(new DHCPv6ListenerDeletedEvent { Id = base.Id });

        protected override IPv6Address GetAddressFromString(string address) => IPv6Address.FromString(address);

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
