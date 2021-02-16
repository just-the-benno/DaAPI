using DaAPI.Core.Common;
using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Packets.DHCPv6;
using DaAPI.Core.Scopes;
using DaAPI.Core.Scopes.DHCPv6;
using DaAPI.Host.ApiControllers;
using DaAPI.Infrastructure.StorageEngine.DHCPv6;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using DaAPI.TestHelper;
using static DaAPI.Shared.Requests.StatisticsControllerRequests.V1;
using static DaAPI.Shared.Responses.StatisticsControllerResponses.V1;
using static DaAPI.Core.Scopes.DHCPv6.DHCPv6ScopeEvents;
using DaAPI.Infrastructure.NotificationEngine;

namespace DaAPI.UnitTests.Host.ApiControllers
{
    public class StatisticsControllerTester
    {
        protected DHCPv6RootScope GetRootScope()
        {
            Mock<ILoggerFactory> factoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);
            factoryMock.Setup(x => x.CreateLogger(It.IsAny<String>())).Returns(Mock.Of<ILogger<DHCPv6RootScope>>());

            var scope = new DHCPv6RootScope(Guid.NewGuid(), Mock.Of<IScopeResolverManager<DHCPv6Packet, IPv6Address>>(), factoryMock.Object);

            return scope;
        }

        [Fact]
        public async Task GetDashboard()
        {
            Random random = new Random();
            Guid grantParentScopeId = random.NextGuid();
            Guid parentScopeId = random.NextGuid();
            Guid childScopeId = random.NextGuid();

            DHCPv6RootScope rootScope = GetRootScope();
            rootScope.Load(new List<DomainEvent>
            {
                new DHCPv6ScopeEvents.DHCPv6ScopeAddedEvent(
                new DHCPv6ScopeCreateInstruction
                {
                    Name = "grant parent",
                    Id = grantParentScopeId,
                }),
                new DHCPv6ScopeEvents.DHCPv6ScopeAddedEvent(
                new DHCPv6ScopeCreateInstruction
                {
                    Name = "parent",
                    Id = parentScopeId,
                    ParentId = grantParentScopeId
                }),
                new DHCPv6ScopeEvents.DHCPv6ScopeAddedEvent(
                new DHCPv6ScopeCreateInstruction
                {
                    Name = "child",
                    Id = childScopeId,
                    ParentId = parentScopeId
                }),
            });

            DashboardResponse response = new DashboardResponse
            {
                DHCPv6 = new DHCPOverview<DHCPv6LeaseEntry, DHCPv6PacketHandledEntry>
                {
                    ActiveInterfaces = random.Next(3, 10),
                },
            };

            Int32 expectedPipelineAmount = random.Next(3, 10);

            Mock<IDHCPv6ReadStore> readStoreMock = new Mock<IDHCPv6ReadStore>(MockBehavior.Strict);
            readStoreMock.Setup(x => x.GetDashboardOverview()).ReturnsAsync(response).Verifiable();

            Mock<INotificationEngine> notificationEngineMock = new Mock<INotificationEngine>(MockBehavior.Strict);
            notificationEngineMock.Setup(x => x.GetPipelineAmount()).Returns(expectedPipelineAmount).Verifiable();

            var controller = new DHCPv6StatisticsController(rootScope, readStoreMock.Object, notificationEngineMock.Object);
            var actionResult = await controller.GetDashboard();

            var result = actionResult.EnsureOkObjectResult<DashboardResponse>(true);
            Assert.NotNull(result);
            Assert.NotNull(result.DHCPv6);

            Assert.Equal(3, result.DHCPv6.ScopeAmount);
            Assert.Equal(response.DHCPv6.ActiveInterfaces, result.DHCPv6.ActiveInterfaces);
            Assert.Equal(expectedPipelineAmount, result.AmountOfPipelines);

            readStoreMock.Verify();
        }

        [Fact]
        public async Task GetIncomingDHCPv6PacketTypes()
        {
            Random random = new Random();

            DHCPv6RootScope rootScope = GetRootScope();

            var response = new Dictionary<DateTime, IDictionary<DHCPv6PacketTypes, Int32>>();
            DateTime start = DateTime.UtcNow.AddHours(-random.Next(5, 10));
            DateTime end = DateTime.UtcNow.AddHours(-random.Next(1, 2));
            GroupStatisticsResultBy groupBy = random.GetEnumValue<GroupStatisticsResultBy>();

            Mock<IDHCPv6ReadStore> readStoreMock = new Mock<IDHCPv6ReadStore>(MockBehavior.Strict);
            readStoreMock.Setup(x => x.GetIncomingDHCPv6PacketTypes(start, end, groupBy)).ReturnsAsync(response).Verifiable();

            var controller = new DHCPv6StatisticsController(rootScope, readStoreMock.Object, Mock.Of<INotificationEngine>(MockBehavior.Strict));
            var actionResult = await controller.GetIncomingDHCPv6PacketTypes(new GroupedTimeSeriesFilterRequest
            {
                Start = start,
                End = end,
                GroupbBy = groupBy,
            });

            var result = actionResult.EnsureOkObjectResult<IDictionary<DateTime, IDictionary<DHCPv6PacketTypes, Int32>>>(true);
            Assert.NotNull(result);
            Assert.Equal(response, result);

            readStoreMock.Verify();
        }

