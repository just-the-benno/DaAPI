using DaAPI.Core.Common;
using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Packets.DHCPv6;
using DaAPI.Core.Scopes;
using DaAPI.Core.Scopes.DHCPv6;
using DaAPI.Core.Scopes.DHCPv6.ScopeProperties;
using DaAPI.TestHelper;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using static DaAPI.Core.Scopes.DHCPv6.DHCPv6ScopeEvents;

namespace DaAPI.UnitTests.Core.Scopes.DHCPv6
{
    public class DHCPv6ScopeTester
    {
        private DHCPv6RootScope GetRootScope()
        {
            Mock<ILoggerFactory> factoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);
            factoryMock.Setup(x => x.CreateLogger(It.IsAny<String>())).Returns(Mock.Of<ILogger<DHCPv6RootScope>>());

            var scope = new DHCPv6RootScope(Guid.NewGuid(), Mock.Of<IScopeResolverManager<DHCPv6Packet, IPv6Address>>(), factoryMock.Object);
            return scope;
        }

        [Fact]
        public void DHCPv6Scope_ScopePropertiesInherientce()
        {
            Random random = new Random();

            Byte onylGrandParentOptionIdentifier = 47;
            Byte onlyParentOptionIdentifier = 55;
            Byte onlyChildOptionIdentifier = 90;
            Byte overridenByParentOptionIdentifier = 100;
            Byte overridenByChildOptionIdentifier = 110;
            Byte deletedByChildOptionIdentifier = 140;

            Dictionary<Byte, String> inputs = new Dictionary<byte, String>
            {
                { onylGrandParentOptionIdentifier,random.GetAlphanumericString()  },
                { onlyParentOptionIdentifier,random.GetAlphanumericString()  },
                { onlyChildOptionIdentifier,random.GetAlphanumericString()  },
                { overridenByParentOptionIdentifier,random.GetAlphanumericString()  },
                { overridenByChildOptionIdentifier,random.GetAlphanumericString()  },
                { deletedByChildOptionIdentifier,random.GetAlphanumericString()  },
            };

            DHCPv6ScopeProperties grantParentProperties = new DHCPv6ScopeProperties(
                new DHCPv6TextScopeProperty(onylGrandParentOptionIdentifier, inputs[onylGrandParentOptionIdentifier]),
                new DHCPv6TextScopeProperty(overridenByParentOptionIdentifier, random.GetAlphanumericString()),
                new DHCPv6TextScopeProperty(overridenByChildOptionIdentifier, random.GetAlphanumericString()),
                new DHCPv6TextScopeProperty(deletedByChildOptionIdentifier, random.GetAlphanumericString())
                );

            DHCPv6ScopeProperties parentProperties = new DHCPv6ScopeProperties(
                new DHCPv6TextScopeProperty(onlyParentOptionIdentifier, inputs[onlyParentOptionIdentifier]),
                new DHCPv6TextScopeProperty(overridenByParentOptionIdentifier, inputs[overridenByParentOptionIdentifier]),
                new DHCPv6TextScopeProperty(deletedByChildOptionIdentifier, random.GetAlphanumericString())
                );

            DHCPv6ScopeProperties childProperties = new DHCPv6ScopeProperties(
                new DHCPv6TextScopeProperty(onlyChildOptionIdentifier, inputs[onlyChildOptionIdentifier]),
                new DHCPv6TextScopeProperty(overridenByChildOptionIdentifier, inputs[overridenByChildOptionIdentifier])
                );

            childProperties.RemoveFromInheritance(deletedByChildOptionIdentifier);

            DHCPv6ScopeProperties expectedProperties = new DHCPv6ScopeProperties(
                new DHCPv6TextScopeProperty(onylGrandParentOptionIdentifier, inputs[onylGrandParentOptionIdentifier]),
                new DHCPv6TextScopeProperty(onlyParentOptionIdentifier, inputs[onlyParentOptionIdentifier]),
                new DHCPv6TextScopeProperty(onlyChildOptionIdentifier, inputs[onlyChildOptionIdentifier]),
                new DHCPv6TextScopeProperty(overridenByParentOptionIdentifier, inputs[overridenByParentOptionIdentifier]),
                new DHCPv6TextScopeProperty(overridenByChildOptionIdentifier, inputs[overridenByChildOptionIdentifier])
                );

            Guid grantParentId = Guid.NewGuid();
            Guid parentId = Guid.NewGuid();
            Guid childId = Guid.NewGuid();

            List<DomainEvent> events = new List<DomainEvent>
            {
                 new DHCPv6ScopeAddedEvent(new DHCPv6ScopeCreateInstruction
                 {
                     Id = grantParentId,
                     ScopeProperties = grantParentProperties,
                 }),
                 new DHCPv6ScopeAddedEvent(new DHCPv6ScopeCreateInstruction
                 {
                     Id = parentId,
                     ParentId = grantParentId,
                     ScopeProperties = parentProperties,
                 }),
                 new DHCPv6ScopeAddedEvent(new DHCPv6ScopeCreateInstruction
                 {
                     Id = childId,
                     ParentId = parentId,
                     ScopeProperties = childProperties,
                 }),
            };

            DHCPv6RootScope rootScope = GetRootScope();
            rootScope.Load(events);

            var scope = rootScope.GetScopeById(childId);
            var actualProperties = scope.GetScopeProperties();

            Assert.Equal(expectedProperties, actualProperties);
        }

