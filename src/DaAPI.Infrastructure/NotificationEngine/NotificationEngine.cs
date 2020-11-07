using DaAPI.Core.Notifications;
using DaAPI.Core.Notifications.Actors;
using DaAPI.Core.Notifications.Conditions;
using DaAPI.Core.Notifications.Triggers;
using DaAPI.Infrastructure.StorageEngine.DHCPv6;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DaAPI.Infrastructure.NotificationEngine.NotifciationsReadModels.V1;

namespace DaAPI.Infrastructure.NotificationEngine
{
    public class NotificationEngine : INotificationEngine
    {
        private readonly IDHCPv6StorageEngine storageEngine;

        private List<NotificationPipeline> _pipelines;

        public NotificationEngine(IDHCPv6StorageEngine storageEngine)
        {
            this.storageEngine = storageEngine;
        }

        public async Task Initialize()
        {
            _pipelines = new List<NotificationPipeline>(await storageEngine.GetAllNotificationPipeleines());
        }

        public async Task<Boolean> AddNotificationPipeline(NotificationPipeline pipeline)
        {
            Boolean result = await storageEngine.Save(pipeline);
            if (result == true)
            {
                _pipelines.Add(pipeline);
            }

            return result;
        }

        public async Task<Boolean> DeletePipeline(Guid id)
        {
            var pipeline = _pipelines.FirstOrDefault(x => x.Id == id);
            if(pipeline == null)
            {
                return false;
            }

            if(await storageEngine.DeleteAggregateRoot<NotificationPipeline>(pipeline.Id) == false)
            {
                return false;
            }

            _pipelines.Remove(pipeline);
            return true;
        }

        public async Task HandleTrigger(NotifcationTrigger trigger)
        {
            foreach (var item in _pipelines)
            {
                if (item.CanExecute(trigger) == true)
                {
                    await item.Execute(trigger);
                }
            }
        }

        public Task<IEnumerable<NotificationPipelineReadModel>> GetPipelines()
        {
            var result = _pipelines.Select(x => new NotificationPipelineReadModel
            {
                Id = x.Id,
                Name = x.Name,
                TriigerName = x.TriggerIdentifier,
                ActorName = x.Actor.GetType().Name,
                ConditionName = x.Condition == null ? "None" : x.Condition.GetType().Name,
            }).ToList();

            return Task.FromResult<IEnumerable<NotificationPipelineReadModel>>(result);
        }

        public Int32 GetPipelineAmount() => _pipelines.Count();

        public Task<NotificationPipelineDescriptions> GetPiplelineDescriptions()
        {
            var triggerDescriptions = new[]
            {
                new NotificationTriggerDescription
                {
                    Name = nameof(PrefixEdgeRouterBindingUpdatedTrigger),
                }
            };

            var conditionsDescriptions = new[]
            {
                new NotifcationCondititonDescription
                {
                    Name = nameof(DHCPv6ScopeIdNotificationCondition),
                    Properties = new Dictionary<string, NotifcationCondititonDescription.ConditionsPropertyTtpes>
                    {
                        { nameof(DHCPv6ScopeIdNotificationCondition.IncludesChildren), NotifcationCondititonDescription.ConditionsPropertyTtpes.Boolean  },
                        { nameof(DHCPv6ScopeIdNotificationCondition.ScopeIds), NotifcationCondititonDescription.ConditionsPropertyTtpes.DHCPv6ScopeList  },
                    }
                },
            };

            var actorDescription = new[]
            {
                new NotifcationActorDescription
                {
                    Name = nameof(NxOsStaticRouteUpdaterNotificationActor),
                    Properties = new Dictionary<String,NotifcationActorDescription.ActorPropertyTtpes>
                    {
                        { nameof(NxOsStaticRouteUpdaterNotificationActor.Url),  NotifcationActorDescription.ActorPropertyTtpes.Endpoint  },
                        { nameof(NxOsStaticRouteUpdaterNotificationActor.Username),  NotifcationActorDescription.ActorPropertyTtpes.Username  },
                        { nameof(NxOsStaticRouteUpdaterNotificationActor.Password),  NotifcationActorDescription.ActorPropertyTtpes.Password  },
                    }
                }
            };

            var mapping = new[]
            {
                new NotificationPipelineTriggerMapperEnry
                {
                 TriggerName = nameof(PrefixEdgeRouterBindingUpdatedTrigger),
                 CompactibleConditions = new[] { nameof(DHCPv6ScopeIdNotificationCondition) },
                 CompactibleActors = new[] { nameof(NxOsStaticRouteUpdaterNotificationActor) }
                }
            };

            NotificationPipelineDescriptions result = new NotificationPipelineDescriptions
            {
                Actors = actorDescription,
                Conditions = conditionsDescriptions,
                Trigger = triggerDescriptions,
                MapperEnries = mapping,
            };

            return Task.FromResult(result);
        }
    }
}
