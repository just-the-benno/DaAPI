using DaAPI.Core.Scopes;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Scopes.DHCPv6.ScopeProperties
{
    public enum DHCPv6ScopePropertyType
    {
        AddressList  = 1,
        Byte = 2,
        UInt16 = 3,
        UInt32 = 4,
        Text = 5
    }

    public abstract class DHCPv6ScopeProperty : ScopeProperty<UInt16, DHCPv6ScopePropertyType>
    {
        protected DHCPv6ScopeProperty()
        {

        }

        protected DHCPv6ScopeProperty(UInt16 optionIdenfier, DHCPv6ScopePropertyType propertyType) :  base(optionIdenfier,propertyType)
        {
                
        }
    }
}
