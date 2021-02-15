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
    public class DHCPv4Option82Resolver : IScopeResolver<DHCPv4Packet, IPv4Address>
    {
        #region Properties

        public Byte[] Value { get; private set; } = Array.Empty<Byte>();

        #endregion

        #region Methods

        public Boolean HasUniqueIdentifier => true;
        public byte[] GetUniqueIdentifier(DHCPv4Packet packet) => packet.GetOptionByIdentifier(82)?.OptionData ?? Array.Empty<Byte>();

        public Boolean PacketMeetsCondition(DHCPv4Packet packet)
        {
            Byte[] rawData = GetUniqueIdentifier(packet);

            return ByteHelper.AreEqual(rawData, Value);
        }

        public Boolean ArePropertiesAndValuesValid(IDictionary<String, String> valueMapper, ISerializer serializer)
        {
            if (valueMapper.ContainsKey(nameof(Value)) == false) { return false; }

            try
            {
                Byte[] result = serializer.Deserialze<Byte[]>(valueMapper[nameof(Value)]);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void ApplyValues(IDictionary<String, String> valueMapper, ISerializer serializer)
        {
            Value = serializer.Deserialze<Byte[]>(valueMapper[nameof(Value)]);
        }

        public ScopeResolverDescription GetDescription()
        {
            return new ScopeResolverDescription(
                nameof(DHCPv4RelayAgentResolver),
                new List<ScopeResolverPropertyDescription>
                {
                   new ScopeResolverPropertyDescription(nameof(Value),ScopeResolverPropertyDescription.ScopeResolverPropertyValueTypes.ByteArray),
                }
                );
        }

        public IDictionary<String, String> GetValues() => new Dictionary<String, String>
        {
            { nameof(Value), System.Text.Json.JsonSerializer.Serialize(Value) },
        };

        #endregion

    }
}
