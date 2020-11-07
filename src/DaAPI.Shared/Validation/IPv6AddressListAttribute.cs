using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text;

namespace DaAPI.Shared.Validation
{
    public class IPv6AddressListAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if(value == null) { return true; }
            if(value is IEnumerable<String> == false) { return false; }

            foreach (var item in (IEnumerable<String>)value)
            {
                if (IPAddress.TryParse(item, out IPAddress address) == true)
                {
                    if(address.AddressFamily != System.Net.Sockets.AddressFamily.InterNetworkV6)
                    {
                        return false;
                    }
                }

                return true;

            }

            return true;

        }
    }
}
