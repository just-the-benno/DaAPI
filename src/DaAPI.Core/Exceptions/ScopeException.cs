using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Exceptions
{
    public enum DHCPv4ScopeExceptionReasons
    {
        NoInput = 1,
        ScopeParentNotFound = 2,
        IdExists = 3,
        NotInParentRange = 4,
        AddressPropertiesInvalidForParents = 5,
        InvalidTimeRanges = 6,
        InvalidResolver = 7,
        InvalidPacket = 8,
        ScopeNotFound = 9,
        ParentCanBeAddedAsChild = 10,
    }

    [Serializable]
    public class ScopeException : System.Exception
    {
        #region Properties

        public String Details { get; set; } = String.Empty;
        public DHCPv4ScopeExceptionReasons Reason { get; set; }

        #endregion

        #region Constructor

        public ScopeException()
        {
        }

        public ScopeException(DHCPv4ScopeExceptionReasons reason) : base($"unable to complete an operation. Reason {reason}")
        {
            Reason = reason;
        }

        public ScopeException(DHCPv4ScopeExceptionReasons reason, String details) : base($"unable to complete an operation. Reason {reason}. Details {details}")
        {
            Details = details;
            Reason = reason;
        }

        public ScopeException(DHCPv4ScopeExceptionReasons reason, Exception inner) : base($"unable to complete an operation. Reason {reason}", inner)
        {
            Reason = reason;
        }

        protected ScopeException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
        }

        #endregion
    }
}
