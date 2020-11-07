using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DaAPI.Host.Application.Commands.Notifications
{
    public class CreateNotificationPipelineCommand : IRequest<Guid?>
    {
        public String Name { get; }
        public String Description { get; }

        public String TriggerName { get; }

        public String CondtionName { get; }
        public IDictionary<String, String> ConditionProperties { get; }

        public String ActorName { get; }

        public IDictionary<String, String> ActorProperties { get; }

        public CreateNotificationPipelineCommand(
            String name, String description,
            String triggerName,
            String condtionName, IDictionary<String, String> conditionProperties,
            String actorName, IDictionary<String, String> actorProperties)
        {
            Name = name;
            Description = description;
            TriggerName = triggerName;
            CondtionName = condtionName;
            ConditionProperties = conditionProperties;
            ActorName = actorName;
            ActorProperties = actorProperties;
        }
    }
}
