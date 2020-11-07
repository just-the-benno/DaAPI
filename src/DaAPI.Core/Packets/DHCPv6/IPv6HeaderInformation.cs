using DaAPI.Core.Common;
using DaAPI.Core.Common.DHCPv6;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace DaAPI.Core.Packets.DHCPv6
{
    public class IPv6HeaderInformation : IPHeader<IPv6Address>
    {
        #region Properties

        #endregion

        #region Constructor and factories

        public IPv6HeaderInformation(IPv6Address source, IPv6Address destionation) : base(source, destionation, destionation)
        {
        }

        public IPv6HeaderInformation(IPv6Address source, IPv6Address destionation, IPv6Address listener) : base(source, destionation, listener)
        {
        }

        public static IPv6HeaderInformation AsResponse(IPHeader<IPv6Address> header) =>
            new IPv6HeaderInformation(header.Destionation, header.Source,header.ListenerAddress);

        #endregion
    }
}
