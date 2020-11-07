using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Scopes.DHCPv6.ScopeProperties
{
    public class DHCPv6TextScopeProperty : DHCPv6ScopeProperty
    {
        #region Property

        public String Value { get; private set; }

        #endregion

        #region Constructor

        public DHCPv6TextScopeProperty() : base()
        {
            Value = String.Empty;
        }

        public DHCPv6TextScopeProperty(UInt16 optionIdentifier, String text) : base(optionIdentifier, DHCPv6ScopePropertyType.Text)
        {
            Value = text;
        }

        #endregion
    }
}
