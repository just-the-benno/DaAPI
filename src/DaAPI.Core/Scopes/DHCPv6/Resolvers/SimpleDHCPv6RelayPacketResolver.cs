using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Packets.DHCPv6;
using DaAPI.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Scopes.DHCPv6.Resolvers
{
    public abstract class SimpleDHCPv6RelayPacketResolver : IScopeResolver<DHCPv6Packet, IPv6Address>
    {
        public virtual bool HasUniqueIdentifier => false;

        public abstract void ApplyValues(IDictionary<string, string> valueMapper, ISerializer serializer);
        public abstract bool ArePropertiesAndValuesValid(IDictionary<string, string> valueMapper, ISerializer serializer);

        public abstract ScopeResolverDescription GetDescription();

        public virtual byte[] GetUniqueIdentifier(DHCPv6Packet packet) => throw new NotImplementedException();

        protected bool PacketMeetsCondition(DHCPv6Packet packet, Func<DHCPv6RelayPacket, Boolean> matcher)
        {
            if (packet.PacketType != DHCPv6PacketTypes.RELAY_FORW) { return false; }

            var relayPackets = ((DHCPv6RelayPacket)packet).GetRelayPacketChain();

            foreach (var item in relayPackets)
            {
                if (matcher(item) == true)
                {
                    return true;
                }
            }

            return false;
        }

        public abstract bool PacketMeetsCondition(DHCPv6Packet packet);

        public abstract IDictionary<String, String> GetValues();
    }
}
