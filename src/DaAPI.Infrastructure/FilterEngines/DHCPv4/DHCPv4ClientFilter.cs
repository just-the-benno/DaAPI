using DaAPI.Core.Common;
using DaAPI.Infrastructure.AggregateStore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DaAPI.Infrastructure.FilterEngines.DHCPv4
{
    public class DHCPv4ClientFilter : IDHCPv4ClientFilter
    {
        #region Fields

        private readonly IDHCPv4AggregateStore _aggregateStore;
        private readonly ILogger<DHCPv4ClientFilter> _logger;

        #endregion

        #region Properties

        #endregion

        #region Constructor

        public DHCPv4ClientFilter(
            IDHCPv4AggregateStore aggregateStore,
            ILogger<DHCPv4ClientFilter> logger
            )
        {
            this._aggregateStore = aggregateStore ?? throw new ArgumentNullException(nameof(aggregateStore));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Methods

        public async Task<Boolean> FilterClientByClientIdentifier(byte[] identifierRawVaue)
        {
            _logger.LogTrace("FilterClientByClientIdentifier. {identifier}:", identifierRawVaue);

            DHCPv4ClientIdentifier clientIdentifier = DHCPv4ClientIdentifier.FromOptionData(identifierRawVaue);

            Boolean exists = await _aggregateStore.CheckIfBlockedClientExists(clientIdentifier);
            if (exists == true)
            {
                _logger.LogInformation("client identifier {identifier} should be filtered", clientIdentifier);
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<Boolean> FilterClientByHardwareAddress(Byte[] hardwareAddress)
        {
            _logger.LogTrace("FilterClientByHardwareAddress. {hwAddress}:", hardwareAddress);

            var clientIdentifier = DHCPv4ClientIdentifier.FromHwAddress(hardwareAddress);
            Boolean exits = await _aggregateStore.CheckIfBlockedClientExists(clientIdentifier);
          
            if (exits == true)
            {
                _logger.LogInformation("client with {hwAddress} should be filtered", hardwareAddress);
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
