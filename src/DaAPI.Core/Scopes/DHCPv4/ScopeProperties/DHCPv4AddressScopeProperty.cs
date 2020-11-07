using DaAPI.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Scopes.DHCPv4
{
    public class DHCPv4AddressScopeProperty : DHCPv4ScopeProperty
    {
        #region Properties

        public IPv4Address Address { get; private set; }

        #endregion

        #region Constructor

        private DHCPv4AddressScopeProperty() : base()
        {
            Address = IPv4Address.Empty;
        }

        public DHCPv4AddressScopeProperty(Byte optionIdentifier, IPv4Address value) : 
            base(optionIdentifier,DHCPv4ScopePropertyType.Address)
        {
            Address = value;
        }

        public DHCPv4AddressScopeProperty(Byte optionIdentifier, String address) : this(optionIdentifier, IPv4Address.FromString(address))
        {
        }

        #endregion
    }
}
