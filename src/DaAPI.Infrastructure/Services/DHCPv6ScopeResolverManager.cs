using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Packets.DHCPv6;
using DaAPI.Core.Scopes;
using DaAPI.Core.Scopes.DHCPv6.Resolvers;
using DaAPI.Core.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Infrastructure.Services
{
    public class DHCPv6ScopeResolverManager : ScopeResolverManagerBase<DHCPv6Packet, IPv6Address>
    {
        public DHCPv6ScopeResolverManager(
            ISerializer serializer,
            ILogger<DHCPv6ScopeResolverManager> logger) : base(serializer, logger)
        {
            AddOrUpdateScopeResolver(nameof(DHCPv6AndResolver), () => new DHCPv6AndResolver());
            AddOrUpdateScopeResolver(nameof(DHCPv6OrResolver), () => new DHCPv6OrResolver());
            AddOrUpdateScopeResolver(nameof(DHCPv6RemoteIdentifierEnterpriseNumberResolver), () => new DHCPv6RemoteIdentifierEnterpriseNumberResolver(logger));
            AddOrUpdateScopeResolver(nameof(DHCPv6RelayAgentSubnetResolver), () => new DHCPv6RelayAgentSubnetResolver());
            AddOrUpdateScopeResolver(nameof(DHCPv6RelayAgentResolver), () => new DHCPv6RelayAgentResolver());
            AddOrUpdateScopeResolver(nameof(DHCPv6PseudoResolver), () => new DHCPv6PseudoResolver());
            AddOrUpdateScopeResolver(nameof(DHCPv6MilegateResolver), () => new DHCPv6MilegateResolver());
            AddOrUpdateScopeResolver(nameof(DHCPv6PeerAddressResolver), () => new DHCPv6PeerAddressResolver());
            AddOrUpdateScopeResolver(nameof(DHCPv6ClientDUIDResolver), () => new DHCPv6ClientDUIDResolver());
            AddOrUpdateScopeResolver(nameof(DHCPv6SimpleZyxelIESResolver), () => new DHCPv6SimpleZyxelIESResolver());
        }
    }
}
