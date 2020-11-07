using DaAPI.Core.Common;
using DaAPI.Core.Packets;
using DaAPI.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Scopes
{
    public interface IScopeResolverContainingOtherResolvers<TPacket,TAddress> : IScopeResolver<TPacket,TAddress>
        where TPacket : DHCPPacket<TPacket, TAddress>
        where TAddress : IPAddress<TAddress>
    {
        Boolean AddResolver(IScopeResolver<TPacket, TAddress> resolver);
        IEnumerable<CreateScopeResolverInformation> ExtractResolverCreateModels(CreateScopeResolverInformation item, ISerializer serializer);
    }
}
