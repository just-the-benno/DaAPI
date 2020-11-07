using DaAPI.Core.Common;
using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Packets.DHCPv6;
using DaAPI.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Scopes.DHCPv6.Resolvers
{
    public class DHCPv6RelayAgentSubnetResolver : IScopeResolver<DHCPv6Packet, IPv6Address>
    {
        #region Properties

        public IPv6Address NetworkAddress { get; private set; }
        public IPv6SubnetMask SubnetMask { get; private set; }

        #endregion

        #region Constructor

        public DHCPv6RelayAgentSubnetResolver()
        {

        }

        #endregion

        #region Methods

        public Boolean HasUniqueIdentifier => false;
        public Byte[] GetUniqueIdentifier(DHCPv6Packet packet) => throw new NotImplementedException();

        public Boolean ArePropertiesAndValuesValid(IDictionary<String, String> valueMapper, ISerializer serializer)
        {
            if (valueMapper.ContainsKeys(new[] { nameof(NetworkAddress), nameof(SubnetMask) }) == false)
            {
                return false;
            }

            try
            {
                IPv6Address networkAddress = serializer.Deserialze<IPv6Address>(valueMapper[nameof(NetworkAddress)]);
                IPv6SubnetMask mask = serializer.Deserialze<IPv6SubnetMask>(valueMapper[nameof(SubnetMask)]);

                return mask.IsIPv6AdressANetworkAddress(networkAddress);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void ApplyValues(IDictionary<string, string> valueMapper, ISerializer serializer)
        {
            NetworkAddress = serializer.Deserialze<IPv6Address>(valueMapper[nameof(NetworkAddress)]);
            SubnetMask = serializer.Deserialze<IPv6SubnetMask>(valueMapper[nameof(SubnetMask)]);
        }

        public bool PacketMeetsCondition(DHCPv6Packet packet)
        {
            if (packet.PacketType != DHCPv6PacketTypes.RELAY_FORW) { return false; }

            var relayPackets = ((DHCPv6RelayPacket)packet).GetRelayPacketChain();

            foreach (var item in relayPackets)
            {
                if(SubnetMask.IsAddressInSubnet(NetworkAddress,item.LinkAddress) == true)
                {
                    return true;
                }
            }

            return false;
        }

        public ScopeResolverDescription GetDescription() => new ScopeResolverDescription(
           nameof(DHCPv6RelayAgentSubnetResolver), new[] {
             new ScopeResolverPropertyDescription(nameof(NetworkAddress),ScopeResolverPropertyDescription.ScopeResolverPropertyValueTypes.IPv6NetworkAddress),
             new ScopeResolverPropertyDescription(nameof(SubnetMask), ScopeResolverPropertyDescription.ScopeResolverPropertyValueTypes.IPv6Subnet),
           });

        public IDictionary<string, string> GetValues() => new Dictionary<String, String>
        {
            { nameof(NetworkAddress), NetworkAddress.ToString() },
            { nameof(SubnetMask), SubnetMask.ToString() },
        };

        #endregion
    }
}
