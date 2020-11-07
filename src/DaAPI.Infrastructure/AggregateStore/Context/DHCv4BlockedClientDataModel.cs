using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DaAPI.Infrastructure.AggregateStore.Context
{
    [Table("DHCPv4BlockedClients")]
    public class DHCPv4BlockedClientDataModel
    {
        [Key]
        public Guid Id { get; set; }

        public String HWAddress { get; set; }

        public String Identifier { get; set; }

        public DHCPv4BlockedClientDataModel()
        {

        }

    }
}
