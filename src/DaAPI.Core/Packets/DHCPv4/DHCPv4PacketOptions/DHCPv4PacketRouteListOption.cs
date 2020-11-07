using DaAPI.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DaAPI.Core.Packets.DHCPv4
{
    public class DHCPv4PacketRouteListOption : DHCPv4PacketOption, IEquatable<DHCPv4PacketRouteListOption>
    {
        #region Fields

        #endregion

        #region Properties

        public IEnumerable<IPv4Route> Routes { get; private set; }


        #endregion

        #region constructor and factories

        public DHCPv4PacketRouteListOption(DHCPv4OptionTypes type, IEnumerable<IPv4Route> routes) : this((Byte)type, routes)
        {

        }

        public DHCPv4PacketRouteListOption(Byte type, IEnumerable<IPv4Route> routes) : base(
            type,
            ByteHelper.ConcatBytes(
                routes.Select(x => 
                ByteHelper.ConcatBytes(new List<Byte[]> { x.Network.GetBytes(), x.SubnetMask.GetBytes()}))))
        {
            if (routes == null || routes.Any() == false)
            {
                throw new ArgumentException(nameof(routes));
            }

            Routes = new List<IPv4Route>(routes);
        }

        public static DHCPv4PacketRouteListOption FromByteArray(Byte[] data, Int32 offset)
        {
            if (data == null || data.Length < offset + 10)
            {
                throw new ArgumentException(nameof(data));
            }

            Byte length = data[offset + 1];
            Int32 routeAmount = length / 8;

            Int32 index = offset + 2;

            IPv4Route[] routes = new IPv4Route[routeAmount];

            for (int i = 0; i < routeAmount; i++, index += 8)
            {
                IPv4Address network = IPv4Address.FromByteArray(data, index);
                IPv4SubnetMask mask = IPv4SubnetMask.FromByteArray(data, index + 4);
                routes[i] = new IPv4Route(network,mask);
            }

            return new DHCPv4PacketRouteListOption(data[offset], routes);
        }

        #endregion

        #region Methods

        public bool Equals(DHCPv4PacketRouteListOption other)
        {
            return base.Equals(other);
        }

        public override string ToString()
        {
            String routes = String.Empty;
            foreach (var item in Routes)
            {
                routes += $"network: {item.Network}/{item.SubnetMask.GetSlashNotation()} ";
            }

            return $"type: {OptionType} | addresses : {routes}";
        }

        #endregion

    }
}
