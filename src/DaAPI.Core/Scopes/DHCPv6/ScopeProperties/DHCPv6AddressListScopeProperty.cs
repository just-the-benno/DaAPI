using DaAPI.Core.Common.DHCPv6;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Scopes.DHCPv6.ScopeProperties
{
    public class DHCPv6AddressListScopeProperty : DHCPv6ScopeProperty
    {
        #region Properties

        public IEnumerable<IPv6Address> Addresses { get; private set; }

        #endregion

        #region Constructor

        public DHCPv6AddressListScopeProperty(UInt16 optionIdentifier, IEnumerable<IPv6Address> addresses) : base(
            optionIdentifier, DHCPv6ScopePropertyType.AddressList)
        {
            Addresses = new List<IPv6Address>(addresses);
        }

        #endregion
    }
}
