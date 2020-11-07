using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Packets.DHCPv6;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Scopes.DHCPv6.Resolvers
{
    public abstract class DHCPv6ScopeResolverContainingOtherResolvers 
        : ScopeResolverContainingOtherResolvers<DHCPv6Packet,IPv6Address>
    {
    }
}
