using DaAPI.Core.Listeners;
using DaAPI.Host.ApiControllers;
using DaAPI.Host.Application.Commands.DHCPv6Interfaces;
using DaAPI.Infrastructure.InterfaceEngines;
using DaAPI.TestHelper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using static DaAPI.Shared.Requests.DHCPv6InterfaceRequests.V1;
using static DaAPI.Shared.Responses.DHCPv6InterfaceResponses.V1;

namespace DaAPI.UnitTests.Host.ApiControllers
{
    public class DHCPv6InterfaceControllerTester
    {
        public IEnumerable<DHCPv6Listener> GetPossibleListeners()
        {
            List<DHCPv6Listener> result = new List<DHCPv6Listener>();

            foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                var properites = nic.GetIPProperties();
                if (properites == null) { continue; }

                foreach (var ipAddress in properites.UnicastAddresses)
                {
                    if (ipAddress.Address.AddressFamily != System.Net.Sockets.AddressFamily.InterNetworkV6)
                    {
                        continue;
                    }

                    if (ipAddress.Address.IsIPv6LinkLocal == true)
                    {
                        continue;
                    }

                    DHCPv6Listener listener = DHCPv6Listener.FromNIC(nic, ipAddress.Address);
                    result.Add(listener);
                }
            }

            return result;
        }

        [Fact]
        public async Task GetAllInterface()
        {
            var nicListeners = GetPossibleListeners();
            var activeListeners = new List<DHCPv6Listener>();

            Mock<IDHCPv6InterfaceEngine> interfaceEngineMock = new Mock<IDHCPv6InterfaceEngine>(MockBehavior.Strict);

            interfaceEngineMock.Setup(x => x.GetPossibleListeners()).Returns(nicListeners).Verifiable();
            interfaceEngineMock.Setup(x => x.GetActiveListeners()).ReturnsAsync(activeListeners).Verifiable();

            var controller = new DHCPv6InterfaceController(Mock.Of<IMediator>(MockBehavior.Strict), interfaceEngineMock.Object);

            var actionResult = await controller.GetAllInterface();
            var result = actionResult.EnsureOkObjectResult<DHCPv6InterfaceOverview>(true);

            interfaceEngineMock.Verify();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task CreateListener(Boolean successfullMediatorResult)
        {
            Random random = new Random();
            String ipv6Address = random.GetIPv6Address().ToString();
            String name = random.GetAlphanumericString();
            String interfaceId = random.NextGuid().ToString();

            Guid? systemId = successfullMediatorResult == true ? random.NextGuid() : new Guid?();

            Mock<IMediator> mediatorMock = new Mock<IMediator>(MockBehavior.Strict);
            mediatorMock.Setup(x => x.Send(It.Is<CreateDHCPv6InterfaceListenerCommand>(y =>
            y.IPv6Addres == ipv6Address &&
            y.Name == name &&
            y.NicId == interfaceId
            ), It.IsAny<CancellationToken>())).ReturnsAsync(systemId).Verifiable();

            var controller = new DHCPv6InterfaceController(mediatorMock.Object, Mock.Of<IDHCPv6InterfaceEngine>(MockBehavior.Strict));

            var actionResult = await controller.CreateListener(new CreateDHCPv6Listener
            {
                InterfaceId = interfaceId,
                IPv6Address = ipv6Address,
                Name = name,
            });

            if (successfullMediatorResult == true)
            {
                Guid actual = actionResult.EnsureOkObjectResult<Guid>(true);
                Assert.Equal(systemId.Value, actual);
            }
            else
            {
                actionResult.EnsureBadRequestObjectResult("unable to create listener");
            }

            mediatorMock.Verify();
        }

        private async Task CheckModelState(Func<DHCPv6InterfaceController, Task<IActionResult>> controllerExecuter)
        {
            Random random = new Random();

            var controller = new DHCPv6InterfaceController(
                Mock.Of<IMediator>(MockBehavior.Strict),
                Mock.Of<IDHCPv6InterfaceEngine>(MockBehavior.Strict)
                //Mock.Of<ILogger<LocalUserController>>()
                );

            String modelErrorKey = "a" + random.GetAlphanumericString();
            String modelErrorMessage = random.GetAlphanumericString();
            controller.ModelState.AddModelError(modelErrorKey, modelErrorMessage);

            var result = await controllerExecuter(controller);

            result.EnsureBadRequestObjectResultForError(modelErrorKey, modelErrorMessage);
        }

        [Fact]
        public async Task CreateUser_ModelStateError()
        {
            await CheckModelState((controller) => controller.CreateListener(null));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task DeleteListener(Boolean mediatorResult)
        {
            Random random = new Random();
            Guid systemId = random.NextGuid();

            Mock<IMediator> mediatorMock = new Mock<IMediator>(MockBehavior.Strict);
            mediatorMock.Setup(x => x.Send(It.Is<DeleteDHCPv6InterfaceListenerCommand>(y =>
            y.Id == systemId
            ), It.IsAny<CancellationToken>())).ReturnsAsync(mediatorResult).Verifiable();

            var controller = new DHCPv6InterfaceController(mediatorMock.Object, Mock.Of<IDHCPv6InterfaceEngine>(MockBehavior.Strict));

            var actionResult = await controller.DeleteListener(systemId);
            
            if (mediatorResult == true)
            {
                Boolean actual = actionResult.EnsureOkObjectResult<Boolean>(true);
                Assert.True(actual);
            }
            else
            {
                actionResult.EnsureBadRequestObjectResult("unable to delete listener");
            }

            mediatorMock.Verify();
        }

    }
}
