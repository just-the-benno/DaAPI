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
    public class ResetLocalUserPasswordCommandHandlerTester
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Handle(Boolean userServiceResult)
        {
            Random random = new Random();
            String userId = random.GetAlphanumericString();
            String password = random.GetAlphanumericString();

            var localUserServiceMock = new Mock<ILocalUserService>(MockBehavior.Strict);
            localUserServiceMock.Setup(x => x.CheckIfUserExists(userId)).ReturnsAsync(true).Verifiable();
            localUserServiceMock.Setup(x => x.ResetPassword(userId,password)).ReturnsAsync(userServiceResult).Verifiable();

            var handler = new ResetLocalUserPasswordCommandHandler(localUserServiceMock.Object,
                Mock.Of<ILogger<ResetLocalUserPasswordCommandHandler>>());

            Boolean result = await handler.Handle(new ResetLocalUserPasswordCommand(userId, password), CancellationToken.None);
            Assert.Equal(result, userServiceResult);

            localUserServiceMock.Verify();
        }

        [Fact]
        public async Task Handle_Failed_UserNotFound()
        {
            Random random = new Random();
            String userId = random.GetAlphanumericString();

            var localUserServiceMock = new Mock<ILocalUserService>(MockBehavior.Strict);
            localUserServiceMock.Setup(x => x.CheckIfUserExists(userId)).ReturnsAsync(false).Verifiable();

            var handler = new ResetLocalUserPasswordCommandHandler(localUserServiceMock.Object,
                Mock.Of<ILogger<ResetLocalUserPasswordCommandHandler>>());

            Boolean result = await handler.Handle(new ResetLocalUserPasswordCommand(userId, random.GetAlphanumericString()), CancellationToken.None);
            Assert.False(result);

            localUserServiceMock.Verify();
        }
    }
}
