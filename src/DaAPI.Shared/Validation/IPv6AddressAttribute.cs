using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text;

namespace DaAPI.Shared.Validation
{
    public class IPv6AddressAttribute : ValidationAttribute
    {
        public IPv6AddressAttribute() : base("is not a valid ipv6 address")
        {

        }

        public override bool IsValid(object value)
        {
            if(value is String == false) { return false; }

            if (IPAddress.TryParse((String)value, out IPAddress address) == true)
            {
                return address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6;
            }

            return false;
        }
    }
}
