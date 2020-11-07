using DaAPI.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Scopes.DHCPv4
{
    public class DHCPv4CreateScopeResolverInformation : IDataTransferObject
    {
        #region Properties

        public String Typename { get; set; }
        public IDictionary<String, String> PropertiesAndValues { get; set; }

        #endregion

        #region Constructor

        public DHCPv4CreateScopeResolverInformation()
        {
            PropertiesAndValues = new Dictionary<String, String>();
        }

        #endregion
    }
}
