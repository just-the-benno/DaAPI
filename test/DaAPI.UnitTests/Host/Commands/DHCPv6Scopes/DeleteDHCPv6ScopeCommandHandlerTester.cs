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
using static DaAPI.Core.Scopes.DHCPv6.DHCPv6ScopeEvents;

namespace DaAPI.UnitTests.Host.Commands.DHCPv6Scopes
{
    public class DeleteDHCPv6ScopeCommandHandlerTester
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Handle_HasNoChildren(Boolean requestedToDeleteChildrenAsWell)
        {
            Random random = new Random();

            Guid id = random.NextGuid();

            String resolverName = random.GetAlphanumericString();

            Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>> scopeResolverMock = new Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>>();
            scopeResolverMock.Setup(x => x.InitializeResolver(It.Is<CreateScopeResolverInformation>(y =>
            y.Typename == resolverName
            ))).Returns(Mock.Of<IScopeResolver<DHCPv6Packet, IPv6Address>>()).Verifiable();

            Mock<ILoggerFactory> factoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);
            factoryMock.Setup(x => x.CreateLogger(It.IsAny<String>())).Returns(Mock.Of<ILogger<DHCPv6RootScope>>());

            DHCPv6RootScope rootScope = new DHCPv6RootScope(random.NextGuid(), scopeResolverMock.Object, factoryMock.Object);
            rootScope.Load(new[]{
                new DHCPv6ScopeAddedEvent
                {
                    Instructions = new DHCPv6ScopeCreateInstruction
                    {
                        Id = id,
                        ResolverInformation = new CreateScopeResolverInformation
                        {
                            Typename = resolverName,
                        }
                    }
                }
            });


            Mock<IDHCPv6StorageEngine> storageMock = new Mock<IDHCPv6StorageEngine>(MockBehavior.Strict);
            storageMock.Setup(x => x.Save(rootScope)).ReturnsAsync(true).Verifiable();

            var command = new DeleteDHCPv6ScopeCommand(id, requestedToDeleteChildrenAsWell);

            var handler = new DeleteDHCPv6ScopeCommandHandler(storageMock.Object, rootScope,
                Mock.Of<ILogger<DeleteDHCPv6ScopeCommandHandler>>());

            Boolean result = await handler.Handle(command, CancellationToken.None);
            Assert.True(result);

            Assert.Single(rootScope.GetChanges());

            storageMock.Verify();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Handle_WithChildren(Boolean requestedToDeleteChildrenAsWell)
        {
            Random random = new Random();

            Guid grantParentId = random.NextGuid();
            Guid parentId = random.NextGuid();
            Guid childId = random.NextGuid();

            String resolverName = random.GetAlphanumericString();

            Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>> scopeResolverMock = new Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>>();
            scopeResolverMock.Setup(x => x.InitializeResolver(It.Is<CreateScopeResolverInformation>(y =>
            y.Typename == resolverName
            ))).Returns(Mock.Of<IScopeResolver<DHCPv6Packet, IPv6Address>>()).Verifiable();

            Mock<ILoggerFactory> factoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);
            factoryMock.Setup(x => x.CreateLogger(It.IsAny<String>())).Returns(Mock.Of<ILogger<DHCPv6RootScope>>());

            DHCPv6RootScope rootScope = new DHCPv6RootScope(random.NextGuid(), scopeResolverMock.Object, factoryMock.Object);
            rootScope.Load(new[]{
                new DHCPv6ScopeAddedEvent
                {
                    Instructions = new DHCPv6ScopeCreateInstruction
                    {
                        Id = grantParentId,
                        ResolverInformation = new CreateScopeResolverInformation
                        {
                            Typename = resolverName,
                        },
                        AddressProperties = new DHCPv6ScopeAddressProperties(IPv6Address.FromString("fe80::1"),IPv6Address.FromString("fe80::2")),
                    }
                },
                new DHCPv6ScopeAddedEvent
                {
                    Instructions = new DHCPv6ScopeCreateInstruction
                    {
                        Id = parentId,
                        ParentId = grantParentId,
                        ResolverInformation = new CreateScopeResolverInformation
                        {
                            Typename = resolverName,
                        },
                        AddressProperties = new DHCPv6ScopeAddressProperties(IPv6Address.FromString("fe80::1"),IPv6Address.FromString("fe80::2")),
                    }
                },
                new DHCPv6ScopeAddedEvent
                {
                    Instructions = new DHCPv6ScopeCreateInstruction
                    {
                        Id = childId,
                        ParentId = parentId,
                        ResolverInformation = new CreateScopeResolverInformation
                        {
                            Typename = resolverName,
                        },
                        AddressProperties = new DHCPv6ScopeAddressProperties(IPv6Address.FromString("fe80::1"),IPv6Address.FromString("fe80::2")),
                    }
                },
            });


            Mock<IDHCPv6StorageEngine> storageMock = new Mock<IDHCPv6StorageEngine>(MockBehavior.Strict);
            storageMock.Setup(x => x.Save(rootScope)).ReturnsAsync(true).Verifiable();

            var command = new DeleteDHCPv6ScopeCommand(parentId, requestedToDeleteChildrenAsWell);

            var handler = new DeleteDHCPv6ScopeCommandHandler(storageMock.Object, rootScope,
                Mock.Of<ILogger<DeleteDHCPv6ScopeCommandHandler>>());

            Boolean result = await handler.Handle(command, CancellationToken.None);
            Assert.True(result);

            Assert.Single(rootScope.GetChanges());

            if (requestedToDeleteChildrenAsWell == true)
            {
                Assert.Null(rootScope.GetScopeById(childId));
            }
            Assert.Null(rootScope.GetScopeById(parentId));
            Assert.NotNull(rootScope.GetScopeById(grantParentId));

            storageMock.Verify();
        }

        [Fact]
        public async Task Handle_NotFound()
        {
            Random random = new Random();

            Guid id = random.NextGuid();

            String resolverName = random.GetAlphanumericString();

            Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>> scopeResolverMock = new Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>>();
            scopeResolverMock.Setup(x => x.InitializeResolver(It.Is<CreateScopeResolverInformation>(y =>
            y.Typename == resolverName
            ))).Returns(Mock.Of<IScopeResolver<DHCPv6Packet, IPv6Address>>()).Verifiable();

            Mock<ILoggerFactory> factoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);
            factoryMock.Setup(x => x.CreateLogger(It.IsAny<String>())).Returns(Mock.Of<ILogger<DHCPv6RootScope>>());

            DHCPv6RootScope rootScope = new DHCPv6RootScope(random.NextGuid(), scopeResolverMock.Object, factoryMock.Object);
            rootScope.Load(new[]{
                new DHCPv6ScopeAddedEvent
                {
                    Instructions = new DHCPv6ScopeCreateInstruction
                    {
                        Id = id,
                        ResolverInformation = new CreateScopeResolverInformation
                        {
                            Typename = resolverName,
                        }
                    }
                }
            });

            var command = new DeleteDHCPv6ScopeCommand(random.NextGuid(), random.NextBoolean());

            var handler = new DeleteDHCPv6ScopeCommandHandler(Mock.Of<IDHCPv6StorageEngine>(MockBehavior.Strict), rootScope,
                Mock.Of<ILogger<DeleteDHCPv6ScopeCommandHandler>>());

            Boolean result = await handler.Handle(command, CancellationToken.None);
            Assert.False(result);

            Assert.Empty(rootScope.GetChanges());
        }
    }
}
