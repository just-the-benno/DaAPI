using DaAPI.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Scopes
{
    public class ScopeResolverDescription : Value<ScopeResolverDescription>
    {
        #region Properties

        public String TypeName { get; private set; }
        public IEnumerable<ScopeResolverPropertyDescription> Properties { get; private set; }

        #endregion

        #region Constructor

        public ScopeResolverDescription(String typename, IEnumerable<ScopeResolverPropertyDescription> properties )
        {
            TypeName = typename;
            Properties = new List<ScopeResolverPropertyDescription>(properties);
        }

        #endregion
    }
}
