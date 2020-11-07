using DaAPI.Core.Common;
using DaAPI.Core.Packets;
using DaAPI.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Scopes
{
    public interface IScopeResolver<TPacket, TAddress> 
        where TPacket: DHCPPacket<TPacket, TAddress> 
        where TAddress : IPAddress<TAddress>
    {
        //Boolean ForceReuseOfAddress { get; }
        Boolean HasUniqueIdentifier { get; }
        Boolean PacketMeetsCondition(TPacket packet);
        Byte[] GetUniqueIdentifier(TPacket packet);
        Boolean ArePropertiesAndValuesValid(IDictionary<String, String> valueMapper, ISerializer serializer);
        void ApplyValues(IDictionary<String, String> valueMapper, ISerializer serializer);
        ScopeResolverDescription GetDescription();
        IDictionary<String, String> GetValues();
    }
}
