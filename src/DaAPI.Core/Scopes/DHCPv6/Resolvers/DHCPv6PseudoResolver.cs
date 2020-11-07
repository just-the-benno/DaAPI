using DaAPI.Core.Common;
using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Packets.DHCPv6;
using DaAPI.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Scopes.DHCPv6.Resolvers
{
    public class DHCPv6PseudoResolver : IPseudoResolver, IScopeResolver<DHCPv6Packet, IPv6Address>
    {
        #region Constructor

        public DHCPv6PseudoResolver()
        {

        }

        #endregion

        #region Methods

        public Boolean HasUniqueIdentifier => false;
        public Byte[] GetUniqueIdentifier(DHCPv6Packet packet) => throw new NotImplementedException();
        public Boolean ArePropertiesAndValuesValid(IDictionary<String, String> valueMapper, ISerializer serializer) => true;

        public void ApplyValues(IDictionary<string, string> valueMapper, ISerializer serializer)
        {
        }

        public bool PacketMeetsCondition(DHCPv6Packet packet) => true;

        public ScopeResolverDescription GetDescription() => new ScopeResolverDescription(
           nameof(DHCPv6PseudoResolver), Array.Empty<ScopeResolverPropertyDescription>());

        public IDictionary<String, String> GetValues() => new Dictionary<String, String>();

        #endregion
    }
}
