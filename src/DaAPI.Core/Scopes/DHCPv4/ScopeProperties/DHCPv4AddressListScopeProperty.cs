using DaAPI.Core.Common;
using DaAPI.Core.Packets.DHCPv4;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Scopes.DHCPv4
{
    public class DHCPv4AddressListScopeProperty : DHCPv4ScopeProperty
    {
        #region Properties

        public IEnumerable<IPv4Address> Addresses { get; private set; }

        #endregion

        #region Constructor

        public DHCPv4AddressListScopeProperty(Byte optionIdentifier, IEnumerable<IPv4Address> addresses) : base(
            optionIdentifier,DHCPv4ScopePropertyType.AddressList)
        {
            Addresses = new List<IPv4Address>(addresses);
        }

        #endregion
    }
}
