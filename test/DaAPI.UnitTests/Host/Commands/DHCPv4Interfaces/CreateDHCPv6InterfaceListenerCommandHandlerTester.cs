using Castle.Core.Logging;
using DaAPI.Core.Common;
using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Listeners;
using DaAPI.Host.Application.Commands.DHCPv4Interfaces;
using DaAPI.Host.Application.Commands.DHCPv6Interfaces;
using DaAPI.Infrastructure.InterfaceEngines;
using DaAPI.Infrastructure.StorageEngine.DHCPv4;
using DaAPI.Infrastructure.StorageEngine.DHCPv6;
using DaAPI.TestHelper;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace DaAPI.UnitTests.Host.Commands.DHCPv4Interfaces
{
    public class CreateDHCPv4InterfaceListenerCommandHandlerTester
    {
        public IEnumerable<DHCPv4Listener> GetPossibleListeners()
        {
            List<DHCPv4Listener> result = new List<DHCPv4Listener>();

            foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                var properites = nic.GetIPProperties();
                if (properites == null) { continue; }

                foreach (var ipAddress in properites.UnicastAddresses)
                {
                    if (ipAddress.Address.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        continue;
                    }

                    DHCPv4Listener listener = DHCPv4Listener.FromNIC(nic, ipAddress.Address);
                    result.Add(listener);
                }
            }

            return result;
        }

        [Fact]
        public async Task Handle()
        {
            Random random = new Random();
            String interfaceName = random.GetAlphanumericString();

            var possibleListeners = GetPossibleListeners();

            var selectedListener = possibleListeners.ElementAt(1);
            var command = new CreateDHCPv4InterfaceListenerCommand(
                selectedListener.PhysicalInterfaceId,
                selectedListener.Address.ToString(), interfaceName);

            Mock<IDHCPv4InterfaceEngine> interfaceEngineMock = new Mock<IDHCPv4InterfaceEngine>(MockBehavior.Strict);
            interfaceEngineMock.Setup(x => x.GetPossibleListeners()).Returns(possibleListeners).Verifiable();
            interfaceEngineMock.Setup(x => x.GetActiveListeners()).ReturnsAsync(possibleListeners.Take(1)).Verifiable();
            interfaceEngineMock.Setup(x => x.OpenListener(It.Is<DHCPv4Listener>(y =>
            y.Address == IPv4Address.FromString(command.IPv4Addres)))).Returns(true).Verifiable();

            Mock<IDHCPv4StorageEngine> storageEngineMock = new Mock<IDHCPv4StorageEngine>(MockBehavior.Strict);
            storageEngineMock.Setup(x => x.Save(It.Is<DHCPv4Listener>(y =>
            y.Name == interfaceName && y.PhysicalInterfaceId == command.NicId
            && y.Address == IPv4Address.FromString(command.IPv4Addres)
            ))).ReturnsAsync(true).Verifiable();

            var commandHandler = new CreateDHCPv4InterfaceListenerCommandHandler(
                interfaceEngineMock.Object, storageEngineMock.Object,
                Mock.Of<ILogger<CreateDHCPv4InterfaceListenerCommandHandler>>());

            Guid? result = await commandHandler.Handle(command, CancellationToken.None);

            Assert.True(result.HasValue);
            Assert.NotEqual(Guid.Empty, result.Value);

            interfaceEngineMock.Verify();
            storageEngineMock.Verify();
        }

        [Fact]
        public async Task Handle_InterfaceNotFound()
        {
            Random random = new Random();
            String interfaceName = random.GetAlphanumericString();

            var possibleListeners = GetPossibleListeners();

            var selectedListener = possibleListeners.ElementAt(1);
            var command = new CreateDHCPv4InterfaceListenerCommand(
                selectedListener.PhysicalInterfaceId,
                random.GetIPv4Address().ToString(), interfaceName);

            Mock<IDHCPv4InterfaceEngine> interfaceEngineMock = new Mock<IDHCPv4InterfaceEngine>(MockBehavior.Strict);
            interfaceEngineMock.Setup(x => x.GetPossibleListeners()).Returns(possibleListeners).Verifiable();

            var commandHandler = new CreateDHCPv4InterfaceListenerCommandHandler(
                interfaceEngineMock.Object, Mock.Of<IDHCPv4StorageEngine>(MockBehavior.Strict),
                Mock.Of<ILogger<CreateDHCPv4InterfaceListenerCommandHandler>>());

            Guid? result = await commandHandler.Handle(command, CancellationToken.None);

            Assert.False(result.HasValue);

            interfaceEngineMock.Verify();
        }

        [Fact]
        public async Task Handle_IsAlreadyActive()
        {
            Random random = new Random();
            String interfaceName = random.GetAlphanumericString();

            var possibleListeners = GetPossibleListeners();

            var selectedListener = possibleListeners.ElementAt(0);
            var command = new CreateDHCPv4InterfaceListenerCommand(
                selectedListener.PhysicalInterfaceId,
                selectedListener.Address.ToString(), interfaceName);

            Mock<IDHCPv4InterfaceEngine> interfaceEngineMock = new Mock<IDHCPv4InterfaceEngine>(MockBehavior.Strict);
            interfaceEngineMock.Setup(x => x.GetPossibleListeners()).Returns(possibleListeners).Verifiable();
            interfaceEngineMock.Setup(x => x.GetActiveListeners()).ReturnsAsync(possibleListeners.Take(1)).Verifiable();
            
            var commandHandler = new CreateDHCPv4InterfaceListenerCommandHandler(
                interfaceEngineMock.Object, Mock.Of<IDHCPv4StorageEngine>(MockBehavior.Strict),
                Mock.Of<ILogger<CreateDHCPv4InterfaceListenerCommandHandler>>());

            Guid? result = await commandHandler.Handle(command, CancellationToken.None);

            Assert.False(result.HasValue);

            interfaceEngineMock.Verify();
        }

        [Fact]
        public async Task Handle_Failed_StorageEngine()
        {
            Random random = new Random();
            String interfaceName = random.GetAlphanumericString();

            var possibleListeners = GetPossibleListeners();

            var selectedListener = possibleListeners.ElementAt(1);
            var command = new CreateDHCPv4InterfaceListenerCommand(
                selectedListener.PhysicalInterfaceId,
                selectedListener.Address.ToString(), interfaceName);

            Mock<IDHCPv4InterfaceEngine> interfaceEngineMock = new Mock<IDHCPv4InterfaceEngine>(MockBehavior.Strict);
            interfaceEngineMock.Setup(x => x.GetPossibleListeners()).Returns(possibleListeners).Verifiable();
            interfaceEngineMock.Setup(x => x.GetActiveListeners()).ReturnsAsync(possibleListeners.Take(1)).Verifiable();

            Mock<IDHCPv4StorageEngine> storageEngineMock = new Mock<IDHCPv4StorageEngine>(MockBehavior.Strict);
            storageEngineMock.Setup(x => x.Save(It.Is<DHCPv4Listener>(y =>
            y.Name == interfaceName && y.PhysicalInterfaceId == command.NicId
            && y.Address == IPv4Address.FromString(command.IPv4Addres)
            ))).ReturnsAsync(false).Verifiable();

            var commandHandler = new CreateDHCPv4InterfaceListenerCommandHandler(
                interfaceEngineMock.Object, storageEngineMock.Object,
                Mock.Of<ILogger<CreateDHCPv4InterfaceListenerCommandHandler>>());

            Guid? result = await commandHandler.Handle(command, CancellationToken.None);

            Assert.False(result.HasValue);

            interfaceEngineMock.Verify();
            storageEngineMock.Verify();
        }
    }
}
