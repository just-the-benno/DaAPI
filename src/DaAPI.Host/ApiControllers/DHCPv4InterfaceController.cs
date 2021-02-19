using DaAPI.Host.Application.Commands.DHCPv4Interfaces;
using DaAPI.Infrastructure.InterfaceEngines;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static DaAPI.Shared.Requests.DHCPv4InterfaceRequests.V1;
using static DaAPI.Shared.Responses.DHCPv4InterfaceResponses.V1;

namespace DaAPI.Host.ApiControllers
{
    [Authorize(AuthenticationSchemes = "Bearer", Policy = "DaAPI-API")]
    [ApiController]
    public class DHCPv4InterfaceController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IDHCPv4InterfaceEngine _interfaceEngine;

        public DHCPv4InterfaceController(
            IMediator mediator,
            IDHCPv4InterfaceEngine interfaceEngine)
        {
            this._mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            this._interfaceEngine = interfaceEngine ?? throw new ArgumentNullException(nameof(interfaceEngine));
        }

        [HttpGet("/api/interfaces/dhcpv4/")]
        public async Task<IActionResult> GetAllInterface()
        {
            var possibleInterfaces = _interfaceEngine.GetPossibleListeners();
            var activeInterfaces = await _interfaceEngine.GetActiveListeners();

            List<ActiveDHCPv4InterfaceEntry> activeEntries = new List<ActiveDHCPv4InterfaceEntry>();
            List<DHCPv4InterfaceEntry> unboundEntries = new List<DHCPv4InterfaceEntry>();

            foreach (var item in possibleInterfaces)
            {
                var activeInterface = activeInterfaces.FirstOrDefault(x => x.PhysicalInterfaceId == item.PhysicalInterfaceId && x.Address == item.Address);
                if (activeInterface != null)
                {
                    activeEntries.Add(new ActiveDHCPv4InterfaceEntry
                    {
                        SystemId = activeInterface.Id,
                        IPv4Address = activeInterface.Address.ToString(),
                        MACAddress = item.PhysicalAddress,
                        Name = activeInterface.Name,
                        PhysicalInterfaceName = item.Interfacename,
                        PhysicalInterfaceId = activeInterface.PhysicalInterfaceId
                    });
                }
                else
                {
                    unboundEntries.Add(new DHCPv4InterfaceEntry
                    {
                        IPv4Address = item.Address.ToString(),
                        MACAddress = item.PhysicalAddress,
                        InterfaceName = item.Interfacename,
                        PhysicalInterfaceId = item.PhysicalInterfaceId,
                    });
                }
            }

            var detachedInterfaces = activeInterfaces
                .Where(x => possibleInterfaces.Count(y => y.PhysicalInterfaceId == x.PhysicalInterfaceId) == 0)
                .Select(x => new DetachedDHCPv4InterfaceEntry
                {
                    SystemId = x.Id,
                    IPv4Address = x.Address.ToString(),
                    Name = x.Name
                }).ToList();


            var result = new DHCPv4InterfaceOverview
            {
                ActiveEntries = activeEntries,
                Entries = unboundEntries,
                DetachedEntries = detachedInterfaces
            };

            return base.Ok(result);
        }

        [HttpPost("/api/interfaces/dhcpv4/")]
        public async Task<IActionResult> CreateListener([FromBody] CreateDHCPv4Listener listener)
        {
            if (ModelState.IsValid == false)
            {
                return BadRequest(ModelState);
            }

            var command = new CreateDHCPv4InterfaceListenerCommand(listener.InterfaceId, listener.IPv4Address, listener.Name);
            Guid? systemId = await _mediator.Send(command);

            if (systemId.HasValue == false)
            {
                return BadRequest("unable to create listener");
            }

            return Ok(systemId.Value);
        }

        [HttpDelete("/api/interfaces/dhcpv4/{id}")]
        public async Task<IActionResult> DeleteListener([FromRoute(Name = "id")] Guid systemId)
        {
            var command = new DeleteDHCPv4InterfaceListenerCommand(systemId);
            Boolean result = await _mediator.Send(command);

            if (result == false)
            {
                return BadRequest("unable to delete listener");
            }

            return Ok(true);
        }
    }
}
