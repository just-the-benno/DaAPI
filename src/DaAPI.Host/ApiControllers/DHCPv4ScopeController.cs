using DaAPI.Core.Packets.DHCPv4;
using DaAPI.Core.Scopes;
using DaAPI.Core.Scopes.DHCPv4;
using DaAPI.Shared.Helper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static DaAPI.Shared.Responses.DHCPv4ScopeResponses.V1;
using static DaAPI.Shared.Requests.DHCPv4ScopeRequests.V1;
using DaAPI.Host.Application.Commands.DHCPv4Scopes;
using DaAPI.Core.Common;

namespace DaAPI.Host.ApiControllers
{
    [Authorize(AuthenticationSchemes = "Bearer", Policy = "DaAPI-API")]
    [ApiController]
    public class DHCPv4ScopeController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IScopeResolverManager<DHCPv4Packet, IPv4Address> _resolverManager;
        private readonly DHCPv4RootScope _rootScope;

        public DHCPv4ScopeController(
            IMediator mediator,
            IScopeResolverManager<DHCPv4Packet, IPv4Address> resolverManager,
            DHCPv4RootScope rootScope)
        {
            this._mediator = mediator;
            this._resolverManager = resolverManager;
            _rootScope = rootScope;
        }

        private void GenerateScopeTree(DHCPv4Scope scope, ICollection<DHCPv4ScopeTreeViewItem> parentChildren)
        {
            List<DHCPv4ScopeTreeViewItem> childItems = new List<DHCPv4ScopeTreeViewItem>();
            var node = new DHCPv4ScopeTreeViewItem
            {
                Id = scope.Id,
                StartAddress = scope.AddressRelatedProperties.Start.ToString(),
                EndAddress = scope.AddressRelatedProperties.End.ToString(),
                Name = scope.Name,
                ChildScopes = childItems,
            };

            parentChildren.Add(node);

            if (scope.GetChildScopes().Any())
            {
                foreach (var item in scope.GetChildScopes())
                {
                    GenerateScopeTree(item, childItems);
                }
            }
        }

        [HttpGet("/api/scopes/dhcpv4/tree")]
        public IActionResult GetScopesAsTreeView()
        {
            List<DHCPv4ScopeTreeViewItem> result = new List<DHCPv4ScopeTreeViewItem>();

            foreach (var item in _rootScope.GetRootScopes())
            {
                GenerateScopeTree(item, result);
            }

            return base.Ok(result);
        }

        private void GenerateScopeList(DHCPv4Scope scope, ICollection<DHCPv4ScopeItem> collection)
        {
            var node = new DHCPv4ScopeItem
            {
                Id = scope.Id,
                StartAddress = scope.AddressRelatedProperties.Start.ToString(),
                EndAddress = scope.AddressRelatedProperties.End.ToString(),
                Name = scope.Name,
            };

            collection.Add(node);

            if (scope.GetChildScopes().Any())
            {
                foreach (var item in scope.GetChildScopes())
                {
                    GenerateScopeList(item, collection);
                }
            }
        }

        [HttpGet("/api/scopes/dhcpv4/list")]
        public IActionResult GetScopesAsList()
        {
            List<DHCPv4ScopeItem> result = new List<DHCPv4ScopeItem>();

            foreach (var item in _rootScope.GetRootScopes())
            {
                GenerateScopeList(item, result);
            }

            return base.Ok(result);
        }

        [HttpGet("/api/scopes/dhcpv4/resolvers/description")]
        public IActionResult GetResolverDescription()
        {
            var descriptions = _resolverManager.GetRegisterResolverDescription();
            return base.Ok(descriptions);
        }

        private DHCPv4ScopePropertyResponse GetScopePropertyResponse(DHCPv4ScopeProperty property)
        {
            return property switch
            {
                DHCPv4AddressListScopeProperty item => new DHCPv4AddressListScopePropertyResponse
                {
                    Addresses = item.Addresses.Select(x => x.ToString()).ToList(),
                    OptionCode = item.OptionIdentifier,
                    Type = item.ValueType,
                },
                DHCPv4AddressScopeProperty item => new DHCPv4AddressScopePropertyResponse
                {
                    Value = item.Address.ToString(),
                    OptionCode = item.OptionIdentifier,
                    Type = item.ValueType,
                },
                DHCPv4BooleanScopeProperty item => new DHCPv4BooleanScopePropertyResponse
                {
                    Value = item.Value,
                    OptionCode = item.OptionIdentifier,
                    Type = item.ValueType,
                },
                DHCPv4NumericValueScopeProperty item => new DHCPv4NumericScopePropertyResponse
                {
                    Value = item.Value,
                    NumericType = item.NumericType,
                    Type = item.ValueType,
                    OptionCode = item.OptionIdentifier,
                },
                DHCPv4TextScopeProperty item => new DHCPv4TextScopePropertyResponse
                {
                    Value = item.Value,
                    OptionCode = item.OptionIdentifier,
                    Type = item.ValueType,
                },
                DHCPv4TimeScopeProperty item => new DHCPv4TimeScopePropertyResponse
                {
                    Value = item.Value,
                    OptionCode = item.OptionIdentifier,
                    Type = item.ValueType,
                },
                _ => throw new NotImplementedException(),
            };
        }

