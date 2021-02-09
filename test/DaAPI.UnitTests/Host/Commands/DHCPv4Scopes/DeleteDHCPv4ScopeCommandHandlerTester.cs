using DaAPI.Core.Common;
using DaAPI.Core.Packets.DHCPv4;
using DaAPI.Core.Scopes;
using DaAPI.Core.Scopes.DHCPv4;
using DaAPI.Host.Application.Commands.DHCPv4Scopes;
using DaAPI.Infrastructure.StorageEngine.DHCPv4;
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
using static DaAPI.Core.Scopes.DHCPv4.DHCPv4ScopeEvents;

namespace DaAPI.UnitTests.Host.Commands.DHCPv4Scopes
{
    public class DeleteDHCPv4ScopeCommandHandlerTester
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Handle_HasNoChildren(Boolean requestedToDeleteChildrenAsWell)
        {
            Random random = new Random();

            Guid id = random.NextGuid();

            String resolverName = random.GetAlphanumericString();

            Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>> scopeResolverMock = new Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>>();
            scopeResolverMock.Setup(x => x.InitializeResolver(It.Is<CreateScopeResolverInformation>(y =>
            y.Typename == resolverName
            ))).Returns(Mock.Of<IScopeResolver<DHCPv4Packet, IPv4Address>>()).Verifiable();

            Mock<ILoggerFactory> factoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);
            factoryMock.Setup(x => x.CreateLogger(It.IsAny<String>())).Returns(Mock.Of<ILogger<DHCPv4RootScope>>());

            DHCPv4RootScope rootScope = new DHCPv4RootScope(random.NextGuid(), scopeResolverMock.Object, factoryMock.Object);
            rootScope.Load(new[]{
                new DHCPv4ScopeAddedEvent
                {
                    Instructions = new DHCPv4ScopeCreateInstruction
                    {
                        Id = id,
                        ResolverInformation = new CreateScopeResolverInformation
                        {
                            Typename = resolverName,
                        }
                    }
                }
            });

            Mock<IDHCPv4StorageEngine> storageMock = new Mock<IDHCPv4StorageEngine>(MockBehavior.Strict);
            storageMock.Setup(x => x.Save(rootScope)).ReturnsAsync(true).Verifiable();

            var command = new DeleteDHCPv4ScopeCommand(id, requestedToDeleteChildrenAsWell);

            var handler = new DeleteDHCPv4ScopeCommandHandler(storageMock.Object, rootScope,
                Mock.Of<ILogger<DeleteDHCPv4ScopeCommandHandler>>());

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

            Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>> scopeResolverMock = new Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>>();
            scopeResolverMock.Setup(x => x.InitializeResolver(It.Is<CreateScopeResolverInformation>(y =>
            y.Typename == resolverName
            ))).Returns(Mock.Of<IScopeResolver<DHCPv4Packet, IPv4Address>>()).Verifiable();

            Mock<ILoggerFactory> factoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);
            factoryMock.Setup(x => x.CreateLogger(It.IsAny<String>())).Returns(Mock.Of<ILogger<DHCPv4RootScope>>());

            DHCPv4RootScope rootScope = new DHCPv4RootScope(random.NextGuid(), scopeResolverMock.Object, factoryMock.Object);
            rootScope.Load(new[]{
                new DHCPv4ScopeAddedEvent
                {
                    Instructions = new DHCPv4ScopeCreateInstruction
                    {
                        Id = grantParentId,
                        ResolverInformation = new CreateScopeResolverInformation
                        {
                            Typename = resolverName,
                        },
                        AddressProperties = new DHCPv4ScopeAddressProperties(IPv4Address.FromString("192.168.0.1"),IPv4Address.FromString("192.168.0.20")),
                    }
                },
                new DHCPv4ScopeAddedEvent
                {
                    Instructions = new DHCPv4ScopeCreateInstruction
                    {
                        Id = parentId,
                        ParentId = grantParentId,
                        ResolverInformation = new CreateScopeResolverInformation
                        {
                            Typename = resolverName,
                        },
                        AddressProperties = new DHCPv4ScopeAddressProperties(IPv4Address.FromString("192.168.0.5"),IPv4Address.FromString("192.168.0.8")),
                    }
                },
                new DHCPv4ScopeAddedEvent
                {
                    Instructions = new DHCPv4ScopeCreateInstruction
                    {
                        Id = childId,
                        ParentId = parentId,
                        ResolverInformation = new CreateScopeResolverInformation
                        {
                            Typename = resolverName,
                        },
                        AddressProperties = new DHCPv4ScopeAddressProperties(IPv4Address.FromString("192.168.0.7"),IPv4Address.FromString("192.168.0.7")),
                    }
                },
            });


            Mock<IDHCPv4StorageEngine> storageMock = new Mock<IDHCPv4StorageEngine>(MockBehavior.Strict);
            storageMock.Setup(x => x.Save(rootScope)).ReturnsAsync(true).Verifiable();

            var command = new DeleteDHCPv4ScopeCommand(parentId, requestedToDeleteChildrenAsWell);

            var handler = new DeleteDHCPv4ScopeCommandHandler(storageMock.Object, rootScope,
                Mock.Of<ILogger<DeleteDHCPv4ScopeCommandHandler>>());

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

            Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>> scopeResolverMock = new Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>>();
            scopeResolverMock.Setup(x => x.InitializeResolver(It.Is<CreateScopeResolverInformation>(y =>
            y.Typename == resolverName
            ))).Returns(Mock.Of<IScopeResolver<DHCPv4Packet, IPv4Address>>()).Verifiable();

            Mock<ILoggerFactory> factoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);
            factoryMock.Setup(x => x.CreateLogger(It.IsAny<String>())).Returns(Mock.Of<ILogger<DHCPv4RootScope>>());

            DHCPv4RootScope rootScope = new DHCPv4RootScope(random.NextGuid(), scopeResolverMock.Object, factoryMock.Object);
            rootScope.Load(new[]{
                new DHCPv4ScopeAddedEvent
                {
                    Instructions = new DHCPv4ScopeCreateInstruction
                    {
                        Id = id,
                        ResolverInformation = new CreateScopeResolverInformation
                        {
                            Typename = resolverName,
                        }
                    }
                }
            });

            var command = new DeleteDHCPv4ScopeCommand(random.NextGuid(), random.NextBoolean());

            var handler = new DeleteDHCPv4ScopeCommandHandler(Mock.Of<IDHCPv4StorageEngine>(MockBehavior.Strict), rootScope,
                Mock.Of<ILogger<DeleteDHCPv4ScopeCommandHandler>>());

            Boolean result = await handler.Handle(command, CancellationToken.None);
            Assert.False(result);

            Assert.Empty(rootScope.GetChanges());
        }
    }
}
