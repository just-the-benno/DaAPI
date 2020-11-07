using DaAPI.Core.Packets.DHCPv4;
using DaAPI.Core.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Scopes.DHCPv4
{
    public class DHCPv4AndResolver : DHCPv4ResolverWithInnerResolverBase
    {
        #region Fields


        #endregion

        #region Properties

        #endregion

        #region Constructor

        public DHCPv4AndResolver(
            ISerializer serializer
            ) : base(serializer)
        {
        }


        #endregion

        #region Methods

        public override Boolean PacketMeetsCondition(DHCPv4Packet packet)
        {
            foreach (IDHCPv4ScopeResolver resolver in InnerResolvers)
            {
                Boolean resolverResult = resolver.PacketMeetsCondition(packet);
                if (resolverResult == false)
                {
                    return false;
                }
            }

            return true;
        }

        #endregion
    }
}
