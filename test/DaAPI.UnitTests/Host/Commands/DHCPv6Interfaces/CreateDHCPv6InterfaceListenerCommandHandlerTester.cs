using Castle.Core.Logging;
using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Listeners;
using DaAPI.Host.Application.Commands.DHCPv6Interfaces;
using DaAPI.Infrastructure.InterfaceEngines;
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

namespace DaAPI.UnitTests.Host.Commands.DHCPv6Interfaces
{
    public class CreateDHCPv6InterfaceListenerCommandHandlerTester
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
        public async Task Handle()
        {
            Random random = new Random();
            String interfaceName = random.GetAlphanumericString();

            var possibleListeners = GetPossibleListeners();

            var selectedListener = possibleListeners.ElementAt(1);
            var command = new CreateDHCPv6InterfaceListenerCommand(
                selectedListener.PhysicalInterfaceId,
                selectedListener.Address.ToString(), interfaceName);

            Mock<IDHCPv6InterfaceEngine> interfaceEngineMock = new Mock<IDHCPv6InterfaceEngine>(MockBehavior.Strict);
            interfaceEngineMock.Setup(x => x.GetPossibleListeners()).Returns(possibleListeners).Verifiable();
            interfaceEngineMock.Setup(x => x.GetActiveListeners()).ReturnsAsync(possibleListeners.Take(1)).Verifiable();
            interfaceEngineMock.Setup(x => x.OpenListener(It.Is<DHCPv6Listener>(y =>
            y.Address == IPv6Address.FromString(command.IPv6Addres)))).Returns(true).Verifiable();

            Mock<IDHCPv6StorageEngine> storageEngineMock = new Mock<IDHCPv6StorageEngine>(MockBehavior.Strict);
            storageEngineMock.Setup(x => x.Save(It.Is<DHCPv6Listener>(y =>
            y.Name == interfaceName && y.PhysicalInterfaceId == command.NicId
            && y.Address == IPv6Address.FromString(command.IPv6Addres)
            ))).ReturnsAsync(true).Verifiable();

            var commandHandler = new CreateDHCPv6InterfaceListenerCommandHandler(
                interfaceEngineMock.Object, storageEngineMock.Object,
                Mock.Of<ILogger<CreateDHCPv6InterfaceListenerCommandHandler>>());

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
            var command = new CreateDHCPv6InterfaceListenerCommand(
                selectedListener.PhysicalInterfaceId,
                random.GetIPv6Address().ToString(), interfaceName);

            Mock<IDHCPv6InterfaceEngine> interfaceEngineMock = new Mock<IDHCPv6InterfaceEngine>(MockBehavior.Strict);
            interfaceEngineMock.Setup(x => x.GetPossibleListeners()).Returns(possibleListeners).Verifiable();

            var commandHandler = new CreateDHCPv6InterfaceListenerCommandHandler(
                interfaceEngineMock.Object, Mock.Of<IDHCPv6StorageEngine>(MockBehavior.Strict),
                Mock.Of<ILogger<CreateDHCPv6InterfaceListenerCommandHandler>>());

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
            var command = new CreateDHCPv6InterfaceListenerCommand(
                selectedListener.PhysicalInterfaceId,
                selectedListener.Address.ToString(), interfaceName);

            Mock<IDHCPv6InterfaceEngine> interfaceEngineMock = new Mock<IDHCPv6InterfaceEngine>(MockBehavior.Strict);
            interfaceEngineMock.Setup(x => x.GetPossibleListeners()).Returns(possibleListeners).Verifiable();
            interfaceEngineMock.Setup(x => x.GetActiveListeners()).ReturnsAsync(possibleListeners.Take(1)).Verifiable();
            
            var commandHandler = new CreateDHCPv6InterfaceListenerCommandHandler(
                interfaceEngineMock.Object, Mock.Of<IDHCPv6StorageEngine>(MockBehavior.Strict),
                Mock.Of<ILogger<CreateDHCPv6InterfaceListenerCommandHandler>>());

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
            var command = new CreateDHCPv6InterfaceListenerCommand(
                selectedListener.PhysicalInterfaceId,
                selectedListener.Address.ToString(), interfaceName);

            Mock<IDHCPv6InterfaceEngine> interfaceEngineMock = new Mock<IDHCPv6InterfaceEngine>(MockBehavior.Strict);
            interfaceEngineMock.Setup(x => x.GetPossibleListeners()).Returns(possibleListeners).Verifiable();
            interfaceEngineMock.Setup(x => x.GetActiveListeners()).ReturnsAsync(possibleListeners.Take(1)).Verifiable();

            Mock<IDHCPv6StorageEngine> storageEngineMock = new Mock<IDHCPv6StorageEngine>(MockBehavior.Strict);
            storageEngineMock.Setup(x => x.Save(It.Is<DHCPv6Listener>(y =>
            y.Name == interfaceName && y.PhysicalInterfaceId == command.NicId
            && y.Address == IPv6Address.FromString(command.IPv6Addres)
            ))).ReturnsAsync(false).Verifiable();

            var commandHandler = new CreateDHCPv6InterfaceListenerCommandHandler(
                interfaceEngineMock.Object, storageEngineMock.Object,
                Mock.Of<ILogger<CreateDHCPv6InterfaceListenerCommandHandler>>());

            Guid? result = await commandHandler.Handle(command, CancellationToken.None);

            Assert.False(result.HasValue);

            interfaceEngineMock.Verify();
            storageEngineMock.Verify();
        }
    }
}
