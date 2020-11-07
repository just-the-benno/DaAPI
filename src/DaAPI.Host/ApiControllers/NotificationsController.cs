using DaAPI.Host.Application.Commands.Notifications;
using DaAPI.Infrastructure.NotificationEngine;
using DaAPI.Shared.Helper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static DaAPI.Shared.Requests.NotificationPipelineRequests.V1;

namespace DaAPI.Host.ApiControllers
{
    [Authorize(AuthenticationSchemes = "Bearer", Policy = "DaAPI-API")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationEngine _notificationEngine;
        private readonly IMediator _mediator;
        private readonly ILogger<NotificationsController> _logger;

        public NotificationsController(
            INotificationEngine notificationEngine,
            IMediator mediator,
            ILogger<NotificationsController> logger)
        {
            this._notificationEngine = notificationEngine;
            this._mediator = mediator;
            this._logger = logger;
        }

        [HttpGet("/api/notifications/pipelines/")]
        public async Task<IActionResult> GetAllPipelines()
        {
            _logger.LogDebug("GetAllPipelines");

            var result = await _notificationEngine.GetPipelines();
            return base.Ok(result);
        }

        [HttpGet("/api/notifications/pipelines/descriptions")]
        public async Task<IActionResult> GetPiplelineDescriptions()
        {
            var result = await _notificationEngine.GetPiplelineDescriptions();
            return base.Ok(result);
        }

        [HttpPost("/api/notifications/pipelines/")]
        public async Task<IActionResult> CreatePipeline([FromBody] CreateNotifcationPipelineRequest request)
        {
            if (ModelState.IsValid == false)
            {
                return BadRequest(ModelState);
            }

            request.ConditionProperties = DictionaryHelper.NormelizedProperties(request.ConditionProperties);
            request.ActorProperties = DictionaryHelper.NormelizedProperties(request.ActorProperties);

            var command = new CreateNotificationPipelineCommand(request.Name, request.Description,
                request.TriggerName, request.CondtionName, request.ConditionProperties, request.ActorName, request.ActorProperties);

            Guid? id = await _mediator.Send(command);

            if (id.HasValue == false)
            {
                return BadRequest("unable to create pipeline");
            }

            return base.Ok(id.Value);
        }

        [HttpDelete("/api/notifications/pipelines/{id}")]
        public async Task<IActionResult> DeletePipeline([FromRoute(Name ="id")]Guid pipelineId)
        {
            if(ModelState.IsValid == false)
            {
                return BadRequest(ModelState);
            }

            Boolean result = await _mediator.Send(new DeleteNotificationPipelineCommand(pipelineId));
            if(result == false)
            {
                return BadRequest("unable to delete the pipeline");
            }

            return NoContent();
        }
    }
}
