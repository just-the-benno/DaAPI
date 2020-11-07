using DaAPI.Core.Packets.DHCPv4;
using DaAPI.Core.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Scopes.DHCPv4
{
    public class DHCPv4ExclusiveOrResolver : DHCPv4ResolverWithInnerResolverBase
    {
        #region Fields

        #endregion

        #region Properties

        #endregion

        #region Constructor

        public DHCPv4ExclusiveOrResolver(
            ISerializer serializer
            ) : base(serializer)
        {
        }

        #endregion

        #region Methods

        public override Boolean PacketMeetsCondition(DHCPv4Packet packet)
        {
            Boolean positiveResultFound = false;
            foreach (IDHCPv4ScopeResolver resolver in InnerResolvers)
            {
                Boolean resolverResult = resolver.PacketMeetsCondition(packet);
                if (resolverResult == true)
                {
                    if(positiveResultFound == true)
                    {
                        return false;
                    }
                    else
                    {
                        positiveResultFound = true;
                    }
                }
            }

            return positiveResultFound;
        }

        #endregion
    }
}