        [HttpGet("/api/scopes/dhcpv4/{id}/properties")]
        public IActionResult GetScopeProperties([FromRoute(Name = "id")] Guid scopeId, [FromQuery] Boolean includeParents = true)
        {
            var scope = _rootScope.GetScopeById(scopeId);
            if (scope == DHCPv4Scope.NotFound)
            {
                return NotFound();
            }

            var addressProperties = scope.AddressRelatedProperties;
            var scopeProperties = scope.Properties ?? DHCPv4ScopeProperties.Empty;
            if (includeParents == true)
            {
                addressProperties = scope.GetAddressProperties();
                scopeProperties = scope.GetScopeProperties();
            }

            var response = new DHCPv4ScopePropertiesResponse
            {
                Name = scope.Name,
                Description = scope.Description,
                ParentId = scope.ParentScope == null ? new Guid?() : scope.ParentScope.Id,
                Resolver = new ScopeResolverResponse
                {
                    Typename = scope.Resolver.GetDescription().TypeName,
                    PropertiesAndValues = scope.Resolver.GetValues(),
                },
                Properties = scopeProperties.Properties.Select(x => GetScopePropertyResponse(x)).ToArray(),
                AddressRelated = new DHCPv4ScopeAddressPropertiesResponse
                {
                    AcceptDecline = addressProperties.AcceptDecline,
                    AddressAllocationStrategy = addressProperties.AddressAllocationStrategy == null ? new DHCPv4ScopeAddressPropertyReqest.AddressAllocationStrategies?() : (DHCPv4ScopeAddressPropertyReqest.AddressAllocationStrategies)addressProperties.AddressAllocationStrategy,
                    End = addressProperties.End.ToString(),
                    ExcludedAddresses = addressProperties.ExcludedAddresses.Select(x => x.ToString()).ToList(),
                    InformsAreAllowd = addressProperties.InformsAreAllowd,
                    ReuseAddressIfPossible = addressProperties.ReuseAddressIfPossible,
                    Start = addressProperties.Start.ToString(),
                    SupportDirectUnicast = addressProperties.SupportDirectUnicast,
                    PreferredLifetime = addressProperties.PreferredLifetime,
                    LeaseTime = addressProperties.LeaseTime,
                    RenewalTime = addressProperties.RenewalTime,
                }
            };

            return base.Ok(response);
        }

        [HttpPost("/api/scopes/dhcpv4/")]
        public async Task<IActionResult> CreateScope([FromBody] CreateOrUpdateDHCPv4ScopeRequest request)
        {
            if (ModelState.IsValid == false)
            {
                return BadRequest(ModelState);
            }

            request.Resolver.PropertiesAndValues = Shared.Helper.DictionaryHelper.NormelizedProperties(request.Resolver.PropertiesAndValues);

            Guid? item = await _mediator.Send(new CreateDHCPv4ScopeCommand(
                request.Name, request.Description, request.ParentId, request.AddressProperties, request.Resolver, request.Properties));

            if (item.HasValue == false)
            {
                return BadRequest("unable to create scope");
            }

            return Ok(item.Value);
        }

        private async Task<IActionResult> ExecuteCommand(IRequest<Boolean> command)
        {
            Boolean result = await _mediator.Send(command);
            if (result == true)
            {
                return base.NoContent();
            }

            return BadRequest("unable to execute service operation");
        }

        [HttpPut("/api/scopes/dhcpv4/{id}")]
        public async Task<IActionResult> UpdateScope([FromBody] CreateOrUpdateDHCPv4ScopeRequest request, [FromRoute(Name = "id")] Guid scopeId)
        {
            request.Resolver.PropertiesAndValues = Shared.Helper.DictionaryHelper.NormelizedProperties(request.Resolver.PropertiesAndValues);

            var command =  new UpdateDHCPv4ScopeCommand(scopeId,
                request.Name, request.Description, request.ParentId, request.AddressProperties, request.Resolver, request.Properties);

            return await ExecuteCommand(command);
        }

        [HttpDelete("/api/scopes/dhcpv4/{id}/")]
        public async Task<IActionResult> DeleteScope([FromRoute(Name = "id")] Guid scopeId, [FromQuery] Boolean includeChildren = false)
        {
            DeleteDHCPv4ScopeCommand command = new DeleteDHCPv4ScopeCommand(scopeId, includeChildren);
            return await ExecuteCommand(command);
        }
    }
}
