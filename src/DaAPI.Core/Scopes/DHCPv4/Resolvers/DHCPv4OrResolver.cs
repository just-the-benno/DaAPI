using DaAPI.Core.Packets.DHCPv4;
using DaAPI.Core.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Scopes.DHCPv4
{
    public class DHCPv4OrResolver : DHCPv4ResolverWithInnerResolverBase
    {
        #region Fields

        #endregion

        #region Properties

        #endregion

        #region Constructor

        public DHCPv4OrResolver(
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
                if (resolverResult == true)
                {
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}
