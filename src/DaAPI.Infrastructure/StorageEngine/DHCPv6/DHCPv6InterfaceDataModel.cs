using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DaAPI.Infrastructure.StorageEngine.DHCPv6
{
    public class DHCPv6InterfaceDataModel
    {
        [Key]
        public Guid Id { get; set; }

        public String Name { get; set; }
        public String IPv6Address { get; set; }
        public String InterfaceId { get; set; }

        public DHCPv6InterfaceDataModel()
        {

        }

    }
}
