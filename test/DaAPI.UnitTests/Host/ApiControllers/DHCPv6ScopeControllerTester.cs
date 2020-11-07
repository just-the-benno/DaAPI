using DaAPI.Core.Common;
using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Packets.DHCPv6;
using DaAPI.Core.Scopes;
using DaAPI.Core.Scopes.DHCPv6;
using DaAPI.Host.ApiControllers;
using DaAPI.Host.Application.Commands.DHCPv6Scopes;
using DaAPI.TestHelper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
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
using static DaAPI.Shared.Requests.DHCPv6ScopeRequests.V1;
using static DaAPI.Shared.Responses.DHCPv6ScopeResponses.V1;

namespace DaAPI.UnitTests.Host.ApiControllers
{
    public class DHCPv6ScopeControllerTester
    {

        private void GenerateScopeTree(
   Double randomValue, Random random, List<Guid> parents,
   ICollection<DomainEvent> events
   )
        {
            if (randomValue > 0)
            {
                return;
            }

            Int32 scopeAmount = random.Next(3, 10);
            Guid directParentId = parents.Last();
            for (int i = 0; i < scopeAmount; i++)
            {
                Guid scopeId = Guid.NewGuid();
                IPv6Address start = random.GetIPv6Address();
                IPv6Address end = start + 100;

                events.Add(new DHCPv6ScopeAddedEvent(new DHCPv6ScopeCreateInstruction
                {
                    Id = scopeId,
                    Name = random.GetAlphanumericString(),
                    AddressProperties = new DHCPv6ScopeAddressProperties(start, end),
                    ParentId = directParentId,
                }));

                List<Guid> newParentList = new List<Guid>(parents)
                {
                    scopeId
                };

                GenerateScopeTree(
                    randomValue + random.NextDouble(), random,
                    newParentList, events);
            }
        }

        [Fact]
        public void GetScopesAsList()
        {
            Random random = new Random();

            var events = new List<DomainEvent>();
            Int32 rootScopeAmount = random.Next(10, 30);
            List<Guid> rootScopeIds = new List<Guid>(rootScopeAmount);
            for (int i = 0; i < rootScopeAmount; i++)
            {
                Guid scopeId = Guid.NewGuid();
                IPv6Address start = random.GetIPv6Address();
                IPv6Address end = start + 100;

                events.Add(new DHCPv6ScopeAddedEvent(new DHCPv6ScopeCreateInstruction
                {
                    Id = scopeId,
                    Name = random.GetAlphanumericString(),
                    AddressProperties = new DHCPv6ScopeAddressProperties(start, end),
                }));

                rootScopeIds.Add(scopeId);

                GenerateScopeTree(
                    random.NextDouble(), random,
                    new List<Guid> { scopeId }, events);
            }

            var scopeResolverMock = new Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>>(MockBehavior.Strict);
            scopeResolverMock.Setup(x => x.InitializeResolver(It.IsAny<CreateScopeResolverInformation>())).Returns(Mock.Of<IScopeResolver<DHCPv6Packet, IPv6Address>>());

            Mock<ILoggerFactory> factoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);
            factoryMock.Setup(x => x.CreateLogger(It.IsAny<String>())).Returns(Mock.Of<ILogger<DHCPv6RootScope>>());

            DHCPv6RootScope rootScope = new DHCPv6RootScope(random.NextGuid(), scopeResolverMock.Object, factoryMock.Object);
            rootScope.Load(events);

            var controller = new DHCPv6ScopeController(
                Mock.Of<IMediator>(MockBehavior.Strict),
                Mock.Of<IScopeResolverManager<DHCPv6Packet, IPv6Address>>(MockBehavior.Strict),
                rootScope);
            var actionResult = controller.GetScopesAsList();
            var result = actionResult.EnsureOkObjectResult<IEnumerable<ScopeItem>>(true);

            Assert.Equal(events.Count, result.Count());
            for (int i = 0; i < events.Count; i++)
            {
                var scope = result.ElementAt(i);
                var @event = (DHCPv6ScopeAddedEvent)events[i];
                Assert.Equal(@event.Instructions.Name, scope.Name);
                Assert.Equal(@event.Instructions.Id, scope.Id);
                Assert.Equal(@event.Instructions.AddressProperties.Start.ToString(), scope.StartAddress);
                Assert.Equal(@event.Instructions.AddressProperties.End.ToString(), scope.EndAddress);
            }
        }

        private void CheckTreeItem(DHCPv6Scope item, ScopeTreeViewItem viewItem)
        {
            Assert.Equal(item.Name, viewItem.Name);
            Assert.Equal(item.Id, viewItem.Id);
            Assert.Equal(item.AddressRelatedProperties.Start.ToString(), viewItem.StartAddress);
            Assert.Equal(item.AddressRelatedProperties.End.ToString(), viewItem.EndAddress);

            if (item.GetChildScopes().Any() == true)
            {
                Assert.Equal(item.GetChildScopes().Count(), viewItem.ChildScopes.Count());
                Int32 index = 0;
                foreach (var childScope in item.GetChildScopes())
                {
                    var childViewItem = viewItem.ChildScopes.ElementAt(index);
                    CheckTreeItem(childScope, childViewItem);

                    index++;
                }
            }
            else
            {
                Assert.Empty(viewItem.ChildScopes);
            }
        }

