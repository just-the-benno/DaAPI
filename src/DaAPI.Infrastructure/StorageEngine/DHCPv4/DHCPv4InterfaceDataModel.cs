using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DaAPI.Infrastructure.StorageEngine.DHCPv4
{
    public class DHCPv4InterfaceDataModel
    {
        [Key]
        public Guid Id { get; set; }

        public String Name { get; set; }
        public String IPv4Address { get; set; }
        public String InterfaceId { get; set; }

        public DHCPv4InterfaceDataModel()
        {

        }

    }
}
