using DaAPI.App.Resources;
using DaAPI.App.Resources.Pages.Notifications;
using DaAPI.App.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using static DaAPI.Shared.Requests.NotificationPipelineRequests.V1;
using static DaAPI.Shared.Responses.NotificationPipelineResponses.V1;
using static DaAPI.Shared.Responses.NotificationPipelineResponses.V1.NotifcationActorDescription;
using static DaAPI.Shared.Responses.NotificationPipelineResponses.V1.NotificationCondititonDescription;

namespace DaAPI.App.Pages.Notifications
{
    public class NotificationPipelineConditionPropertyEntry
    {
        public ConditionsPropertyTypes Type { get; }
        public String Name { get; }

        public IEnumerable<String> Values { get; set; }
        public String Value { get; set; }
        public Boolean ValueAsBoolean { get; set; }

        public NotificationPipelineConditionPropertyEntry(String name, ConditionsPropertyTypes type)
        {
            Name = name;
            Type = type;
        }

        public String GetSerializedValues() =>
            Type switch
            {
                ConditionsPropertyTypes.Boolean => JsonSerializer.Serialize(ValueAsBoolean),
                ConditionsPropertyTypes.DHCPv6ScopeList => JsonSerializer.Serialize(Values),
                _ => String.Empty,
            };
    }

    public class NotificationPipelineActorPropertyEntry
    {
        public ActorPropertyTypes Type { get; }
        public String Name { get; }

        [Required(ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.Required))]
        [NotificationPipelineActorProperty(ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.NotificationPipelineActorProperty))]
        public String Value { get; set; }

        public NotificationPipelineActorPropertyEntry(String name, ActorPropertyTypes type)
        {
            Name = name;
            Type = type;
        }

        public String GetSerializedValues() => "\"" + Value + "\"";
    }

    public class CreateNotificationPipelineViewModel
    {
        private NotificationPipelineDescriptions _descriptions;

        [Required(ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.Required))]
        [MaxLength(100, ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.MaxLength))]
        [MinLength(3, ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.MinLength))]
        [Display(Name = nameof(CreateNotificationPipelineViewModelDisplay.Name), ResourceType = typeof(CreateNotificationPipelineViewModelDisplay))]
        public String Name { get; set; }

        [MaxLength(500, ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.MaxLength))]
        [Display(Name = nameof(CreateNotificationPipelineViewModelDisplay.Description), ResourceType = typeof(CreateNotificationPipelineViewModelDisplay))]
        public String Description { get; set; }

        private String _triggerName;

        [Required(ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.Required))]
        [Display(Name = nameof(CreateNotificationPipelineViewModelDisplay.Trigger), ResourceType = typeof(CreateNotificationPipelineViewModelDisplay))]
        public String TriggerName
        {
            get => _triggerName;
            set
            {
                _triggerName = value;
                var mapperEntry = _descriptions.MapperEnries.First(x => x.TriggerName == value);
                PossibleCondtions = _descriptions.Conditions.Where(x => mapperEntry.CompactibleConditions.Contains(x.Name)).Select(x => x.Name).ToList();
                PossibleActors = _descriptions.Actors.Where(x => mapperEntry.CompactibleActors.Contains(x.Name)).Select(x => x.Name).ToList();
            }
        }

        private String _conditionName;

        [Display(Name = nameof(CreateNotificationPipelineViewModelDisplay.Condition), ResourceType = typeof(CreateNotificationPipelineViewModelDisplay))]
        public String ConditionName
        {
            get => _conditionName;
            set
            {
                _conditionName = value;
                if (String.IsNullOrEmpty(value) == false)
                {
                    ConditionsProperties = _descriptions.Conditions.First(x => x.Name == value).Properties
                          .Select(x => new NotificationPipelineConditionPropertyEntry(x.Key, x.Value))
                          .ToList();
                }
                else
                {
                    ConditionsProperties.Clear();
                }
            }
        }

        private String _actorName;

        [Required(ErrorMessageResourceType = typeof(ValidationErrorMessages), ErrorMessageResourceName = nameof(ValidationErrorMessages.Required))]
        [Display(Name = nameof(CreateNotificationPipelineViewModelDisplay.Condition), ResourceType = typeof(CreateNotificationPipelineViewModelDisplay))]
        public String ActorName
        {
            get => _actorName;
            set
            {
                _actorName = value;

                ActorProperties = _descriptions.Actors.First(x => x.Name == value).Properties
                         .Select(x => new NotificationPipelineActorPropertyEntry(x.Key, x.Value))
                         .ToList();
            }
        }

        public IEnumerable<String> PossibleCondtions { get; private set; } = new List<String>();
        public IEnumerable<String> PossibleActors { get; private set; } = new List<String>();

        public List<NotificationPipelineConditionPropertyEntry> ConditionsProperties { get; private set; } = new List<NotificationPipelineConditionPropertyEntry>();
        public List<NotificationPipelineActorPropertyEntry> ActorProperties { get; private set; } = new List<NotificationPipelineActorPropertyEntry>();

        public void AddDescriptions(NotificationPipelineDescriptions descriptions) => _descriptions = descriptions;

        public CreateNotifcationPipelineRequest GetRequest() => new CreateNotifcationPipelineRequest
        {
            Name = Name,
            Description = Description,
            TriggerName = TriggerName,
            CondtionName = ConditionName,
            ConditionProperties = ConditionsProperties.ToDictionary(x => x.Name, x => x.GetSerializedValues()),
            ActorName = ActorName,
            ActorProperties = ActorProperties.ToDictionary(x => x.Name, x => x.GetSerializedValues())
        };

    }
}
