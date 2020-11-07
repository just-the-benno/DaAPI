using DaAPI.Core.Common;
using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Packets.DHCPv6;
using DaAPI.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Scopes.DHCPv6.Resolvers
{
    public class DHCPv6RelayAgentResolver : SimpleDHCPv6RelayPacketResolver
    {
        #region Properties

        public IPv6Address RelayAgentAddress { get; private set; }

        #endregion

        #region Constructor

        public DHCPv6RelayAgentResolver()
        {

        }

        #endregion

        #region Methods

        public override Boolean ArePropertiesAndValuesValid(IDictionary<String, String> valueMapper, ISerializer serializer)
        {
            if (valueMapper.ContainsKey(nameof(RelayAgentAddress)) == false)
            {
                return false;
            }

            try
            {
                IPv6Address address = serializer.Deserialze<IPv6Address>(valueMapper[nameof(RelayAgentAddress)]);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public override void ApplyValues(IDictionary<string, string> valueMapper, ISerializer serializer)
        {
            RelayAgentAddress = serializer.Deserialze<IPv6Address>(valueMapper[nameof(RelayAgentAddress)]);
        }

        public override bool PacketMeetsCondition(DHCPv6Packet packet) =>
            PacketMeetsCondition(packet, (input) => input.LinkAddress == RelayAgentAddress);

        public override ScopeResolverDescription GetDescription() => new ScopeResolverDescription(
           nameof(DHCPv6RelayAgentResolver), new[] {
             new ScopeResolverPropertyDescription(nameof(RelayAgentAddress),ScopeResolverPropertyDescription.ScopeResolverPropertyValueTypes.IPv6Address),
           });

        public override IDictionary<String, String> GetValues() => new Dictionary<String, String>
        {
            { nameof(RelayAgentAddress), RelayAgentAddress.ToString() },
        };

        #endregion
    }
}
