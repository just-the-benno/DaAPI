using DaAPI.Shared.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DaAPI.Shared.Requests
{
    public static class DHCPv6InterfaceRequests
    {
        public static class V1
        {
            public class CreateDHCPv6Listener
            {
                [Required]
                [StringLength(100, MinimumLength = 3)]
                public String Name { get; set; }

                [Required]
                [StringLength(100, MinimumLength = 3)]
                [IPv6Address]
                public String IPv6Address { get; set; }

                [Required]
                [StringLength(100, MinimumLength = 3)]
                public String InterfaceId { get; set; }
            }
        }
    }
}