        [Fact]
        public async Task GetHandledDHCPv6PacketByScopeId()
        {
            Random random = new Random();

            Guid scopeId = random.NextGuid();
            Int32 amount = random.Next(10, 250);
            List<DHCPv6PacketHandledEntry> response = new List<DHCPv6PacketHandledEntry>();

            DHCPv6RootScope rootScope = GetRootScope();
            rootScope.Load(new[] { new DHCPv6ScopeAddedEvent { Instructions = new DHCPv6ScopeCreateInstruction
            {
                Id = scopeId,
            }}});

            Mock<IDHCPv6ReadStore> readStoreMock = new Mock<IDHCPv6ReadStore>(MockBehavior.Strict);
            readStoreMock.Setup(x => x.GetHandledDHCPv6PacketByScopeId(scopeId, amount)).ReturnsAsync(response).Verifiable();

            var controller = new DHCPv6StatisticsController(rootScope, readStoreMock.Object, Mock.Of<INotificationEngine>(MockBehavior.Strict));
            var actionResult = await controller.GetHandledDHCPv6PacketByScopeId(scopeId, amount);

            var result = actionResult.EnsureOkObjectResult<IEnumerable<DHCPv6PacketHandledEntry>>(true);
            Assert.NotNull(result);
            Assert.Equal(response, result);

            readStoreMock.Verify();
        }

        [Fact]
        public async Task GetHandledDHCPv6PacketByScopeId_NotFound()
        {
            Random random = new Random();

            Guid scopeId = random.NextGuid();
            Int32 amount = random.Next(10, 250);

            DHCPv6RootScope rootScope = GetRootScope();

            var controller = new DHCPv6StatisticsController(rootScope, Mock.Of<IDHCPv6ReadStore>(MockBehavior.Strict), Mock.Of<INotificationEngine>(MockBehavior.Strict));
            var actionResult = await controller.GetHandledDHCPv6PacketByScopeId(scopeId, amount);

            actionResult.EnsureNotFoundObjectResult("scope not found");
        }

        [Fact]
        public async Task GetFileredDHCPv6Packets()
        {
            Random random = new Random();

            DHCPv6RootScope rootScope = GetRootScope();

            var response = new Dictionary<DateTime, Int32>();
            DateTime start = DateTime.UtcNow.AddHours(-random.Next(5, 10));
            DateTime end = DateTime.UtcNow.AddHours(-random.Next(1, 2));
            GroupStatisticsResultBy groupBy = random.GetEnumValue<GroupStatisticsResultBy>();

            Mock<IDHCPv6ReadStore> readStoreMock = new Mock<IDHCPv6ReadStore>(MockBehavior.Strict);
            readStoreMock.Setup(x => x.GetFileredDHCPv6Packets(start, end, groupBy)).ReturnsAsync(response).Verifiable();

            var controller = new DHCPv6StatisticsController(rootScope, readStoreMock.Object, Mock.Of<INotificationEngine>(MockBehavior.Strict));
            var actionResult = await controller.GetFileredDHCPv6Packets(new GroupedTimeSeriesFilterRequest
            {
                Start = start,
                End = end,
                GroupbBy = groupBy,
            });

            var result = actionResult.EnsureOkObjectResult<IDictionary<DateTime, Int32>>(true);
            Assert.NotNull(result);
            Assert.Equal(response, result);

            readStoreMock.Verify();
        }

        [Fact]
        public async Task GetErrorDHCPv6Packets()
        {
            Random random = new Random();

            DHCPv6RootScope rootScope = GetRootScope();

            var response = new Dictionary<DateTime, Int32>();
            DateTime start = DateTime.UtcNow.AddHours(-random.Next(5, 10));
            DateTime end = DateTime.UtcNow.AddHours(-random.Next(1, 2));
            GroupStatisticsResultBy groupBy = random.GetEnumValue<GroupStatisticsResultBy>();

            Mock<IDHCPv6ReadStore> readStoreMock = new Mock<IDHCPv6ReadStore>(MockBehavior.Strict);
            readStoreMock.Setup(x => x.GetErrorDHCPv6Packets(start, end, groupBy)).ReturnsAsync(response).Verifiable();

            var controller = new DHCPv6StatisticsController(rootScope, readStoreMock.Object, Mock.Of<INotificationEngine>(MockBehavior.Strict));
            var actionResult = await controller.GetErrorDHCPv6Packets(new GroupedTimeSeriesFilterRequest
            {
                Start = start,
                End = end,
                GroupbBy = groupBy,
            });

            var result = actionResult.EnsureOkObjectResult<IDictionary<DateTime, Int32>>(true);
            Assert.NotNull(result);
            Assert.Equal(response, result);

            readStoreMock.Verify();
        }

