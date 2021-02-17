using DaAPI.Core.Common;
using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Packets.DHCPv4;
using DaAPI.Core.Packets.DHCPv6;
using DaAPI.Core.Scopes;
using DaAPI.Core.Scopes.DHCPv4;
using DaAPI.Core.Scopes.DHCPv6;
using DaAPI.Host.ApiControllers;
using DaAPI.Infrastructure.NotificationEngine;
using DaAPI.Infrastructure.StorageEngine.DHCPv6;
using DaAPI.TestHelper;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static DaAPI.Shared.Responses.StatisticsControllerResponses.V1;

namespace DaAPI.UnitTests.Host.ApiControllers
{
    public class DashboardControllerTester
    {
        protected DHCPv6RootScope GetDHCPv6RootScope()
        {
            Mock<ILoggerFactory> factoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);
            factoryMock.Setup(x => x.CreateLogger(It.IsAny<String>())).Returns(Mock.Of<ILogger<DHCPv6RootScope>>());

            var scope = new DHCPv6RootScope(Guid.NewGuid(), Mock.Of<IScopeResolverManager<DHCPv6Packet, IPv6Address>>(), factoryMock.Object);

            return scope;
        }

        protected DHCPv4RootScope GetDHCPv4RootScope()
        {
            Mock<ILoggerFactory> factoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);
            factoryMock.Setup(x => x.CreateLogger(It.IsAny<String>())).Returns(Mock.Of<ILogger<DHCPv4RootScope>>());

            var scope = new DHCPv4RootScope(Guid.NewGuid(), Mock.Of<IScopeResolverManager<DHCPv4Packet, IPv4Address>>(), factoryMock.Object);

            return scope;
        }

        [Fact]
        public async Task GetDashboard()
        {
            Random random = new Random();
            Guid grantParentScopeId = random.NextGuid();
            Guid parentScopeId = random.NextGuid();
            Guid childScopeId = random.NextGuid();

            DHCPv6RootScope rootScope = GetDHCPv6RootScope();
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

            DHCPv4RootScope dhcpv4RootScope = GetDHCPv4RootScope();
            dhcpv4RootScope.Load(new List<DomainEvent>
            {
                new DHCPv4ScopeEvents.DHCPv4ScopeAddedEvent(
                new DHCPv4ScopeCreateInstruction
                {
                    Name = "grant parent",
                    Id = grantParentScopeId,
                }),
                new DHCPv4ScopeEvents.DHCPv4ScopeAddedEvent(
                new DHCPv4ScopeCreateInstruction
                {
                    Name = "parent",
                    Id = parentScopeId,
                    ParentId = grantParentScopeId
                }),
                new DHCPv4ScopeEvents.DHCPv4ScopeAddedEvent(
                new DHCPv4ScopeCreateInstruction
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
                DHCPv4 = new DHCPOverview<DHCPv4LeaseEntry, DHCPv4PacketHandledEntry>
                {
                    ActiveInterfaces = random.Next(3, 10),
                },
            };

            Int32 expectedPipelineAmount = random.Next(3, 10);

            Mock<IDHCPv6ReadStore> readStoreMock = new Mock<IDHCPv6ReadStore>(MockBehavior.Strict);
            readStoreMock.Setup(x => x.GetDashboardOverview()).ReturnsAsync(response).Verifiable();

            Mock<INotificationEngine> notificationEngineMock = new Mock<INotificationEngine>(MockBehavior.Strict);
            notificationEngineMock.Setup(x => x.GetPipelineAmount()).Returns(expectedPipelineAmount).Verifiable();

            var controller = new DashboardController(rootScope, dhcpv4RootScope, readStoreMock.Object, notificationEngineMock.Object);
            var actionResult = await controller.GetDashboard();

            var result = actionResult.EnsureOkObjectResult<DashboardResponse>(true);
            Assert.NotNull(result);
            Assert.NotNull(result.DHCPv6);
            Assert.NotNull(result.DHCPv4);

            Assert.Equal(3, result.DHCPv6.ScopeAmount);
            Assert.Equal(response.DHCPv6.ActiveInterfaces, result.DHCPv6.ActiveInterfaces);
            Assert.Equal(expectedPipelineAmount, result.AmountOfPipelines);

            Assert.Equal(3, result.DHCPv4.ScopeAmount);
            Assert.Equal(response.DHCPv4.ActiveInterfaces, result.DHCPv4.ActiveInterfaces);

            readStoreMock.Verify();
        }
    }
}
