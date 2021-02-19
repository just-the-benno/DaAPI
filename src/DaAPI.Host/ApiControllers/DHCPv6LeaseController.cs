using DaAPI.Core.Scopes.DHCPv6;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static DaAPI.Shared.Responses.DHCPv6LeasesResponses.V1;

namespace DaAPI.Host.ApiControllers
{
    [Authorize(AuthenticationSchemes = "Bearer", Policy = "DaAPI-API")]
    [ApiController]
    public class DHCPv6LeaseController : ControllerBase
    {
        private readonly DHCPv6RootScope _rootScope;
        private readonly ILogger<DHCPv6LeaseController> _logger;

        public DHCPv6LeaseController(DHCPv6RootScope rootScope, ILogger<DHCPv6LeaseController> logger)
        {
            this._rootScope = rootScope;
            this._logger = logger;
        }

        private DHCPv6LeaseOverview GetLeaseOverview(DHCPv6Lease lease, DHCPv6Scope scope) => new DHCPv6LeaseOverview
        {
            Address = lease.Address.ToString(),
            ClientIdentifier = lease.ClientDUID,
            Started = lease.Start,
            ExpectedEnd = lease.End,
            Id = lease.Id,
            Prefix = lease.PrefixDelegation != DHCPv6PrefixDelegation.None ? new PrefixOverview { Address = lease.PrefixDelegation.NetworkAddress.ToString(), Mask = lease.PrefixDelegation.Mask.Identifier } : null,
            UniqueIdentifier = lease.UniqueIdentifier,
            State = lease.State,
            Scope = new ScopeOverview
            {
                Id = scope.Id,
                Name = scope.Name,
            }
        };

        private void GetAllLesesRecursivly(ICollection<DHCPv6LeaseOverview> collection, DHCPv6Scope scope)
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

        [HttpGet("/api/leases/dhcpv6/scopes/{id}")]
        public IActionResult GetLeasesByScope([FromRoute(Name = "id")] Guid scopeId, [FromQuery(Name = "includeChildren")] Boolean includeChildren = false)
        {
            _logger.LogDebug("GetLeasesByScope");

            var scope = _rootScope.GetScopeById(scopeId);
            if (scope == DHCPv6Scope.NotFound)
            {
                return NotFound($"no scope with id {scopeId} found");
            }
            List<DHCPv6LeaseOverview> result;
            if (includeChildren == false)
            {
                result = scope.Leases.GetAllLeases().Select(x => GetLeaseOverview(x, scope)).ToList();
            }
            else
            {
                result = new List<DHCPv6LeaseOverview>();
                GetAllLesesRecursivly(result, scope);
            }

            return base.Ok(result.OrderBy(x => x.State).ThenBy(x => x.Address).ToList());
        }
    }
}
