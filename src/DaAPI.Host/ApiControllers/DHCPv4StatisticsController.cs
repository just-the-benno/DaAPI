using DaAPI.Core.Scopes.DHCPv4;
using DaAPI.Infrastructure.NotificationEngine;
using DaAPI.Infrastructure.StorageEngine.DHCPv4;
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
    public class DHCPv4StatisticsController : ControllerBase
    {
        private readonly IDHCPv4ReadStore _storage;
        private readonly INotificationEngine _notificationEngine;
        private readonly DHCPv4RootScope _rootScope;

        public DHCPv4StatisticsController(
            DHCPv4RootScope rootScope,
            IDHCPv4ReadStore storage,
            INotificationEngine notificationEngine)
        {
            _rootScope = rootScope ?? throw new ArgumentNullException(nameof(rootScope));
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _notificationEngine = notificationEngine ?? throw new ArgumentNullException(nameof(notificationEngine));
        }

        [HttpGet("/api/Statistics/HandledDHCPv4Packet/{id}")]
        public async Task<IActionResult> GetHandledDHCPv4PacketByScopeId([FromRoute(Name = "id")]Guid scopeId,[Range(1,1000)][FromQuery]Int32 amount = 100)
        {
            if(_rootScope.GetScopeById(scopeId) == DHCPv4Scope.NotFound)
            {
                return NotFound("scope not found");
            }

            var entries = await _storage.GetHandledDHCPv4PacketByScopeId(scopeId,amount);
            return base.Ok(entries);
        }

        [HttpGet("/api/Statistics/IncomingDHCPv4PacketTypes")]
        public async Task<IActionResult> GetIncomingDHCPv4PacketTypes([FromQuery] GroupedTimeSeriesFilterRequest request)
        {
            var response = await _storage.GetIncomingDHCPv4PacketTypes(request.Start, request.End, request.GroupbBy);
            return base.Ok(response);
        }

        [HttpGet("/api/Statistics/FileredDHCPv4Packets")]
        public async Task<IActionResult> GetFileredDHCPv4Packets([FromQuery] GroupedTimeSeriesFilterRequest request)
        {
            var response = await _storage.GetFileredDHCPv4Packets(request.Start, request.End, request.GroupbBy);
            return base.Ok(response);
        }

        [HttpGet("/api/Statistics/ErrorDHCPv6Packets")]
        public async Task<IActionResult> GetErrorDHCPv4Packets([FromQuery] GroupedTimeSeriesFilterRequest request)
        {
            var response = await _storage.GetErrorDHCPv4Packets(request.Start, request.End, request.GroupbBy);
            return base.Ok(response);
        }

        [HttpGet("/api/Statistics/IncomingDHCPv6Packets")]
        public async Task<IActionResult> GetIncomingDHCPv4PacketAmount([FromQuery] GroupedTimeSeriesFilterRequest request)
        {
            var response = await _storage.GetIncomingDHCPv4PacketAmount(request.Start, request.End, request.GroupbBy);
            return base.Ok(response);
        }

        [HttpGet("/api/Statistics/ActiveDHCPv4Leases")]
        public async Task<IActionResult> GetActiveDHCPv4Leases([FromQuery] GroupedTimeSeriesFilterRequest request)
        {
            var response = await _storage.GetActiveDHCPv4Leases(request.Start, request.End, request.GroupbBy);
            return base.Ok(response);
        }

        [HttpGet("/api/Statistics/ErrorCodesPerDHCPV4RequestType")]
        public async Task<IActionResult> GetErrorCodesPerDHCPV4RequestType([FromQuery] DHCPv4PacketTypeBasedTimeSeriesFilterRequest request)
        {
            var response = await _storage.GetErrorCodesPerDHCPv4RequestType(request.Start, request.End, request.PacketType);
            return base.Ok(response);
        }
    }
}
