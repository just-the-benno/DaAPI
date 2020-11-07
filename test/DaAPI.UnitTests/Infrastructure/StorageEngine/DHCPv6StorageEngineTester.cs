using DaAPI.Core.Common;
using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Listeners;
using DaAPI.Core.Packets.DHCPv6;
using DaAPI.Core.Scopes;
using DaAPI.Core.Scopes.DHCPv6;
using DaAPI.Infrastructure.StorageEngine.DHCPv6;
using DaAPI.TestHelper;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DaAPI.UnitTests.Infrastructure.StorageEngine
{
    public class DHCPv6StorageEngineTester
    {
        [Fact]
        public async Task GetDHCPv6Listener()
        {
            List<DHCPv6Listener> listeners = new List<DHCPv6Listener>();

            Mock<IDHCPv6ReadStore> readStoreMock = new Mock<IDHCPv6ReadStore>(MockBehavior.Strict);
            readStoreMock.Setup(x => x.GetDHCPv6Listener()).ReturnsAsync(listeners).Verifiable();

            Mock<ILoggerFactory> factoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);
            factoryMock.Setup(x => x.CreateLogger(It.IsAny<String>())).Returns(Mock.Of<ILogger<DHCPv6RootScope>>());

            Mock<IServiceProvider> serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(x => x.GetService(typeof(IDHCPv6ReadStore))).Returns(readStoreMock.Object);
            serviceProviderMock.Setup(x => x.GetService(typeof(ILoggerFactory))).Returns(factoryMock.Object);
            serviceProviderMock.Setup(x => x.GetService(typeof(IDHCPv6EventStore))).Returns(Mock.Of<IDHCPv6EventStore>());

            DHCPv6StorageEngine storageEngine = new DHCPv6StorageEngine(serviceProviderMock.Object);

            var actual = await storageEngine.GetDHCPv6Listener();
            Assert.Equal(listeners, actual);

            readStoreMock.Verify();
        }

        [Theory]
        [InlineData(false, true, false)]
        [InlineData(false, false, false)]
        [InlineData(true, false, false)]
        [InlineData(true, true, true)]
        public async Task Save(Boolean eventStoreResult, Boolean projectionResult, Boolean expectedResult)
        {
            List<DummyEvent> events = new List<DummyEvent>();
            for (int i = 0; i < 10; i++)
            {
                events.Add(new DummyEvent());
            }

            var mockedAggregateRoot = new MockedAggregateRoot(events);

            Mock<IDHCPv6EventStore> eventStoreMock = new Mock<IDHCPv6EventStore>(MockBehavior.Strict);
            eventStoreMock.Setup(x => x.Save(mockedAggregateRoot)).ReturnsAsync(eventStoreResult).Verifiable();

            var comparer = new DummyEventEqualityCompare();

            Mock<IDHCPv6ReadStore> readStoreMock = new Mock<IDHCPv6ReadStore>(MockBehavior.Strict);
            if (eventStoreResult == true)
            {
                readStoreMock.Setup(x => x.Project(It.Is<IEnumerable<DomainEvent>>(y =>
                    comparer.Equals(events, y.Cast<DummyEvent>())
                ))).ReturnsAsync(projectionResult).Verifiable();
            }

            Mock<ILoggerFactory> factoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);
            factoryMock.Setup(x => x.CreateLogger(It.IsAny<String>())).Returns(Mock.Of<ILogger<DHCPv6RootScope>>());

            Mock<IServiceProvider> serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(x => x.GetService(typeof(IDHCPv6ReadStore))).Returns(readStoreMock.Object);
            serviceProviderMock.Setup(x => x.GetService(typeof(ILoggerFactory))).Returns(factoryMock.Object);
            serviceProviderMock.Setup(x => x.GetService(typeof(IDHCPv6EventStore))).Returns(eventStoreMock.Object);

            DHCPv6StorageEngine storageEngine = new DHCPv6StorageEngine(serviceProviderMock.Object);

            Boolean actual = await storageEngine.Save(mockedAggregateRoot);
            Assert.Equal(expectedResult, actual);

            eventStoreMock.Verify();
            if (eventStoreResult == false)
            {
                readStoreMock.Verify();
            }
        }


        [Fact]
        public async Task GetRootScope()
        {
            var @event = new DHCPv6ScopeEvents.DHCPv6ScopeAddedEvent(
                new DHCPv6ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv6ScopeAddressProperties(
                        IPv6Address.FromString("fe80::0"),
                        IPv6Address.FromString("fe80::ff"),
                        new List<IPv6Address> { IPv6Address.FromString("fe80::1") },
                        preferredLifeTime: TimeSpan.FromDays(0.5),
                        validLifeTime: TimeSpan.FromDays(1),
                        rapitCommitEnabled: true,
                        addressAllocationStrategy: DHCPv6ScopeAddressProperties.AddressAllocationStrategies.Next),
                    ResolverInformation = null,
                    Name = "Testscope",
                    Id = Guid.NewGuid(),
                });

            Mock<IDHCPv6EventStore> eventStoreMock = new Mock<IDHCPv6EventStore>(MockBehavior.Strict);
            eventStoreMock.Setup(x => x.GetEvents("DHCPv6RootScope")).ReturnsAsync(new DomainEvent[] { @event }).Verifiable();
            eventStoreMock.Setup(x => x.Exists()).ReturnsAsync(true).Verifiable();

            Mock<ILoggerFactory> factoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);
            factoryMock.Setup(x => x.CreateLogger(It.IsAny<String>())).Returns(Mock.Of<ILogger<DHCPv6RootScope>>());

            Mock<IServiceProvider> serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(x => x.GetService(typeof(IDHCPv6ReadStore))).Returns(Mock.Of<IDHCPv6ReadStore>(MockBehavior.Strict));
            serviceProviderMock.Setup(x => x.GetService(typeof(ILoggerFactory))).Returns(factoryMock.Object);
            serviceProviderMock.Setup(x => x.GetService(typeof(IDHCPv6EventStore))).Returns(eventStoreMock.Object);

            DHCPv6StorageEngine storageEngine = new DHCPv6StorageEngine(serviceProviderMock.Object);

            IScopeResolver<DHCPv6Packet, IPv6Address> resolver = null;
            var resolverManagerMock = new Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>>(MockBehavior.Strict);
            resolverManagerMock.Setup(x => x.InitializeResolver(null)).Returns(resolver);

            var actual = await storageEngine.GetRootScope(resolverManagerMock.Object);

            Assert.NotNull(actual);
            Assert.Single(actual.GetRootScopes());

            eventStoreMock.Verify();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task DeleteAggregateRoot(Boolean storeResult)
        {
            MockedAggregateRoot mockedAggregateRoot = new MockedAggregateRoot(null);

            Mock<IDHCPv6EventStore> eventStoreMock = new Mock<IDHCPv6EventStore>(MockBehavior.Strict);
            eventStoreMock.Setup(x => x.DeleteAggregateRoot<MockedAggregateRoot>(mockedAggregateRoot.Id)).ReturnsAsync(storeResult).Verifiable();

            Mock<ILoggerFactory> factoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);
            factoryMock.Setup(x => x.CreateLogger(It.IsAny<String>())).Returns(Mock.Of<ILogger<DHCPv6RootScope>>());

            Mock<IServiceProvider> serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(x => x.GetService(typeof(IDHCPv6ReadStore))).Returns(Mock.Of<IDHCPv6ReadStore>());
            serviceProviderMock.Setup(x => x.GetService(typeof(IDHCPv6EventStore))).Returns(eventStoreMock.Object);

            serviceProviderMock.Setup(x => x.GetService(typeof(ILoggerFactory))).Returns(factoryMock.Object);

            DHCPv6StorageEngine storageEngine = new DHCPv6StorageEngine(serviceProviderMock.Object);

            var actual = await storageEngine.DeleteAggregateRoot<MockedAggregateRoot>(mockedAggregateRoot.Id);
            Assert.Equal(actual, storeResult);

            eventStoreMock.Verify();
        }
    }
}
