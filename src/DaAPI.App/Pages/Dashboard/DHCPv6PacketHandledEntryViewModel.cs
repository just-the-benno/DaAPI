using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static DaAPI.Shared.Responses.DHCPv6ScopeResponses.V1;
using static DaAPI.Shared.Responses.StatisticsControllerResponses.V1;

namespace DaAPI.App.Pages.Dashboard
{
    public class DHCPv6PacketHandledEntryViewModel : DHCPv6PacketHandledEntry
    {
        public ScopeItem Scope { get; set; }
    }
}
