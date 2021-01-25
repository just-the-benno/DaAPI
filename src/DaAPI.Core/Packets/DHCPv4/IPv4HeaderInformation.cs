using DaAPI.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Packets.DHCPv4
{
    public class IPv4HeaderInformation : IPHeader<IPv4Address>
    {
        #region Properties

        #endregion

        #region Constructor and factories

        public IPv4HeaderInformation(IPv4Address source, IPv4Address destionation) : base(source, destionation, destionation)
        {
        }

        public IPv4HeaderInformation(IPv4Address source, IPv4Address destionation, IPv4Address listener) : base(source, destionation, listener)
        {
        }

        public static IPv4HeaderInformation Default => new IPv4HeaderInformation(IPv4Address.Empty, IPv4Address.Empty);

        public static IPv4HeaderInformation AsResponse(IPHeader<IPv4Address> header) =>
            new IPv4HeaderInformation(header.Destionation, header.Source, header.ListenerAddress);

        #endregion
    }
}
