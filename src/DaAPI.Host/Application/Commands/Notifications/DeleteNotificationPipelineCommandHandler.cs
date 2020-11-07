using DaAPI.Infrastructure.NotificationEngine;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DaAPI.Host.Application.Commands.Notifications
{
    public class DeleteNotificationPipelineCommandHandler : IRequestHandler<DeleteNotificationPipelineCommand,Boolean>
    {
        private readonly INotificationEngine _engine;
        private readonly ILogger<DeleteNotificationPipelineCommandHandler> _logger;

        public DeleteNotificationPipelineCommandHandler(
            INotificationEngine engine,
            ILogger<DeleteNotificationPipelineCommandHandler> logger)
        {
            this._engine = engine ?? throw new ArgumentNullException(nameof(engine));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Boolean> Handle(DeleteNotificationPipelineCommand request, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Handle started");

            Boolean result = await _engine.DeletePipeline(request.PipelineId);
            return result;
        }
    }
}
