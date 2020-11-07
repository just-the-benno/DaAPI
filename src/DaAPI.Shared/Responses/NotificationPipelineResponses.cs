using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Shared.Responses
{
    public static class NotificationPipelineResponses
    {
        public static class V1
        {
            public class NotificationPipelineReadModel
            {
                public Guid Id { get; set; }
                public String Name { get; set; }
                public String TrigerName { get; set; }
                public String ConditionName { get; set; }
                public String ActorName { get; set; }
            }

            public class NotificationPipelineTriggerMapperEnry
            {
                public String TriggerName { get; set; }
                public IEnumerable<String> CompactibleConditions { get; set; }
                public IEnumerable<String> CompactibleActors { get; set; }
            }

            public class NotificationTriggerDescription
            {
                public String Name { get; set; }
            }

            public class NotificationCondititonDescription
            {
                public enum ConditionsPropertyTypes
                {
                    Boolean = 1,
                    DHCPv6ScopeList = 2,
                }

                public String Name { get; set; }
                public IDictionary<String, ConditionsPropertyTypes> Properties { get; set; }
            }


            public class NotifcationActorDescription
            {
                public enum ActorPropertyTypes
                {
                    Endpoint = 1,
                    Username = 2,
                    Password = 3,
                }

                public String Name { get; set; }
                public IDictionary<String, ActorPropertyTypes> Properties { get; set; }
            }

            public class NotificationPipelineDescriptions
            {
                public IEnumerable<NotificationTriggerDescription> Trigger { get; set; }
                public IEnumerable<NotificationCondititonDescription> Conditions { get; set; }
                public IEnumerable<NotifcationActorDescription> Actors { get; set; }

                public IEnumerable<NotificationPipelineTriggerMapperEnry> MapperEnries { get; set; }
            }
        }
    }
}
