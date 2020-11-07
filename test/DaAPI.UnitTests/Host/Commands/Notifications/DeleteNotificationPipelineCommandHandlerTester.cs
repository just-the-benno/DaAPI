using DaAPI.Host.Application.Commands.Notifications;
using DaAPI.Infrastructure.NotificationEngine;
using DaAPI.TestHelper;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace DaAPI.UnitTests.Host.Commands.Notifications
{
    public class DeleteNotificationPipelineCommandHandlerTester
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Handle(Boolean engineResult)
        {
            Random random = new Random();
            Guid id = random.NextGuid();

            Mock<INotificationEngine> engineMock = new Mock<INotificationEngine>();
            engineMock.Setup(x => x.DeletePipeline(id)).ReturnsAsync(engineResult).Verifiable();

            var command = new DeleteNotificationPipelineCommand(id);

            var handler = new DeleteNotificationPipelineCommandHandler(engineMock.Object,Mock.Of<ILogger<DeleteNotificationPipelineCommandHandler>>());

            Boolean actual = await handler.Handle(command, CancellationToken.None);
            Assert.Equal(engineResult, actual);

            engineMock.Verify();
        }


    }
}
