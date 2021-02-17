using DaAPI.Core.Common;
using DaAPI.Core.Packets.DHCPv4;
using DaAPI.Core.Scopes;
using DaAPI.Core.Scopes.DHCPv4;
using DaAPI.Host.ApiControllers;
using DaAPI.TestHelper;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static DaAPI.Core.Scopes.DHCPv4.DHCPv4LeaseEvents;
using static DaAPI.Shared.Responses.DHCPv4LeasesResponses.V1;

namespace DaAPI.UnitTests.Host.ApiControllers
{
    public class DHCPv4LeaseControllerTester
    {
        protected DHCPv4RootScope GetRootScope()
        {
            Mock<ILoggerFactory> factoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);
            factoryMock.Setup(x => x.CreateLogger(It.IsAny<String>())).Returns(Mock.Of<ILogger<DHCPv4RootScope>>());

            var scope = new DHCPv4RootScope(Guid.NewGuid(), Mock.Of<IScopeResolverManager<DHCPv4Packet, IPv4Address>>(), factoryMock.Object);

            return scope;
        }

        private class LeaseOverviewEqualityComparer : IEqualityComparer<DHCPv4LeaseOverview>
        {
            public bool Equals([AllowNull] DHCPv4LeaseOverview x, [AllowNull] DHCPv4LeaseOverview y) =>
                 x.Address == y.Address &&
                   ByteHelper.AreEqual(x.MacAddress, y.MacAddress) == true &&
                   x.ExpectedEnd == y.ExpectedEnd &&
                   x.Id == y.Id &&
                   x.Scope.Name == y.Scope.Name &&
                   x.Scope.Id == y.Scope.Id &&
                   x.Started == y.Started &&
                   x.State == y.State &&
                   ByteHelper.AreEqual(x.UniqueIdentifier, y.UniqueIdentifier) == true;

            public int GetHashCode([DisallowNull] DHCPv4LeaseOverview obj) => obj.Id.GetHashCode();
        }

        [Fact]
        public void GetLeasesByScope_OnlyDirect()
        {
            Random random = new Random();
            Guid scopeId = random.NextGuid();
            String scopeName = "Testscope";

            DHCPv4LeaseOverview activeLeaseWithoutPrefix = new DHCPv4LeaseOverview
            {
                Address = random.GetIPv4Address().ToString(),
                MacAddress = random.NextBytes(6),
                ExpectedEnd = DateTime.UtcNow.AddHours(random.Next(10, 20)),
                Started = DateTime.UtcNow.AddHours(-random.Next(10, 20)),
                Id = random.NextGuid(),
                UniqueIdentifier = random.NextBytes(10),
                State = LeaseStates.Active,
                Scope = new DHCPv4ScopeOverview
                {
                    Id = scopeId,
                    Name = scopeName,
                }
            };

            DHCPv4LeaseOverview expiredLeaseWithPrefix = new DHCPv4LeaseOverview
            {
                Address = random.GetIPv4Address().ToString(),
                MacAddress = random.NextBytes(6),
                ExpectedEnd = DateTime.UtcNow.AddHours(random.Next(10, 20)),
                Started = DateTime.UtcNow.AddHours(-random.Next(10, 20)),
                Id = random.NextGuid(),
                UniqueIdentifier = Array.Empty<Byte>(),
                State = LeaseStates.Inactive,
                Scope = new DHCPv4ScopeOverview
                {
                    Id = scopeId,
                    Name = scopeName,
                }
            };

            DHCPv4RootScope rootScope = GetRootScope();
            rootScope.Load(new List<DomainEvent>{ new DHCPv4ScopeEvents.DHCPv4ScopeAddedEvent(
                new DHCPv4ScopeCreateInstruction
                {
                    Name = scopeName,
                    Id = scopeId,
                }),
                 new DHCPv4LeaseCreatedEvent
                {
                    EntityId = expiredLeaseWithPrefix.Id,
                    Address = IPv4Address.FromString(expiredLeaseWithPrefix.Address.ToString()),
                    HardwareAddress =  expiredLeaseWithPrefix.MacAddress,
                    ScopeId = scopeId,
                    StartedAt = expiredLeaseWithPrefix.Started,
                    ValidUntil = expiredLeaseWithPrefix.ExpectedEnd,
                    UniqueIdentifier = null,
                },
                new DHCPv4LeaseActivatedEvent
                {
                    EntityId = expiredLeaseWithPrefix.Id,
                    ScopeId = scopeId,
                },
                new DHCPv4LeaseExpiredEvent
                {
                    EntityId = expiredLeaseWithPrefix.Id,
                    ScopeId = scopeId,
                },
                   new DHCPv4LeaseCreatedEvent
                {
                    EntityId = activeLeaseWithoutPrefix.Id,
                    Address = IPv4Address.FromString(activeLeaseWithoutPrefix.Address.ToString()),
                    HardwareAddress = activeLeaseWithoutPrefix.MacAddress,
                    ScopeId = scopeId,
                    StartedAt = activeLeaseWithoutPrefix.Started,
                    ValidUntil = activeLeaseWithoutPrefix.ExpectedEnd,
                    UniqueIdentifier = activeLeaseWithoutPrefix.UniqueIdentifier
                },
                new DHCPv4LeaseActivatedEvent
                {
                    EntityId = activeLeaseWithoutPrefix.Id,
                    ScopeId = scopeId,
                },
            });;


            var controller = new DHCPv4LeaseController(rootScope, Mock.Of<ILogger<DHCPv4LeaseController>>());

            var actionResult = controller.GetLeasesByScope(scopeId);
            var result = actionResult.EnsureOkObjectResult<IEnumerable<DHCPv4LeaseOverview>>(true);

            Assert.Equal(new[] { activeLeaseWithoutPrefix, expiredLeaseWithPrefix }, result, new LeaseOverviewEqualityComparer());

        }

