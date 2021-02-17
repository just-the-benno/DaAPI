using DaAPI.Core.Scopes.DHCPv4;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static DaAPI.Shared.Responses.DHCPv4LeasesResponses.V1;
using static DaAPI.Shared.Responses.DHCPv6LeasesResponses.V1;

namespace DaAPI.Host.ApiControllers
{
    [Authorize(AuthenticationSchemes = "Bearer", Policy = "DaAPI-API")]
    [ApiController]
    public class DHCPv4LeaseController : ControllerBase
    {
        private readonly DHCPv4RootScope _rootScope;
        private readonly ILogger<DHCPv4LeaseController> _logger;

        public DHCPv4LeaseController(DHCPv4RootScope rootScope, ILogger<DHCPv4LeaseController> logger)
        {
            this._rootScope = rootScope;
            this._logger = logger;
        }

        private DHCPv4LeaseOverview GetLeaseOverview(DHCPv4Lease lease, DHCPv4Scope scope) => new DHCPv4LeaseOverview
        {
            Address = lease.Address.ToString(),
            MacAddress = lease.Identifier.HwAddress,
            Started = lease.Start,
            ExpectedEnd = lease.End,
            Id = lease.Id,
            UniqueIdentifier = lease.UniqueIdentifier,
            State = lease.State,
            Scope = new DHCPv4ScopeOverview
            {
                Id = scope.Id,
                Name = scope.Name,
            }
        };

        private void GetAllLesesRecursivly(ICollection<DHCPv4LeaseOverview> collection, DHCPv4Scope scope)
        {
            var items = scope.Leases.GetAllLeases().Select(x => GetLeaseOverview(x, scope)).ToList();
            foreach (var item in items)
            {
                collection.Add(item);
            }

            var children = scope.GetChildScopes();
            if (children.Any() == false)
            {
                return;
            }

            foreach (var item in children)
            {
                GetAllLesesRecursivly(collection, item);
            }
        }

        [HttpGet("/api/leases/dhcpv4/scopes/{id}")]
        public IActionResult GetLeasesByScope([FromRoute(Name = "id")] Guid scopeId, [FromQuery(Name = "includeChildren")] Boolean includeChildren = false)
        {
            _logger.LogDebug("GetLeasesByScope");

            var scope = _rootScope.GetScopeById(scopeId);
            if (scope == DHCPv4Scope.NotFound)
            {
                return NotFound($"no scope with id {scopeId} found");
            }
            List<DHCPv4LeaseOverview> result;
            if (includeChildren == false)
            {
                result = scope.Leases.GetAllLeases().Select(x => GetLeaseOverview(x, scope)).ToList();
            }
            else
            {
                result = new List<DHCPv4LeaseOverview>();
                GetAllLesesRecursivly(result, scope);
            }

            return base.Ok(result.OrderBy(x => x.State).ThenBy(x => x.Address).ToList());
        }
    }
}
