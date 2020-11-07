using DaAPI.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Scopes
{
    public abstract class ScopeProperty<TOption,TValueTypes> : Value
    {
        public TOption OptionIdentifier { get; private set; }
        public TValueTypes ValueType { get; private set; }

        protected ScopeProperty()
        {

        }

        protected ScopeProperty(TOption optionIdentifier, TValueTypes type)
        {
            OptionIdentifier = optionIdentifier;
            ValueType = type;
        }
    }
}
