using DaAPI.Core.Common;
using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Exceptions;
using DaAPI.Core.Notifications.Triggers;
using DaAPI.Core.Packets.DHCPv4;
using DaAPI.Core.Packets.DHCPv6;
using DaAPI.Core.Scopes;
using DaAPI.Core.Scopes.DHCPv6;
using DaAPI.Core.Scopes.DHCPv6.Resolvers;
using DaAPI.Core.Scopes.DHCPv6.ScopeProperties;
using DaAPI.Core.Services;
using DaAPI.TestHelper;
using IdentityServer4.Test;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using static DaAPI.Core.Scopes.DHCPv6.DHCPv6LeaseEvents;
using static DaAPI.Core.Scopes.DHCPv6.DHCPv6ScopeEvents;

namespace DaAPI.UnitTests.Core.Scopes.DHCPv6
{
    public class DHCPv6RootScopeTester
    {
        public DHCPv6RootScope GetRootScope()
        {
            Mock<ILoggerFactory> factoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);
            factoryMock.Setup(x => x.CreateLogger(It.IsAny<String>())).Returns(Mock.Of<ILogger<DHCPv6RootScope>>());

            var scope = new DHCPv6RootScope(Guid.NewGuid(), Mock.Of<IScopeResolverManager<DHCPv6Packet, IPv6Address>>(), factoryMock.Object);
            return scope;

        }

        public DHCPv6RootScope GetRootScope(Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>> mock)
        {
            Mock<ILoggerFactory> factoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);
            factoryMock.Setup(x => x.CreateLogger(It.IsAny<String>())).Returns(Mock.Of<ILogger<DHCPv6RootScope>>());

