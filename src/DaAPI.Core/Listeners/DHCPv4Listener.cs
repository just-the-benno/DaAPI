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
    public class DHCPv4Listener : DHCPListener<IPv4Address>
    {
        #region Properties


        #endregion

        #region Constructor

        public DHCPv4Listener() : base()
        {

        }

        public static DHCPv4Listener Create(
            String physicalInterfaceId, DHCPListenerName name, IPv4Address ipv6Address)
        {
            DHCPv4Listener listener = new DHCPv4Listener();
            listener.Apply(new DHCPv4ListenerCreatedEvent
            {
                InterfaceId = physicalInterfaceId,
                Name = name,
                Address = ipv6Address.ToString(),
                Id = Guid.NewGuid(),
            });

            return listener;
        }

        #endregion

        public override void Delete() => base.Apply(new DHCPv4ListenerDeletedEvent { Id = base.Id });

        protected override IPv4Address GetAddressFromString(string address) => IPv4Address.FromString(address);

        public static DHCPv4Listener FromNIC(NetworkInterface nic, IPAddress address) =>
         new DHCPv4Listener()
         {
             PhysicalAddress = nic.GetPhysicalAddress().GetAddressBytes(),
             Interfacename = new NICInterfaceName(nic.Description),
             PhysicalInterfaceId = nic.Id,
             Address = IPv4Address.FromByteArray(address.GetAddressBytes()),
             Name = new DHCPListenerName(String.Empty),
         };
    }
}