        [Fact]
        public void DHCPv6Scope_AddressPropertiesInherientce()
        {
            Random random = new Random();

            for (int i = 0; i < 100; i++)
            {

                IPv6Address grantParentStart = random.GetIPv6Address();
                IPv6Address grantParentEnd = random.GetIPv6AddressGreaterThan(grantParentStart);
                List<IPv6Address> grantParentExcludedAddresses = random.GetIPv6AddressesBetween(grantParentStart, grantParentEnd);

                DHCPv6TimeScale grantParentT1 = DHCPv6TimeScale.FromDouble(0.2);
                DHCPv6TimeScale grantParentT2 = DHCPv6TimeScale.FromDouble(0.6);
                TimeSpan grantParentPreferredLifeTime = TimeSpan.FromMinutes(random.Next(10, 30));
                TimeSpan grantParentValuidLifeTime = TimeSpan.FromMinutes(random.Next(40, 60));

                Boolean grantParentReuseAddressIfPossible = random.NextBoolean();
                var grantParentAllocationStrategy = DHCPv6ScopeAddressProperties.AddressAllocationStrategies.Next;

                Boolean grantParentSupportDirectUnicast = random.NextBoolean();
                Boolean grantParentAcceptDecline = random.NextBoolean();
                Boolean grantParentInformsAreAllowd = random.NextBoolean();

                DHCPv6ScopeAddressProperties grantParentProperties = new DHCPv6ScopeAddressProperties(
                grantParentStart, grantParentEnd, grantParentExcludedAddresses,
                grantParentT1, grantParentT2,
                grantParentPreferredLifeTime, grantParentValuidLifeTime,
                grantParentReuseAddressIfPossible, grantParentAllocationStrategy,
                grantParentInformsAreAllowd, grantParentAcceptDecline, grantParentInformsAreAllowd,
                null, DHCPv6PrefixDelgationInfo.FromValues(IPv6Address.FromString("2001:e68:5423:5ffd::0"), new IPv6SubnetMaskIdentifier(64), new IPv6SubnetMaskIdentifier(70)));

                IPv6Address parentStart = random.GetIPv6Address();
                IPv6Address parentEnd = random.GetIPv6AddressGreaterThan(parentStart);
                List<IPv6Address> parentExcludedAddresses = random.GetIPv6AddressesBetween(parentStart, parentEnd);

                TimeSpan? parentPreferedLifeTime = null;
                TimeSpan? parentValuidLifetime = null;
                DHCPv6TimeScale parentT1 = DHCPv6TimeScale.FromDouble(0.3);
                DHCPv6TimeScale parentT2 = null;

                Boolean? parentReuseAddressIfPossible = null;
                DHCPv6ScopeAddressProperties.AddressAllocationStrategies? parentAllocationStrategy = null;

                Boolean? parentSupportDirectUnicast = null;
                Boolean? parentAcceptDecline = null;
                Boolean? parentInformsAreAllowd = null;

                if (random.NextBoolean() == true)
                {
                    parentPreferedLifeTime = TimeSpan.FromMinutes(random.Next());
                }
                if (random.NextBoolean() == true)
                {
                    parentValuidLifetime = TimeSpan.FromMinutes(random.Next());
                }

                if (random.NextBoolean() == true)
                {
                    parentReuseAddressIfPossible = random.NextBoolean();
                }
                if (random.NextBoolean() == true)
                {
                    parentAllocationStrategy = DHCPv6ScopeAddressProperties.AddressAllocationStrategies.Random;
                }
                if (random.NextBoolean() == true)
                {
                    parentSupportDirectUnicast = random.NextBoolean();
                }
                if (random.NextBoolean() == true)
                {
                    parentAcceptDecline = random.NextBoolean();
                }
                if (random.NextBoolean() == true)
                {
                    parentInformsAreAllowd = random.NextBoolean();
                }

                DHCPv6ScopeAddressProperties parentProperties = new DHCPv6ScopeAddressProperties(
               parentStart, parentEnd, parentExcludedAddresses,
               parentT1, parentT2,
               parentPreferedLifeTime, parentValuidLifetime,
               parentReuseAddressIfPossible, parentAllocationStrategy,
               parentInformsAreAllowd, parentAcceptDecline, parentInformsAreAllowd,
               null, DHCPv6PrefixDelgationInfo.FromValues(IPv6Address.FromString("2001:e68:5423:5ffe::0"), new IPv6SubnetMaskIdentifier(64), new IPv6SubnetMaskIdentifier(70))
               );

                IPv6Address childStart = random.GetIPv6Address();
                IPv6Address childEnd = random.GetIPv6AddressGreaterThan(childStart);
                List<IPv6Address> childExcludedAddresses = random.GetIPv6AddressesBetween(childStart, childEnd);

                DHCPv6TimeScale childT1 = null;
                DHCPv6TimeScale childT2 = DHCPv6TimeScale.FromDouble(0.9);

                TimeSpan? childPreferredLifeTime = null;
                TimeSpan? childValidLifeTime = null;

                Boolean? childReuseAddressIfPossible = null;
                DHCPv6ScopeAddressProperties.AddressAllocationStrategies? childAllocationStrategy = null;

                Boolean? childSupportDirectUnicast = random.NextDouble() > 0.5;
                Boolean? childAcceptDecline = random.NextDouble() > 0.5;
                Boolean? childInformsAreAllowd = random.NextDouble() > 0.5;

                if (random.NextBoolean() == true)
                {
                    childPreferredLifeTime = TimeSpan.FromMinutes(random.Next());
                }
                if (random.NextBoolean() == true)
                {
                    childValidLifeTime = TimeSpan.FromMinutes(random.Next());
                }
                if (random.NextBoolean() == true)
                {
                    childReuseAddressIfPossible = random.NextBoolean();
                }
                if (random.NextBoolean() == true)
                {
                    childAllocationStrategy = DHCPv6ScopeAddressProperties.AddressAllocationStrategies.Random;
                }
                if (random.NextBoolean() == true)
                {
                    childSupportDirectUnicast = random.NextBoolean();
                }
                if (random.NextBoolean() == true)
                {
                    childAcceptDecline = random.NextBoolean();
                }
                if (random.NextBoolean() == true)
                {
                    childInformsAreAllowd = random.NextBoolean();
                }

                var childPrefixDelegationInfo = DHCPv6PrefixDelgationInfo.FromValues(IPv6Address.FromString("2001:e68:5423:5ffe::0"), new IPv6SubnetMaskIdentifier(64), new IPv6SubnetMaskIdentifier(70));

                DHCPv6ScopeAddressProperties childProperties = new DHCPv6ScopeAddressProperties(
               childStart, childEnd, childExcludedAddresses,
               childT1, childT2,
               childPreferredLifeTime, childValidLifeTime,
               childReuseAddressIfPossible, childAllocationStrategy,
               childSupportDirectUnicast, childAcceptDecline, childInformsAreAllowd,
               null, childPrefixDelegationInfo
               );

                Guid grantParentId = Guid.NewGuid();
                Guid parentId = Guid.NewGuid();
                Guid childId = Guid.NewGuid();

                List<DomainEvent> events = new List<DomainEvent>
            {
                 new DHCPv6ScopeAddedEvent(new DHCPv6ScopeCreateInstruction
                 {
                     Id = grantParentId,
                     AddressProperties = grantParentProperties,
                 }),
                 new DHCPv6ScopeAddedEvent(new DHCPv6ScopeCreateInstruction
                 {
                     Id = parentId,
                     ParentId = grantParentId,
                     AddressProperties = parentProperties,
                 }),
                 new DHCPv6ScopeAddedEvent(new DHCPv6ScopeCreateInstruction
                 {
                     Id = childId,
                     ParentId = parentId,
                     AddressProperties = childProperties,
                 }),
            };

                DHCPv6RootScope rootScope = GetRootScope();
                rootScope.Load(events);

                DHCPv6Scope scope = rootScope.GetScopeById(childId);
                DHCPv6ScopeAddressProperties actualProperties = scope.GetAddressProperties();

                DHCPv6ScopeAddressProperties expectedProperties = new DHCPv6ScopeAddressProperties(
                    childStart, childEnd, grantParentExcludedAddresses.Union(parentExcludedAddresses).Union(childExcludedAddresses).Where(x => x.IsBetween(childStart, childEnd)),
                    childT1 ?? (parentT1 ?? grantParentT1),
                    childT2 ?? (parentT2 ?? grantParentT2),
                    childPreferredLifeTime.HasValue == true ? childPreferredLifeTime.Value : (parentPreferedLifeTime.HasValue == true ? parentPreferedLifeTime.Value : grantParentPreferredLifeTime),
                    childValidLifeTime.HasValue == true ? childValidLifeTime.Value : (parentValuidLifetime.HasValue == true ? parentValuidLifetime.Value : grantParentValuidLifeTime),
                    childReuseAddressIfPossible.HasValue == true ? childReuseAddressIfPossible.Value : (parentReuseAddressIfPossible.HasValue == true ? parentReuseAddressIfPossible.Value : grantParentReuseAddressIfPossible),
                    childAllocationStrategy.HasValue == true ? childAllocationStrategy.Value : (parentAllocationStrategy.HasValue == true ? parentAllocationStrategy.Value : grantParentAllocationStrategy),
                    childSupportDirectUnicast.HasValue == true ? childSupportDirectUnicast.Value : (parentSupportDirectUnicast.HasValue == true ? parentSupportDirectUnicast.Value : grantParentSupportDirectUnicast),
                    childAcceptDecline.HasValue == true ? childAcceptDecline.Value : (parentAcceptDecline.HasValue == true ? parentAcceptDecline.Value : grantParentAcceptDecline),
                    childInformsAreAllowd.HasValue == true ? childInformsAreAllowd.Value : (parentInformsAreAllowd.HasValue == true ? parentInformsAreAllowd.Value : grantParentInformsAreAllowd),
                    null,
                    childPrefixDelegationInfo
                );

                Assert.Equal(expectedProperties, actualProperties);
            }
        }

