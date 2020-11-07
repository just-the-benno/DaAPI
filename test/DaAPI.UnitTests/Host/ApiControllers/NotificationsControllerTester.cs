using DaAPI.Core.Listeners;
using DaAPI.Host.ApiControllers;
using DaAPI.Host.Application.Commands.DHCPv6Interfaces;
using DaAPI.Host.Application.Commands.Notifications;
using DaAPI.Infrastructure.InterfaceEngines;
using DaAPI.Infrastructure.NotificationEngine;
using DaAPI.TestHelper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using static DaAPI.Infrastructure.NotificationEngine.NotifciationsReadModels.V1;
using static DaAPI.Shared.Requests.DHCPv6InterfaceRequests.V1;
using static DaAPI.Shared.Requests.NotificationPipelineRequests.V1;
using static DaAPI.Shared.Responses.DHCPv6InterfaceResponses.V1;

namespace DaAPI.UnitTests.Host.ApiControllers
{
    public class NotificationsControllerTester
    {
        [Fact]
        public async Task GetAllPipelines()
        {
            var pipelines = new List<NotificationPipelineReadModel>();
          
            Mock<INotificationEngine> notificationEngineMock = new Mock<INotificationEngine>(MockBehavior.Strict);
            notificationEngineMock.Setup(x => x.GetPipelines()).ReturnsAsync(pipelines).Verifiable();

            var controller = new NotificationsController(
                notificationEngineMock.Object, Mock.Of<IMediator>(MockBehavior.Strict),
                Mock.Of<ILogger<NotificationsController>>());

            var actionResult = await controller.GetAllPipelines();
            var result = actionResult.EnsureOkObjectResult<IEnumerable<NotificationPipelineReadModel>>(true);

            Assert.Equal(pipelines, result);

            notificationEngineMock.Verify();
        }

        [Fact]
        public async Task GetPiplelineDescriptions()
        {
            var descriptions = new NotificationPipelineDescriptions();

            Mock<INotificationEngine> notificationEngineMock = new Mock<INotificationEngine>(MockBehavior.Strict);
            notificationEngineMock.Setup(x => x.GetPiplelineDescriptions()).ReturnsAsync(descriptions).Verifiable();

            var controller = new NotificationsController(
                notificationEngineMock.Object, Mock.Of<IMediator>(MockBehavior.Strict),
                Mock.Of<ILogger<NotificationsController>>());

            var actionResult = await controller.GetPiplelineDescriptions();
            var result = actionResult.EnsureOkObjectResult<NotificationPipelineDescriptions>(true);

            Assert.Equal(descriptions, result);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task CreatePipeline(Boolean successfullMediatorResult)
        {
            Random random = new Random();
            String name = random.GetAlphanumericString();
            String description = random.GetAlphanumericString();
            String triggerName = random.GetAlphanumericString();
            String conditionName = random.GetAlphanumericString();
            IDictionary<String, String> conditionProperties = new Dictionary<string, string>();
            String actorName = random.GetAlphanumericString();
            IDictionary<String,String > actorProperties = new Dictionary<string, string>();

            Guid? pipelineId = successfullMediatorResult == true ? random.NextGuid() : new Guid?();

            NonStrictDictionaryComparer<String, String> dictionaryComparer = new NonStrictDictionaryComparer<string, string>();

            Mock<IMediator> mediatorMock = new Mock<IMediator>(MockBehavior.Strict);
            mediatorMock.Setup(x => x.Send(It.Is<CreateNotificationPipelineCommand>(y =>
            y.Name == name &&
            y.Description == description &&
            y.TriggerName == triggerName &&
            y.CondtionName  == conditionName &&
            dictionaryComparer.Equals(y.ConditionProperties,conditionProperties) == true &&
            y.ActorName == actorName &&
            dictionaryComparer.Equals(y.ActorProperties,actorProperties) == true
            ), It.IsAny<CancellationToken>())).ReturnsAsync(pipelineId).Verifiable();

            var controller = new NotificationsController(
                Mock.Of<INotificationEngine>(MockBehavior.Strict), mediatorMock.Object,
                Mock.Of<ILogger<NotificationsController>>());

            var actionResult = await controller.CreatePipeline(new CreateNotifcationPipelineRequest
            {
                Name = name,
                Description = description,
                TriggerName = triggerName,
                CondtionName = conditionName,
                ConditionProperties = conditionProperties,
                ActorName = actorName,
                ActorProperties = actorProperties,
            });

            if (successfullMediatorResult == true)
            {
                Guid actual = actionResult.EnsureOkObjectResult<Guid>(true);
                Assert.Equal(pipelineId.Value, actual);
            }
            else
            {
                actionResult.EnsureBadRequestObjectResult("unable to create pipeline");
            }

            mediatorMock.Verify();
        }

        private async Task CheckModelState(Func<NotificationsController, Task<IActionResult>> controllerExecuter)
        {
            Random random = new Random();

            var controller = new NotificationsController(
               Mock.Of<INotificationEngine>(MockBehavior.Strict), Mock.Of<IMediator>(MockBehavior.Strict),
               Mock.Of<ILogger<NotificationsController>>());


            String modelErrorKey = "a" + random.GetAlphanumericString();
            String modelErrorMessage = random.GetAlphanumericString();
            controller.ModelState.AddModelError(modelErrorKey, modelErrorMessage);

            var result = await controllerExecuter(controller);

            result.EnsureBadRequestObjectResultForError(modelErrorKey, modelErrorMessage);
        }

        [Fact]
        public async Task CreatePipeline_ModelStateError()
        {
            await CheckModelState((controller) => controller.CreatePipeline(null));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task DeleteListener(Boolean mediatorResult)
        {
            Random random = new Random();
            Guid pipelineId = random.NextGuid();

            Mock<IMediator> mediatorMock = new Mock<IMediator>(MockBehavior.Strict);
            mediatorMock.Setup(x => x.Send(It.Is<DeleteNotificationPipelineCommand>(y =>
            y.PipelineId == pipelineId
            ), It.IsAny<CancellationToken>())).ReturnsAsync(mediatorResult).Verifiable();

            var controller = new NotificationsController(
                Mock.Of<INotificationEngine>(MockBehavior.Strict), mediatorMock.Object,
                Mock.Of<ILogger<NotificationsController>>());

            var actionResult = await controller.DeletePipeline(pipelineId);
            
            if (mediatorResult == true)
            {
                actionResult.EnsureNoContentResult();
            }
            else
            {
                actionResult.EnsureBadRequestObjectResult("unable to delete the pipeline");
            }

            mediatorMock.Verify();
        }

        [Fact]
        public async Task DeletePipeline_ModelStateError()
        {
            await CheckModelState((controller) => controller.DeletePipeline(Guid.Empty));
        }
    }
}
