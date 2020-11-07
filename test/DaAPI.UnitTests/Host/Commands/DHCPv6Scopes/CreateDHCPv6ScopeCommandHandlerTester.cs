using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Packets.DHCPv6;
using DaAPI.Core.Scopes;
using DaAPI.Core.Scopes.DHCPv6;
using DaAPI.Host.Application.Commands.DHCPv6Scopes;
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
using static DaAPI.Shared.Requests.DHCPv6ScopeRequests.V1;

namespace DaAPI.UnitTests.Host.Commands.DHCPv6Scopes
{
    public class CreateDHCPv6ScopeCommandHandlerTester
    {
        [Fact]
        public async Task Handle()
        {
            Random random = new Random();

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

            Mock<IDHCPv6StorageEngine> storageMock = new Mock<IDHCPv6StorageEngine>(MockBehavior.Strict);
            storageMock.Setup(x => x.Save(rootScope)).ReturnsAsync(true).Verifiable();

            var command = new CreateDHCPv6ScopeCommand(name, description, null,
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

            var handler = new CreateDHCPv6ScopeCommandHandler(storageMock.Object, rootScope,
                Mock.Of<ILogger<CreateDHCPv6ScopeCommandHandler>>());

            Guid? result = await handler.Handle(command, CancellationToken.None);
            Assert.True(result.HasValue);

            var scope = rootScope.GetRootScopes().First();
            Assert.True(scope.Properties.IsMarkedAsRemovedFromInheritance(64));
            Assert.False(scope.Properties.IsMarkedAsRemovedFromInheritance(24));

            scopeResolverMock.Verify();
            storageMock.Verify();
        }
    }
}
