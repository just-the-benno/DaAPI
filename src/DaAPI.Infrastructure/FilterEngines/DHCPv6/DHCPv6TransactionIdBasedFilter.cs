//using DaAPI.Core.Database;
//using Microsoft.Extensions.Logging;
//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.Threading.Tasks;

//namespace DaAPI.Core.FilterEngines.DHCPv6
//{
//    public class DHCPv6TransactionIdBasedFilter : IDHCPv6TransactionIdBasedFilter
//    {
//        #region Fields

//        private readonly ILogger<DHCPv6TransactionIdBasedFilter> _logger;
//        private readonly IDaAPIContext _context;

//        #endregion

//        #region Properties

//        #endregion

//        #region Constructor

//        public DHCPv6TransactionIdBasedFilter(
//            IDaAPIContext context,
//            ILogger<DHCPv6TransactionIdBasedFilter> logger
//            )
//        {
//            this._context = context ?? throw new ArgumentNullException(nameof(context));
//            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
//        }

//        #endregion

//        #region Methods

//        public async Task<Boolean> FilterByTranscationId(UInt32 transactionId)
//        {
//            _logger.LogTrace("FilterByTranscationId. {transactionId}:", transactionId);

//            Boolean result = await _context.CheckIfDHCPv6TransactionIdExists(transactionId);
//            if(result == false)
//            {
//                _logger.LogWarning("transactionId: {transactionId} not found", transactionId);
//                return true;
//            }
//            else
//            {
//                return false;
//            }
//        }

//        #endregion
//    }
//}
