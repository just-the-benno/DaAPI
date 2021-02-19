using DaAPI.Core.Common;
using DaAPI.Core.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaAPI.Infrastructure.FilterEngines
{
    public interface IDHCPPacketFilter<TPacket, TAddress>
          where TPacket : DHCPPacket<TPacket, TAddress>
          where TAddress : IPAddress<TAddress>
    {
        Task<Boolean> ShouldPacketBeFiltered(TPacket packet);

    }
}
