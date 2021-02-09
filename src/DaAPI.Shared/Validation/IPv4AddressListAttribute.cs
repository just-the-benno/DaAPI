using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DaAPI.Shared.Validation
{
    public class IPv4AddressListAttribute : IPAddressListAttribute
    {
        public override AddressFamily ValidAddressFamily => AddressFamily.InterNetwork;
    }
}
