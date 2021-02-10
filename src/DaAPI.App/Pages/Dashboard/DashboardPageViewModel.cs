using DaAPI.App.Pages.DHCPv6Scopes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static DaAPI.Shared.Responses.StatisticsControllerResponses.V1;
using static DaAPI.Shared.Responses.DHCPv6ScopeResponses.V1;
using System.Security.Cryptography.X509Certificates;

namespace DaAPI.App.Pages.Dashboard
{
    public class DashboardViewModelResponse
    {
        public DHCPOverview<DHCPv6LeaseEntryViewModel, DHCPv6PacketHandledEntryViewModel> DHCPv6 { get; set; }
        public Int32 AmountOfPipelines { get; set; }
    }

    public class DashboardPageViewModel
    {
        public DashboardViewModelResponse Response { get; set; }
        public IDictionary<Guid, DHCPv6ScopeItem> Scopes { get; set; }

        private DHCPv6ScopeItem GetDefaultScope() => GetDefaultScope(Guid.Empty);
        private DHCPv6ScopeItem GetDefaultScope(Guid id) => new DHCPv6ScopeItem { Id = id, Name = String.Empty };

        public void SetScopes(IEnumerable<DHCPv6ScopeItem> scopes)
        {
            Scopes = scopes.ToDictionary(x => x.Id, x => x);

            foreach (var item in Response.DHCPv6.ActiveLeases)
            {
                item.Scope = GetScopeById(item.ScopeId);
            }

            foreach (var item in Response.DHCPv6.Packets)
            {
                item.Scope = item.ScopeId.HasValue == false ? GetDefaultScope():  GetScopeById(item.ScopeId.Value);
            }
        }

        public DHCPv6ScopeItem GetScopeById(Guid id) => Scopes.ContainsKey(id) == false ? GetDefaultScope(id) : Scopes[id];

    }
}
