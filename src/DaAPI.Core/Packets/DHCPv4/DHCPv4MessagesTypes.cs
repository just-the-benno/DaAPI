using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaAPI.Core.Packets.DHCPv4
{
    public enum DHCPv4MessagesTypes
    {
        Unkown = 0,
        Discover = 1,
        Offer = 2,
        Request = 3,
        Decline = 4,
        Acknowledge = 5,
        NotAcknowledge = 6,
        Release = 7,
        Inform = 8,
    }
}
