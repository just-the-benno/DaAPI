using DaAPI.Core.Packets.DHCPv6;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Scopes.DHCPv6.Resolvers
{
    public class DHCPv6AndResolver : DHCPv6ScopeResolverContainingOtherResolvers
    {

        public DHCPv6AndResolver()
        {

        }

        public override Boolean PacketMeetsCondition(DHCPv6Packet packet)
        {
            foreach (var resolver in InnerResolvers)
            {
                Boolean resolverResult = resolver.PacketMeetsCondition(packet);
                if (resolverResult == false)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
