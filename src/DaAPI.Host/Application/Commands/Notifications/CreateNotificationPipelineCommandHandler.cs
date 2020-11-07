using DaAPI.Core.Notifications;
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
    public class CreateNotificationPipelineCommandHandler : IRequestHandler<CreateNotificationPipelineCommand, Guid?>
    {
        private readonly INotificationEngine _engine;
        private readonly INotificationConditionFactory _conditionFactory;
        private readonly INotificationActorFactory _actorFactory;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<CreateNotificationPipelineCommandHandler> _logger;

        public CreateNotificationPipelineCommandHandler(
            INotificationEngine engine,
            INotificationConditionFactory conditionFactory,
            INotificationActorFactory actorFactory,
            ILoggerFactory loggerFactory,
            ILogger<CreateNotificationPipelineCommandHandler> logger)
        {
            this._engine = engine ?? throw new ArgumentNullException(nameof(engine));
            this._conditionFactory = conditionFactory;
            this._actorFactory = actorFactory;
            this._loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Guid?> Handle(CreateNotificationPipelineCommand request, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Handle started");

            var pipeline = NotificationPipeline.Create(
                NotificationPipelineName.FromString(request.Name),
               String.IsNullOrEmpty(request.Description) == true ? NotificationPipelineDescription.Empty : NotificationPipelineDescription.FromString(request.Description),
                request.TriggerName,
                 _conditionFactory.Initilize(new NotificationConditionCreateModel
                 {
                     Typename = request.CondtionName,
                     PropertiesAndValues = request.ConditionProperties,
                 }),
                _actorFactory.Initilize(new NotificationActorCreateModel
                {
                    Typename = request.ActorName,
                    PropertiesAndValues = request.ActorProperties,
                }),
                _loggerFactory.CreateLogger<NotificationPipeline>(),
                _conditionFactory, _actorFactory
                );

            Boolean storeResult = await _engine.AddNotificationPipeline(pipeline);
            if (storeResult == false)
            {
                return null;
            }

            return pipeline.Id;
        }
    }
}
