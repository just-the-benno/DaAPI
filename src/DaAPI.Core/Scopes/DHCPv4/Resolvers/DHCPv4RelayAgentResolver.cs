using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DaAPI.Core.Common;
using DaAPI.Core.Packets.DHCPv4;
using DaAPI.Core.Services;
using Microsoft.Extensions.Logging;

namespace DaAPI.Core.Scopes.DHCPv4
{
    public class DHCPv4RelayAgentResolver : IDHCPv4ScopeResolver
    {
        #region Fields

        private readonly ILogger<DHCPv4RelayAgentResolver> _logger;
        private readonly ISerializer _serializer;

        #endregion

        #region Properties

        public IEnumerable<IPv4Address> AgentAddresses { get; private set; }

        public Boolean ForceReuseOfAddress => false;
        public Boolean HasUniqueIdentifier => false;

        #endregion

        #region Constructor

        public DHCPv4RelayAgentResolver(
            ILogger<DHCPv4RelayAgentResolver> logger,
            ISerializer serializer
            )
        {
            AgentAddresses = new List<IPv4Address>();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        }

        #endregion

        #region Methods

        public Boolean PacketMeetsCondition(DHCPv4Packet packet)
        {
            foreach (IPv4Address address in AgentAddresses)
            {
                if (packet.GatewayIPAdress == address)
                {
                    return true;
                }
            }

            return false;
        }

        public byte[] GetUniqueIdentifier(DHCPv4Packet packet)
        {
            throw new NotImplementedException();
        }

        private IEnumerable<IPv4Address> GetAddresses(IDictionary<string, string> propertiesAndValues)
        {
            String rawAddress = propertiesAndValues[nameof(AgentAddresses)];
            IEnumerable<IPv4Address> addresses = _serializer.Deserialze<IEnumerable<IPv4Address>>(rawAddress);
            return addresses;
        }

        public bool ArePropertiesAndValuesValid(IDictionary<string, string> propertiesAndValues)
        {
            if(propertiesAndValues == null)
            {
                return false;
            }

            IEnumerable<String> keys = new List<String> { nameof(AgentAddresses) };

            Boolean keyresult = propertiesAndValues.ContainsKeys(keys);
            if (keyresult == false) { return false; }

            try
            {
                var addresses = GetAddresses(propertiesAndValues);
                if(addresses.Any() == false)
                {
                    return false;
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void ApplyValues(IDictionary<String, String> propertiesAndValues)
        {
            IEnumerable<IPv4Address> addresses = GetAddresses(propertiesAndValues);
            this.AgentAddresses = new List<IPv4Address>(addresses);
        }

        public ScopeResolverDescription GetDescription()
        {
            return new ScopeResolverDescription(
                nameof(DHCPv4RelayAgentResolver),
                new List<ScopeResolverPropertyDescription>
                {
                   new ScopeResolverPropertyDescription(nameof(AgentAddresses),ScopeResolverPropertyDescription.ScopeResolverPropertyValueTypes.IPv4AddressList),
                }
                );
        }

        #endregion

    }
}
