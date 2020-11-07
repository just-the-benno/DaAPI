using DaAPI.Core.Common;
using DaAPI.Core.Scopes.DHCPv4;
using DaAPI.Core.Services;
using DaAPI.TestHelper;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using static DaAPI.Core.Scopes.DHCPv4.DHCPv4ScopeAddressProperties;
using static DaAPI.Core.Scopes.DHCPv4.DHCPv4ScopeEvents;

namespace DaAPI.UnitTests.Core.Scopes.DHCPv4
{
    public class DHCPv4ScopeTester
    {
        private DHCPv4RootScope GetRootScope() =>
        new DHCPv4RootScope(Guid.NewGuid(), Mock.Of<IDHCPv4ScopeResolverManager>());

        [Fact]
        public void DHCPv4Scope_ScopePropertiesInherientce()
        {
            Random random = new Random();

            Byte onylGrandParentOptionIdentifier = 47;
            Byte onlyParentOptionIdentifier = 55;
            Byte onlyChildOptionIdentifier = 90;
            Byte overridenByParentOptionIdentifier = 100;
            Byte overridenByChildOptionIdentifier = 110;

            Dictionary<Byte, IPv4Address> inputs = new Dictionary<byte, IPv4Address>
            {
                { onylGrandParentOptionIdentifier,random.GetIPv4Address()  },
                { onlyParentOptionIdentifier,random.GetIPv4Address()  },
                { onlyChildOptionIdentifier,random.GetIPv4Address()  },
                { overridenByParentOptionIdentifier,random.GetIPv4Address()  },
                { overridenByChildOptionIdentifier,random.GetIPv4Address()  },
            };

            DHCPv4ScopeProperties grantParentProperties = new DHCPv4ScopeProperties(
                new DHCPv4AddressScopeProperty(onylGrandParentOptionIdentifier, inputs[onylGrandParentOptionIdentifier]),
                new DHCPv4AddressScopeProperty(overridenByParentOptionIdentifier, random.GetIPv4Address()),
                new DHCPv4AddressScopeProperty(overridenByChildOptionIdentifier, random.GetIPv4Address())
                );

            DHCPv4ScopeProperties parentProperties = new DHCPv4ScopeProperties(
                new DHCPv4AddressScopeProperty(onlyParentOptionIdentifier, inputs[onlyParentOptionIdentifier]),
                new DHCPv4AddressScopeProperty(overridenByParentOptionIdentifier, inputs[overridenByParentOptionIdentifier])
                );

            DHCPv4ScopeProperties childProperties = new DHCPv4ScopeProperties(
                new DHCPv4AddressScopeProperty(onlyChildOptionIdentifier, inputs[onlyChildOptionIdentifier]),
                new DHCPv4AddressScopeProperty(overridenByChildOptionIdentifier, inputs[overridenByChildOptionIdentifier])
                );

            DHCPv4ScopeProperties expectedProperties = new DHCPv4ScopeProperties(
                new DHCPv4AddressScopeProperty(onylGrandParentOptionIdentifier, inputs[onylGrandParentOptionIdentifier]),
                new DHCPv4AddressScopeProperty(onlyParentOptionIdentifier, inputs[onlyParentOptionIdentifier]),
                new DHCPv4AddressScopeProperty(onlyChildOptionIdentifier, inputs[onlyChildOptionIdentifier]),
                new DHCPv4AddressScopeProperty(overridenByParentOptionIdentifier, inputs[overridenByParentOptionIdentifier]),
                new DHCPv4AddressScopeProperty(overridenByChildOptionIdentifier, inputs[overridenByChildOptionIdentifier])
                );

            Guid grantParentId = Guid.NewGuid();
            Guid parentId = Guid.NewGuid();
            Guid childId = Guid.NewGuid();

            List<DomainEvent> events = new List<DomainEvent>
            {
                 new DHCPv4ScopeAddedEvent(new DHCPv4ScopeCreateInstruction
                 {
                     Id = grantParentId,
                     Properties = grantParentProperties,
                 }),
                 new DHCPv4ScopeAddedEvent(new DHCPv4ScopeCreateInstruction
                 {
                     Id = parentId,
                     ParentId = grantParentId,
                     Properties = parentProperties,
                 }),
                 new DHCPv4ScopeAddedEvent(new DHCPv4ScopeCreateInstruction
                 {
                     Id = childId,
                     ParentId = parentId,
                     Properties = childProperties,
                 }),
            };

            DHCPv4RootScope rootScope = GetRootScope();
            rootScope.Load(events);

            DHCPv4Scope scope = rootScope.GetScopeById(childId);
            DHCPv4ScopeProperties actualProperties = scope.GetScopeProperties();

            Assert.Equal(expectedProperties, actualProperties);
        }


