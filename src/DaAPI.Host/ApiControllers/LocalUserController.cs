using DaAPI.Host.Application.Commands;
using DaAPI.Host.Infrastrucutre;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static DaAPI.Shared.Requests.LocalUserRequests.V1;

namespace DaAPI.Host.ApiControllers
{
    [Authorize(AuthenticationSchemes = "Bearer", Policy = "DaAPI-API")]
    [ApiController]
    public class LocalUserController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILocalUserService _localUserService;
        private readonly ILogger<LocalUserController> _logger;

        public LocalUserController(
            IMediator mediator,
            ILocalUserService localUserService,
            ILogger<LocalUserController> logger
            )
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _localUserService = localUserService;
            _logger = logger;
        }

        [HttpGet("/api/LocalUsers")]
        public async Task<IActionResult> GetAllLocalUsers()
        {
            _logger.LogDebug("GetAllLocalUsers");

            var users = await _localUserService.GetAllUsersSortedByName();
            return base.Ok(users);
        }

        private async Task<IActionResult> ExecuteCommand(IRequest<Boolean> command)
        {
            Boolean result = await _mediator.Send(command);
            if (result == false)
            {
                return BadRequest("unable to complete service operation");
            }

            return NoContent();
        }

        [HttpPut("/api/LocalUsers/ChangePassword/{id}")]
        public async Task<IActionResult> ResetUserPassword(
            [FromRoute(Name = "id")] String userId,[FromBody] ResetPasswordRequest request)
        { 
            if(ModelState.IsValid == false)
            {
                return BadRequest(ModelState);
            }

            return await ExecuteCommand(new ResetLocalUserPasswordCommand(userId, request.Password));
        }

        [HttpDelete("/api/LocalUsers/{id}")]
        public async Task<IActionResult> DeleteUser(
        [FromRoute(Name = "id")] String userId)
        {
            return await ExecuteCommand(new DeleteLocalUserCommand(userId));
        }

        [HttpPost("/api/LocalUsers/")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            if(ModelState.IsValid == false)
            {
                return BadRequest(ModelState);
            }

            String id = await _mediator.Send(new CreateLocalUserCommand(request.Username, request.Password));
            if(String.IsNullOrEmpty(id) == true)
            {
                return BadRequest("unable to complete service operation");
            }

            return base.Ok(id);
        }
    }
}

