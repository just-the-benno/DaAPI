using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DaAPI.Shared.Validation
{
    public abstract class IPAddressListAttribute : ValidationAttribute
    {
        public abstract AddressFamily ValidAddressFamily { get;  }

        public override bool IsValid(object value)
        {
            if(value == null) { return true; }
            if(value is IEnumerable<String> == false) { return false; }

            foreach (var item in (IEnumerable<String>)value)
            {
                if (IPAddress.TryParse(item, out IPAddress address) == true)
                {
                    if(address.AddressFamily != ValidAddressFamily)
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
