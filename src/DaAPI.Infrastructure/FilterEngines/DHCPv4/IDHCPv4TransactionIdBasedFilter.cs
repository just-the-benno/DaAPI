using DaAPI.Core.Packets;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DaAPI.Infrastructure.FilterEngines.DHCPv4
{
    public interface IDHCPv4TransactionIdBasedFilter
    {
        Task<Boolean> FilterByTranscationId(UInt32 transactionId);
    }
}
