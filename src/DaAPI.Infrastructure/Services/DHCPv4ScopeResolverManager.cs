using DaAPI.Core.Common;
using DaAPI.Core.Packets.DHCPv4;
using DaAPI.Core.Scopes;
using DaAPI.Core.Scopes.DHCPv4;
using DaAPI.Core.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Infrastructure.Services
{
    public class DHCPv4ScopeResolverManager : ScopeResolverManagerBase<DHCPv4Packet, IPv4Address>
    {
        public DHCPv4ScopeResolverManager(
            ISerializer serializer,
            ILogger<DHCPv4ScopeResolverManager> logger) : base(serializer, logger)
        {
            //AddOrUpdateScopeResolver(nameof(DHCPv6AndResolver), () => new DHCPv6AndResolver());
        }
    }
}
