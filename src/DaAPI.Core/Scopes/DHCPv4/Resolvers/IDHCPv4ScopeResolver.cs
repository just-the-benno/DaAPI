using DaAPI.Core.Packets.DHCPv4;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Scopes.DHCPv4
{
    public interface IDHCPv4ScopeResolver
    {
        Boolean ForceReuseOfAddress { get; }
        Boolean HasUniqueIdentifier { get; }
        Boolean PacketMeetsCondition(DHCPv4Packet packet);
        Byte[] GetUniqueIdentifier(DHCPv4Packet packet);
        Boolean ArePropertiesAndValuesValid(IDictionary<String, String> propertiesAndValues);
        void ApplyValues(IDictionary<String, String> propertiesAndValues);
        ScopeResolverDescription GetDescription();
    }
}
