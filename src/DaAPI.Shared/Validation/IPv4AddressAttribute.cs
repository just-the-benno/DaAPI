using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text;

namespace DaAPI.Shared.Validation
{
    public class IPv4AddressAttribute : ValidationAttribute
    {
        public IPv4AddressAttribute() : base("is not a valid ipv4 address")
        {

        }

        public override bool IsValid(object value)
        {
            if(value is String == false) { return false; }

            if (IPAddress.TryParse((String)value, out IPAddress address) == true)
            {
                return address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork;
            }

            return false;
        }
    }
}
