using DaAPI.Core.Common;
using DaAPI.Core.Packets.DHCPv4;
using DaAPI.Core.Scopes;
using DaAPI.Core.Scopes.DHCPv4;
using DaAPI.Host.ApiControllers;
using DaAPI.Infrastructure.StorageEngine.DHCPv4;
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
using static DaAPI.Core.Scopes.DHCPv4.DHCPv4ScopeEvents;
using DaAPI.Infrastructure.NotificationEngine;

namespace DaAPI.UnitTests.Host.ApiControllers
{
    public class DHCPv4StatisticsControllerTester
    {
        protected DHCPv4RootScope GetRootScope()
        {
            Mock<ILoggerFactory> factoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);
            factoryMock.Setup(x => x.CreateLogger(It.IsAny<String>())).Returns(Mock.Of<ILogger<DHCPv4RootScope>>());

            var scope = new DHCPv4RootScope(Guid.NewGuid(), Mock.Of<IScopeResolverManager<DHCPv4Packet, IPv4Address>>(), factoryMock.Object);

            return scope;
        }

        [Fact]
        public async Task GetIncomingDHCPv4MessagesTypes()
        {
            Random random = new Random();

            DHCPv4RootScope rootScope = GetRootScope();

            var response = new Dictionary<DateTime, IDictionary<DHCPv4MessagesTypes, Int32>>();
            DateTime start = DateTime.UtcNow.AddHours(-random.Next(5, 10));
            DateTime end = DateTime.UtcNow.AddHours(-random.Next(1, 2));
            GroupStatisticsResultBy groupBy = random.GetEnumValue<GroupStatisticsResultBy>();

            Mock<IDHCPv4ReadStore> readStoreMock = new Mock<IDHCPv4ReadStore>(MockBehavior.Strict);
            readStoreMock.Setup(x => x.GetIncomingDHCPv4PacketTypes(start, end, groupBy)).ReturnsAsync(response).Verifiable();

            var controller = new DHCPv4StatisticsController(rootScope, readStoreMock.Object);
            var actionResult = await controller.GetIncomingDHCPv4PacketTypes(new GroupedTimeSeriesFilterRequest
            {
                Start = start,
                End = end,
                GroupbBy = groupBy,
            });

            var result = actionResult.EnsureOkObjectResult<IDictionary<DateTime, IDictionary<DHCPv4MessagesTypes, Int32>>>(true);
            Assert.NotNull(result);
            Assert.Equal(response, result);

            readStoreMock.Verify();
        }

        [Fact]
        public async Task GetHandledDHCPv4PacketByScopeId()
        {
            Random random = new Random();

            Guid scopeId = random.NextGuid();
            Int32 amount = random.Next(10, 250);
            List<DHCPv4PacketHandledEntry> response = new List<DHCPv4PacketHandledEntry>();

            DHCPv4RootScope rootScope = GetRootScope();
            rootScope.Load(new[] { new DHCPv4ScopeAddedEvent { Instructions = new DHCPv4ScopeCreateInstruction
            {
                Id = scopeId,
            }}});

            Mock<IDHCPv4ReadStore> readStoreMock = new Mock<IDHCPv4ReadStore>(MockBehavior.Strict);
            readStoreMock.Setup(x => x.GetHandledDHCPv4PacketByScopeId(scopeId, amount)).ReturnsAsync(response).Verifiable();

            var controller = new DHCPv4StatisticsController(rootScope, readStoreMock.Object);
            var actionResult = await controller.GetHandledDHCPv4PacketByScopeId(scopeId, amount);

            var result = actionResult.EnsureOkObjectResult<IEnumerable<DHCPv4PacketHandledEntry>>(true);
            Assert.NotNull(result);
            Assert.Equal(response, result);

            readStoreMock.Verify();
        }

        [Fact]
        public async Task GetHandledDHCPv4PacketByScopeId_NotFound()
        {
            Random random = new Random();

            Guid scopeId = random.NextGuid();
            Int32 amount = random.Next(10, 250);

            DHCPv4RootScope rootScope = GetRootScope();

            var controller = new DHCPv4StatisticsController(rootScope, Mock.Of<IDHCPv4ReadStore>(MockBehavior.Strict));
            var actionResult = await controller.GetHandledDHCPv4PacketByScopeId(scopeId, amount);

            actionResult.EnsureNotFoundObjectResult("scope not found");
        }

