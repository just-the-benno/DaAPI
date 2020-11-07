using Castle.Core.Logging;
using DaAPI.Host.Application.Commands;
using DaAPI.Host.Infrastrucutre;
using DaAPI.TestHelper;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace DaAPI.UnitTests.Host.Commands.LocalUserCommands
{
    public class CreateLocalUserCommandHandlerTester
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Handle(Boolean userServiceResult)
        {
            Random random = new Random();
            String username = random.GetAlphanumericString();
            String password = random.GetAlphanumericString();

            Guid? userId = userServiceResult == true ? random.NextGuid() : new Guid?();

            var localUserServiceMock = new Mock<ILocalUserService>(MockBehavior.Strict);
            localUserServiceMock.Setup(x => x.CreateUser(username, password)).ReturnsAsync(userId).Verifiable();

            var handler = new CreateLocalUserCommandHandler(localUserServiceMock.Object,
                Mock.Of<ILogger<CreateLocalUserCommandHandler>>());

            String result = await handler.Handle(new CreateLocalUserCommand(username, password), CancellationToken.None);
            if (userServiceResult == true)
            {
                Assert.Equal(userId.ToString(), result);
            }
            else
            {
                Assert.True(String.IsNullOrEmpty(result));
            }

            localUserServiceMock.Verify();
        }
    }
}