        private void GenerateScopeTree(
            Double randomValue, Random random, List<Guid> parents,
            ICollection<DomainEvent> events,
            Dictionary<Guid, List<Guid>> directChildRelations,
            Dictionary<Guid, List<Guid>> allChildRelations
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
                directChildRelations.Add(scopeId, new List<Guid>());
                allChildRelations.Add(scopeId, new List<Guid>());

                directChildRelations[directParentId].Add(scopeId);

                foreach (Guid item in parents)
                {
                    allChildRelations[item].Add(scopeId);
                }

                events.Add(new DHCPv6ScopeAddedEvent(new DHCPv6ScopeCreateInstruction
                {
                    Id = scopeId,
                    ParentId = directParentId,
                }));

                List<Guid> newParentList = new List<Guid>(parents)
                {
                    scopeId
                };

                GenerateScopeTree(
                    randomValue + random.NextDouble(), random,
                    newParentList, events,
                    directChildRelations, allChildRelations);
            }
        }

        private void GenerateScopeTree(Random random, out Dictionary<Guid, List<Guid>> directChildRelations, out Dictionary<Guid, List<Guid>> allChildRelations, out List<DomainEvent> events)
        {
            directChildRelations = new Dictionary<Guid, List<Guid>>();
            allChildRelations = new Dictionary<Guid, List<Guid>>();
            events = new List<DomainEvent>();
            Int32 rootScopeAmount = random.Next(3, 10);
            for (int i = 0; i < rootScopeAmount; i++)
            {
                Guid scopeId = Guid.NewGuid();

                directChildRelations.Add(scopeId, new List<Guid>());
                allChildRelations.Add(scopeId, new List<Guid>());

                events.Add(new DHCPv6ScopeAddedEvent(new DHCPv6ScopeCreateInstruction
                {
                    Id = scopeId,
                }));

                GenerateScopeTree(
                    random.NextDouble(), random,
                    new List<Guid> { scopeId }, events,
                    directChildRelations, allChildRelations);
            }
        }

        [Fact]
        public void GetChildIds()
        {
            Random random = new Random();
            GenerateScopeTree(random, out Dictionary<Guid, List<Guid>> directChildRelations, out Dictionary<Guid, List<Guid>> allChildRelations, out List<DomainEvent> events);

            DHCPv6RootScope rootScope = GetRootScope();
            rootScope.Load(events);

            foreach (var item in directChildRelations)
            {
                DHCPv6Scope scope = rootScope.GetScopeById(item.Key);
                IEnumerable<Guid> actualDirectIds = scope.GetChildIds(true);

                Assert.Equal(item.Value.OrderBy(x => x), actualDirectIds.OrderBy(x => x));

                IEnumerable<Guid> allChildIds = scope.GetChildIds(false);

                Assert.Equal(allChildRelations[item.Key].OrderBy(x => x), allChildIds.OrderBy(x => x));
            }
        }

        [Fact]
        public void GetChildScopes()
        {
            Random random = new Random();

            GenerateScopeTree(random, out Dictionary<Guid, List<Guid>> directChildRelations, out Dictionary<Guid, List<Guid>> allChildRelations, out List<DomainEvent> events);

            DHCPv6RootScope rootScope = GetRootScope();
            rootScope.Load(events);

            foreach (var item in directChildRelations)
            {
                DHCPv6Scope scope = rootScope.GetScopeById(item.Key);
                IEnumerable<DHCPv6Scope> childScopes = scope.GetChildScopes();

                List<Guid> childScopesId = childScopes.Select(x => x.Id).ToList();

                Assert.Equal(item.Value.OrderBy(x => x), childScopesId.OrderBy(x => x));
            }
        }
    }
}

