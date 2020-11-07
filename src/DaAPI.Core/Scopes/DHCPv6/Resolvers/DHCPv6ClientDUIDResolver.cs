using DaAPI.Core.Common;
using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Packets.DHCPv6;
using DaAPI.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Scopes.DHCPv6.Resolvers
{
    public class DHCPv6ClientDUIDResolver : IScopeResolver<DHCPv6Packet, IPv6Address>
    {
        #region Properties

        public DUID ClientDuid { get; private set; }

        public Boolean HasUniqueIdentifier => true;

        #endregion

        #region Constructor

        public DHCPv6ClientDUIDResolver()
        {

        }

        #endregion

        #region Methods

        public Boolean ArePropertiesAndValuesValid(IDictionary<String, String> valueMapper, ISerializer serializer)
        {
            if (valueMapper.ContainsKey(nameof(ClientDuid)) == false)
            {
                return false;
            }

            try
            {
                String rawByteValue = serializer.Deserialze<String>(valueMapper[nameof(ClientDuid)]);
                Byte[] parsedBytes = ByteHelper.GetBytesFromHexString(rawByteValue);

                var duid = DUIDFactory.GetDUID(parsedBytes);
                return duid.Type != DUID.DUIDTypes.Unknown;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void ApplyValues(IDictionary<String, String> valueMapper, ISerializer serializer)
        {
            String rawByteValue = serializer.Deserialze<String>(valueMapper[nameof(ClientDuid)]);
            Byte[] parsedBytes = ByteHelper.GetBytesFromHexString(rawByteValue);
            ClientDuid = DUIDFactory.GetDUID(parsedBytes);
        }

        public bool PacketMeetsCondition(DHCPv6Packet packet)
        {
            DHCPv6Packet innerPacket = packet.GetInnerPacket();
            DUID clientDuid = innerPacket.GetClientIdentifer();

            return clientDuid == ClientDuid;
        }

        public ScopeResolverDescription GetDescription() => new ScopeResolverDescription(
           nameof(DHCPv6ClientDUIDResolver), new[] {
             new ScopeResolverPropertyDescription(nameof(ClientDuid),ScopeResolverPropertyDescription.ScopeResolverPropertyValueTypes.ByteArray)
           });

        public byte[] GetUniqueIdentifier(DHCPv6Packet packet) => ClientDuid.GetAsByteStream();

        public IDictionary<String, String> GetValues() => new Dictionary<String, String>
        {
            { nameof(ClientDuid), ByteHelper.ToString(ClientDuid.GetAsByteStream(),false) }
        };

        #endregion
    }
}
