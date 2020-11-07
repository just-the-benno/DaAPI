using DaAPI.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Packets.DHCPv4
{
    public class IPv4HeaderInformation
    {
        #region Properties

        public IPv4Address Source { get; private set; }
        public IPv4Address Destionation { get; private set; }

        #endregion

        #region Constructor and factories

        private IPv4HeaderInformation()
        {

        }

        public IPv4HeaderInformation(IPv4Address source, IPv4Address destionation)
        {
            Source = source;
            Destionation = destionation;
        }

        public static IPv4HeaderInformation Default => new IPv4HeaderInformation()
        {
            Source = IPv4Address.Empty,
            Destionation = IPv4Address.Empty,
        };

        #endregion
    }
}
