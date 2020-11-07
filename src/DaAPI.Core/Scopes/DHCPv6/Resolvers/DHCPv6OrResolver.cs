using DaAPI.Core.Packets.DHCPv6;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Scopes.DHCPv6.Resolvers
{
    public class DHCPv6OrResolver : DHCPv6ScopeResolverContainingOtherResolvers
    {

        public DHCPv6OrResolver()
        {

        }

        public override Boolean PacketMeetsCondition(DHCPv6Packet packet)
        {
            foreach (var resolver in InnerResolvers)
            {
                Boolean resolverResult = resolver.PacketMeetsCondition(packet);
                if (resolverResult == true)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