        [Fact]
        public async Task GetIncomingDHCPv6PacketAmount()
        {
            Random random = new Random();

            DHCPv6RootScope rootScope = GetRootScope();

            var response = new Dictionary<DateTime, Int32>();
            DateTime start = DateTime.UtcNow.AddHours(-random.Next(5, 10));
            DateTime end = DateTime.UtcNow.AddHours(-random.Next(1, 2));
            GroupStatisticsResultBy groupBy = random.GetEnumValue<GroupStatisticsResultBy>();

            Mock<IDHCPv6ReadStore> readStoreMock = new Mock<IDHCPv6ReadStore>(MockBehavior.Strict);
            readStoreMock.Setup(x => x.GetIncomingDHCPv6PacketAmount(start, end, groupBy)).ReturnsAsync(response).Verifiable();

            var controller = new DHCPv6StatisticsController(rootScope, readStoreMock.Object, Mock.Of<INotificationEngine>(MockBehavior.Strict));
            var actionResult = await controller.GetIncomingDHCPv6PacketAmount(new GroupedTimeSeriesFilterRequest
            {
                Start = start,
                End = end,
                GroupbBy = groupBy,
            });

            var result = actionResult.EnsureOkObjectResult<IDictionary<DateTime, Int32>>(true);
            Assert.NotNull(result);
            Assert.Equal(response, result);

            readStoreMock.Verify();
        }

        [Fact]
        public async Task GetActiveDHCPv6Leases()
        {
            Random random = new Random();
            DHCPv6RootScope rootScope = GetRootScope();

            var response = new Dictionary<DateTime, Int32>();
            DateTime start = DateTime.UtcNow.AddHours(-random.Next(5, 10));
            DateTime end = DateTime.UtcNow.AddHours(-random.Next(1, 2));
            GroupStatisticsResultBy groupBy = random.GetEnumValue<GroupStatisticsResultBy>();

            Mock<IDHCPv6ReadStore> readStoreMock = new Mock<IDHCPv6ReadStore>(MockBehavior.Strict);
            readStoreMock.Setup(x => x.GetActiveDHCPv6Leases(start, end, groupBy)).ReturnsAsync(response).Verifiable();

            var controller = new DHCPv6StatisticsController(rootScope, readStoreMock.Object, Mock.Of<INotificationEngine>(MockBehavior.Strict));
            var actionResult = await controller.GetActiveDHCPv6Leases(new GroupedTimeSeriesFilterRequest
            {
                Start = start,
                End = end,
                GroupbBy = groupBy,
            });

            var result = actionResult.EnsureOkObjectResult<IDictionary<DateTime, Int32>>(true);
            Assert.NotNull(result);
            Assert.Equal(response, result);

            readStoreMock.Verify();
        }

        [Fact]
        public async Task GetErrorCodesPerDHCPV6RequestType()
        {
            Random random = new Random();
            DHCPv6RootScope rootScope = GetRootScope();

            var response = new Dictionary<Int32, Int32>();
            DateTime start = DateTime.UtcNow.AddHours(-random.Next(5, 10));
            DateTime end = DateTime.UtcNow.AddHours(-random.Next(1, 2));
            DHCPv6PacketTypes packetType = random.GetEnumValue<DHCPv6PacketTypes>();

            Mock<IDHCPv6ReadStore> readStoreMock = new Mock<IDHCPv6ReadStore>(MockBehavior.Strict);
            readStoreMock.Setup(x => x.GetErrorCodesPerDHCPV6RequestType(start, end, packetType)).ReturnsAsync(response).Verifiable();

            var controller = new DHCPv6StatisticsController(rootScope, readStoreMock.Object, Mock.Of<INotificationEngine>(MockBehavior.Strict));
            var actionResult = await controller.GetErrorCodesPerDHCPV6RequestType(new DHCPv6PacketTypeBasedTimeSeriesFilterRequest
            {
                Start = start,
                End = end,
                PacketType = packetType,
            });

            var result = actionResult.EnsureOkObjectResult<IDictionary<Int32, Int32>>(true);
            Assert.NotNull(result);
            Assert.Equal(response, result);

            readStoreMock.Verify();
        }
    }
}
