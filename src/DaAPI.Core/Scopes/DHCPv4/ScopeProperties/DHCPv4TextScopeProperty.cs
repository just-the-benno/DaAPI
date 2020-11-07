using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Scopes.DHCPv4
{
    public class DHCPv4TextScopeProperty : DHCPv4ScopeProperty
    {
        #region Property

        public String Value { get; private set; }

        #endregion

        #region Constructor

        public DHCPv4TextScopeProperty() : base()
        {
            Value = String.Empty;
        }

        public DHCPv4TextScopeProperty(Byte optionIdentifier, String text) : base(optionIdentifier,DHCPv4ScopePropertyType.Text)
        {
            Value = text;
        }

        #endregion
    }
}
