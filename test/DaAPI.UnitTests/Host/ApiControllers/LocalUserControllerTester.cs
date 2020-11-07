using Castle.Core.Logging;
using DaAPI.Host.ApiControllers;
using DaAPI.Host.Application.Commands;
using DaAPI.Host.Infrastrucutre;
using DaAPI.TestHelper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using static DaAPI.Shared.Requests.LocalUserRequests.V1;
using static DaAPI.Shared.Responses.LocalUsersResponses.V1;

namespace DaAPI.UnitTests.Host.ApiControllers
{
    public class LocalUserControllerTester
    {
        [Fact]
        public async Task GetAllLocalUsers()
        {
            List<LocalUserOverview> expectedResult = new List<LocalUserOverview>();

            Mock<ILocalUserService> userServiceMock = new Mock<ILocalUserService>(MockBehavior.Strict);
            userServiceMock.Setup(x => x.GetAllUsersSortedByName()).ReturnsAsync(expectedResult).Verifiable();

            var controller = new LocalUserController(
                Mock.Of<IMediator>(MockBehavior.Strict),
                userServiceMock.Object,
                Mock.Of<ILogger<LocalUserController>>()
                );

            var actionResult = await controller.GetAllLocalUsers();
            var actual = actionResult.EnsureOkObjectResult<IEnumerable<LocalUserOverview>>(true);
            Assert.Equal(expectedResult, actual);

            userServiceMock.Verify();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ResetUserPassword(Boolean mediatorResult)
        {
            Random random = new Random();
            String userId = random.GetAlphanumericString(5);
            String password = random.GetAlphanumericString(15);

            var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);
            mediatorMock.Setup(x => x.Send(It.Is<ResetLocalUserPasswordCommand>(y =>
           y.UserId == userId &&
           y.Password == password), It.IsAny<CancellationToken>())).ReturnsAsync(mediatorResult).Verifiable();

            var controller = new LocalUserController(
                mediatorMock.Object,
                Mock.Of<ILocalUserService>(MockBehavior.Strict),
                Mock.Of<ILogger<LocalUserController>>()
                );

            var actionResult = await controller.ResetUserPassword(userId, new ResetPasswordRequest { Password = password });
            if (mediatorResult == true)
            {
                actionResult.EnsureNoContentResult();
            }
            else
            {
                actionResult.EnsureBadRequestObjectResult("unable to complete service operation");
            }

            mediatorMock.Verify();
        }

        private async Task CheckModelState(Func<LocalUserController,  Task<IActionResult>> controllerExecuter)
        {
            Random random = new Random();

            var controller = new LocalUserController(
                Mock.Of<IMediator>(MockBehavior.Strict),
                Mock.Of<ILocalUserService>(MockBehavior.Strict),
                Mock.Of<ILogger<LocalUserController>>()
                );

            String modelErrorKey = "a" + random.GetAlphanumericString();
            String modelErrorMessage = random.GetAlphanumericString();
            controller.ModelState.AddModelError(modelErrorKey, modelErrorMessage);

            var result = await controllerExecuter(controller);

            result.EnsureBadRequestObjectResultForError(modelErrorKey, modelErrorMessage);
        }

        [Fact]
        public async Task ResetUserPassword_ModelStateError()
        {
            await CheckModelState((controller) => controller.ResetUserPassword("",null));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task DeleteUser(Boolean mediatorResult)
        {
            Random random = new Random();
            String userId = random.GetAlphanumericString(5);

            var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);
            mediatorMock.Setup(x => x.Send(It.Is<DeleteLocalUserCommand>(y =>
           y.UserId == userId), It.IsAny<CancellationToken>())).ReturnsAsync(mediatorResult).Verifiable();

            var controller = new LocalUserController(
                mediatorMock.Object,
                Mock.Of<ILocalUserService>(MockBehavior.Strict),
                Mock.Of<ILogger<LocalUserController>>()
                );

            var actionResult = await controller.DeleteUser(userId);
            if (mediatorResult == true)
            {
                actionResult.EnsureNoContentResult();
            }
            else
            {
                actionResult.EnsureBadRequestObjectResult("unable to complete service operation");
            }

            mediatorMock.Verify();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task CreateUser(Boolean mediatorResult)
        {
            Random random = new Random();
            String userId = mediatorResult == true ? random.GetAlphanumericString(5) : null;

            String username = random.GetAlphanumericString();
            String password = random.GetAlphanumericString();

            var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);
            mediatorMock.Setup(x => x.Send(It.Is<CreateLocalUserCommand>(y =>
           y.Username == username &&
           y.Password == password), It.IsAny<CancellationToken>())).ReturnsAsync(userId).Verifiable();

            var controller = new LocalUserController(
                mediatorMock.Object,
                Mock.Of<ILocalUserService>(MockBehavior.Strict),
                Mock.Of<ILogger<LocalUserController>>()
                );

            var actionResult = await controller.CreateUser(new CreateUserRequest { Username = username,  Password = password });
            if (mediatorResult == true)
            {
                String actualId = actionResult.EnsureOkObjectResult<String>(true);
                Assert.Equal(userId, actualId);
            }
            else
            {
                actionResult.EnsureBadRequestObjectResult("unable to complete service operation");
            }

            mediatorMock.Verify();
        }

        [Fact]
        public async Task CreateUser_ModelStateError()
        {
            await CheckModelState((controller) => controller.CreateUser(null));
        }

    }
}
