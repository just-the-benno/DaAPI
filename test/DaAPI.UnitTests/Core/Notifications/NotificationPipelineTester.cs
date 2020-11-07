using DaAPI.Core.Notifications;
using DaAPI.TestHelper;
using DaAPI.UnitTests.Helper;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static DaAPI.Core.Notifications.NotificationsEvent.V1;

namespace DaAPI.UnitTests.Core.Notifications
{
    public class NotificationPipelineTester : AggregateRootWithEventsTesterBase
    {
        public class DummyNotifcationTrigger : NotifcationTrigger
        {
            public string Identiifer { get; }

            public DummyNotifcationTrigger(String identiifer)
            {
                Identiifer = identiifer;
            }

            public override string GetTypeIdentifier() => Identiifer;
        }

        public class DummyNotificationCondition : NotificationCondition
        {
            public Boolean ApplyValuesResult { get; set; } = false;
            public NotificationConditionCreateModel ToCreateModelResult { get; set; } = null;
            public Boolean IsValidResult { get; set; } = false;

            public Boolean ApplyValuesShouldThrowError { get; set; }
            public Boolean IsValidShouldThrowError { get; set; }
            public Boolean ToCreateModelShouldThrowError { get; set; }

            public override bool ApplyValues(IDictionary<string, string> propertiesAndValues)
            {
                if (ApplyValuesShouldThrowError == true) { throw new NotImplementedException(); }

                return ApplyValuesResult;
            }

            public override Task<bool> IsValid(NotifcationTrigger trigger)
            {
                if (IsValidShouldThrowError == true) { throw new NotImplementedException(); }

                return Task.FromResult(IsValidResult);
            }

            public override NotificationConditionCreateModel ToCreateModel()
            {
                if (ToCreateModelShouldThrowError == true) { throw new NotImplementedException(); }

                return ToCreateModelResult;
            }

            public static DummyNotificationCondition AllErrors = new DummyNotificationCondition
            {
                ApplyValuesShouldThrowError = true,
                ToCreateModelShouldThrowError = true,
                IsValidShouldThrowError = true,
            };
        }

        public class DummyNotificationActor : NotificationActor
        {
            public Boolean ApplyResult { get; set; } = false;
            public NotificationActorCreateModel ToCreateModelResult { get; set; } = null;
            public Boolean HandleResult { get; set; } = false;

            public Boolean ApplyValuesShouldThrowError { get; set; }
            public Boolean ToCreateModelShouldThrowError { get; set; }
            public Boolean HandleModelShouldThrowError { get; set; }

            public override bool ApplyValues(IDictionary<string, string> propertiesAndValues)
            {
                if (ApplyValuesShouldThrowError == true) throw new NotImplementedException();

                return ApplyResult;
            }

            public override NotificationActorCreateModel ToCreateModel()
            {
                if (ToCreateModelShouldThrowError == true) throw new NotImplementedException();

                return ToCreateModelResult;
            }

            protected override Task<bool> Handle(NotifcationTrigger trigger)
            {
                if (HandleModelShouldThrowError == true) throw new NotImplementedException();

                return Task.FromResult(HandleResult);
            }

            public static DummyNotificationActor AllErrors = new DummyNotificationActor
            {
                ApplyValuesShouldThrowError = true,
                ToCreateModelShouldThrowError = true,
                HandleModelShouldThrowError = true,
            };
        }

        public static NotificationPipeline CreatePipleline(Random random, string triggerIdentifier, NotificationCondition condition = null, NotificationActor actor = null)
        {
            NotificationActorCreateModel actorCreateModel = new NotificationActorCreateModel
            {
                Typename = random.GetAlphanumericString(),
                PropertiesAndValues = new Dictionary<String, String>(),
            };

            NotificationConditionCreateModel conditionCreateModel = new NotificationConditionCreateModel
            {
                Typename = random.GetAlphanumericString(),
                PropertiesAndValues = new Dictionary<String, String>(),
            };

            Mock<INotificationConditionFactory> conditionFactoryMock = new Mock<INotificationConditionFactory>(MockBehavior.Strict);
            conditionFactoryMock.Setup(x => x.Initilize(conditionCreateModel)).Returns(condition ?? DummyNotificationCondition.AllErrors).Verifiable();

            Mock<INotificationActorFactory> actorFactoryMock = new Mock<INotificationActorFactory>(MockBehavior.Strict);
            actorFactoryMock.Setup(x => x.Initilize(actorCreateModel)).Returns(actor ?? DummyNotificationActor.AllErrors).Verifiable();

            var pipeline = new NotificationPipeline(conditionFactoryMock.Object,
                actorFactoryMock.Object, Mock.Of<ILogger<NotificationPipeline>>());
            pipeline.Load(new[]{new NotificationPipelineCreatedEvent
            {
                Id = random.NextGuid(),
                Description = random.GetAlphanumericString(),
                Name = random.GetAlphanumericString(),
                TriggerIdentifier = triggerIdentifier,
                ActorCreateInfo = actorCreateModel,
                ConditionCreateInfo = conditionCreateModel,
            }});

            return pipeline;
        }

        public static NotificationPipeline CreatePipleline(Random random, out string triggerIdentifier, NotificationCondition condition = null, NotificationActor actor = null)
        {
            triggerIdentifier = random.GetAlphanumericString();
            return CreatePipleline(random, triggerIdentifier, condition, actor);
        }

