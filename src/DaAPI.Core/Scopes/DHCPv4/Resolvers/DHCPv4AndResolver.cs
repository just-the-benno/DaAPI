using DaAPI.Core.Packets.DHCPv4;
using DaAPI.Core.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Scopes.DHCPv4
{
    public class DHCPv4AndResolver : DHCPv4ScopeResolverContainingOtherResolvers
    {
        public override Boolean PacketMeetsCondition(DHCPv4Packet packet)
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
