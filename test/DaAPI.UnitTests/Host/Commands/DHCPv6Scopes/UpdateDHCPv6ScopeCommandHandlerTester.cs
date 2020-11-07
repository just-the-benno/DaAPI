using DaAPI.Core.Common;
using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Notifications.Triggers;
using DaAPI.Core.Packets.DHCPv6;
using DaAPI.Core.Scopes;
using DaAPI.Core.Scopes.DHCPv6;
using DaAPI.Core.Scopes.DHCPv6.ScopeProperties;
using DaAPI.Host.Application.Commands.DHCPv6Scopes;
using DaAPI.Infrastructure.ServiceBus;
using DaAPI.Infrastructure.ServiceBus.Messages;
using DaAPI.Infrastructure.StorageEngine.DHCPv6;
using DaAPI.TestHelper;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using static DaAPI.Core.Scopes.DHCPv6.DHCPv6LeaseEvents;
using static DaAPI.Core.Scopes.DHCPv6.DHCPv6ScopeEvents;
using static DaAPI.Shared.Requests.DHCPv6ScopeRequests.V1;

namespace DaAPI.UnitTests.Host.Commands.DHCPv6Scopes
{
    public class UpdateDHCPv6ScopeCommandHandlerTester
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Handle(Boolean storeResult)
        {
            Random random = new Random();

            Guid scopeId = random.NextGuid();
            Guid leaseId = random.NextGuid();

            String name = random.GetAlphanumericString();
            String description = random.GetAlphanumericString();

            IPv6Address start = random.GetIPv6Address();
            IPv6Address end = start + 100;

            String resolverName = random.GetAlphanumericString();

            Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>> scopeResolverMock = new Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>>();
            scopeResolverMock.Setup(x => x.IsResolverInformationValid(It.Is<CreateScopeResolverInformation>(y =>
            y.Typename == resolverName
            ))).Returns(true).Verifiable();
            scopeResolverMock.Setup(x => x.InitializeResolver(It.Is<CreateScopeResolverInformation>(y =>
            y.Typename == resolverName
            ))).Returns(Mock.Of<IScopeResolver<DHCPv6Packet, IPv6Address>>()).Verifiable();

            Mock<ILoggerFactory> factoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);
            factoryMock.Setup(x => x.CreateLogger(It.IsAny<String>())).Returns(Mock.Of<ILogger<DHCPv6RootScope>>());

            DHCPv6RootScope rootScope = new DHCPv6RootScope(random.NextGuid(), scopeResolverMock.Object, factoryMock.Object);
            rootScope.Load(new DomainEvent[] { new DHCPv6ScopeAddedEvent {
                Instructions = new DHCPv6ScopeCreateInstruction
                {
                    Name = random.GetAlphanumericString(),
                    Description = random.GetAlphanumericString(),
                    Id = scopeId,
                    ParentId = null,
                    ResolverInformation = new CreateScopeResolverInformation
                    {
                        Typename = resolverName,
                    },
                    ScopeProperties = new DHCPv6ScopeProperties(),
                    AddressProperties =  DHCPv6ScopeAddressProperties.Empty,
                }
            },
            new DHCPv6LeaseCreatedEvent
            {
                    EntityId = leaseId,
                    Address = start - 100,
                    ScopeId = scopeId,
                    HasPrefixDelegation = true,
                    PrefixLength = 64,
                    ClientIdentifier = new UUIDDUID(random.NextGuid()),
                    IdentityAssocationIdForPrefix = random.NextUInt32(),
                    DelegatedNetworkAddress = IPv6Address.FromString("fc70::0"),
            },
            new DHCPv6LeaseActivatedEvent
            {
                EntityId = leaseId,
                ScopeId = scopeId,
            } });

            Mock<IDHCPv6StorageEngine> storageMock = new Mock<IDHCPv6StorageEngine>(MockBehavior.Strict);
            storageMock.Setup(x => x.Save(rootScope)).ReturnsAsync(storeResult).Verifiable();

            var command = new UpdateDHCPv6ScopeCommand(scopeId, name, description, null,
                new DHCPv6ScopeAddressPropertyReqest
                {
                    Start = start.ToString(),
                    End = end.ToString(),
                    ExcludedAddresses = Array.Empty<String>(),
                    AcceptDecline = random.NextBoolean(),
                    AddressAllocationStrategy = DHCPv6ScopeAddressPropertyReqest.AddressAllocationStrategies.Next,
                    InformsAreAllowd = random.NextBoolean(),
                    RapitCommitEnabled = random.NextBoolean(),
                    ReuseAddressIfPossible = random.NextBoolean(),
                    SupportDirectUnicast = random.NextBoolean(),
                    PreferredLifeTime = TimeSpan.FromDays(0.5),
                    ValidLifeTime = TimeSpan.FromDays(1),
                    PrefixDelgationInfo = new DHCPv6PrefixDelgationInfoRequest
                    {
                        AssingedPrefixLength = 80,
                        Prefix = "fe80::0",
                        PrefixLength = 64
                    },
                    T1 = 0.3,
                    T2 = 0.65,
                },
                new CreateScopeResolverRequest
                {
                    PropertiesAndValues = new Dictionary<String, String>(),
                    Typename = resolverName,
                },
                new[] { new DHCPv6AddressListScopePropertyRequest
                {
                 OptionCode = 24,
                 Type = DaAPI.Core.Scopes.DHCPv6.ScopeProperties.DHCPv6ScopePropertyType.AddressList,
                 Addresses = random.GetIPv6Addresses().Select(x => x.ToString()).ToArray(),
                },
                 new DHCPv6AddressListScopePropertyRequest
                {
                 OptionCode = 64,
                 MarkAsRemovedInInheritance = true,
                }
                }
                );

            var serviceBusMock = new Mock<IServiceBus>(MockBehavior.Strict);
            serviceBusMock.Setup(x => x.Publish(It.Is<NewTriggerHappendMessage>(y => y.Triggers.Count() == 1 &&
            ((PrefixEdgeRouterBindingUpdatedTrigger)y.Triggers.First()).OldBinding.Prefix == IPv6Address.FromString("fc70::0")
            ))).Returns(Task.CompletedTask);
            var handler = new UpdateDHCPv6ScopeCommandHandler(storageMock.Object, serviceBusMock.Object, rootScope,
                Mock.Of<ILogger<UpdateDHCPv6ScopeCommandHandler>>());

            Boolean result = await handler.Handle(command, CancellationToken.None);
            Assert.Equal(storeResult, result);

            var scope = rootScope.GetRootScopes().First();

            Assert.Equal(name, scope.Name);
            Assert.Equal(description, scope.Description);
            Assert.NotNull(scope.Resolver);
            Assert.Equal(start, scope.AddressRelatedProperties.Start);

            Assert.True(scope.Properties.IsMarkedAsRemovedFromInheritance(64));
            Assert.False(scope.Properties.IsMarkedAsRemovedFromInheritance(24));

            scopeResolverMock.Verify();
            storageMock.Verify();

            if (storeResult == true)
            {
                serviceBusMock.Verify();
                Assert.Empty(rootScope.GetTriggers());
            }
            else
            {
                Assert.Single(rootScope.GetTriggers());
            }
        }
    }
}
