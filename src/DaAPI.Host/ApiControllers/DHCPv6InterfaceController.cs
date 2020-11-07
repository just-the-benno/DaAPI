using DaAPI.Host.Application.Commands.DHCPv6Interfaces;
using DaAPI.Infrastructure.InterfaceEngines;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using static DaAPI.Shared.Requests.DHCPv6InterfaceRequests.V1;
using static DaAPI.Shared.Responses.DHCPv6InterfaceResponses.V1;

namespace DaAPI.Host.ApiControllers
{
    [Authorize(AuthenticationSchemes = "Bearer", Policy = "DaAPI-API")]
    [ApiController]
    public class DHCPv6InterfaceController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IDHCPv6InterfaceEngine _interfaceEngine;

        public DHCPv6InterfaceController(
            IMediator mediator,
            IDHCPv6InterfaceEngine interfaceEngine)
        {
            this._mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            this._interfaceEngine = interfaceEngine ?? throw new ArgumentNullException(nameof(interfaceEngine));
        }

        [HttpGet("/api/interfaces/dhcpv6/")]
        public async Task<IActionResult> GetAllInterface()
        {
            var possibleInterfaces = _interfaceEngine.GetPossibleListeners();
            var activeInterfaces = await _interfaceEngine.GetActiveListeners();

            List<ActiveDHCPv6InterfaceEntry> activeEntries = new List<ActiveDHCPv6InterfaceEntry>();
            List<DHCPv6InterfaceEntry> unboundEntries = new List<DHCPv6InterfaceEntry>();

            foreach (var item in possibleInterfaces)
            {
                var activeInterface = activeInterfaces.FirstOrDefault(x => x.PhysicalInterfaceId == item.PhysicalInterfaceId && x.Address == item.Address);
                if (activeInterface != null)
                {
                    activeEntries.Add(new ActiveDHCPv6InterfaceEntry
                    {
                        SystemId = activeInterface.Id,
                        IPv6Address = activeInterface.Address.ToString(),
                        MACAddress = item.PhysicalAddress,
                        Name = activeInterface.Name,
                        PhysicalInterfaceName = item.Interfacename,
                        PhysicalInterfaceId = activeInterface.PhysicalInterfaceId
                    });
                }
                else
                {
                    unboundEntries.Add(new DHCPv6InterfaceEntry
                    {
                        IPv6Address = item.Address.ToString(),
                        MACAddress = item.PhysicalAddress,
                        InterfaceName = item.Interfacename,
                        PhysicalInterfaceId = item.PhysicalInterfaceId,
                    });
                }
            }

            var detachedInterfaces = activeInterfaces
                .Where(x => possibleInterfaces.Count(y => y.PhysicalInterfaceId == x.PhysicalInterfaceId) == 0)
                .Select(x => new DetachedDHCPv6InterfaceEntry
                {
                    SystemId = x.Id,
                    IPv6Address = x.Address.ToString(),
                    Name = x.Name
                }).ToList();


            var result = new DHCPv6InterfaceOverview
            {
                ActiveEntries = activeEntries,
                Entries = unboundEntries,
                DetachedEntries = detachedInterfaces
            };

            return base.Ok(result);
        }

        [HttpPost("/api/interfaces/dhcpv6/")]
        public async Task<IActionResult> CreateListener([FromBody] CreateDHCPv6Listener listener)
        {
            if (ModelState.IsValid == false)
            {
                return BadRequest(ModelState);
            }

            var command = new CreateDHCPv6InterfaceListenerCommand(listener.InterfaceId, listener.IPv6Address, listener.Name);
            Guid? systemId = await _mediator.Send(command);

            if (systemId.HasValue == false)
            {
                return BadRequest("unable to create listener");
            }

            return Ok(systemId.Value);
        }

        [HttpDelete("/api/interfaces/dhcpv6/{id}")]
        public async Task<IActionResult> DeleteListener([FromRoute(Name = "id")] Guid systemId)
        {
            var command = new DeleteDHCPv6InterfaceListenerCommand(systemId);
            Boolean result = await _mediator.Send(command);

            if (result == false)
            {
                return BadRequest("unable to delete listener");
            }

            return Ok(true);
        }
    }
}