        [Fact]
        public void DHCPv4Scope_AddressPropertiesInherientce()
        {
            Random random = new Random();

            for (int i = 0; i < 100; i++)
            {

                IPv4Address grantParentStart = random.GetIPv4Address();
                IPv4Address grantParentEnd = random.GetIPv4AddressGreaterThan(grantParentStart);
                List<IPv4Address> grantParentExcludedAddresses = random.GetIPv4AddressesBetween(grantParentStart, grantParentEnd);

                TimeSpan grantParentValidLifeTime = TimeSpan.FromMinutes(random.Next());
                TimeSpan grantParentPrefferedValidLifeTime = TimeSpan.FromMinutes(random.Next());
                TimeSpan grantParentRenewalTimePrefferedValidLifeTime = TimeSpan.FromMinutes(random.Next());

                Boolean grantParentReuseAddressIfPossible = random.NextBoolean();
                DHCPv4AddressAllocationStrategies grantParentAllocationStrategy = DHCPv4AddressAllocationStrategies.Next;

                Boolean grantParentSupportDirectUnicast = random.NextBoolean();
                Boolean grantParentAcceptDecline = random.NextBoolean();
                Boolean grantParentInformsAreAllowd = random.NextBoolean();

                DHCPv4ScopeAddressProperties grantParentProperties = new DHCPv4ScopeAddressProperties(
                grantParentStart, grantParentEnd, grantParentExcludedAddresses,
                grantParentRenewalTimePrefferedValidLifeTime, grantParentPrefferedValidLifeTime, grantParentValidLifeTime,
                grantParentReuseAddressIfPossible, grantParentAllocationStrategy,
                grantParentInformsAreAllowd, grantParentAcceptDecline, grantParentInformsAreAllowd);

                IPv4Address parentStart = random.GetIPv4Address();
                IPv4Address parentEnd = random.GetIPv4AddressGreaterThan(parentStart);
                List<IPv4Address> parentExcludedAddresses = random.GetIPv4AddressesBetween(parentStart, parentEnd);

                TimeSpan? parentValidLifeTime = null;
                TimeSpan? parentPrefferedValidLifeTime = null;
                TimeSpan? parentRenewalTimePrefferedValidLifeTime = null;

                Boolean? parentReuseAddressIfPossible = null;
                DHCPv4AddressAllocationStrategies? parentAllocationStrategy = null;

                Boolean? parentSupportDirectUnicast = null;
                Boolean? parentAcceptDecline = null;
                Boolean? parentInformsAreAllowd = null;

                if (random.NextBoolean() == true)
                {
                    parentValidLifeTime = TimeSpan.FromMinutes(random.Next());
                }
                if (random.NextBoolean() == true)
                {
                    parentPrefferedValidLifeTime = TimeSpan.FromMinutes(random.Next());
                }
                if (random.NextBoolean() == true)
                {
                    parentRenewalTimePrefferedValidLifeTime = TimeSpan.FromMinutes(random.Next());
                }
                if (random.NextBoolean() == true)
                {
                    parentReuseAddressIfPossible = random.NextBoolean();
                }
                if (random.NextBoolean() == true)
                {
                    parentAllocationStrategy = DHCPv4AddressAllocationStrategies.Random;
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

                DHCPv4ScopeAddressProperties parentProperties = new DHCPv4ScopeAddressProperties(
               parentStart, parentEnd, parentExcludedAddresses,
               parentRenewalTimePrefferedValidLifeTime, parentPrefferedValidLifeTime, parentValidLifeTime,
               parentReuseAddressIfPossible, parentAllocationStrategy,
               parentInformsAreAllowd, parentAcceptDecline, parentInformsAreAllowd);

                IPv4Address childStart = random.GetIPv4Address();
                IPv4Address childEnd = random.GetIPv4AddressGreaterThan(childStart);
                List<IPv4Address> childExcludedAddresses = random.GetIPv4AddressesBetween(childStart, childEnd);

                TimeSpan? childValidLifeTime = null;
                TimeSpan? childPrefferedValidLifeTime = null;
                TimeSpan? childRenewalTimePrefferedValidLifeTime = null;

                Boolean? childReuseAddressIfPossible = null;
                DHCPv4AddressAllocationStrategies? childAllocationStrategy = null;

                Boolean? childSupportDirectUnicast = random.NextDouble() > 0.5;
                Boolean? childAcceptDecline = random.NextDouble() > 0.5;
                Boolean? childInformsAreAllowd = random.NextDouble() > 0.5;

                if (random.NextBoolean() == true)
                {
                    childValidLifeTime = TimeSpan.FromMinutes(random.Next());
                }
                if (random.NextBoolean() == true)
                {
                    childPrefferedValidLifeTime = TimeSpan.FromMinutes(random.Next());
                }
                if (random.NextBoolean() == true)
                {
                    childRenewalTimePrefferedValidLifeTime = TimeSpan.FromMinutes(random.Next());
                }
                if (random.NextBoolean() == true)
                {
                    childReuseAddressIfPossible = random.NextBoolean();
                }
                if (random.NextBoolean() == true)
                {
                    childAllocationStrategy = DHCPv4AddressAllocationStrategies.Random;
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

                DHCPv4ScopeAddressProperties childProperties = new DHCPv4ScopeAddressProperties(
               childStart, childEnd, childExcludedAddresses,
               childRenewalTimePrefferedValidLifeTime, childPrefferedValidLifeTime, childValidLifeTime,
               childReuseAddressIfPossible, childAllocationStrategy,
               childSupportDirectUnicast, childAcceptDecline, childInformsAreAllowd);

                Guid grantParentId = Guid.NewGuid();
                Guid parentId = Guid.NewGuid();
                Guid childId = Guid.NewGuid();

                List<DomainEvent> events = new List<DomainEvent>
            {
                 new DHCPv4ScopeAddedEvent(new DHCPv4ScopeCreateInstruction
                 {
                     Id = grantParentId,
                     AddressProperties = grantParentProperties,
                 }),
                 new DHCPv4ScopeAddedEvent(new DHCPv4ScopeCreateInstruction
                 {
                     Id = parentId,
                     ParentId = grantParentId,
                     AddressProperties = parentProperties,
                 }),
                 new DHCPv4ScopeAddedEvent(new DHCPv4ScopeCreateInstruction
                 {
                     Id = childId,
                     ParentId = parentId,
                     AddressProperties = childProperties,
                 }),
            };

                DHCPv4RootScope rootScope = GetRootScope();
                rootScope.Load(events);

                DHCPv4Scope scope = rootScope.GetScopeById(childId);
                DHCPv4ScopeAddressProperties actualProperties = scope.GetAddressProperties();

                DHCPv4ScopeAddressProperties expectedProperties = new DHCPv4ScopeAddressProperties(
                    childStart, childEnd, grantParentExcludedAddresses.Union(parentExcludedAddresses).Union(childExcludedAddresses).Where(x => x.IsInBetween(childStart, childEnd)),
                    childRenewalTimePrefferedValidLifeTime.HasValue == true ? childRenewalTimePrefferedValidLifeTime.Value : (parentRenewalTimePrefferedValidLifeTime.HasValue == true ? parentRenewalTimePrefferedValidLifeTime.Value : grantParentRenewalTimePrefferedValidLifeTime),
                    childPrefferedValidLifeTime.HasValue == true ? childPrefferedValidLifeTime.Value : (parentPrefferedValidLifeTime.HasValue == true ? parentPrefferedValidLifeTime.Value : grantParentPrefferedValidLifeTime),
                    childValidLifeTime.HasValue == true ? childValidLifeTime.Value : (parentValidLifeTime.HasValue == true ? parentValidLifeTime.Value : grantParentValidLifeTime),
                    childReuseAddressIfPossible.HasValue == true ? childReuseAddressIfPossible.Value : (parentReuseAddressIfPossible.HasValue == true ? parentReuseAddressIfPossible.Value : grantParentReuseAddressIfPossible),
                    childAllocationStrategy.HasValue == true ? childAllocationStrategy.Value : (parentAllocationStrategy.HasValue == true ? parentAllocationStrategy.Value : grantParentAllocationStrategy),
                    childSupportDirectUnicast.HasValue == true ? childSupportDirectUnicast.Value : (parentSupportDirectUnicast.HasValue == true ? parentSupportDirectUnicast.Value : grantParentSupportDirectUnicast),
                    childAcceptDecline.HasValue == true ? childAcceptDecline.Value : (parentAcceptDecline.HasValue == true ? parentAcceptDecline.Value : grantParentAcceptDecline),
                    childInformsAreAllowd.HasValue == true ? childInformsAreAllowd.Value : (parentInformsAreAllowd.HasValue == true ? parentInformsAreAllowd.Value : grantParentInformsAreAllowd)
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

                events.Add(new DHCPv4ScopeAddedEvent(new DHCPv4ScopeCreateInstruction
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

                events.Add(new DHCPv4ScopeAddedEvent(new DHCPv4ScopeCreateInstruction
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
        public void DHCPv4Scope_GetChildIds()
        {
            Random random = new Random();
            GenerateScopeTree(random, out Dictionary<Guid, List<Guid>> directChildRelations, out Dictionary<Guid, List<Guid>> allChildRelations, out List<DomainEvent> events);

            DHCPv4RootScope rootScope = GetRootScope();
            rootScope.Load(events);

            foreach (var item in directChildRelations)
            {
                DHCPv4Scope scope = rootScope.GetScopeById(item.Key);
                IEnumerable<Guid> actualDirectIds = scope.GetChildIds(true);

                Assert.Equal(item.Value.OrderBy(x => x), actualDirectIds.OrderBy(x => x));

                IEnumerable<Guid> allChildIds = scope.GetChildIds(false);

                Assert.Equal(allChildRelations[item.Key].OrderBy(x => x), allChildIds.OrderBy(x => x));
            }
        }

        [Fact]
        public void DHCPv4Scope_GetChildScopes()
        {
            Random random = new Random();
            
            GenerateScopeTree(random, out Dictionary<Guid, List<Guid>> directChildRelations, out Dictionary<Guid, List<Guid>> allChildRelations, out List<DomainEvent> events);

            DHCPv4RootScope rootScope = GetRootScope();
            rootScope.Load(events);

            foreach (var item in directChildRelations)
            {
                DHCPv4Scope scope = rootScope.GetScopeById(item.Key);
                IEnumerable<DHCPv4Scope> childScopes = scope.GetChildScopes();

                List<Guid> childScopesId = childScopes.Select(x => x.Id).ToList();

                Assert.Equal(item.Value.OrderBy(x => x), childScopesId.OrderBy(x => x));
            }
        }
    }
}
