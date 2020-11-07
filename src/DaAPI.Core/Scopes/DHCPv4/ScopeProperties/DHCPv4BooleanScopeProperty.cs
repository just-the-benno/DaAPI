using DaAPI.Core.Packets.DHCPv4;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Scopes.DHCPv4
{
    public class DHCPv4BooleanScopeProperty : DHCPv4ScopeProperty
    {
        #region Property

        public Boolean Value { get; private set; }

        #endregion

        #region Constructor

        public DHCPv4BooleanScopeProperty(Byte optionIdentifier, Boolean value) : 
            base(optionIdentifier, DHCPv4ScopePropertyType.Boolean)
        {
            Value = value;
        }

        #endregion
    }
}
