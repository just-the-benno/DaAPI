using DaAPI.Core.Common;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static DaAPI.Core.Notifications.NotificationsEvent.V1;

namespace DaAPI.Core.Notifications
{
    public enum NotifactionPipelineExecutionResults
    {
        TriggerNotMatch = 1,
        ConditionNotMatched = 2,
        ActorFailed = 3,
        Success = 5,
    }

    public class NotificationPipeline : AggregateRootWithEvents
    {
        private readonly ILogger<NotificationPipeline> _logger;
        private readonly INotificationConditionFactory _conditionFactory;
        private readonly INotificationActorFactory _actorFactory;

        #region Properties

        public NotificationPipelineName Name { get; private set; }
        public NotificationPipelineDescription Description { get; private set; }
        public String TriggerIdentifier { get; private set; }
        public NotificationCondition Condition { get; private set; }
        public NotificationActor Actor { get; private set; }

        #endregion

        #region Constructor and factories

        public NotificationPipeline(
            INotificationConditionFactory conditionFactory, INotificationActorFactory actorFactory, ILogger<NotificationPipeline> logger
            ) : base(Guid.Empty)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _conditionFactory = conditionFactory;
            _actorFactory = actorFactory;
        }

        public bool CanExecute(NotifcationTrigger trigger) => trigger.GetTypeIdentifier() == TriggerIdentifier;
      
        public static NotificationPipeline Create(NotificationPipelineName name, NotificationPipelineDescription description,
            String triggerTypeIdentifier, NotificationCondition conndition, NotificationActor actor,
            ILogger<NotificationPipeline> logger, INotificationConditionFactory conditionFactory, INotificationActorFactory actorFactory)
        {
            if (conndition == null)
            {
                conndition = NotificationCondition.True;
            }

            var pipeline = new NotificationPipeline(conditionFactory, actorFactory, logger);
            pipeline.Apply(new NotificationPipelineCreatedEvent
            {
                Id = Guid.NewGuid(),
                Name = name,
                Description = description,
                TriggerIdentifier = triggerTypeIdentifier,
                ConditionCreateInfo = conndition.ToCreateModel(),
                ActorCreateInfo = actor.ToCreateModel(),
            }); ;

            return pipeline;
        }

        #endregion

        #region Methods

        public async Task<NotifactionPipelineExecutionResults> Execute(NotifcationTrigger trigger)
        {
            _logger.LogDebug("start of {name} pipeline", Name);

            if (TriggerIdentifier != trigger.GetTypeIdentifier())
            {
                _logger.LogDebug("type mismatch. Expected a trigger of {type} but received {otherType}", TriggerIdentifier, trigger.GetTypeIdentifier());
                return NotifactionPipelineExecutionResults.TriggerNotMatch;
            }

            if (Condition != NotificationCondition.True)
            {
                if (await Condition.IsValid(trigger) == false)
                {
                    _logger.LogDebug("the trigger doens't satisfy the condition. Execution of pipeline stopped");
                    return NotifactionPipelineExecutionResults.ConditionNotMatched;
                }
                else
                {
                    _logger.LogDebug("the trigger {trgger} meet the condtion {condition}", trigger, Condition);
                }
            }
            else
            {
                _logger.LogDebug("no conditions applied. actor enabled");
            }

            _logger.LogDebug("executing actor...");
            try
            {
                Boolean actorResult = await Actor.Handle(trigger);
                if (actorResult == false)
                {
                    _logger.LogError("Actor {actor} of pipeline {name} failed.", Actor, Name);
                    return NotifactionPipelineExecutionResults.ActorFailed;
                }

                return NotifactionPipelineExecutionResults.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError("unable to execute actor", ex.ToString());
                return NotifactionPipelineExecutionResults.ActorFailed;
            }
        }

        #endregion

        #region When


        protected override void When(DomainEvent domainEvent)
        {
            switch (domainEvent)
            {
                case NotificationPipelineCreatedEvent e:
                    Name = new NotificationPipelineName(e.Name);
                    Description = new NotificationPipelineDescription(e.Description);
                    Id = e.Id;
                    TriggerIdentifier = e.TriggerIdentifier;
                    Condition = _conditionFactory.Initilize(e.ConditionCreateInfo);
                    Actor = _actorFactory.Initilize(e.ActorCreateInfo);
                    break;
                default:
                    break;
            }
        }

        #endregion

    }
}
