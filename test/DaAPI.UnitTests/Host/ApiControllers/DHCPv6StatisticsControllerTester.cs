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
    public class DHCPv6StatisticsControllerTester
    {
        protected DHCPv6RootScope GetRootScope()
        {
            Mock<ILoggerFactory> factoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);
            factoryMock.Setup(x => x.CreateLogger(It.IsAny<String>())).Returns(Mock.Of<ILogger<DHCPv6RootScope>>());

            var scope = new DHCPv6RootScope(Guid.NewGuid(), Mock.Of<IScopeResolverManager<DHCPv6Packet, IPv6Address>>(), factoryMock.Object);

            return scope;
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

            var controller = new DHCPv6StatisticsController(rootScope, readStoreMock.Object);
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

            var controller = new DHCPv6StatisticsController(rootScope, readStoreMock.Object);
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

            var controller = new DHCPv6StatisticsController(rootScope, Mock.Of<IDHCPv6ReadStore>(MockBehavior.Strict));
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

            var controller = new DHCPv6StatisticsController(rootScope, readStoreMock.Object);
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

            var controller = new DHCPv6StatisticsController(rootScope, readStoreMock.Object);
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

            var controller = new DHCPv6StatisticsController(rootScope, readStoreMock.Object);
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

            var controller = new DHCPv6StatisticsController(rootScope, readStoreMock.Object);
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

            var controller = new DHCPv6StatisticsController(rootScope, readStoreMock.Object);
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
