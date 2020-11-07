using DaAPI.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Scopes
{
    public class CreateScopeResolverInformation : IDataTransferObject
    {
        #region Properties

        public String Typename { get; set; }
        public IDictionary<String, String> PropertiesAndValues { get; set; }

        #endregion

        #region Constructor

        public CreateScopeResolverInformation()
        {
            PropertiesAndValues = new Dictionary<String, String>();
        }

        #endregion
    }
}
