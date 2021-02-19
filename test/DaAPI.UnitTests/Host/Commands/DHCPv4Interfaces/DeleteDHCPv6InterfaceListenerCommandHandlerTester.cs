using Castle.Core.Logging;
using DaAPI.Core.Common;
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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using static DaAPI.Core.Listeners.DHCPListenerEvents;

namespace DaAPI.UnitTests.Host.Commands.DHCPv4Interfaces
{
    public class DeleteDHCPv4InterfaceListenerCommandHandlerTester
    {
        [Fact]
        public async Task Handle()
        {
            Random random = new Random();
            Guid id = random.NextGuid();

            DHCPv4Listener listener = new DHCPv4Listener();
            listener.Load(new DomainEvent[] {
                new DHCPv4ListenerCreatedEvent
                {
                    Id = id,
                    Address = random.GetIPv4Address().ToString(),
                }
            });

            var command = new DeleteDHCPv4InterfaceListenerCommand(id);

            Mock<IDHCPv4StorageEngine> storageMock = new Mock<IDHCPv4StorageEngine>(MockBehavior.Strict);
            storageMock.Setup(x => x.CheckIfAggrerootExists<DHCPv4Listener>(id)).ReturnsAsync(true).Verifiable();
            storageMock.Setup(x => x.GetAggregateRoot<DHCPv4Listener>(id)).ReturnsAsync(listener).Verifiable();
            storageMock.Setup(x => x.Save(listener)).ReturnsAsync(true).Verifiable();

            Mock<IDHCPv4InterfaceEngine> interfaceEngineMock = new Mock<IDHCPv4InterfaceEngine>(MockBehavior.Strict);
            interfaceEngineMock.Setup(x => x.CloseListener(listener)).Returns(true).Verifiable();

            var handler = new DeleteDHCPv4InterfaceListenerCommandHandler(
                interfaceEngineMock.Object, storageMock.Object, Mock.Of<ILogger<DeleteDHCPv4InterfaceListenerCommandHandler>>());

            Boolean actual = await handler.Handle(command, CancellationToken.None);

            Assert.True(actual);

            storageMock.Verify();
            interfaceEngineMock.Verify();
        }

        [Fact]
        public async Task Handle_Failed_NotFound()
        {
            Random random = new Random();
            Guid id = random.NextGuid();

            DHCPv4Listener listener = new DHCPv4Listener();
            listener.Load(new DomainEvent[] {
                new DHCPv4ListenerCreatedEvent
                {
                    Id = id,
                    Address = random.GetIPv4Address().ToString(),
                }
            });

            var command = new DeleteDHCPv4InterfaceListenerCommand(id);

            Mock<IDHCPv4StorageEngine> storageMock = new Mock<IDHCPv4StorageEngine>(MockBehavior.Strict);
            storageMock.Setup(x => x.CheckIfAggrerootExists<DHCPv4Listener>(id)).ReturnsAsync(false).Verifiable();

            var handler = new DeleteDHCPv4InterfaceListenerCommandHandler(
                Mock.Of<IDHCPv4InterfaceEngine>(MockBehavior.Strict), storageMock.Object, Mock.Of<ILogger<DeleteDHCPv4InterfaceListenerCommandHandler>>());

            Boolean actual = await handler.Handle(command, CancellationToken.None);

            Assert.False(actual);

            storageMock.Verify();
        }

        [Fact]
        public async Task Handle_Failed_NotSaved()
        {
            Random random = new Random();
            Guid id = random.NextGuid();

            DHCPv4Listener listener = new DHCPv4Listener();
            listener.Load(new DomainEvent[] {
                new DHCPv4ListenerCreatedEvent
                {
                    Id = id,
                    Address = random.GetIPv4Address().ToString(),
                }
            });

            var command = new DeleteDHCPv4InterfaceListenerCommand(id);

            Mock<IDHCPv4StorageEngine> storageMock = new Mock<IDHCPv4StorageEngine>(MockBehavior.Strict);
            storageMock.Setup(x => x.CheckIfAggrerootExists<DHCPv4Listener>(id)).ReturnsAsync(true).Verifiable();
            storageMock.Setup(x => x.GetAggregateRoot<DHCPv4Listener>(id)).ReturnsAsync(listener).Verifiable();
            storageMock.Setup(x => x.Save(listener)).ReturnsAsync(false).Verifiable();

            var handler = new DeleteDHCPv4InterfaceListenerCommandHandler(
                Mock.Of<IDHCPv4InterfaceEngine>(MockBehavior.Strict), storageMock.Object, Mock.Of<ILogger<DeleteDHCPv4InterfaceListenerCommandHandler>>());

            Boolean actual = await handler.Handle(command, CancellationToken.None);

            Assert.False(actual);

            storageMock.Verify();
        }
    }
}