        [Fact]
        public async Task GetFileredDHCPv4Packets()
        {
            Random random = new Random();

            DHCPv4RootScope rootScope = GetRootScope();

            var response = new Dictionary<DateTime, Int32>();
            DateTime start = DateTime.UtcNow.AddHours(-random.Next(5, 10));
            DateTime end = DateTime.UtcNow.AddHours(-random.Next(1, 2));
            GroupStatisticsResultBy groupBy = random.GetEnumValue<GroupStatisticsResultBy>();

            Mock<IDHCPv4ReadStore> readStoreMock = new Mock<IDHCPv4ReadStore>(MockBehavior.Strict);
            readStoreMock.Setup(x => x.GetFileredDHCPv4Packets(start, end, groupBy)).ReturnsAsync(response).Verifiable();

            var controller = new DHCPv4StatisticsController(rootScope, readStoreMock.Object);
            var actionResult = await controller.GetFileredDHCPv4Packets(new GroupedTimeSeriesFilterRequest
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
        public async Task GetErrorDHCPv4Packets()
        {
            Random random = new Random();

            DHCPv4RootScope rootScope = GetRootScope();

            var response = new Dictionary<DateTime, Int32>();
            DateTime start = DateTime.UtcNow.AddHours(-random.Next(5, 10));
            DateTime end = DateTime.UtcNow.AddHours(-random.Next(1, 2));
            GroupStatisticsResultBy groupBy = random.GetEnumValue<GroupStatisticsResultBy>();

            Mock<IDHCPv4ReadStore> readStoreMock = new Mock<IDHCPv4ReadStore>(MockBehavior.Strict);
            readStoreMock.Setup(x => x.GetErrorDHCPv4Packets(start, end, groupBy)).ReturnsAsync(response).Verifiable();

            var controller = new DHCPv4StatisticsController(rootScope, readStoreMock.Object);
            var actionResult = await controller.GetErrorDHCPv4Packets(new GroupedTimeSeriesFilterRequest
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
        public async Task GetIncomingDHCPv4PacketAmount()
        {
            Random random = new Random();

            DHCPv4RootScope rootScope = GetRootScope();

            var response = new Dictionary<DateTime, Int32>();
            DateTime start = DateTime.UtcNow.AddHours(-random.Next(5, 10));
            DateTime end = DateTime.UtcNow.AddHours(-random.Next(1, 2));
            GroupStatisticsResultBy groupBy = random.GetEnumValue<GroupStatisticsResultBy>();

            Mock<IDHCPv4ReadStore> readStoreMock = new Mock<IDHCPv4ReadStore>(MockBehavior.Strict);
            readStoreMock.Setup(x => x.GetIncomingDHCPv4PacketAmount(start, end, groupBy)).ReturnsAsync(response).Verifiable();

            var controller = new DHCPv4StatisticsController(rootScope, readStoreMock.Object);
            var actionResult = await controller.GetIncomingDHCPv4PacketAmount(new GroupedTimeSeriesFilterRequest
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
        public async Task GetActiveDHCPv4Leases()
        {
            Random random = new Random();
            DHCPv4RootScope rootScope = GetRootScope();

            var response = new Dictionary<DateTime, Int32>();
            DateTime start = DateTime.UtcNow.AddHours(-random.Next(5, 10));
            DateTime end = DateTime.UtcNow.AddHours(-random.Next(1, 2));
            GroupStatisticsResultBy groupBy = random.GetEnumValue<GroupStatisticsResultBy>();

            Mock<IDHCPv4ReadStore> readStoreMock = new Mock<IDHCPv4ReadStore>(MockBehavior.Strict);
            readStoreMock.Setup(x => x.GetActiveDHCPv4Leases(start, end, groupBy)).ReturnsAsync(response).Verifiable();

            var controller = new DHCPv4StatisticsController(rootScope, readStoreMock.Object);
            var actionResult = await controller.GetActiveDHCPv4Leases(new GroupedTimeSeriesFilterRequest
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
        public async Task GetErrorCodesPerDHCPv4RequestType()
        {
            Random random = new Random();
            DHCPv4RootScope rootScope = GetRootScope();

            var response = new Dictionary<Int32, Int32>();
            DateTime start = DateTime.UtcNow.AddHours(-random.Next(5, 10));
            DateTime end = DateTime.UtcNow.AddHours(-random.Next(1, 2));
            DHCPv4MessagesTypes packetType = random.GetEnumValue<DHCPv4MessagesTypes>();

            Mock<IDHCPv4ReadStore> readStoreMock = new Mock<IDHCPv4ReadStore>(MockBehavior.Strict);
            readStoreMock.Setup(x => x.GetErrorCodesPerDHCPv4DHCPv4MessagesTypes(start, end, packetType)).ReturnsAsync(response).Verifiable();

            var controller = new DHCPv4StatisticsController(rootScope, readStoreMock.Object);
            var actionResult = await controller.GetErrorCodesPerDHCPv4MessageTypes(new DHCPv4PacketTypeBasedTimeSeriesFilterRequest
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
