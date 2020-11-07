using DaAPI.Core.Common;
using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Packets.DHCPv6;
using DaAPI.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace DaAPI.Core.Scopes.DHCPv6.Resolvers
{
    public class DHCPv6PeerAddressResolver : SimpleDHCPv6RelayPacketResolver
    {
        #region Properties

        public IPv6Address PeerAddress { get; private set; }
        public Boolean IsUnique { get; private set; }

        #endregion

        #region Constructor

        public DHCPv6PeerAddressResolver()
        {

        }

        #endregion

        #region Methods

        public override Boolean HasUniqueIdentifier => IsUnique;
        public override Byte[] GetUniqueIdentifier(DHCPv6Packet packet) => PeerAddress.GetBytes();

        public override Boolean ArePropertiesAndValuesValid(IDictionary<String, String> valueMapper, ISerializer serializer)
        {
            if (valueMapper.ContainsKeys(new[] { nameof(PeerAddress), nameof(IsUnique) }) == false)
            {
                return false;
            }

            try
            {
                IPv6Address address = serializer.Deserialze<IPv6Address>(valueMapper[nameof(PeerAddress)]);
                String rawValue = serializer.Deserialze<String>(valueMapper[nameof(IsUnique)]);
                Boolean.Parse(rawValue);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public override void ApplyValues(IDictionary<string, string> valueMapper, ISerializer serializer)
        {
            PeerAddress = serializer.Deserialze<IPv6Address>(valueMapper[nameof(PeerAddress)]);
            IsUnique = serializer.Deserialze<Boolean>(valueMapper[nameof(IsUnique)]);
        }

        public override bool PacketMeetsCondition(DHCPv6Packet packet) =>
            PacketMeetsCondition(packet, (input) => input.PeerAddress == PeerAddress);

        public override ScopeResolverDescription GetDescription() => new ScopeResolverDescription(
           nameof(DHCPv6PeerAddressResolver), new[] {
             new ScopeResolverPropertyDescription(nameof(IsUnique),ScopeResolverPropertyDescription.ScopeResolverPropertyValueTypes.Boolean),
             new ScopeResolverPropertyDescription(nameof(PeerAddress),ScopeResolverPropertyDescription.ScopeResolverPropertyValueTypes.IPv6Address),
           });

        public override IDictionary<String, String> GetValues() => new Dictionary<String, String>
        {
            { nameof(IsUnique), IsUnique.ToString().ToLower() },
            { nameof(PeerAddress), PeerAddress.ToString() },
        };

        #endregion
    }
}
