using DaAPI.Core.Clients;
using DaAPI.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DaAPI.Infrastructure.AggregateStore
{
    public interface IDHCPv4AggregateStore
    {
        Task<Boolean> CheckIfBlockedClientExists(DHCPv4ClientIdentifier identifier);
        Task<Boolean> CheckIfActiveTransactionExists(UInt32 transactionId);
        Task<DHCPv4Client> GetClientByClientIdentifier(DHCPv4ClientIdentifier clientIdentifier);
        Task Save(AggregateRootWithEvents root);
    }
}
