using DaAPI.Core.Scopes.DHCPv4;
using DaAPI.Core.Scopes.DHCPv6;
using DaAPI.Infrastructure.NotificationEngine;
using DaAPI.Infrastructure.StorageEngine.DHCPv6;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using static DaAPI.Shared.Requests.StatisticsControllerRequests.V1;
using static DaAPI.Shared.Responses.StatisticsControllerResponses.V1;

namespace DaAPI.Host.ApiControllers
{
    [Authorize(AuthenticationSchemes = "Bearer", Policy = "DaAPI-API")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IDHCPv6ReadStore _storage;
        private readonly INotificationEngine _notificationEngine;
        private readonly DHCPv6RootScope _rootScope;
        private readonly DHCPv4RootScope _dhcpv4RootScope;

        public DashboardController(
            DHCPv6RootScope rootScope,
            DHCPv4RootScope dhcpv4RootScope,
            IDHCPv6ReadStore storage,
            INotificationEngine notificationEngine)
        {
            _rootScope = rootScope ?? throw new ArgumentNullException(nameof(rootScope));
            _dhcpv4RootScope = dhcpv4RootScope ?? throw new ArgumentNullException(nameof(dhcpv4RootScope));
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _notificationEngine = notificationEngine ?? throw new ArgumentNullException(nameof(notificationEngine));
        }

        [HttpGet("/api/Statistics/Dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            DashboardResponse response = await _storage.GetDashboardOverview();
            response.DHCPv6.ScopeAmount = _rootScope.GetTotalAmountOfScopes();
            response.DHCPv4.ScopeAmount = _dhcpv4RootScope.GetTotalAmountOfScopes();
            response.AmountOfPipelines =  _notificationEngine.GetPipelineAmount();
            return base.Ok(response);
        }
    }
}
