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
    public class DHCPv4RelayAgentResolver : IScopeResolver<DHCPv4Packet, IPv4Address>
    {
        #region Properties

        public IEnumerable<IPv4Address> AgentAddresses { get; private set; } = new List<IPv4Address>();

        #endregion

        #region Methods

        public Boolean HasUniqueIdentifier => false;
        public byte[] GetUniqueIdentifier(DHCPv4Packet packet) => throw new NotImplementedException();

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

        public Boolean ArePropertiesAndValuesValid(IDictionary<String, String> valueMapper, ISerializer serializer)
        {
            try
            {
                if (valueMapper.ContainsKey(nameof(AgentAddresses)) == false)
                {
                    return false;
                }

                var addresses = serializer.Deserialze<IEnumerable<String>>(valueMapper[nameof(AgentAddresses)]);
                foreach (var item in addresses)
                {
                    var address = IPv4Address.FromString(item);
                    if (address == IPv4Address.Empty || address == IPv4Address.Broadcast)
                    {
                        return false;
                    }
                }

                return true;
            }
            catch (Exception)
            {

                return false;
            }
        }

        public void ApplyValues(IDictionary<String, String> valueMapper, ISerializer serializer)
        {
            AgentAddresses = serializer.Deserialze<IEnumerable<String>>(valueMapper[nameof(AgentAddresses)])
                .Select(x => IPv4Address.FromString(x)).ToArray();
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

        public IDictionary<String, String> GetValues() => new Dictionary<String, String>
        {
            { nameof(AgentAddresses), System.Text.Json.JsonSerializer.Serialize(AgentAddresses.Select(x => x.ToString())) },
        };

        #endregion

    }
}