        [Fact]
        public void GetScopesAsTreeView()
        {
            Random random = new Random();

            var events = new List<DomainEvent>();
            Int32 rootScopeAmount = random.Next(10, 30);
            List<Guid> rootScopeIds = new List<Guid>(rootScopeAmount);
            for (int i = 0; i < rootScopeAmount; i++)
            {
                Guid scopeId = Guid.NewGuid();
                IPv6Address start = random.GetIPv6Address();
                IPv6Address end = start + 100;

                events.Add(new DHCPv6ScopeAddedEvent(new DHCPv6ScopeCreateInstruction
                {
                    Id = scopeId,
                    Name = random.GetAlphanumericString(),
                    AddressProperties = new DHCPv6ScopeAddressProperties(start, end),
                }));

                rootScopeIds.Add(scopeId);

                GenerateScopeTree(
                    random.NextDouble(), random,
                    new List<Guid> { scopeId }, events);
            }

            var scopeResolverMock = new Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>>(MockBehavior.Strict);
            scopeResolverMock.Setup(x => x.InitializeResolver(It.IsAny<CreateScopeResolverInformation>())).Returns(Mock.Of<IScopeResolver<DHCPv6Packet, IPv6Address>>());

            Mock<ILoggerFactory> factoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);
            factoryMock.Setup(x => x.CreateLogger(It.IsAny<String>())).Returns(Mock.Of<ILogger<DHCPv6RootScope>>());

            DHCPv6RootScope rootScope = new DHCPv6RootScope(random.NextGuid(), scopeResolverMock.Object, factoryMock.Object);
            rootScope.Load(events);

            var controller = new DHCPv6ScopeController(
                Mock.Of<IMediator>(MockBehavior.Strict),
                Mock.Of<IScopeResolverManager<DHCPv6Packet, IPv6Address>>(MockBehavior.Strict),
                rootScope);
            var actionResult = controller.GetScopesAsTreeView();
            var result = actionResult.EnsureOkObjectResult<IEnumerable<ScopeTreeViewItem>>(true);

            Assert.Equal(rootScopeAmount, result.Count());
            Int32 index = 0;
            foreach (var item in rootScope.GetRootScopes())
            {
                var scope = result.ElementAt(index);

                CheckTreeItem(item, scope);

                index++;
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task CreateScope(Boolean mediatorResultShouldBeSuccessful)
        {
            Random random = new Random();
            Guid? id = mediatorResultShouldBeSuccessful == true ? random.NextGuid() : new Guid?();

            String name = random.GetAlphanumericString();
            String description = random.GetAlphanumericString();
            Guid? parentId = random.NextBoolean() == true ? random.NextGuid() : new Guid?();

            var addressProperties = new DHCPv6ScopeAddressPropertyReqest();
            var resolverInfo = new CreateScopeResolverRequest
            {
                Typename = random.GetAlphanumericString(),
                PropertiesAndValues = new Dictionary<String, String>()
            };

            Mock<ILoggerFactory> factoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);
            factoryMock.Setup(x => x.CreateLogger(It.IsAny<String>())).Returns(Mock.Of<ILogger<DHCPv6RootScope>>());

            DHCPv6RootScope rootScope = new DHCPv6RootScope(random.NextGuid(), Mock.Of<IScopeResolverManager<DHCPv6Packet, IPv6Address>>(MockBehavior.Strict), factoryMock.Object);

            Mock<IMediator> mediatorMock = new Mock<IMediator>(MockBehavior.Strict);
            mediatorMock.Setup(x => x.Send(It.Is<CreateDHCPv6ScopeCommand>(y =>
            y.Name == name && y.Description == description && y.ParentId == parentId &&
            y.AddressProperties == addressProperties && y.Resolver == resolverInfo
            ), It.IsAny<CancellationToken>())).ReturnsAsync(id).Verifiable();

            var request = new CreateOrUpdateDHCPv6ScopeRequest
            {
                Name = name,
                Description = description,
                AddressProperties = addressProperties,
                ParentId = parentId,
                Resolver = resolverInfo,
            };

            var controller = new DHCPv6ScopeController(mediatorMock.Object,
                Mock.Of<IScopeResolverManager<DHCPv6Packet, IPv6Address>>(MockBehavior.Strict),
                rootScope);

            var actionResult = await controller.CreateScope(request);
            if (mediatorResultShouldBeSuccessful == true)
            {
                Guid result = actionResult.EnsureOkObjectResult<Guid>(true);
                Assert.Equal(id, result);
            }
            else
            {
                actionResult.EnsureBadRequestObjectResult("unable to create scope");
            }

            mediatorMock.Verify();
        }

        private async Task CheckModelState(Func<DHCPv6ScopeController, Task<IActionResult>> controllerExecuter)
        {
            Mock<ILoggerFactory> factoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);
            factoryMock.Setup(x => x.CreateLogger(It.IsAny<String>())).Returns(Mock.Of<ILogger<DHCPv6RootScope>>());

