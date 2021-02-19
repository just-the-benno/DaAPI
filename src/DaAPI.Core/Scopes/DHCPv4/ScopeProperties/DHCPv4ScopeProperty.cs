using DaAPI.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Scopes.DHCPv4
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

    public abstract class DHCPv4ScopeProperty : ScopeProperty<Byte, DHCPv4ScopePropertyType>
    {
        #region Properties

        #endregion

        #region Constructor

        protected DHCPv4ScopeProperty()
        {

        }

        protected DHCPv4ScopeProperty(Byte optionIdenfier, DHCPv4ScopePropertyType propertyType) : base(optionIdenfier, propertyType)
        {

        }

        #endregion
    }
}