        [Fact]
        public void CanExecute()
        {
            Random random = new Random();
            var pipeline = CreatePipleline(random, out string triggerIdentifier);

            Boolean firstTriggerResult = pipeline.CanExecute(new DummyNotifcationTrigger(triggerIdentifier));
            Boolean secondTriggerResult = pipeline.CanExecute(new DummyNotifcationTrigger(triggerIdentifier.Substring(1)));

            Assert.True(firstTriggerResult);
            Assert.False(secondTriggerResult);
        }

        [Fact]
        public async Task Execute_NullCondition()
        {
            Random random = new Random();

            var pipeline = CreatePipleline(random, out string triggerIdentifier, NotificationCondition.True, new DummyNotificationActor { HandleResult = true });

            NotifactionPipelineExecutionResults result = await pipeline.Execute(new DummyNotifcationTrigger(triggerIdentifier));
            Assert.Equal(NotifactionPipelineExecutionResults.Success, result);
        }

        [Fact]
        public async Task Execute_NullCondition_ActorFailed()
        {
            Random random = new Random();

            var pipeline = CreatePipleline(random, out string triggerIdentifier, NotificationCondition.True, new DummyNotificationActor { HandleResult = false });

            NotifactionPipelineExecutionResults result = await pipeline.Execute(new DummyNotifcationTrigger(triggerIdentifier));
            Assert.Equal(NotifactionPipelineExecutionResults.ActorFailed, result);
        }

        [Fact]
        public async Task Execute_ConditionTrue()
        {
            Random random = new Random();

            var pipeline = CreatePipleline(random, out string triggerIdentifier, new DummyNotificationCondition { IsValidResult = true }, new DummyNotificationActor { HandleResult = true });

            NotifactionPipelineExecutionResults result = await pipeline.Execute(new DummyNotifcationTrigger(triggerIdentifier));
            Assert.Equal(NotifactionPipelineExecutionResults.Success, result);
        }

        [Fact]
        public async Task Execute_ConditionTrue_ActorFailed()
        {
            Random random = new Random();

            var pipeline = CreatePipleline(random, out string triggerIdentifier, new DummyNotificationCondition { IsValidResult = true }, new DummyNotificationActor { HandleResult = false });

            NotifactionPipelineExecutionResults result = await pipeline.Execute(new DummyNotifcationTrigger(triggerIdentifier));
            Assert.Equal(NotifactionPipelineExecutionResults.ActorFailed, result);
        }

        [Fact]
        public async Task Execute_ConditionFalse()
        {
            Random random = new Random();

            var pipeline = CreatePipleline(random, out string triggerIdentifier, new DummyNotificationCondition { IsValidResult = false }, new DummyNotificationActor { HandleResult = true });

            NotifactionPipelineExecutionResults result = await pipeline.Execute(new DummyNotifcationTrigger(triggerIdentifier));
            Assert.Equal(NotifactionPipelineExecutionResults.ConditionNotMatched, result);
        }

        [Fact]
        public async Task Execute_TriggerNotValid()
        {
            Random random = new Random();

            var pipeline = CreatePipleline(random, out string triggerIdentifier);

            NotifactionPipelineExecutionResults result = await pipeline.Execute(new DummyNotifcationTrigger(triggerIdentifier.Substring(1)));
            Assert.Equal(NotifactionPipelineExecutionResults.TriggerNotMatch, result);
        }

        [Fact]
        public void Ceate()
        {
            Random random = new Random();
            String name = random.GetAlphanumericString();
            String description = random.GetAlphanumericString();
            String triggerInputName = random.GetAlphanumericString();

            NotificationConditionCreateModel conditionCreateModel = new NotificationConditionCreateModel()
            {
                Typename = random.GetAlphanumericString()
            };

            NotificationActorCreateModel actorCreateModel = new NotificationActorCreateModel()
            {
                Typename = random.GetAlphanumericString()
            };

            DummyNotificationCondition condition = new DummyNotificationCondition { ToCreateModelResult = conditionCreateModel };
            DummyNotificationActor actor = new DummyNotificationActor { ToCreateModelResult = actorCreateModel };

            Mock<INotificationConditionFactory> conditionFactoryMock = new Mock<INotificationConditionFactory>(MockBehavior.Strict);
            conditionFactoryMock.Setup(x => x.Initilize(conditionCreateModel)).Returns(condition).Verifiable();

            Mock<INotificationActorFactory> actorFactoryMock = new Mock<INotificationActorFactory>(MockBehavior.Strict);
            actorFactoryMock.Setup(x => x.Initilize(actorCreateModel)).Returns(actor).Verifiable();

            var pipeline = NotificationPipeline.Create(
                NotificationPipelineName.FromString(name), NotificationPipelineDescription.FromString(description),
                triggerInputName, condition, actor, Mock.Of<ILogger<NotificationPipeline>>(),
                conditionFactoryMock.Object, actorFactoryMock.Object);

            Assert.Equal(name, pipeline.Name);
            Assert.Equal(description, pipeline.Description);
            Assert.Equal(triggerInputName, pipeline.TriggerIdentifier);
            Assert.Equal(condition, pipeline.Condition);
            Assert.Equal(actor, pipeline.Actor);

            var @event = GetFirstEvent<NotificationPipelineCreatedEvent>(pipeline);

            Assert.Equal(name, @event.Name);
            Assert.Equal(description, @event.Description);
            Assert.Equal(triggerInputName, @event.TriggerIdentifier);
            Assert.Equal(actorCreateModel, @event.ActorCreateInfo);
            Assert.Equal(conditionCreateModel, @event.ConditionCreateInfo);
        }
    }
}
