using DaAPI.Core.Common;
using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Packets.DHCPv6;
using DaAPI.Core.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using static DaAPI.Core.Scopes.ScopeResolverPropertyDescription;

namespace DaAPI.Core.Scopes.DHCPv6.Resolvers
{
    public class DHCPv6RemoteIdentifierEnterpriseNumberResolver : IScopeResolver<DHCPv6Packet, IPv6Address>
    {
        #region Fields

        private readonly ILogger _logger;

        #endregion

        #region Properties

        public Boolean HasUniqueIdentifier => false;
        public UInt32 EnterpriseNumber { get; private set; }
        public Int32? RelayAgentIndex { get; private set; }

        #endregion

        #region Constructor

        public DHCPv6RemoteIdentifierEnterpriseNumberResolver(ILogger logger)
        {
            this._logger = logger;
        }


        #endregion


        public void ApplyValues(IDictionary<string, string> valueMapper, ISerializer serializer)
        {
            EnterpriseNumber = serializer.Deserialze<UInt32>(valueMapper[nameof(EnterpriseNumber)]);
            RelayAgentIndex = serializer.Deserialze<Int32?>(valueMapper[nameof(RelayAgentIndex)]);
        }

        public Boolean ArePropertiesAndValuesValid(IDictionary<String, String> valueMapper, ISerializer serializer)
        {
            if (valueMapper == null)
            {
                return false;
            }

            List<String> neededProperties = new List<string> { nameof(EnterpriseNumber), nameof(RelayAgentIndex) };
            if (valueMapper.ContainsKeys(neededProperties) == false) { return false; }

            try
            {
                serializer.Deserialze<UInt32>(valueMapper[nameof(EnterpriseNumber)]);
                serializer.Deserialze<Int32?>(valueMapper[nameof(RelayAgentIndex)]);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public ScopeResolverDescription GetDescription() => new ScopeResolverDescription(
                 nameof(DHCPv6RemoteIdentifierEnterpriseNumberResolver),
                 new List<ScopeResolverPropertyDescription>
                {
                   new ScopeResolverPropertyDescription (nameof(EnterpriseNumber),  ScopeResolverPropertyValueTypes.UInt32 ),
                   new ScopeResolverPropertyDescription (nameof(RelayAgentIndex),  ScopeResolverPropertyValueTypes.NullableUInt32 ),

                 }
                );

        public byte[] GetUniqueIdentifier(DHCPv6Packet packet) => throw new InvalidOperationException();

        public bool PacketMeetsCondition(DHCPv6Packet packet)
        {
            if (packet.PacketType != DHCPv6PacketTypes.RELAY_FORW) { return false; }

            var relayPackets = ((DHCPv6RelayPacket)packet).GetRelayPacketChain();

            if (RelayAgentIndex.HasValue == true)
            {
                if (RelayAgentIndex >= relayPackets.Count) { return false; }

                DHCPv6RelayPacket relayPacket = relayPackets[RelayAgentIndex.Value];
                var remoteIdOption = relayPacket.GetOption<DHCPv6PacketRemoteIdentifierOption>(DHCPv6PacketOptionTypes.RemoteIdentifier);
                if (remoteIdOption == null) { return false; }

                return remoteIdOption.EnterpriseNumber == EnterpriseNumber;
            }
            else
            {
                foreach (var item in relayPackets)
                {
                    var remoteIdOption = item.GetOption<DHCPv6PacketRemoteIdentifierOption>(DHCPv6PacketOptionTypes.RemoteIdentifier);
                    if (remoteIdOption == null) { continue; }

                    if (remoteIdOption.EnterpriseNumber == EnterpriseNumber)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public IDictionary<string, string> GetValues() => new Dictionary<String, String>
        {
            { nameof(EnterpriseNumber), EnterpriseNumber.ToString() },
            { nameof(RelayAgentIndex), RelayAgentIndex.ToString() },
        };
    }
}
