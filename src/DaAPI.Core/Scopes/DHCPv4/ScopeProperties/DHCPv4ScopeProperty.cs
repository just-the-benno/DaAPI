using DaAPI.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Scopes.DHCPv4
{
    public abstract class DHCPv4ScopeProperty : Value
    {
        public enum DHCPv4ScopePropertyType
        {
            Address = 1,
            Subnet = 2,
            AddressList = 3,
            Time = 5,
            TimeOffset = 6,
            Byte = 4,
            Boolean = 7,
            UInt16 = 8,
            UInt32 = 9,
            AddressAndMask = 10,
            Text = 11,
            ByteArray = 12
        }

        #region Properties

        public Byte OptionIdentifier { get; private set; }
        public DHCPv4ScopePropertyType ValueType { get; private set; }

        #endregion

        #region Constructor

        protected DHCPv4ScopeProperty()
        {

        }

        public DHCPv4ScopeProperty(Byte optionIdentifier, DHCPv4ScopePropertyType type)
        {
            OptionIdentifier = optionIdentifier;
            ValueType = type;
        }

        #endregion
    }
}