        [Fact]
        public void GetLeasesByScope_WithParents()
        {
            Random random = new Random();
            Guid grantParentId = random.NextGuid();
            String grantParentScopeName = "Grant parent";

            Guid parentId = random.NextGuid();
            String parentScopeName = "Parent";

            Guid childId = random.NextGuid();
            String childScopeName = "Child";

            DHCPv4LeaseOverview activeLeaseWithoutPrefix = new DHCPv4LeaseOverview
            {
                Address = random.GetIPv4Address().ToString(),
                MacAddress = random.NextBytes(6),
                ExpectedEnd = DateTime.UtcNow.AddHours(random.Next(10, 20)),
                Started = DateTime.UtcNow.AddHours(-random.Next(10, 20)),
                Id = random.NextGuid(),
                UniqueIdentifier = random.NextBytes(10),
                State = LeaseStates.Active,
                Scope = new DHCPv4ScopeOverview
                {
                    Id = childId,
                    Name = childScopeName,
                }
            };

            DHCPv4LeaseOverview expiredLeaseWithPrefix = new DHCPv4LeaseOverview
            {
                Address = random.GetIPv4Address().ToString(),
                MacAddress = random.NextBytes(6),
                ExpectedEnd = DateTime.UtcNow.AddHours(random.Next(10, 20)),
                Started = DateTime.UtcNow.AddHours(-random.Next(10, 20)),
                Id = random.NextGuid(),
                UniqueIdentifier = Array.Empty<Byte>(),
                State = LeaseStates.Inactive,
                Scope = new DHCPv4ScopeOverview
                {
                    Id = grantParentId,
                    Name = grantParentScopeName,
                }
            };

            DHCPv4RootScope rootScope = GetRootScope();
            rootScope.Load(new List<DomainEvent>{ new DHCPv4ScopeEvents.DHCPv4ScopeAddedEvent(
                new DHCPv4ScopeCreateInstruction
                {
                    Name = grantParentScopeName,
                    Id = grantParentId,
                }),
                new DHCPv4ScopeEvents.DHCPv4ScopeAddedEvent(new DHCPv4ScopeCreateInstruction
                {
                    Name = parentScopeName,
                    Id = parentId,
                    ParentId = grantParentId,
                }),
                new DHCPv4ScopeEvents.DHCPv4ScopeAddedEvent(new DHCPv4ScopeCreateInstruction
                {
                    Name = childScopeName,
                    Id = childId,
                    ParentId = parentId,
                }),
                 new DHCPv4LeaseCreatedEvent
                {
                    EntityId = expiredLeaseWithPrefix.Id,
                    Address = IPv4Address.FromString(expiredLeaseWithPrefix.Address.ToString()),
                    HardwareAddress = expiredLeaseWithPrefix.MacAddress,
                    ScopeId = grantParentId,
                    StartedAt = expiredLeaseWithPrefix.Started,
                    ValidUntil = expiredLeaseWithPrefix.ExpectedEnd,
                    UniqueIdentifier = null
                },
                new DHCPv4LeaseActivatedEvent
                {
                    EntityId = expiredLeaseWithPrefix.Id,
                    ScopeId = grantParentId,
                },
                new DHCPv4LeaseExpiredEvent
                {
                    EntityId = expiredLeaseWithPrefix.Id,
                    ScopeId = grantParentId,
                },
                   new DHCPv4LeaseCreatedEvent
                {
                    EntityId = activeLeaseWithoutPrefix.Id,
                    Address = IPv4Address.FromString(activeLeaseWithoutPrefix.Address),
                    HardwareAddress = activeLeaseWithoutPrefix.MacAddress,
                    ScopeId = childId,
                    StartedAt = activeLeaseWithoutPrefix.Started,
                    ValidUntil = activeLeaseWithoutPrefix.ExpectedEnd,
                    UniqueIdentifier = activeLeaseWithoutPrefix.UniqueIdentifier
                },
                new DHCPv4LeaseActivatedEvent
                {
                    EntityId = activeLeaseWithoutPrefix.Id,
                    ScopeId = childId,
                },
            });


            var controller = new DHCPv4LeaseController(rootScope, Mock.Of<ILogger<DHCPv4LeaseController>>());

            var actionResult = controller.GetLeasesByScope(grantParentId, true);
            var result = actionResult.EnsureOkObjectResult<IEnumerable<DHCPv4LeaseOverview>>(true);

            Assert.Equal(new[] { activeLeaseWithoutPrefix, expiredLeaseWithPrefix }, result, new LeaseOverviewEqualityComparer());

        }

    }
}
