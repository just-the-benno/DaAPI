using DaAPI.Infrastructure.AggregateStore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace DaAPI.Infrastructure.FilterEngines.DHCPv4
{
    public class DHCPv4TransactionIdBasedFilter : IDHCPv4TransactionIdBasedFilter
    {
        #region Fields

        private readonly ILogger<DHCPv4TransactionIdBasedFilter> _logger;
        private readonly IDHCPv4AggregateStore _aggregateStore;

        #endregion

        #region Properties

        #endregion

        #region Constructor

        public DHCPv4TransactionIdBasedFilter(
            IDHCPv4AggregateStore aggregateStore,
            ILogger<DHCPv4TransactionIdBasedFilter> logger
            )
        {
            _aggregateStore = aggregateStore ?? throw new ArgumentNullException(nameof(aggregateStore));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        #endregion

        #region Methods

        public async Task<Boolean> FilterByTranscationId(UInt32 transactionId)
        {
            _logger.LogTrace("FilterByTranscationId. {transactionId}:", transactionId);


            Boolean exists = await _aggregateStore.CheckIfActiveTransactionExists(transactionId);
            if (exists == false)
            {
                _logger.LogWarning("transactionId: {transactionId} not found", transactionId);
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion
    }
}
