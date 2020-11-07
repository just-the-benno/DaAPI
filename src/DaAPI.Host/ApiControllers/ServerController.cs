using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DaAPI.Host.Application.Commands;
using DaAPI.Host.Infrastrucutre;
using DaAPI.Infrastructure.StorageEngine.DHCPv6;
using DaAPI.Shared.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static DaAPI.Shared.Responses.ServerControllerResponses;
using static DaAPI.Shared.Responses.ServerControllerResponses.V1;

namespace DaAPI.Host.ApiControllers
{
    [Authorize(AuthenticationSchemes = "Bearer", Policy = "DaAPI-API")]
    [ApiController]
    public class ServerController : ControllerBase
    {
        private readonly IMediator mediator;
        private readonly OpenIdConnectOptions _openIdConnectOptions;
        private readonly IDHCPv6ReadStore _storage;

        public ServerController(
            IMediator mediator,
            OpenIdConnectOptions openIdConnectOptions,
            IDHCPv6ReadStore storage)
        {
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _openIdConnectOptions = openIdConnectOptions ?? throw new ArgumentNullException(nameof(openIdConnectOptions));
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }


        [AllowAnonymous]
        [HttpGet("/api/Server/IsInitialized")]
        public async Task<IActionResult> CheckIfServerIsInitialized()
        {
            var serverProperties = await _storage.GetServerProperties();
            Boolean isNotInitilized = serverProperties == null || serverProperties.IsInitilized == false;

            return base.Ok(new ServerInitilizedResponse(!isNotInitilized, _openIdConnectOptions.IsSelfHost));
        }

        [AllowAnonymous]
        [HttpPost("/api/Server/Initialize")]
        public async Task<IActionResult> InitializeServer([FromBody] InitilizeServeRequest request)
        {
            if (ModelState.IsValid == false)
            {
                return BadRequest(ModelState);
            }

            var command = new InitilizeServerCommand(request.UserName, request.Password);
            Boolean result = await mediator.Send(command);
            if (result == false)
            {
                return BadRequest("unable to complete the servicec operation");
            }

            return NoContent();
        }

        [HttpGet("/api/Server/IsInitialized2")]
        public async Task<IActionResult> CheckIfServerIsInitialized2()
        {
            var serverProperties = await _storage.GetServerProperties();
            Boolean isNotInitilized = serverProperties == null || serverProperties.IsInitilized == false;

            return base.Ok(new ServerInitilizedResponse(!isNotInitilized, _openIdConnectOptions.IsSelfHost));
        }
    }
}
