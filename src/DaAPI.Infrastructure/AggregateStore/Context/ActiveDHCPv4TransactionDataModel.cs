using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DaAPI.Infrastructure.AggregateStore.Context
{
    [Table("ActiveDHCPv4Transactions")]
    public class ActiveDHCPv4TransactionDataModel
    {
        [Key]
        public Guid Id { get; set; }

        public UInt32 TransactionIdentifier { get; set; }


        public ActiveDHCPv4TransactionDataModel()
        {

        }

    }
}
