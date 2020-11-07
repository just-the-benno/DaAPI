using Castle.Core.Logging;
using DaAPI.App.Pages;
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
    public class DeleteLocalUserCommandHandlerTester
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Handle(Boolean userServiceResult)
        {
            Random random = new Random();
            String userId = random.GetAlphanumericString();

            var userIdTokenExtractorMock = new Mock<IUserIdTokenExtractor>(MockBehavior.Strict);
            userIdTokenExtractorMock.Setup(x => x.GetUserId(true)).Returns(random.NextGuid().ToString()).Verifiable();

            var localUserServiceMock = new Mock<ILocalUserService>(MockBehavior.Strict);
            localUserServiceMock.Setup(x => x.GetUserAmount()).ReturnsAsync(random.Next(3,10)).Verifiable();
            localUserServiceMock.Setup(x => x.CheckIfUserExists(userId)).ReturnsAsync(true).Verifiable();

            localUserServiceMock.Setup(x => x.DeleteUser(userId)).ReturnsAsync(userServiceResult).Verifiable();

            var handler = new DeleteLocalUserCommandHandler(
                userIdTokenExtractorMock.Object, localUserServiceMock.Object,
                Mock.Of<ILogger<DeleteLocalUserCommandHandler>>());

            Boolean result = await handler.Handle(new DeleteLocalUserCommand(userId), CancellationToken.None);
            Assert.Equal(userServiceResult, result);

            localUserServiceMock.Verify();
            userIdTokenExtractorMock.Verify();
        }

        [Fact]
        public async Task Handle_Failed_DontDeleteOwnAccount()
        {
            Random random = new Random();
            String userId = random.GetAlphanumericString();

            var userIdTokenExtractorMock = new Mock<IUserIdTokenExtractor>(MockBehavior.Strict);
            userIdTokenExtractorMock.Setup(x => x.GetUserId(true)).Returns(userId).Verifiable();

            var handler = new DeleteLocalUserCommandHandler(
                userIdTokenExtractorMock.Object, Mock.Of<ILocalUserService>(MockBehavior.Strict),
                Mock.Of<ILogger<DeleteLocalUserCommandHandler>>());

            Boolean result = await handler.Handle(new DeleteLocalUserCommand(userId), CancellationToken.None);
            Assert.False(result);

            userIdTokenExtractorMock.Verify();
        }

        [Fact]
        public async Task Handle_Failed_UserNotFound()
        {
            Random random = new Random();
            String userId = random.GetAlphanumericString();

            var userIdTokenExtractorMock = new Mock<IUserIdTokenExtractor>(MockBehavior.Strict);
            userIdTokenExtractorMock.Setup(x => x.GetUserId(true)).Returns(random.NextGuid().ToString()).Verifiable();

            var localUserServiceMock = new Mock<ILocalUserService>(MockBehavior.Strict);
            localUserServiceMock.Setup(x => x.CheckIfUserExists(userId)).ReturnsAsync(false).Verifiable();

            var handler = new DeleteLocalUserCommandHandler(
                userIdTokenExtractorMock.Object, localUserServiceMock.Object,
                Mock.Of<ILogger<DeleteLocalUserCommandHandler>>());

            Boolean result = await handler.Handle(new DeleteLocalUserCommand(userId), CancellationToken.None);
            Assert.False(result);

            userIdTokenExtractorMock.Verify();
        }

        [Fact]
        public async Task Handle_Failed_DontDeleteLastccount()
        {
            Random random = new Random();
            String userId = random.GetAlphanumericString();

            var userIdTokenExtractorMock = new Mock<IUserIdTokenExtractor>(MockBehavior.Strict);
            userIdTokenExtractorMock.Setup(x => x.GetUserId(true)).Returns(random.NextGuid().ToString()).Verifiable();

            var localUserServiceMock = new Mock<ILocalUserService>(MockBehavior.Strict);
            localUserServiceMock.Setup(x => x.GetUserAmount()).ReturnsAsync(1).Verifiable();
            localUserServiceMock.Setup(x => x.CheckIfUserExists(userId)).ReturnsAsync(true).Verifiable();

            var handler = new DeleteLocalUserCommandHandler(
                userIdTokenExtractorMock.Object, localUserServiceMock.Object,
                Mock.Of<ILogger<DeleteLocalUserCommandHandler>>());

            Boolean result = await handler.Handle(new DeleteLocalUserCommand(userId), CancellationToken.None);
            Assert.False(result);

            userIdTokenExtractorMock.Verify();
        }
    }
}
