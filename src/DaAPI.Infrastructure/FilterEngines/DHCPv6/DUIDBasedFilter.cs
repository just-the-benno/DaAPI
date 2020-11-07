//using DaAPI.Core.Database;
//using DaAPI.Core.Models;
//using Microsoft.Extensions.Logging;
//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.Threading.Tasks;

//namespace DaAPI.Core.FilterEngines.DHCPv6
//{
//    public class DUIDBasedFilter : IDUIDBasedFilter
//    {
//        #region Fields

//        private readonly ILogger<DUIDBasedFilter> _logger;
//        private readonly IDaAPIContext _context;

//        #endregion

//        #region Properties

//        #endregion

//        #region Constructor

//        public DUIDBasedFilter(
//            ILogger<DUIDBasedFilter> logger,
//            IDaAPIContext context
//            )
//        {
//            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
//            this._context = context ?? throw new ArgumentNullException(nameof(context));
//        }

//        #endregion

//        #region Methods

//        public async Task<Boolean> FilterClientDuid(DUIDModel duid)
//        {
//            _logger.LogTrace("FilterClientDuid. duid: {duid}",duid);

//            Boolean result = await _context.IsDuidBlocked(duid);
//            _logger.LogTrace("duid filter result: {result}", result);
//            if (result == true)
//            {
//                _logger.LogWarning("duid blocked. duid: {duid}", duid);
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