            Random random = new Random();
            DHCPv6RootScope rootScope = new DHCPv6RootScope(random.NextGuid(), Mock.Of<IScopeResolverManager<DHCPv6Packet, IPv6Address>>(MockBehavior.Strict), factoryMock.Object);

            var controller = new DHCPv6ScopeController(
                Mock.Of<IMediator>(MockBehavior.Strict),
                Mock.Of<IScopeResolverManager<DHCPv6Packet, IPv6Address>>(MockBehavior.Strict),
                rootScope
                );

            String modelErrorKey = "a" + random.GetAlphanumericString();
            String modelErrorMessage = random.GetAlphanumericString();
            controller.ModelState.AddModelError(modelErrorKey, modelErrorMessage);

            var result = await controllerExecuter(controller);

            result.EnsureBadRequestObjectResultForError(modelErrorKey, modelErrorMessage);
        }

        [Fact]
        public async Task CreateScope_ModelStateError()
        {
            await CheckModelState((controller) => controller.CreateScope(null));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Delete(Boolean mediatorResultShouldBeSuccessful)
        {
            Random random = new Random();

            Guid id = random.NextGuid();
            Boolean includeChildren = random.NextBoolean();

            Mock<ILoggerFactory> factoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);
            factoryMock.Setup(x => x.CreateLogger(It.IsAny<String>())).Returns(Mock.Of<ILogger<DHCPv6RootScope>>());

            DHCPv6RootScope rootScope = new DHCPv6RootScope(random.NextGuid(), Mock.Of<IScopeResolverManager<DHCPv6Packet, IPv6Address>>(MockBehavior.Strict), factoryMock.Object);

            Mock<IMediator> mediatorMock = new Mock<IMediator>(MockBehavior.Strict);
            mediatorMock.Setup(x => x.Send(It.Is<DeleteDHCPv6ScopeCommand>(y =>
            y.ScopeId == id && y.IncludeChildren == includeChildren
            ), It.IsAny<CancellationToken>())).ReturnsAsync(mediatorResultShouldBeSuccessful).Verifiable();

            var controller = new DHCPv6ScopeController(mediatorMock.Object,
                Mock.Of<IScopeResolverManager<DHCPv6Packet, IPv6Address>>(MockBehavior.Strict),
                rootScope);

            var actionResult = await controller.DeleteScope(id, includeChildren);
            if (mediatorResultShouldBeSuccessful == true)
            {
                actionResult.EnsureNoContentResult();
            }
            else
            {
                actionResult.EnsureBadRequestObjectResult("unable to execute service operation");
            }

            mediatorMock.Verify();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task UpdateScope(Boolean mediatorResultShouldBeSuccessful)
        {
            Random random = new Random();

            Guid id = random.NextGuid();
            String name = random.GetAlphanumericString();
            String description = random.GetAlphanumericString();
            Guid? parentId = random.NextBoolean() == true ? random.NextGuid() : new Guid?();

            var addressProperties = new DHCPv6ScopeAddressPropertyReqest();
            var resolverInfo = new CreateScopeResolverRequest
            {
                Typename = random.GetAlphanumericString(),
                PropertiesAndValues = new Dictionary<String, String>()
            };

            Mock<ILoggerFactory> factoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);
            factoryMock.Setup(x => x.CreateLogger(It.IsAny<String>())).Returns(Mock.Of<ILogger<DHCPv6RootScope>>());

            DHCPv6RootScope rootScope = new DHCPv6RootScope(random.NextGuid(), Mock.Of<IScopeResolverManager<DHCPv6Packet, IPv6Address>>(MockBehavior.Strict), factoryMock.Object);

            Mock<IMediator> mediatorMock = new Mock<IMediator>(MockBehavior.Strict);
            mediatorMock.Setup(x => x.Send(It.Is<UpdateDHCPv6ScopeCommand>(y =>
            y.ScopeId == id &&
            y.Name == name && y.Description == description && y.ParentId == parentId &&
            y.AddressProperties == addressProperties && y.Resolver == resolverInfo
            ), It.IsAny<CancellationToken>())).ReturnsAsync(mediatorResultShouldBeSuccessful).Verifiable();

            var request = new CreateOrUpdateDHCPv6ScopeRequest
            {
                Name = name,
                Description = description,
                AddressProperties = addressProperties,
                ParentId = parentId,
                Resolver = resolverInfo,
            };

            var controller = new DHCPv6ScopeController(mediatorMock.Object,
                Mock.Of<IScopeResolverManager<DHCPv6Packet, IPv6Address>>(MockBehavior.Strict),
                rootScope);

            var actionResult = await controller.UpdateScope(request, id);
            if (mediatorResultShouldBeSuccessful == true)
            {
                actionResult.EnsureNoContentResult();
            }
            else
            {
                actionResult.EnsureBadRequestObjectResult("unable to execute service operation");
            }

            mediatorMock.Verify();
        }
    }
}
