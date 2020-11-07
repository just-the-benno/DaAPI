using DaAPI.Core.Common;
using DaAPI.Core.Packets.DHCPv4;
using DaAPI.Core.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using static DaAPI.Core.Scopes.ScopeResolverPropertyDescription;

namespace DaAPI.Core.Scopes.DHCPv4
{
    public class DHCPv4RelayAgentSubnetResolver : IDHCPv4ScopeResolver
    {
        #region Fields

        private readonly ILogger<DHCPv4RelayAgentSubnetResolver> _logger;
        private readonly ISerializer _serializer;

        #endregion

        #region Properties

        public IPv4Address NetworkAddress { get; private set; }
        public IPv4SubnetMask Mask { get; private set; }

        public Boolean ForceReuseOfAddress => false;
        public Boolean HasUniqueIdentifier => false;

        #endregion

        #region Constructor

        public DHCPv4RelayAgentSubnetResolver(
            ILogger<DHCPv4RelayAgentSubnetResolver> logger,
             ISerializer serializer
            )
        {
            this._serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            NetworkAddress = IPv4Address.Empty;
            Mask =  IPv4SubnetMask.AllZero;
        }

        #endregion

        #region Methods

        public Boolean PacketMeetsCondition(DHCPv4Packet packet)
        {
            Byte[] target = ByteHelper.AndArray(Mask.GetBytes(), NetworkAddress.GetBytes());
            Byte[] actual = ByteHelper.AndArray(Mask.GetBytes(), packet.GatewayIPAdress.GetBytes());

            Boolean result = ByteHelper.AreEqual(target, actual);

            return result;
        }

        public byte[] GetUniqueIdentifier(DHCPv4Packet packet)
        {
            throw new NotImplementedException();
        }

        public void ApplyValues(IDictionary<String, String> valueMapper)
        {
            IPv4Address network = _serializer.Deserialze<IPv4Address>(valueMapper[nameof(NetworkAddress)]);
            IPv4SubnetMask mask = _serializer.Deserialze<IPv4SubnetMask>(valueMapper[nameof(Mask)]);

            this.NetworkAddress = network;
            this.Mask = mask;
        }

        public Boolean ArePropertiesAndValuesValid(IDictionary<String, String> valueMapper)
        {
            if(valueMapper == null)
            {
                return false;
            }

            List<String> neededProperties = new List<string> { nameof(Mask), nameof(NetworkAddress) };
            if(valueMapper.ContainsKeys(neededProperties) == false) { return false; }

            try
            {
                IPv4Address address = _serializer.Deserialze<IPv4Address>(valueMapper[nameof(NetworkAddress)]);
                IPv4SubnetMask mask = _serializer.Deserialze<IPv4SubnetMask>(valueMapper[nameof(Mask)]);

                Boolean addressIsNetwork = mask.IsIPAdressANetworkAddress(address);
                
                if (addressIsNetwork == false)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public ScopeResolverDescription GetDescription()
        {
            return new ScopeResolverDescription(
                 nameof(DHCPv4RelayAgentSubnetResolver),
                 new List<ScopeResolverPropertyDescription>
                {
                   new ScopeResolverPropertyDescription (nameof(NetworkAddress),  ScopeResolverPropertyValueTypes.IPv4Address ),
                   new ScopeResolverPropertyDescription ( nameof(Mask),  ScopeResolverPropertyValueTypes.IPv4Subnetmask ),
                }
                );
        }

        #endregion
    }
}