            var scope = new DHCPv6RootScope(Guid.NewGuid(), mock.Object, factoryMock.Object);
            return scope;
        }



        [Fact]
        public void GetScoopById()
        {
            Random random = new Random();

            List<DomainEvent> events = new List<DomainEvent>();
            Dictionary<Guid, Boolean> expectedResult = new Dictionary<Guid, bool>();
            Int32 existingAmount = random.Next(30, 100);
            for (int i = 0; i < existingAmount; i++)
            {
                Guid scopeId = Guid.NewGuid();
                events.Add(new DHCPv6ScopeAddedEvent(new DHCPv6ScopeCreateInstruction
                {
                    Id = scopeId,
                }));

                expectedResult.Add(scopeId, true);
            }

            Int32 nonExistingAmount = random.Next(30, 100);
            for (int i = 0; i < nonExistingAmount; i++)
            {
                expectedResult.Add(Guid.NewGuid(), false);
            }

            DHCPv6RootScope rootScope = GetRootScope();
            rootScope.Load(events);

            foreach (var item in expectedResult)
            {
                DHCPv6Scope scope = rootScope.GetScopeById(item.Key);

                if (item.Value == true)
                {
                    Assert.NotEqual(DHCPv6Scope.NotFound, scope);
                }
                else
                {
                    Assert.Equal(DHCPv6Scope.NotFound, scope);
                }
            }
        }

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
                    newParentList, events);
            }
        }

        [Fact]
        public void GetRootScopes()
        {
            Random random = new Random();

            var events = new List<DomainEvent>();
            Int32 rootScopeAmount = random.Next(10, 30);
            List<Guid> rootScopeIds = new List<Guid>(rootScopeAmount);
            for (int i = 0; i < rootScopeAmount; i++)
            {
                Guid scopeId = Guid.NewGuid();

                events.Add(new DHCPv6ScopeAddedEvent(new DHCPv6ScopeCreateInstruction
                {
                    Id = scopeId,
                }));

                rootScopeIds.Add(scopeId);

                GenerateScopeTree(
                    random.NextDouble(), random,
                    new List<Guid> { scopeId }, events);
            }

            DHCPv6RootScope rootScope = GetRootScope();
            rootScope.Load(events);

            IEnumerable<DHCPv6Scope> rootScopes = rootScope.GetRootScopes();

            Assert.Equal(rootScopeAmount, rootScopes.Count());
            Assert.Equal(rootScopeIds.OrderBy(x => x), rootScopes.Select(x => x.Id).OrderBy(x => x));
        }

        private TEvent CheckScopeChangesDomainEvent<TEvent>(DHCPv6RootScope rootScope, Action<TEvent> validator, Int32 expectedEventAmount = 1)
            where TEvent : DomainEvent
        {
            var changes = rootScope.GetChanges();
            Assert.Equal(expectedEventAmount, changes.Count());

            DomainEvent @event = changes.First();
            Assert.IsAssignableFrom<TEvent>(@event);

            validator((TEvent)@event);

            return (TEvent)@event;
        }

        private TEvent CheckScopeChangesEvent<TEvent>(
            DHCPv6RootScope rootScope, Guid scopeId, Action<TEvent> validator, Int32 expectedEventAmount = 1)
            where TEvent : EntityBasedDomainEvent
        {
            EntityBasedDomainEvent @event =
                CheckScopeChangesDomainEvent(rootScope, validator, expectedEventAmount);

            EntityBasedDomainEvent preCastedEvent = (EntityBasedDomainEvent)@event;
            Assert.Equal(scopeId, preCastedEvent.EntityId);

            return (TEvent)preCastedEvent;
        }

        [Fact]
        public void UpdateScopeName()
        {
            Random random = new Random();

            String initialName = random.GetAlphanumericString(30);
            Guid scopeId = Guid.NewGuid();

            DHCPv6RootScope rootScope = GetRootScope();
            rootScope.Load(new List<DomainEvent> {
                new DHCPv6ScopeAddedEvent(new DHCPv6ScopeCreateInstruction
            {
                Name = ScopeName.FromString(initialName),
                Id = scopeId,
            })
            });

            String newName = random.GetAlphanumericString(20);

            Boolean result = rootScope.UpdateScopeName(scopeId, ScopeName.FromString(newName));
            Assert.True(result);

            DHCPv6Scope scope = rootScope.GetScopeById(scopeId);
            Assert.NotEqual(DHCPv6Scope.NotFound, scope);

            Assert.Equal(newName, scope.Name);

            CheckScopeChangesEvent<DHCPv6ScopeNameUpdatedEvent>(rootScope, scopeId, (castedEvent) =>
            {
                Assert.Equal(newName, castedEvent.Name);
            });
        }

        [Fact]
        public void UpdateScopeDescription()
        {
            Random random = new Random();

            String initialDescription = random.GetAlphanumericString(30);
            Guid scopeId = Guid.NewGuid();

            DHCPv6RootScope rootScope = GetRootScope();
            rootScope.Load(new List<DomainEvent> {
                new DHCPv6ScopeAddedEvent(new DHCPv6ScopeCreateInstruction
            {
                Description = ScopeDescription.FromString(initialDescription),
                Id = scopeId,
            })
            });

            String newDescription = random.GetAlphanumericString(20);

            Boolean result = rootScope.UpdateScopeDescription(scopeId, ScopeDescription.FromString(newDescription));
            Assert.True(result);

            DHCPv6Scope scope = rootScope.GetScopeById(scopeId);
            Assert.NotEqual(DHCPv6Scope.NotFound, scope);

            Assert.Equal(newDescription, scope.Description);

            CheckScopeChangesEvent<DHCPv6ScopeDescriptionUpdatedEvent>(rootScope, scopeId, (castedEvent) =>
            {
                Assert.Equal(newDescription, castedEvent.Description);
            });
        }

        [Fact]
        public void UpdateScopeResolver_NoLeases()
        {
            Random random = new Random();

            String initialDescription = random.GetAlphanumericString(30);
            Guid scopeId = Guid.NewGuid();

            CreateScopeResolverInformation initalInformation = new CreateScopeResolverInformation
            {
                Typename = "my initial resolver",
            };


            CreateScopeResolverInformation information = new CreateScopeResolverInformation
            {
                Typename = "my mocked resolver",
            };

            Mock<IScopeResolver<DHCPv6Packet, IPv6Address>> resolverMock = new Mock<IScopeResolver<DHCPv6Packet, IPv6Address>>(MockBehavior.Strict);

            Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>> managerMock = new Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>>(MockBehavior.Strict);
            managerMock.Setup(x => x.IsResolverInformationValid(information)).Returns(true);
            managerMock.Setup(x => x.InitializeResolver(information)).Returns(resolverMock.Object);
            managerMock.Setup(x => x.InitializeResolver(initalInformation)).Returns<IScopeResolver<DHCPv6Packet, IPv6Address>>(null);

            DHCPv6RootScope rootScope = GetRootScope(managerMock);
            rootScope.Load(new List<DomainEvent> {
                new DHCPv6ScopeAddedEvent(new DHCPv6ScopeCreateInstruction
            {
                Id = scopeId,
                ResolverInformation = initalInformation,
            })
            });

            Boolean result = rootScope.UpdateScopeResolver(scopeId, information);
            Assert.True(result);

            DHCPv6Scope scope = rootScope.GetScopeById(scopeId);
            Assert.Equal(resolverMock.Object, scope.Resolver);

            CheckScopeChangesEvent<DHCPv6ScopeResolverUpdatedEvent>(rootScope, scopeId,
                (castedEvent) =>
                {
                    Assert.Equal(information, castedEvent.ResolverInformationen);
                }
            );
        }

        [Fact]
        public void UpdateScopeResolver_WithLeases()
        {
            Random random = new Random();

            String initialDescription = random.GetAlphanumericString(30);
            Guid scopeId = Guid.NewGuid();

            CreateScopeResolverInformation initalInformation = new CreateScopeResolverInformation
            {
                Typename = "my initial resolver",
            };

            CreateScopeResolverInformation information = new CreateScopeResolverInformation
            {
                Typename = "my mocked resolver",
            };

            Mock<IScopeResolver<DHCPv6Packet, IPv6Address>> resolverMock = new Mock<IScopeResolver<DHCPv6Packet, IPv6Address>>(MockBehavior.Strict);

            Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>> managerMock = new Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>>(MockBehavior.Strict);
            managerMock.Setup(x => x.IsResolverInformationValid(information)).Returns(true);
            managerMock.Setup(x => x.InitializeResolver(information)).Returns(resolverMock.Object);
            managerMock.Setup(x => x.InitializeResolver(initalInformation)).Returns<IScopeResolver<DHCPv6Packet, IPv6Address>>(null);

            DHCPv6RootScope rootScope = GetRootScope(managerMock);

            List<DomainEvent> events = new List<DomainEvent>
            {
                new DHCPv6ScopeAddedEvent(new DHCPv6ScopeCreateInstruction
                {
                    Id = scopeId,
                    ResolverInformation = initalInformation,
                })
            };

            Dictionary<Guid, Boolean> expectedResults =
                    DHCPv6LeaseTester.AddEventsForCancelableLeases(random, scopeId, events);


            rootScope.Load(events);

            Boolean result = rootScope.UpdateScopeResolver(scopeId, information);
            Assert.True(result);

            DHCPv6Scope scope = rootScope.GetScopeById(scopeId);
            Assert.Equal(resolverMock.Object, scope.Resolver);

            IEnumerable<DomainEvent> changes = rootScope.GetChanges();

            Int32 expectedAmountOfEvents = 1 + expectedResults.Where(x => x.Value == true).Count();
            Assert.Equal(expectedAmountOfEvents, changes.Count());

            DomainEvent firstEvent = changes.First();
            Assert.IsAssignableFrom<DHCPv6ScopeResolverUpdatedEvent>(firstEvent);

            DHCPv6ScopeResolverUpdatedEvent castedFirstEvent = (DHCPv6ScopeResolverUpdatedEvent)firstEvent;
            Assert.Equal(scopeId, castedFirstEvent.EntityId);
            Assert.Equal(information, castedFirstEvent.ResolverInformationen);

            foreach (var item in expectedResults)
            {
                DHCPv6Lease lease = scope.Leases.GetLeaseById(item.Key);
                if (item.Value == true)
                {
                    Assert.Equal(LeaseStates.Canceled, lease.State);
                }
                else
                {
                    Assert.NotEqual(LeaseStates.Canceled, lease.State);
                }
            }

            HashSet<Guid> blub = expectedResults.Where(x => x.Value == true).Select(x => x.Key).ToHashSet();
            foreach (DomainEvent item in changes.Skip(1))
            {
                Assert.IsAssignableFrom<DHCPv6LeaseCanceledEvent>(item);

                DHCPv6LeaseCanceledEvent castedEvent = (DHCPv6LeaseCanceledEvent)item;
                Assert.Contains(castedEvent.EntityId, blub);

                Assert.Equal(LeaseCancelReasons.ResolverChanged, castedEvent.Reason);
            }


        }

        [Fact]
        public void UpdateScopeResolver_Failed_InvalidResolver()
        {
            Random random = new Random();

            DHCPv6RootScope rootScope = GetRootScope();
            Guid scopeId = Guid.NewGuid();

            CreateScopeResolverInformation initalInformation = new CreateScopeResolverInformation
            {
                Typename = "my initial resolver",
            };

            CreateScopeResolverInformation information = new CreateScopeResolverInformation
            {
                Typename = "my mocked resolver",
            };

            Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>> managerMock = new Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>>(MockBehavior.Strict);
            managerMock.Setup(x => x.IsResolverInformationValid(information)).Returns(false);
            managerMock.Setup(x => x.InitializeResolver(initalInformation)).Returns<IScopeResolver<DHCPv6Packet, IPv6Address>>(null);

            rootScope.Load(new List<DomainEvent>
            {
                new DHCPv6ScopeAddedEvent(new DHCPv6ScopeCreateInstruction
            {
                Id = scopeId,
                ResolverInformation = initalInformation,
            })

            });

            ScopeException exp = Assert.Throws<ScopeException>(
                () => rootScope.UpdateScopeResolver(scopeId, information));

            Assert.Equal(DHCPv4ScopeExceptionReasons.InvalidResolver, exp.Reason);
        }

        [Fact]
        public void UpdateScopeResolver_Failed_NoInformation()
        {
            Random random = new Random();

            DHCPv6RootScope rootScope = GetRootScope();
            Guid scopeId = Guid.NewGuid();

            rootScope.Load(new List<DomainEvent>
            {
                new DHCPv6ScopeAddedEvent(new DHCPv6ScopeCreateInstruction
            {
                Id = scopeId,
            })

            });

            ScopeException exp = Assert.Throws<ScopeException>(
                () => rootScope.UpdateScopeResolver(scopeId, null));

            Assert.Equal(DHCPv4ScopeExceptionReasons.NoInput, exp.Reason);
        }

        [Fact]
        public void UpdateScopeResolver_Failed_ScopeNotFound()
        {
            Random random = new Random();

            DHCPv6RootScope rootScope = GetRootScope();
            Guid scopeId = Guid.NewGuid();

            CreateScopeResolverInformation initalInformation = new CreateScopeResolverInformation
            {
                Typename = "my initial resolver",
            };

            CreateScopeResolverInformation information = new CreateScopeResolverInformation
            {
                Typename = "my mocked resolver",
            };

            Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>> managerMock = new Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>>(MockBehavior.Strict);
            managerMock.Setup(x => x.InitializeResolver(initalInformation)).Returns<IScopeResolver<DHCPv6Packet, IPv6Address>>(null);

            rootScope.Load(new List<DomainEvent>
            {
                new DHCPv6ScopeAddedEvent(new DHCPv6ScopeCreateInstruction
            {
                Id = scopeId,
                ResolverInformation = initalInformation,
            })

            });

            ScopeException exp = Assert.Throws<ScopeException>(
                () => rootScope.UpdateScopeResolver(Guid.NewGuid(), information));

            Assert.Equal(DHCPv4ScopeExceptionReasons.ScopeNotFound, exp.Reason);
        }

        //[Fact]
        //public void UpdateScopePropertiesn()
        //{
        //    Random random = new Random();

        //    String initialDescription = random.GetAlphanumericString(30);
        //    Guid scopeId = Guid.NewGuid();

        //    DHCPv6RootScope rootScope = GetRootScope();
        //    rootScope.Load(new List<DomainEvent> {
        //        new DHCPv6ScopeAddedEvent(new DHCPv6ScopeCreateInstruction
        //    {
        //        Id = scopeId,
        //    })
        //    });

        //    IEnumerable<DHCPv6ScopeProperty> propertyItems = random.GenerateProperties();
        //    DHCPv6ScopeProperties properties = new DHCPv6ScopeProperties(propertyItems);

        //    Boolean result = rootScope.UpdateScopeProperties(scopeId, properties);
        //    Assert.True(result);

        //    DHCPv6Scope scope = rootScope.GetScopeById(scopeId);
        //    Assert.NotEqual(DHCPv6Scope.NotFound, scope);

        //    Assert.Equal(properties, scope.Properties);

        //    CheckScopeChangesEvent<DHCPv6ScopePropertiesUpdatedEvent>(rootScope, scopeId, (castedEvent) =>
        //    {
        //        Assert.Equal(properties, castedEvent.Properties);
        //    });
        //}

        //[Fact]
        //public void UpdateScopePropertiesn_Failed_ScopeNotFound()
        //{
        //    Random random = new Random();

        //    String initialDescription = random.GetAlphanumericString(30);
        //    Guid scopeId = Guid.NewGuid();

        //    DHCPv6RootScope rootScope = GetRootScope();
        //    rootScope.Load(new List<DomainEvent> {
        //        new DHCPv6ScopeAddedEvent(new DHCPv6ScopeCreateInstruction
        //    {
        //        Id = scopeId,
        //    })
        //    });

        //    IEnumerable<DHCPv6ScopeProperty> propertyItems = random.GenerateProperties();
        //    DHCPv6ScopeProperties properties = new DHCPv6ScopeProperties(propertyItems);

        //    ScopeException exception = Assert.Throws<ScopeException>(
        //        () => rootScope.UpdateScopeProperties(Guid.NewGuid(), properties));

        //    Assert.Equal(DHCPv4ScopeExceptionReasons.ScopeNotFound, exception.Reason);
        //}

        //[Fact]
        //public void UpdateScopePropertiesn_Failed_PropertiesAreNull()
        //{
        //    Random random = new Random();

        //    String initialDescription = random.GetAlphanumericString(30);
        //    Guid scopeId = Guid.NewGuid();

        //    DHCPv6RootScope rootScope = GetRootScope();
        //    rootScope.Load(new List<DomainEvent> {
        //        new DHCPv6ScopeAddedEvent(new DHCPv6ScopeCreateInstruction
        //    {
        //        Id = scopeId,
        //    })
        //    });

        //    ScopeException exception = Assert.Throws<ScopeException>(
        //        () => rootScope.UpdateScopeProperties(scopeId, null));

        //    Assert.Equal(DHCPv4ScopeExceptionReasons.NoInput, exception.Reason);
        //}

        [Fact]
        public void UpdateAddressProperties_ForRootScope()
        {
            Random random = new Random();

            String initialDescription = random.GetAlphanumericString(30);
            Guid scopeId = Guid.NewGuid();

            IPv6Address start = random.GetIPv6Address();
            IPv6Address end = random.GetIPv6AddressGreaterThan(start);

            DHCPv6ScopeAddressProperties properties = new DHCPv6ScopeAddressProperties
            (start, end, new IPv6Address[0],
            DHCPv6TimeScale.FromDouble(0.5), DHCPv6TimeScale.FromDouble(0.85),
            TimeSpan.FromMinutes(random.Next(20, 30)), TimeSpan.FromMinutes(random.Next(40, 60)),
            random.NextBoolean(), DHCPv6ScopeAddressProperties.AddressAllocationStrategies.Next,
            random.NextBoolean(), random.NextBoolean(), random.NextBoolean(), random.NextBoolean()
                );

            DHCPv6RootScope rootScope = GetRootScope();
            rootScope.Load(new List<DomainEvent> {
                new DHCPv6ScopeAddedEvent(new DHCPv6ScopeCreateInstruction
            {
                Id = scopeId,
            })
            });

            Boolean result = rootScope.UpdateAddressProperties(scopeId, properties);
            Assert.True(result);

            DHCPv6Scope scope = rootScope.GetScopeById(scopeId);
            Assert.NotEqual(DHCPv6Scope.NotFound, scope);

            Assert.Equal(properties, scope.AddressRelatedProperties);

            CheckScopeChangesEvent<DHCPv6ScopeAddressPropertiesUpdatedEvent>(rootScope, scopeId, (castedEvent) =>
            {
                Assert.Equal(properties, castedEvent.AddressProperties);
            });
        }

        [Fact]
        public void UpdateAddressProperties_ForRootScope_WithCancellingLeases()
        {
            Random random = new Random();

            String initialDescription = random.GetAlphanumericString(30);
            Guid scopeId = Guid.NewGuid();

            IPv6Address start = random.GetIPv6Address();
            IPv6Address end = random.GetIPv6AddressGreaterThan(start);

            DHCPv6ScopeAddressProperties properties = new DHCPv6ScopeAddressProperties
            (start, end, new IPv6Address[0],
            DHCPv6TimeScale.FromDouble(0.5), DHCPv6TimeScale.FromDouble(0.85),
            TimeSpan.FromMinutes(random.Next(20, 30)), TimeSpan.FromMinutes(random.Next(40, 60)),
            random.NextBoolean(), DHCPv6ScopeAddressProperties.AddressAllocationStrategies.Next,
            random.NextBoolean(), random.NextBoolean(), random.NextBoolean(), random.NextBoolean()
                );

            Dictionary<Guid, Boolean> expectedLeases = new Dictionary<Guid, bool>();
            Dictionary<IPv6Address, PrefixBinding> expectedBindings = new Dictionary<IPv6Address, PrefixBinding>();

            var events = new List<DomainEvent> {
                new DHCPv6ScopeAddedEvent(new DHCPv6ScopeCreateInstruction
                {
                    Id = scopeId,
                })
            };

            Int32 amount = random.Next(8, 15);
            for (int i = 0; i < amount; i++)
            {
                Guid leaseId = random.NextGuid();
                Boolean isNotInNewRange = random.NextBoolean();

                IPv6Address leaseAddress = null;
                if (isNotInNewRange == true)
                {
                    leaseAddress = random.NextBoolean() == true ? random.GetIPv6AddressGreaterThan(end) : random.GetIPv6AddressSmallerThan(start);
                }
                else
                {
                    leaseAddress = random.GetIPv6AddressBetween(start, end);
                }

                PrefixBinding binding = null;
                if (random.NextBoolean() == true)
                {
                    binding = new PrefixBinding(
                    IPv6Address.FromString("fe80::"),
                    new IPv6SubnetMask(new IPv6SubnetMaskIdentifier(64)),
                    leaseAddress);

                    if(isNotInNewRange == true)
                    {
                        expectedBindings.Add(leaseAddress, binding);
                    }
                }

                events.Add(new DHCPv6LeaseCreatedEvent
                {
                    EntityId = leaseId,
                    Address = leaseAddress,
                    ScopeId = scopeId,
                    HasPrefixDelegation = binding != null,
                    PrefixLength = binding == null ? (Byte)0 : binding.Mask.Identifier,
                    IdentityAssocationIdForPrefix = random.NextUInt32(),
                    DelegatedNetworkAddress = binding?.Prefix,
                    ClientIdentifier = new UUIDDUID(random.NextGuid()),
                });

                expectedLeases.Add(leaseId, isNotInNewRange);
            }

            DHCPv6RootScope rootScope = GetRootScope();
            rootScope.Load(events);

            Boolean result = rootScope.UpdateAddressProperties(scopeId, properties);
            Assert.True(result);

            DHCPv6Scope scope = rootScope.GetScopeById(scopeId);
            Assert.NotEqual(DHCPv6Scope.NotFound, scope);

            Assert.Equal(properties, scope.AddressRelatedProperties);

            var changes = rootScope.GetChanges();

            CheckScopeChangesEvent<DHCPv6ScopeAddressPropertiesUpdatedEvent>(rootScope, scopeId, (castedEvent) =>
            {
                Assert.Equal(properties, castedEvent.AddressProperties);
            }, expectedLeases.Values.Count(x => x == true) + 1);

            HashSet<Guid> expectedGuids = expectedLeases.Where(x => x.Value == true).Select(x => x.Key).ToHashSet();
            foreach (var item in changes.Skip(1).Cast<DHCPv6LeaseCanceledEvent>())
            {
                Assert.Contains(item.EntityId, expectedGuids);
                Assert.Equal(scopeId, item.ScopeId);
                Assert.Equal(LeaseCancelReasons.AddressRangeChanged, item.Reason);

                expectedGuids.Remove(item.EntityId);
            }

            foreach (var item in expectedLeases)
            {
                var lease = scope.Leases.GetLeaseById(item.Key);
                if(item.Value == false)
                {
                    Assert.Equal(LeaseStates.Pending, lease.State);
                }
                else
                {
                    Assert.Equal(LeaseStates.Canceled, lease.State);
                }
            }

            foreach (var item in rootScope.GetTriggers())
            {
                Assert.NotNull(item);
                Assert.IsAssignableFrom<PrefixEdgeRouterBindingUpdatedTrigger>(item);

                var trigger = (PrefixEdgeRouterBindingUpdatedTrigger)item;

                Assert.Null(trigger.NewBinding);
                Assert.Equal(scopeId, trigger.ScopeId);

                Assert.NotNull(trigger.OldBinding);

                Assert.True(expectedBindings.ContainsKey(trigger.OldBinding.Host));

                var expectedBinding = expectedBindings[trigger.OldBinding.Host];
                Assert.Equal(expectedBinding.Mask, trigger.OldBinding.Mask);
                Assert.Equal(expectedBinding.Prefix, trigger.OldBinding.Prefix);

                expectedBindings.Remove(trigger.OldBinding.Host);
            }

            Assert.Empty(expectedBindings);
        }

        [Fact]
        public void UpdateAddressProperties_ForNonRootScope()
        {
            Random random = new Random();

            String initialDescription = random.GetAlphanumericString(30);
            Guid parentId = Guid.NewGuid();
            Guid scopeId = Guid.NewGuid();

            IPv6Address start = random.GetIPv6Address();
            IPv6Address end = random.GetIPv6AddressGreaterThan(start);

            DHCPv6ScopeAddressProperties properties = new DHCPv6ScopeAddressProperties
            (start, end, new IPv6Address[0],
            DHCPv6TimeScale.FromDouble(0.5), DHCPv6TimeScale.FromDouble(0.85),
            TimeSpan.FromMinutes(random.Next(20, 30)), TimeSpan.FromMinutes(random.Next(40, 60)),
            random.NextBoolean(), DHCPv6ScopeAddressProperties.AddressAllocationStrategies.Next,
            random.NextBoolean(), random.NextBoolean(), random.NextBoolean()
                );

            DHCPv6ScopeAddressProperties childProperties = new DHCPv6ScopeAddressProperties
            (start + (UInt64)random.Next(3, 10), end - (UInt64)random.Next(2, 4), new IPv6Address[0]);

            DHCPv6RootScope rootScope = GetRootScope();
            rootScope.Load(new List<DomainEvent> {
                new DHCPv6ScopeAddedEvent(new DHCPv6ScopeCreateInstruction
            {
                Id = parentId,
                AddressProperties = properties,
                Name = "Parent",
            }),
             new DHCPv6ScopeAddedEvent(new DHCPv6ScopeCreateInstruction
            {
                Id = scopeId,
                ParentId = parentId,
                Name = "Child",
            })
            });

            Boolean result = rootScope.UpdateAddressProperties(scopeId, childProperties);
            Assert.True(result);

            DHCPv6Scope scope = rootScope.GetScopeById(scopeId);
            Assert.NotEqual(DHCPv6Scope.NotFound, scope);

            Assert.Equal(childProperties, scope.AddressRelatedProperties);

            CheckScopeChangesEvent<DHCPv6ScopeAddressPropertiesUpdatedEvent>(rootScope, scopeId, (castedEvent) =>
            {
                Assert.Equal(childProperties, castedEvent.AddressProperties);
            });
        }

        [Fact]
        public void UpdateAddressProperties_Failed_InvalidForRoot()
        {
            Random random = new Random();

            Guid scopeId = Guid.NewGuid();

            IPv6Address start = random.GetIPv6Address();
            IPv6Address end = random.GetIPv6AddressGreaterThan(start);

            List<DHCPv6ScopeAddressProperties> invalidProperties = new List<DHCPv6ScopeAddressProperties>
                {
            new DHCPv6ScopeAddressProperties
            (start, end, new IPv6Address[0],
            DHCPv6TimeScale.FromDouble(0.5), DHCPv6TimeScale.FromDouble(0.85),
            TimeSpan.FromMinutes(random.Next(20, 30)), TimeSpan.FromMinutes(random.Next(40, 60)),
            random.NextBoolean(), DHCPv6ScopeAddressProperties.AddressAllocationStrategies.Next,
            random.NextBoolean(), random.NextBoolean(), null
                ),
            new DHCPv6ScopeAddressProperties
            (start, end, new IPv6Address[0],
            DHCPv6TimeScale.FromDouble(0.5), DHCPv6TimeScale.FromDouble(0.85),
            TimeSpan.FromMinutes(random.Next(20, 30)), TimeSpan.FromMinutes(random.Next(40, 60)),
            random.NextBoolean(), DHCPv6ScopeAddressProperties.AddressAllocationStrategies.Next,
            random.NextBoolean(), null, random.NextBoolean()
                ),
            new DHCPv6ScopeAddressProperties
            (start, end, new IPv6Address[0],
            DHCPv6TimeScale.FromDouble(0.5), DHCPv6TimeScale.FromDouble(0.85),
            TimeSpan.FromMinutes(random.Next(20, 30)), TimeSpan.FromMinutes(random.Next(40, 60)),
            random.NextBoolean(), DHCPv6ScopeAddressProperties.AddressAllocationStrategies.Next,
            null, random.NextBoolean(), random.NextBoolean()
                ),
            new DHCPv6ScopeAddressProperties
            (start, end, new IPv6Address[0],
            DHCPv6TimeScale.FromDouble(0.5), DHCPv6TimeScale.FromDouble(0.85),
            TimeSpan.FromMinutes(random.Next(20, 30)), TimeSpan.FromMinutes(random.Next(40, 60)),
            random.NextBoolean(), null,
            random.NextBoolean(), random.NextBoolean(), random.NextBoolean()
                ),
            new DHCPv6ScopeAddressProperties
            (start, end, new IPv6Address[0],
            DHCPv6TimeScale.FromDouble(0.5), DHCPv6TimeScale.FromDouble(0.85),
            TimeSpan.FromMinutes(random.Next(20, 30)), TimeSpan.FromMinutes(random.Next(40, 60)),
            null, DHCPv6ScopeAddressProperties.AddressAllocationStrategies.Next,
            random.NextBoolean(), random.NextBoolean(), random.NextBoolean()
                ),
            new DHCPv6ScopeAddressProperties
            (start, end, new IPv6Address[0],
            DHCPv6TimeScale.FromDouble(0.5), DHCPv6TimeScale.FromDouble(0.85),
            TimeSpan.FromMinutes(random.Next(20, 30)),  null,
            random.NextBoolean(), DHCPv6ScopeAddressProperties.AddressAllocationStrategies.Next,
            random.NextBoolean(), random.NextBoolean(), random.NextBoolean()
                ),
            new DHCPv6ScopeAddressProperties
            (start, end, new IPv6Address[0],
            DHCPv6TimeScale.FromDouble(0.5), DHCPv6TimeScale.FromDouble(0.85),
            null, TimeSpan.FromMinutes(random.Next(70, 90)),
            random.NextBoolean(), DHCPv6ScopeAddressProperties.AddressAllocationStrategies.Next,
            random.NextBoolean(), random.NextBoolean(), random.NextBoolean()
                ),
            new DHCPv6ScopeAddressProperties
            (start, end, new IPv6Address[0],
            DHCPv6TimeScale.FromDouble(0.5), null,
            TimeSpan.FromMinutes(random.Next(40, 60)), TimeSpan.FromMinutes(random.Next(20, 30)),
            random.NextBoolean(), DHCPv6ScopeAddressProperties.AddressAllocationStrategies.Next,
            random.NextBoolean(), random.NextBoolean(), random.NextBoolean()
                ),
                        new DHCPv6ScopeAddressProperties
            (start, end, new IPv6Address[0],
            null, DHCPv6TimeScale.FromDouble(0.85),
            TimeSpan.FromMinutes(random.Next(40, 60)), TimeSpan.FromMinutes(random.Next(20, 30)),
            random.NextBoolean(), DHCPv6ScopeAddressProperties.AddressAllocationStrategies.Next,
            random.NextBoolean(), random.NextBoolean(), random.NextBoolean()
                ),
            new DHCPv6ScopeAddressProperties
            (start, end, new IPv6Address[0],
            DHCPv6TimeScale.FromDouble(0.85), DHCPv6TimeScale.FromDouble(0.5),
            TimeSpan.FromMinutes(random.Next(20, 30)), TimeSpan.FromMinutes(random.Next(40, 60)),
            random.NextBoolean(), DHCPv6ScopeAddressProperties.AddressAllocationStrategies.Next,
            random.NextBoolean(), random.NextBoolean(), null
                ),
            };

            foreach (var item in invalidProperties)
            {

                DHCPv6RootScope rootScope = GetRootScope();
                rootScope.Load(new List<DomainEvent> {
                new DHCPv6ScopeAddedEvent(new DHCPv6ScopeCreateInstruction
                {
                    Id = scopeId,
                    AddressProperties = item
                })
                });

                ScopeException exp = Assert.Throws<ScopeException>(() => rootScope.UpdateAddressProperties(scopeId, item));

                Assert.Equal(DHCPv4ScopeExceptionReasons.AddressPropertiesInvalidForParents, exp.Reason);
            }
        }

        [Theory]
        [InlineData("fe80::1", "fe80::A", "fe80::1", "fe80::2", true)]
        [InlineData("fe80::1", "fe80::A", "fe80::2", "fe80::A", true)]
        [InlineData("fe80::1", "fe80::A", "fe80::1", "fe80::A", true)]
        [InlineData("fe80::1", "fe80::1", "fe80::1", "fe80::1", true)]

        [InlineData("fe80::1", "fe80::A", "fe80::0", "fe80::2", false)]
        [InlineData("fe80::1", "fe80::A", "fe80::2", "fe80::B", false)]
        [InlineData("fe80::1", "fe80::A", "fe80::0", "fe80::B", false)]
        [InlineData("fe80::1", "fe80::A", "fe80::B", "fe80::D", false)]
        [InlineData("fe80::5", "fe80::A", "fe80::1", "fe80::4", false)]
        public void DHCPv6RootScope_UpdateAddressProperties_AddressRangeOfParen(
            String rawParentStart, String rawParentEnd,
            String rawChildStart, String rawChildEnd,
            Boolean shouldBeInRange
            )
        {
            Random random = new Random();

            String initialDescription = random.GetAlphanumericString(30);
            Guid parentId = Guid.NewGuid();
            Guid scopeId = Guid.NewGuid();

            IPv6Address start = IPv6Address.FromString(rawParentStart);
            IPv6Address end = IPv6Address.FromString(rawParentEnd);

            DHCPv6ScopeAddressProperties properties = new DHCPv6ScopeAddressProperties
            (start, end, new IPv6Address[0],
            DHCPv6TimeScale.FromDouble(0.5), DHCPv6TimeScale.FromDouble(0.85),
            TimeSpan.FromMinutes(random.Next(20, 30)), TimeSpan.FromMinutes(random.Next(40, 60)),
            random.NextBoolean(), DHCPv6ScopeAddressProperties.AddressAllocationStrategies.Next,
            random.NextBoolean(), random.NextBoolean(), random.NextBoolean()
                );

            IPv6Address startChild = IPv6Address.FromString(rawChildStart);
            IPv6Address endChild = IPv6Address.FromString(rawChildEnd);

            DHCPv6ScopeAddressProperties childProperties = new DHCPv6ScopeAddressProperties
            (startChild, endChild, new IPv6Address[0]);

            DHCPv6RootScope rootScope = GetRootScope();
            rootScope.Load(new List<DomainEvent> {
                new DHCPv6ScopeAddedEvent(new DHCPv6ScopeCreateInstruction
            {
                Id = parentId,
                AddressProperties = properties,
            }),
             new DHCPv6ScopeAddedEvent(new DHCPv6ScopeCreateInstruction
            {
                Id = scopeId,
                ParentId = parentId,
            })
            });

            if (shouldBeInRange == false)
            {

                ScopeException exp = Assert.Throws<ScopeException>(() => rootScope.UpdateAddressProperties(scopeId, childProperties));
                Assert.Equal(DHCPv4ScopeExceptionReasons.NotInParentRange, exp.Reason);
            }
            else
            {
                Boolean result = rootScope.UpdateAddressProperties(scopeId, childProperties);
                Assert.True(result);
            }
        }

        [Theory]
        [InlineData(10, 20, null, null, true)]
        [InlineData(10, 20, null, 35, true)]
        [InlineData(10, 20, 15, null, true)]
        [InlineData(10, 20, 30, 40, true)]
        [InlineData(10, 20, 20, 30, true)]

        [InlineData(10, 20, 35, null, false)]
        [InlineData(10, 20, null, 9, false)]
        [InlineData(10, 20, 5, 3, false)]
        public void UpdateAddressProperties_TimeRange(
            Int32 parentPreferredLifetime, Int32 parenValidLifetime,
            Int32? childPreferredLifetime, Int32? childValidLifetime,
            Boolean shouldBeInRange
        )
        {
            Random random = new Random();

            String initialDescription = random.GetAlphanumericString(30);
            Guid parentId = Guid.NewGuid();
            Guid scopeId = Guid.NewGuid();

            IPv6Address start = random.GetIPv6Address();
            IPv6Address end = random.GetIPv6AddressGreaterThan(start);

            DHCPv6ScopeAddressProperties properties = new DHCPv6ScopeAddressProperties
            (start, end, new IPv6Address[0],
            t1: DHCPv6TimeScale.FromDouble(0.5), t2: DHCPv6TimeScale.FromDouble(0.75),
             preferredLifeTime: TimeSpan.FromMinutes(parentPreferredLifetime),
             validLifeTime: TimeSpan.FromMinutes(parenValidLifetime)
                );

            DHCPv6ScopeAddressProperties childProperties = new DHCPv6ScopeAddressProperties
            (start + (UInt64)random.Next(3, 10), end - (UInt64)random.Next(2, 4), new IPv6Address[0],
             preferredLifeTime: childPreferredLifetime.HasValue == false ? new TimeSpan?() : TimeSpan.FromMinutes(childPreferredLifetime.Value),
             validLifeTime: childValidLifetime.HasValue == false ? new TimeSpan?() : TimeSpan.FromMinutes(childValidLifetime.Value)
            );

            DHCPv6RootScope rootScope = GetRootScope();
            rootScope.Load(new List<DomainEvent> {
                new DHCPv6ScopeAddedEvent(new DHCPv6ScopeCreateInstruction
            {
                Id = parentId,
                AddressProperties = properties,
                Name = "Parent",
            }),
             new DHCPv6ScopeAddedEvent(new DHCPv6ScopeCreateInstruction
            {
                Id = scopeId,
                ParentId = parentId,
                Name = "Child",
            })
            });

            if (shouldBeInRange == false)
            {
                ScopeException exp = Assert.Throws<ScopeException>(() => rootScope.UpdateAddressProperties(scopeId, childProperties));
                Assert.Equal(DHCPv4ScopeExceptionReasons.InvalidTimeRanges, exp.Reason);
            }
            else
            {
                Boolean result = rootScope.UpdateAddressProperties(scopeId, childProperties);
                Assert.True(result);
            }
        }

        [Theory]
        [InlineData(0.10, 0.20, null, null, true)]
        [InlineData(0.10, 0.20, null, 0.35, true)]
        [InlineData(0.10, 0.20, 0.15, null, true)]
        [InlineData(0.10, 0.20, 0.30, 0.40, true)]
        [InlineData(0.10, 0.20, 0.20, 0.30, true)]

        [InlineData(0.10, 0.20, 0.35, null, false)]
        [InlineData(0.10, 0.20, null, 0.09, false)]
        [InlineData(0.10, 0.20, 0.5, 0.3, false)]
        public void UpdateAddressProperties_T1AndT2(
          Double parentT1, Double parentT2,
          Double? childT1, Double? childT2,
          Boolean shouldBeInRange
      )
        {
            Random random = new Random();

            String initialDescription = random.GetAlphanumericString(30);
            Guid parentId = Guid.NewGuid();
            Guid scopeId = Guid.NewGuid();

            IPv6Address start = random.GetIPv6Address();
            IPv6Address end = random.GetIPv6AddressGreaterThan(start);

            DHCPv6ScopeAddressProperties properties = new DHCPv6ScopeAddressProperties
            (start, end, new IPv6Address[0],
            t1: DHCPv6TimeScale.FromDouble(parentT1), t2: DHCPv6TimeScale.FromDouble(parentT2),
             preferredLifeTime: TimeSpan.FromHours(6),
             validLifeTime: TimeSpan.FromHours(12)
                );

            DHCPv6ScopeAddressProperties childProperties = new DHCPv6ScopeAddressProperties
            (start + (UInt64)random.Next(3, 10), end - (UInt64)random.Next(2, 4), new IPv6Address[0],
             t1: childT1.HasValue == true ? DHCPv6TimeScale.FromDouble(childT1.Value) : null,
             t2: childT2.HasValue == true ? DHCPv6TimeScale.FromDouble(childT2.Value) : null
            );

            DHCPv6RootScope rootScope = GetRootScope();
            rootScope.Load(new List<DomainEvent> {
                new DHCPv6ScopeAddedEvent(new DHCPv6ScopeCreateInstruction
            {
                Id = parentId,
                AddressProperties = properties,
                Name = "Parent",
            }),
             new DHCPv6ScopeAddedEvent(new DHCPv6ScopeCreateInstruction
            {
                Id = scopeId,
                ParentId = parentId,
                Name = "Child",
            })
            });

            if (shouldBeInRange == false)
            {
                ScopeException exp = Assert.Throws<ScopeException>(() => rootScope.UpdateAddressProperties(scopeId, childProperties));
                Assert.Equal(DHCPv4ScopeExceptionReasons.InvalidTimeRanges, exp.Reason);
            }
            else
            {
                Boolean result = rootScope.UpdateAddressProperties(scopeId, childProperties);
                Assert.True(result);
            }
        }

        [Fact]
        public void DeleteScope_NonRootScope_IncludeChildren()
        {
            Random random = new Random();

            Guid parentId = Guid.NewGuid();
            Guid scopeId = Guid.NewGuid();

            DHCPv6RootScope rootScope = GetRootScope();
            List<DomainEvent> events = new List<DomainEvent> {
                new DHCPv6ScopeAddedEvent(new DHCPv6ScopeCreateInstruction
            {
                Id = parentId,
            }),
             new DHCPv6ScopeAddedEvent(new DHCPv6ScopeCreateInstruction
            {
                 ParentId = parentId,
                Id = scopeId,
            }) };

            Int32 subChildAmount = random.Next(3, 10);
            List<Guid> subChildIds = new List<Guid>(subChildAmount);
            for (int i = 0; i < subChildAmount; i++)
            {
                Guid subScopeId = Guid.NewGuid();
                subChildIds.Add(subScopeId);
                events.Add(new DHCPv6ScopeAddedEvent(new DHCPv6ScopeCreateInstruction
                {
                    ParentId = scopeId,
                    Id = subScopeId,
                }));
            }

            rootScope.Load(events);

            Boolean result = rootScope.DeleteScope(scopeId, true);
            Assert.True(result);

            DHCPv6Scope scope = rootScope.GetScopeById(scopeId);
            Assert.Equal(DHCPv6Scope.NotFound, scope);

            foreach (Guid subChildId in subChildIds)
            {
                DHCPv6Scope subchildScope = rootScope.GetScopeById(subChildId);
                Assert.Equal(DHCPv6Scope.NotFound, subchildScope);
            }

            IEnumerable<DomainEvent> changes = rootScope.GetChanges();
            Assert.NotEmpty(changes);
            Assert.Single(changes);

            Assert.IsAssignableFrom<DHCPv6ScopeDeletedEvent>(changes.First());
            DHCPv6ScopeDeletedEvent castedEvent = (DHCPv6ScopeDeletedEvent)changes.First();

            Assert.Equal(scopeId, castedEvent.EntityId);
            Assert.True(castedEvent.IncludeChildren);
        }

        [Fact]
        public void DeleteScope_NonRootScope_NotIncludeChildren()
        {
            Random random = new Random();

            Guid parentId = Guid.NewGuid();
            Guid scopeId = Guid.NewGuid();

            DHCPv6RootScope rootScope = GetRootScope();
            List<DomainEvent> events = new List<DomainEvent> {
                new DHCPv6ScopeAddedEvent(new DHCPv6ScopeCreateInstruction
            {
                Id = parentId,
                AddressProperties = DHCPv6ScopeAddressProperties.Empty,
            }),
             new DHCPv6ScopeAddedEvent(new DHCPv6ScopeCreateInstruction
            {
                    ParentId = parentId,
                    Id = scopeId,
                    AddressProperties = DHCPv6ScopeAddressProperties.Empty,
            })
            };

            Int32 subChildAmount = random.Next(3, 10);
            List<Guid> subChildIds = new List<Guid>(subChildAmount);
            for (int i = 0; i < subChildAmount; i++)
            {
                Guid subScopeId = Guid.NewGuid();
                subChildIds.Add(subScopeId);
                events.Add(new DHCPv6ScopeAddedEvent(new DHCPv6ScopeCreateInstruction
                {
                    ParentId = scopeId,
                    Id = subScopeId,
                    AddressProperties = DHCPv6ScopeAddressProperties.Empty,
                }));
            }

            rootScope.Load(events);

            Boolean result = rootScope.DeleteScope(scopeId, false);
            Assert.True(result);

            DHCPv6Scope scope = rootScope.GetScopeById(scopeId);
            Assert.Equal(DHCPv6Scope.NotFound, scope);

            foreach (Guid subChildId in subChildIds)
            {
                DHCPv6Scope subchildScope = rootScope.GetScopeById(subChildId);
                Assert.NotEqual(DHCPv6Scope.NotFound, subchildScope);
                Assert.NotEqual(DHCPv6Scope.NotFound, subchildScope.ParentScope);
                Assert.Equal(parentId, subchildScope.ParentScope.Id);
            }

            CheckScopeChangesEvent<DHCPv6ScopeDeletedEvent>(rootScope, scopeId, (castedEvent) =>
            {
                Assert.False(castedEvent.IncludeChildren);
                Assert.Equal(scopeId, castedEvent.EntityId);
            });
        }

        [Fact]
        public void DeleteScope_RootScope_IncludeChildren()
        {
            Random random = new Random();

            Guid parentId = Guid.NewGuid();
            Guid scopeId = Guid.NewGuid();

            DHCPv6RootScope rootScope = GetRootScope();
            List<DomainEvent> events = new List<DomainEvent> {
                new DHCPv6ScopeAddedEvent(new DHCPv6ScopeCreateInstruction
            {
                Id = parentId,
                AddressProperties = DHCPv6ScopeAddressProperties.Empty,
            }),
             new DHCPv6ScopeAddedEvent(new DHCPv6ScopeCreateInstruction
            {
                ParentId = parentId,
                Id = scopeId,
                AddressProperties = DHCPv6ScopeAddressProperties.Empty,
            }) };

            Int32 subChildAmount = random.Next(3, 10);
            List<Guid> subChildIds = new List<Guid>(subChildAmount);
            for (int i = 0; i < subChildAmount; i++)
            {
                Guid subScopeId = Guid.NewGuid();
                subChildIds.Add(subScopeId);
                events.Add(new DHCPv6ScopeAddedEvent(new DHCPv6ScopeCreateInstruction
                {
                    ParentId = scopeId,
                    Id = subScopeId,
                    AddressProperties = DHCPv6ScopeAddressProperties.Empty,
                }));
            }

            rootScope.Load(events);

            Boolean result = rootScope.DeleteScope(parentId, true);
            Assert.True(result);

            DHCPv6Scope parent = rootScope.GetScopeById(parentId);
            Assert.Equal(DHCPv6Scope.NotFound, parent);

            DHCPv6Scope scope = rootScope.GetScopeById(scopeId);
            Assert.Equal(DHCPv6Scope.NotFound, scope);

            foreach (Guid subChildId in subChildIds)
            {
                DHCPv6Scope subchildScope = rootScope.GetScopeById(subChildId);
                Assert.Equal(DHCPv6Scope.NotFound, subchildScope);
            }

            CheckScopeChangesEvent<DHCPv6ScopeDeletedEvent>(rootScope, parentId, (castedEvent) =>
            {
                Assert.True(castedEvent.IncludeChildren);
            });
        }

        [Fact]
        public void DeleteScope_RootScope_NotIncludeChildren()
        {
            Random random = new Random();

            Guid parentId = Guid.NewGuid();
            Guid scopeId = Guid.NewGuid();

            DHCPv6RootScope rootScope = GetRootScope();
            List<DomainEvent> events = new List<DomainEvent> {
                new DHCPv6ScopeAddedEvent(new DHCPv6ScopeCreateInstruction
            {
                Id = parentId,
                AddressProperties =  DHCPv6ScopeAddressProperties.Empty,
            }),
             new DHCPv6ScopeAddedEvent(new DHCPv6ScopeCreateInstruction
            {
                 ParentId = parentId,
                Id = scopeId,
                AddressProperties =  DHCPv6ScopeAddressProperties.Empty,
            }) };

            Int32 subChildAmount = random.Next(3, 10);
            List<Guid> subChildIds = new List<Guid>(subChildAmount);
            for (int i = 0; i < subChildAmount; i++)
            {
                Guid subScopeId = Guid.NewGuid();
                subChildIds.Add(subScopeId);
                events.Add(new DHCPv6ScopeAddedEvent(new DHCPv6ScopeCreateInstruction
                {
                    ParentId = scopeId,
                    Id = subScopeId,
                    AddressProperties = DHCPv6ScopeAddressProperties.Empty,
                }));
            }

            rootScope.Load(events);

            Boolean result = rootScope.DeleteScope(parentId, false);
            Assert.True(result);

            DHCPv6Scope parentScope = rootScope.GetScopeById(parentId);
            Assert.Equal(DHCPv6Scope.NotFound, parentScope);

            DHCPv6Scope scope = rootScope.GetScopeById(scopeId);
            Assert.NotEqual(DHCPv6Scope.NotFound, scope);

            Assert.Equal(subChildIds, scope.GetChildIds(true));

            Assert.Equal(DHCPv6Scope.NotFound, scope.ParentScope);

            foreach (Guid subChildId in subChildIds)
            {
                DHCPv6Scope subchildScope = rootScope.GetScopeById(subChildId);
                Assert.Equal(scope, subchildScope.ParentScope);
            }

            CheckScopeChangesEvent<DHCPv6ScopeDeletedEvent>(rootScope, parentId, (castedEvent) =>
            {
                Assert.False(castedEvent.IncludeChildren);
            });
        }

        [Fact]
        public void DeleteScope_Failed_ScopeNotFound()
        {
            Random random = new Random();

            DHCPv6RootScope rootScope = GetRootScope();
            Guid scopeId = Guid.NewGuid();

            rootScope.Load(new List<DomainEvent>
            {
                new DHCPv6ScopeAddedEvent(new DHCPv6ScopeCreateInstruction
            {
                Id = scopeId,
            })

            });

            ScopeException exp = Assert.Throws<ScopeException>(
                () => rootScope.DeleteScope(Guid.NewGuid(), random.NextBoolean()));

            Assert.Equal(DHCPv4ScopeExceptionReasons.ScopeNotFound, exp.Reason);
        }

        [Fact]
        public void UpdateParent_NonRootToRoot()
        {
            Random random = new Random();

            DHCPv6RootScope rootScope = GetRootScope();
            Guid parentId = Guid.NewGuid();
            Guid scopeId = Guid.NewGuid();

            rootScope.Load(new List<DomainEvent>
            {
            new DHCPv6ScopeAddedEvent(new DHCPv6ScopeCreateInstruction
            {
                Id = parentId,
                AddressProperties =  DHCPv6ScopeAddressProperties.Empty,

            }),
            new DHCPv6ScopeAddedEvent(new DHCPv6ScopeCreateInstruction
            {
               ParentId = parentId,
                Id = scopeId,
                AddressProperties =  DHCPv6ScopeAddressProperties.Empty,
            }),

            });

            Boolean actual = rootScope.UpdateParent(scopeId, null);
            Assert.True(actual);

            DHCPv6Scope parentScope = rootScope.GetScopeById(parentId);
            DHCPv6Scope childScope = rootScope.GetScopeById(scopeId);

            Assert.DoesNotContain(childScope, parentScope.GetChildScopes());
            Assert.Equal(DHCPv6Scope.NotFound, childScope.ParentScope);
            Assert.Empty(childScope.GetChildScopes());

            CheckScopeChangesEvent<DHCPv6ScopeParentUpdatedEvent>(rootScope, scopeId, (castedEvent) =>
            {
                Assert.Null(castedEvent.ParentId);
            });
        }

        [Fact]
        public void UpdateParent_RootToNonRoot()
        {
            Random random = new Random();

            DHCPv6RootScope rootScope = GetRootScope();
            Guid parentId = Guid.NewGuid();
            Guid scopeId = Guid.NewGuid();

            rootScope.Load(new List<DomainEvent>
            {
            new DHCPv6ScopeAddedEvent(new DHCPv6ScopeCreateInstruction
            {
                Id = parentId,
                AddressProperties =  DHCPv6ScopeAddressProperties.Empty,
            }),
            new DHCPv6ScopeAddedEvent(new DHCPv6ScopeCreateInstruction
            {
                Id = scopeId,
                AddressProperties =  DHCPv6ScopeAddressProperties.Empty,
            }),

            });

            Boolean actual = rootScope.UpdateParent(scopeId, parentId);
            Assert.True(actual);

            DHCPv6Scope parentScope = rootScope.GetScopeById(parentId);
            DHCPv6Scope childScope = rootScope.GetScopeById(scopeId);

            Assert.Contains(childScope, parentScope.GetChildScopes());
            Assert.Equal(parentScope, childScope.ParentScope);
            Assert.Empty(childScope.GetChildScopes());

            CheckScopeChangesEvent<DHCPv6ScopeParentUpdatedEvent>(rootScope, scopeId, (castedEvent) =>
            {
                Assert.NotNull(castedEvent.ParentId);
                Assert.Equal(parentId, castedEvent.ParentId.Value);
            });
        }

        [Fact]
        public void UpdateParent_NonRootToAnotherNonRoot()
        {
            Random random = new Random();

            DHCPv6RootScope rootScope = GetRootScope();
            Guid originalParentId = random.NextGuid();
            Guid newParentId = random.NextGuid(); ;

            Guid scopeId = random.NextGuid();

            rootScope.Load(new List<DomainEvent>
            {
            new DHCPv6ScopeAddedEvent(new DHCPv6ScopeCreateInstruction
            {
                Id = originalParentId,
                Name = "Parent A",
                AddressProperties = DHCPv6ScopeAddressProperties.Empty,
            }),
            new DHCPv6ScopeAddedEvent(new DHCPv6ScopeCreateInstruction
            {
               ParentId = originalParentId,
                AddressProperties = DHCPv6ScopeAddressProperties.Empty,
                Name = "Child A",
                Id = scopeId,
            }),
            new DHCPv6ScopeAddedEvent(new DHCPv6ScopeCreateInstruction
            {
                Id = newParentId,
                AddressProperties = DHCPv6ScopeAddressProperties.Empty,
                Name = "Parent B",
            }),
            });

            Boolean actual = rootScope.UpdateParent(scopeId, newParentId);
            Assert.True(actual);

            DHCPv6Scope originalParentScope = rootScope.GetScopeById(originalParentId);
            DHCPv6Scope newParentScope = rootScope.GetScopeById(newParentId);
            DHCPv6Scope childScope = rootScope.GetScopeById(scopeId);

            Assert.DoesNotContain(childScope, originalParentScope.GetChildScopes());
            Assert.Equal(newParentScope, childScope.ParentScope);
            Assert.Contains(childScope, newParentScope.GetChildScopes());

            Assert.Empty(childScope.GetChildScopes());

            CheckScopeChangesEvent<DHCPv6ScopeParentUpdatedEvent>(rootScope, scopeId, (castedEvent) =>
            {
                Assert.NotNull(castedEvent.ParentId);
                Assert.Equal(newParentId, castedEvent.ParentId.Value);
            });
        }

        [Fact]
        public void UpdateParent_Failed_ScopeNotFound()
        {
            Random random = new Random();

            DHCPv6RootScope rootScope = GetRootScope();
            Guid scopeId = Guid.NewGuid();

            rootScope.Load(new List<DomainEvent>
            {
                new DHCPv6ScopeAddedEvent(new DHCPv6ScopeCreateInstruction
            {
                Id = scopeId,
            })

            });

            ScopeException exp = Assert.Throws<ScopeException>(
                () => rootScope.UpdateParent(Guid.NewGuid(), null));

            Assert.Equal(DHCPv4ScopeExceptionReasons.ScopeNotFound, exp.Reason);
        }

        [Fact]
        public void UpdateParent_Failed_ParentNotFound()
        {
            Random random = new Random();

            DHCPv6RootScope rootScope = GetRootScope();
            Guid scopeId = Guid.NewGuid();

            rootScope.Load(new List<DomainEvent>
            {
                new DHCPv6ScopeAddedEvent(new DHCPv6ScopeCreateInstruction
            {
                Id = scopeId,
            })

            });

            ScopeException exp = Assert.Throws<ScopeException>(
                () => rootScope.UpdateParent(scopeId, Guid.NewGuid()));

            Assert.Equal(DHCPv4ScopeExceptionReasons.ScopeParentNotFound, exp.Reason);
        }

        [Fact]
        public void UpdateParent_Failed_ParentAddedAsChild()
        {
            Random random = new Random();

            DHCPv6RootScope rootScope = GetRootScope();
            Guid scopeId = Guid.NewGuid();
            Guid parentId = Guid.NewGuid();

            rootScope.Load(new List<DomainEvent>
            {
                   new DHCPv6ScopeAddedEvent(new DHCPv6ScopeCreateInstruction
            {
                Id = parentId,
                Name = "Parent"
            }),
                new DHCPv6ScopeAddedEvent(new DHCPv6ScopeCreateInstruction
            {
                Id = scopeId,
                ParentId = parentId,
                Name = "Child",
            })
            });

            ScopeException exp = Assert.Throws<ScopeException>(
                () => rootScope.UpdateParent(parentId, scopeId));

            //Assert.Equal(DHCPv4ScopeExceptionReasons.ParentNotMoveable, exp.Reason);
        }

        [Fact]
        public void AddScope_RootScope()
        {
            Random random = new Random();

            Guid scopeId = Guid.NewGuid();
            String description = random.GetAlphanumericString(30);
            String name = random.GetAlphanumericString(20);

            CreateScopeResolverInformation information = new CreateScopeResolverInformation
            {
                Typename = "my mocked resolver",
            };

            Mock<IScopeResolver<DHCPv6Packet, IPv6Address>> resolverMock = new Mock<IScopeResolver<DHCPv6Packet, IPv6Address>>(MockBehavior.Strict);
            var mockedResolver = resolverMock.Object;

            Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>> managerMock = new Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>>(MockBehavior.Strict);
            managerMock.Setup(x => x.IsResolverInformationValid(information)).Returns(true);
            managerMock.Setup(x => x.InitializeResolver(information)).Returns(mockedResolver);

            DHCPv6RootScope rootScope = GetRootScope(managerMock);

            //var properties = new DHCPv6ScopeProperties(random.GenerateProperties());
            DHCPv6ScopeAddressProperties addressProperties = new DHCPv6ScopeAddressProperties(
                IPv6Address.FromString("fe80::1"), IPv6Address.FromString("fe80::A"), Array.Empty<IPv6Address>(),
                DHCPv6TimeScale.FromDouble(0.5), DHCPv6TimeScale.FromDouble(0.85),

                TimeSpan.FromHours(10), TimeSpan.FromHours(20), true, DHCPv6ScopeAddressProperties.AddressAllocationStrategies.Next, true, true, true, true);

            DHCPv6ScopeCreateInstruction instruction = new DHCPv6ScopeCreateInstruction
            {
                Id = scopeId,
                AddressProperties = addressProperties,
                Description = ScopeDescription.FromString(description),
                Name = ScopeName.FromString(name),
                ScopeProperties = new DHCPv6ScopeProperties(
                    new DHCPv6AddressListScopeProperty(random.NextUInt16(), random.GetIPv6Addresses()),
                    new DHCPv6TextScopeProperty(random.NextUInt16(), random.GetAlphanumericString()),
                    new DHCPv6NumericValueScopeProperty(random.NextUInt16(), random.NextByte(), NumericScopePropertiesValueTypes.Byte, DHCPv6ScopePropertyType.Byte)
                    ),
                ResolverInformation = information,
            };

            Boolean actual = rootScope.AddScope(instruction);
            Assert.True(actual);

            DHCPv6Scope scope = rootScope.GetScopeById(scopeId);

            Assert.Equal(scopeId, scope.Id);
            Assert.Equal(name, scope.Name);
            Assert.Equal(description, scope.Description);
            //Assert.Equal(properties, scope.Properties);
            Assert.Equal(addressProperties, scope.AddressRelatedProperties);
            Assert.Equal(mockedResolver, scope.Resolver);
            Assert.Equal(DHCPv6Scope.NotFound, scope.ParentScope);

            CheckScopeChangesDomainEvent<DHCPv6ScopeAddedEvent>(rootScope, (castedEvent) =>
            {
                Assert.Equal(scopeId, castedEvent.Instructions.Id);
                Assert.Equal(addressProperties, castedEvent.Instructions.AddressProperties);
                Assert.Equal(description, castedEvent.Instructions.Description);
                Assert.Equal(name, castedEvent.Instructions.Name);
                Assert.Null(castedEvent.Instructions.ParentId);
                //Assert.Equal(properties, castedEvent.Instructions.Properties);
                Assert.Equal(information, castedEvent.Instructions.ResolverInformation);
            });
        }

        [Fact]
        public void CleanUpLeases_Expired()
        {
            Random random = new Random();
            Guid grantParentId = random.NextGuid();
            Guid parentId = random.NextGuid();
            Guid childId = random.NextGuid();

            Dictionary<Guid, Boolean> expectedResults = new Dictionary<Guid, bool>();

            List<DomainEvent> events = new List<DomainEvent>
            {
                 new DHCPv6ScopeAddedEvent(new DHCPv6ScopeCreateInstruction
                 {
                     Id = grantParentId,
                     Name = "grant parent"
                 }),
                 new DHCPv6ScopeAddedEvent(new DHCPv6ScopeCreateInstruction
                 {
                     Id = parentId,
                     ParentId = grantParentId,
                     Name = "parent"
                 }),
                 new DHCPv6ScopeAddedEvent(new DHCPv6ScopeCreateInstruction
                 {
                     Id = childId,
                     ParentId = parentId,
                     Name = "child"

                 }),
            };

            DateTime now = DateTime.UtcNow;
            Dictionary<Guid, Guid> leaseToScopeMapper = new Dictionary<Guid, Guid>();

            Dictionary<Guid, Boolean> prefixMapper = new Dictionary<Guid, bool>();

            Int32 amount = random.Next(30, 100);
            for (int i = 0; i < amount; i++)
            {
                Boolean result = random.NextBoolean();
                Guid leaseId = random.NextGuid();
                DateTime end;
                if (result == true)
                {
                    end = now.AddMinutes(-random.Next(3, 10));
                }
                else
                {
                    end = now.AddMinutes(random.Next(3, 10));
                }

                Guid scopeId = random.NextBoolean() == true ? grantParentId : (random.NextBoolean() == true ? parentId : childId);

                Boolean hasPrefix = random.NextBoolean();

                events.Add(new DHCPv6LeaseCreatedEvent
                {
                    EntityId = leaseId,
                    Address = random.GetIPv6Address(),
                    ValidUntil = end,
                    StartedAt = DateTime.UtcNow,
                    ScopeId = scopeId,
                    HasPrefixDelegation = hasPrefix,
                    DelegatedNetworkAddress = IPv6Address.FromString("fc07::0"),
                    PrefixLength = 40,
                    IdentityAssocationIdForPrefix = random.NextUInt32(),
                    ClientIdentifier = new UUIDDUID(random.NextGuid()),
                });

                prefixMapper.Add(leaseId, hasPrefix);

                events.Add(new DHCPv6LeaseActivatedEvent
                {
                    EntityId = leaseId,
                    ScopeId = scopeId,
                });

                expectedResults.Add(leaseId, result);
                leaseToScopeMapper.Add(leaseId, scopeId);
            }

            DHCPv6RootScope rootScope = GetRootScope();
            rootScope.Load(events);

            Int32 expiredLeaseAmount = rootScope.CleanUpLeases();
            Assert.Equal(expectedResults.Values.Count(x => x == true), expiredLeaseAmount);

            var triggers = rootScope.GetTriggers().Cast<PrefixEdgeRouterBindingUpdatedTrigger>().ToList();
            foreach (var item in expectedResults)
            {
                var scope = rootScope.GetScopeById(leaseToScopeMapper[item.Key]);
                var lease = scope.Leases.GetLeaseById(item.Key);
                if (item.Value == true)
                {
                    Assert.False(lease.AddressIsInUse());
                    Assert.Equal(LeaseStates.Inactive, lease.State);

                    var trigger = triggers.FirstOrDefault(x => x.OldBinding.Host == lease.Address);
                    if (prefixMapper[item.Key] == true)
                    {
                        Assert.NotNull(trigger);
                        Assert.Equal(leaseToScopeMapper[item.Key], trigger.ScopeId);
                        Assert.Equal(lease.Address, trigger.OldBinding.Host);
                        Assert.Equal(lease.PrefixDelegation.NetworkAddress, trigger.OldBinding.Prefix);
                        Assert.Equal(lease.PrefixDelegation.Mask, trigger.OldBinding.Mask);

                        Assert.Null(trigger.NewBinding);
                        triggers.Remove(trigger);
                    }
                    else
                    {
                        Assert.Null(trigger);
                    }
                }
                else
                {
                    Assert.True(lease.IsActive());
                    Assert.Equal(LeaseStates.Active, lease.State);
                }
            }

            Assert.Empty(triggers);
        }

        [Fact]
        public void CleanUpLeases_Pending()
        {
            Random random = new Random();
            Guid grantParentId = random.NextGuid();
            Guid parentId = random.NextGuid();
            Guid childId = random.NextGuid();

            Dictionary<Guid, Boolean> expectedResults = new Dictionary<Guid, bool>();

            List<DomainEvent> events = new List<DomainEvent>
            {
                 new DHCPv6ScopeAddedEvent(new DHCPv6ScopeCreateInstruction
                 {
                     Id = grantParentId,
                     Name = "grant parent"
                 }),
                 new DHCPv6ScopeAddedEvent(new DHCPv6ScopeCreateInstruction
                 {
                     Id = parentId,
                     ParentId = grantParentId,
                     Name = "parent"
                 }),
                 new DHCPv6ScopeAddedEvent(new DHCPv6ScopeCreateInstruction
                 {
                     Id = childId,
                     ParentId = parentId,
                     Name = "child"

                 }),
            };

            DateTime now = DateTime.UtcNow;
            Dictionary<Guid, Guid> leaseToScopeMapper = new Dictionary<Guid, Guid>();

            Int32 amount = random.Next(30, 100);
            for (int i = 0; i < amount; i++)
            {
                Boolean result = random.NextBoolean();
                Guid leaseId = random.NextGuid();
                DateTime start;
                if (result == true)
                {
                    start = now.AddMinutes(-random.Next(6, 10));
                }
                else
                {
                    start = now.AddMinutes(-random.Next(1, 5));
                }

                Guid scopeId = random.NextBoolean() == true ? grantParentId : (random.NextBoolean() == true ? parentId : childId);

                events.Add(new DHCPv6LeaseCreatedEvent
                {
                    EntityId = leaseId,
                    Address = random.GetIPv6Address(),
                    ValidUntil = now.AddHours(random.Next(3, 10)),
                    StartedAt = start,
                    ScopeId = scopeId,
                    ClientIdentifier = new UUIDDUID(random.NextGuid()),
                });

                expectedResults.Add(leaseId, result);
                leaseToScopeMapper.Add(leaseId, scopeId);
            }

            DHCPv6RootScope rootScope = GetRootScope();
            rootScope.Load(events);

            Int32 expiredLeaseAmount = rootScope.CleanUpLeases();
            Assert.Equal(expectedResults.Values.Count(x => x == true), expiredLeaseAmount);

            foreach (var item in expectedResults)
            {
                var scope = rootScope.GetScopeById(leaseToScopeMapper[item.Key]);
                var lease = scope.Leases.GetLeaseById(item.Key);
                if (item.Value == true)
                {
                    Assert.False(lease.AddressIsInUse());
                    Assert.Equal(LeaseStates.Canceled, lease.State);
                }
                else
                {
                    Assert.True(lease.IsPending());
                    Assert.Equal(LeaseStates.Pending, lease.State);
                }
            }
        }

        [Fact]
        public void DropUnusedLeasesOlderThan()
        {
            Random random = new Random();
            Guid grantParentId = random.NextGuid();
            Guid parentId = random.NextGuid();
            Guid childId = random.NextGuid();

            Dictionary<Guid, Boolean> expectedResults = new Dictionary<Guid, bool>();

            List<DomainEvent> events = new List<DomainEvent>
            {
                 new DHCPv6ScopeAddedEvent(new DHCPv6ScopeCreateInstruction
                 {
                     Id = grantParentId,
                     Name = "grant parent"
                 }),
                 new DHCPv6ScopeAddedEvent(new DHCPv6ScopeCreateInstruction
                 {
                     Id = parentId,
                     ParentId = grantParentId,
                     Name = "parent"
                 }),
                 new DHCPv6ScopeAddedEvent(new DHCPv6ScopeCreateInstruction
                 {
                     Id = childId,
                     ParentId = parentId,
                     Name = "child"

                 }),
            };

            DateTime now = DateTime.UtcNow;
            Dictionary<Guid, Guid> leaseToScopeMapper = new Dictionary<Guid, Guid>();

            Int32 amount = random.Next(30, 100);
            for (int i = 0; i < amount; i++)
            {
                Boolean result = random.NextBoolean();
                Guid leaseId = random.NextGuid();
                DateTime start = now.AddDays(-2);
                if (result == false)
                {
                    start = now.AddMinutes(-random.Next(6, 10));
                }

                Guid scopeId = random.NextBoolean() == true ? grantParentId : (random.NextBoolean() == true ? parentId : childId);

                events.Add(new DHCPv6LeaseCreatedEvent
                {
                    EntityId = leaseId,
                    Address = random.GetIPv6Address(),
                    ValidUntil = start.AddHours(random.Next(3, 10)),
                    StartedAt = start,
                    ScopeId = scopeId,
                    ClientIdentifier = new UUIDDUID(random.NextGuid()),
                });
                events.Add(new DHCPv6LeaseActivatedEvent
                {
                    ScopeId = scopeId,
                    EntityId = leaseId,
                });

                if (result == true)
                {
                    events.Add(new DHCPv6LeaseExpiredEvent
                    {
                        ScopeId = scopeId,
                        EntityId = leaseId,
                    });
                }

                expectedResults.Add(leaseId, result);
                leaseToScopeMapper.Add(leaseId, scopeId);
            }

            DHCPv6RootScope rootScope = GetRootScope();
            rootScope.Load(events);

            rootScope.DropUnusedLeasesOlderThan(now.AddDays(-1));

            foreach (var item in expectedResults)
            {
                var scope = rootScope.GetScopeById(leaseToScopeMapper[item.Key]);
                var lease = scope.Leases.GetLeaseById(item.Key);
                if (item.Value == true)
                {
                    Assert.Equal(DHCPv6Lease.NotFound, lease);
                }
                else
                {
                    Assert.True(lease.IsActive());
                }
            }
        }

        [Fact]
        public void ComplexResolerStructureWithPseudoResolver()
        {
            Random random = new Random();

            UInt32 iaid = random.NextUInt32();

            DHCPv6Packet input = DHCPv6Packet.AsOuter(new IPv6HeaderInformation(random.GetIPv6Address(), random.GetIPv6Address()),
                random.NextUInt16(), DHCPv6PacketTypes.Solicit, new DHCPv6PacketOption[] {
                 new DHCPv6PacketIdentityAssociationNonTemporaryAddressesOption(iaid),
                 new DHCPv6PacketIdentifierOption(DHCPv6PacketOptionTypes.ClientIdentifier,new UUIDDUID(Guid.Empty))
                });

            Mock<ILoggerFactory> factoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);
            factoryMock.Setup(x => x.CreateLogger(It.IsAny<String>())).Returns(Mock.Of<ILogger<DHCPv6RootScope>>());

            String pseudoResolverTypename = "PseudoResolver";
            String nonPseudoResolverTypename = "NonPseudoResolver";

            DHCPv6PseudoResolver pseudoResolver = new DHCPv6PseudoResolver();

            Mock<IScopeResolver<DHCPv6Packet, IPv6Address>> nonPseudoResolverMock = new Mock<IScopeResolver<DHCPv6Packet, IPv6Address>>();
            nonPseudoResolverMock.Setup(x => x.PacketMeetsCondition(input)).Returns(true);

            Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>> resolverManager = new Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>>();
            resolverManager.Setup(x => x.IsResolverInformationValid(It.Is<CreateScopeResolverInformation>(y => y.Typename == pseudoResolverTypename))).Returns(true);
            resolverManager.Setup(x => x.IsResolverInformationValid(It.Is<CreateScopeResolverInformation>(y => y.Typename == nonPseudoResolverTypename))).Returns(true);

            resolverManager.Setup(x => x.InitializeResolver(It.Is<CreateScopeResolverInformation>(y => y.Typename == pseudoResolverTypename))).Returns(pseudoResolver);
            resolverManager.Setup(x => x.InitializeResolver(It.Is<CreateScopeResolverInformation>(y => y.Typename == nonPseudoResolverTypename))).Returns(nonPseudoResolverMock.Object);

            Mock<IDHCPv6ServerPropertiesResolver> serverPropertiesResovlerMock = new Mock<IDHCPv6ServerPropertiesResolver>();
            serverPropertiesResovlerMock.Setup(x => x.GetServerDuid()).Returns(new UUIDDUID(random.NextGuid()));

            DHCPv6RootScope rootScope = new DHCPv6RootScope(Guid.Empty, resolverManager.Object, factoryMock.Object);

            Guid rootScopeId = random.NextGuid();
            Guid firstChildLevel1Id = random.NextGuid();
            Guid firstChildLevel2Id = random.NextGuid();
            Guid firstChildLevel3Id = random.NextGuid();

            Guid firstNonPseudoChild = random.NextGuid();
            Guid secondNonPseudoChild = random.NextGuid();

            rootScope.AddScope(new DHCPv6ScopeCreateInstruction
            {
                Name = "Rootscope",
                Id = rootScopeId,
                ResolverInformation = new CreateScopeResolverInformation { Typename = pseudoResolverTypename, },
                ScopeProperties = DHCPv6ScopeProperties.Empty,
                AddressProperties = new DHCPv6ScopeAddressProperties(
                    IPv6Address.FromString("fe80::1"), IPv6Address.FromString("fe80::FF"), Array.Empty<IPv6Address>(),
                    DHCPv6TimeScale.FromDouble(0.5), DHCPv6TimeScale.FromDouble(0.75), TimeSpan.FromHours(24), TimeSpan.FromHours(36),
                    true, DHCPv6ScopeAddressProperties.AddressAllocationStrategies.Next, true, true, true, true, null)
            });

            rootScope.AddScope(new DHCPv6ScopeCreateInstruction
            {
                Name = "First child Level 1 ",
                Id = firstChildLevel1Id,
                ParentId = rootScopeId,
                ResolverInformation = new CreateScopeResolverInformation { Typename = pseudoResolverTypename, },
                ScopeProperties = DHCPv6ScopeProperties.Empty,
                AddressProperties = new DHCPv6ScopeAddressProperties(
                    IPv6Address.FromString("fe80::1"), IPv6Address.FromString("fe80::FF"), Array.Empty<IPv6Address>()),
            });

            rootScope.AddScope(new DHCPv6ScopeCreateInstruction
            {
                Name = "First child Level 2 ",
                Id = firstChildLevel2Id,
                ParentId = firstChildLevel1Id,
                ResolverInformation = new CreateScopeResolverInformation { Typename = pseudoResolverTypename, },
                ScopeProperties = DHCPv6ScopeProperties.Empty,
                AddressProperties = new DHCPv6ScopeAddressProperties(
                    IPv6Address.FromString("fe80::1"), IPv6Address.FromString("fe80::FF"), Array.Empty<IPv6Address>()),
            });

            rootScope.AddScope(new DHCPv6ScopeCreateInstruction
            {
                Name = "First child Level 3 ",
                Id = firstChildLevel3Id,
                ParentId = firstChildLevel2Id,
                ResolverInformation = new CreateScopeResolverInformation { Typename = pseudoResolverTypename, },
                ScopeProperties = DHCPv6ScopeProperties.Empty,
                AddressProperties = new DHCPv6ScopeAddressProperties(
                    IPv6Address.FromString("fe80::1"), IPv6Address.FromString("fe80::FF"), Array.Empty<IPv6Address>()),
            });

            rootScope.AddScope(new DHCPv6ScopeCreateInstruction
            {
                Name = "First non pseudo child Level 1",
                Id = firstNonPseudoChild,
                ParentId = rootScopeId,
                ResolverInformation = new CreateScopeResolverInformation { Typename = nonPseudoResolverTypename, },
                ScopeProperties = DHCPv6ScopeProperties.Empty,
                AddressProperties = new DHCPv6ScopeAddressProperties(
                    IPv6Address.FromString("fe80::1"), IPv6Address.FromString("fe80::FF"), Array.Empty<IPv6Address>()),
            });

            rootScope.AddScope(new DHCPv6ScopeCreateInstruction
            {
                Name = "First non pseudo child Level 2",
                Id = secondNonPseudoChild,
                ParentId = firstNonPseudoChild,
                ResolverInformation = new CreateScopeResolverInformation { Typename = nonPseudoResolverTypename, },
                ScopeProperties = DHCPv6ScopeProperties.Empty,
                AddressProperties = new DHCPv6ScopeAddressProperties(
                    IPv6Address.FromString("fe80::10"), IPv6Address.FromString("fe80::10"), Array.Empty<IPv6Address>(),
                    DHCPv6TimeScale.FromDouble(0.5), DHCPv6TimeScale.FromDouble(0.75), TimeSpan.FromHours(24), TimeSpan.FromHours(36),
                    true, DHCPv6ScopeAddressProperties.AddressAllocationStrategies.Next, true, true, true, true, null)
            });

            var result = rootScope.HandleSolicit(input, serverPropertiesResovlerMock.Object);
            Assert.NotNull(result);

            var addressSubOption = result.GetNonTemporaryIdentiyAssocation(iaid).Suboptions.First() as DHCPv6PacketIdentityAssociationAddressSuboption;
            Assert.Equal(IPv6Address.FromString("fe80::10"), addressSubOption.Address);
        }
    }
}

