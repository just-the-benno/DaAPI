using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Exceptions
{
    public enum DHCPv4ClientExceptionReasons
    {
        TransactonNotFound = 1,
        UnknowPacketType = 2,
        UnexpectedPacketType = 3,
    }

    [Serializable]
    public class DHCPv4ClientException : System.Exception
    {
        #region Properties

        public String Details { get; set; } = String.Empty;
        public DHCPv4ClientExceptionReasons Reason { get; set; }

        #endregion

        #region Constructor

        public DHCPv4ClientException()
        {
        }

        public DHCPv4ClientException(DHCPv4ClientExceptionReasons reason) : base($"unable to complete an operation. Reason {reason}")
        {
            Reason = reason;
        }

        public DHCPv4ClientException(DHCPv4ClientExceptionReasons reason, String details) : base($"unable to complete an operation. Reason {reason}. Details {details}")
        {
            Details = details;
            Reason = reason;
        }

        public DHCPv4ClientException(DHCPv4ClientExceptionReasons reason, Exception inner) : base($"unable to complete an operation. Reason {reason}", inner)
        {
            Reason = reason;
        }

        protected DHCPv4ClientException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
        }

        #endregion
    }
}
