using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static DaAPI.Shared.Responses.StatisticsControllerResponses.V1;
using static DaAPI.Shared.Responses.DHCPv4ScopeResponses.V1;


namespace DaAPI.App.Pages.Dashboard
{
    public class DHCPv4LeaseEntryViewModel : DHCPv4LeaseEntry
    {
        public DHCPv4ScopeItem Scope { get; set; }
    }
}
