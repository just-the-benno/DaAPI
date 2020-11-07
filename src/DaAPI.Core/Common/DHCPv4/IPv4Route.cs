using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Common
{
    public class IPv4Route : Value
    {
        #region Properties

        public IPv4Address Network { get; private set; }
        public IPv4SubnetMask SubnetMask { get; private set; }

        #endregion

        #region Constructor and factories

        public IPv4Route(IPv4Address network, IPv4SubnetMask subnetMask)
        {
            Byte[] networkBytes = network.GetBytes();

            Byte[] and = ByteHelper.AndArray(subnetMask.GetBytes(), networkBytes);
            Boolean result = ByteHelper.AreEqual(networkBytes, and);
            
            if(result == false)
            {
                throw new ArgumentException();
            }

            Network = network;
            SubnetMask = subnetMask;
        }

        #endregion
    }
}
