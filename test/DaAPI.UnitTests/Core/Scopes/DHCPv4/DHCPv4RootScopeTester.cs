//using DaAPI.Core.Common;
//using DaAPI.Core.Exceptions;
//using DaAPI.Core.Scopes;
//using DaAPI.Core.Scopes.DHCPv4;
//using DaAPI.Core.Services;
//using DaAPI.TestHelper;
//using Moq;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Xunit;
//using static DaAPI.Core.Scopes.DHCPv4.DHCPv4Lease;
//using static DaAPI.Core.Scopes.DHCPv4.DHCPv4LeaseEvents;
//using static DaAPI.Core.Scopes.DHCPv4.DHCPv4ScopeEvents;

//namespace DaAPI.UnitTests.Core.Scopes.DHCPv4
//{
//    public class DHCPv4RootScopeTester
//    {
//        public DHCPv4RootScope GetRootScope() =>
//                new DHCPv4RootScope(Guid.NewGuid(), Mock.Of<IScopeResolver<DHCPv4Packet, IPv4Address>Manager>());

//        public DHCPv4RootScope GetRootScope(Mock<IScopeResolver<DHCPv4Packet, IPv4Address>Manager> mock) =>
//                new DHCPv4RootScope(Guid.NewGuid(), mock.Object);


//        [Fact]
//        public void DHCPv4RootScope_GetScoopById()
//        {
//            Random random = new Random();

//            List<DomainEvent> events = new List<DomainEvent>();
//            Dictionary<Guid, Boolean> expectedResult = new Dictionary<Guid, bool>();
//            Int32 existingAmount = random.Next(30, 100);
//            for (int i = 0; i < existingAmount; i++)
//            {
//                Guid scopeId = Guid.NewGuid();
//                events.Add(new DHCPv4ScopeAddedEvent(new DHCPv4ScopeCreateInstruction
//                {
//                    Id = scopeId,
//                }));

//                expectedResult.Add(scopeId, true);
//            }

//            Int32 nonExistingAmount = random.Next(30, 100);
//            for (int i = 0; i < nonExistingAmount; i++)
//            {
//                expectedResult.Add(Guid.NewGuid(), false);
//            }

//            DHCPv4RootScope rootScope = GetRootScope();
//            rootScope.Load(events);

//            foreach (var item in expectedResult)
//            {
//                DHCPv4Scope scope = rootScope.GetScopeById(item.Key);

//                if (item.Value == true)
//                {
//                    Assert.NotEqual(DHCPv4Scope.NotFound, scope);
//                }
//                else
//                {
//                    Assert.Equal(DHCPv4Scope.NotFound, scope);
//                }
//            }
//        }

//        private void GenerateScopeTree(
//           Double randomValue, Random random, List<Guid> parents,
//           ICollection<DomainEvent> events
//           )
//        {
//            if (randomValue > 0)
//            {
//                return;
//            }

//            Int32 scopeAmount = random.Next(3, 10);
//            Guid directParentId = parents.Last();
//            for (int i = 0; i < scopeAmount; i++)
//            {
//                Guid scopeId = Guid.NewGuid();

//                events.Add(new DHCPv4ScopeAddedEvent(new DHCPv4ScopeCreateInstruction
//                {
//                    Id = scopeId,
//                    ParentId = directParentId,
//                }));

//                List<Guid> newParentList = new List<Guid>(parents);
//                newParentList.Add(scopeId);

//                GenerateScopeTree(
//                    randomValue + random.NextDouble(), random,
//                    newParentList, events);
//            }
//        }

//        [Fact]
//        public void DHCPv4RootScope_GetRootScopes()
//        {
//            Random random = new Random();

//            var events = new List<DomainEvent>();
//            Int32 rootScopeAmount = random.Next(10, 30);
//            List<Guid> rootScopeIds = new List<Guid>(rootScopeAmount);
//            for (int i = 0; i < rootScopeAmount; i++)
//            {
//                Guid scopeId = Guid.NewGuid();

//                events.Add(new DHCPv4ScopeAddedEvent(new DHCPv4ScopeCreateInstruction
//                {
//                    Id = scopeId,
//                }));

//                rootScopeIds.Add(scopeId);

//                GenerateScopeTree(
//                    random.NextDouble(), random,
//                    new List<Guid> { scopeId }, events);
//            }

//            DHCPv4RootScope rootScope = GetRootScope();
//            rootScope.Load(events);

//            IEnumerable<DHCPv4Scope> rootScopes = rootScope.GetRootScopes();

//            Assert.Equal(rootScopeAmount, rootScopes.Count());
//            Assert.Equal(rootScopeIds.OrderBy(x => x), rootScopes.Select(x => x.Id).OrderBy(x => x));
//        }

//        private TEvent CheckScopeChangesDomainEvent<TEvent>(DHCPv4RootScope rootScope, Guid scopeId, Action<TEvent> validator)
//            where TEvent : DomainEvent
//        {
//            var changes = rootScope.GetChanges();
//            Assert.Single(changes);

//            DomainEvent @event = changes.First();
//            Assert.IsAssignableFrom<TEvent>(@event);

//            validator((TEvent)@event);

//            return (TEvent)@event;
//        }

//        private TEvent CheckScopeChangesEvent<TEvent>(DHCPv4RootScope rootScope, Guid scopeId, Action<TEvent> validator)
//            where TEvent : EntityBasedDomainEvent
//        {
//            EntityBasedDomainEvent @event =  
//                CheckScopeChangesDomainEvent(rootScope, scopeId, validator);

//            EntityBasedDomainEvent preCastedEvent = (EntityBasedDomainEvent)@event;
//            Assert.Equal(scopeId, preCastedEvent.EntityId);

//            return (TEvent)preCastedEvent;
//        }

//        [Fact]
//        public void DHCPv4RootScope_UpdateScopeName()
//        {
//            Random random = new Random();

//            String initialName = random.GetAlphanumericString(30);
//            Guid scopeId = Guid.NewGuid();

//            DHCPv4RootScope rootScope = GetRootScope();
//            rootScope.Load(new List<DomainEvent> {
//                new DHCPv4ScopeAddedEvent(new DHCPv4ScopeCreateInstruction
//            {
//                Name = ScopeName.FromString(initialName),
//                Id = scopeId,
//            })
//            });

//            String newName = random.GetAlphanumericString(20);

//            Boolean result = rootScope.UpdateScopeName(scopeId, ScopeName.FromString(newName));
//            Assert.True(result);

//            DHCPv4Scope scope = rootScope.GetScopeById(scopeId);
//            Assert.NotEqual(DHCPv4Scope.NotFound, scope);

//            Assert.Equal(newName, scope.Name);

//            CheckScopeChangesEvent<DHCPv4ScopeNameUpdatedEvent>(rootScope, scopeId, (castedEvent) =>
//             {
//                 Assert.Equal(newName, castedEvent.Name);
//             });
//        }

//        [Fact]
//        public void DHCPv4RootScope_UpdateScopeDescription()
//        {
//            Random random = new Random();

//            String initialDescription = random.GetAlphanumericString(30);
//            Guid scopeId = Guid.NewGuid();

//            DHCPv4RootScope rootScope = GetRootScope();
//            rootScope.Load(new List<DomainEvent> {
//                new DHCPv4ScopeAddedEvent(new DHCPv4ScopeCreateInstruction
//            {
//                Description = ScopeDescription.FromString(initialDescription),
//                Id = scopeId,
//            })
//            });

//            String newDescription = random.GetAlphanumericString(20);

//            Boolean result = rootScope.UpdateScopeDescription(scopeId, ScopeDescription.FromString(newDescription));
//            Assert.True(result);

//            DHCPv4Scope scope = rootScope.GetScopeById(scopeId);
//            Assert.NotEqual(DHCPv4Scope.NotFound, scope);

//            Assert.Equal(newDescription, scope.Description);

//            CheckScopeChangesEvent<DHCPv4ScopeDescriptionUpdatedEvent>(rootScope, scopeId, (castedEvent) =>
//            {
//                Assert.Equal(newDescription, castedEvent.Description);
//            });
//        }

//        [Fact]
//        public void DHCPv4RootScope_UpdateScopeResolver_NoLeases()
//        {
//            Random random = new Random();

//            String initialDescription = random.GetAlphanumericString(30);
//            Guid scopeId = Guid.NewGuid();

//            IScopeResolverManager<DHCPv4Packet, IPv4Address> initalInformation = new IScopeResolverManager<DHCPv4Packet, IPv4Address>
//            {
//                Typename = "my initial resolver",
//            };


//            IScopeResolverManager<DHCPv4Packet, IPv4Address> information = new IScopeResolverManager<DHCPv4Packet, IPv4Address>
//            {
//                Typename = "my mocked resolver",
//            };

//            Mock<IScopeResolver<DHCPv4Packet, IPv4Address>> resolverMock = new Mock<IScopeResolver<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);

//            Mock<IScopeResolver<DHCPv4Packet, IPv4Address>Manager> managerMock = new Mock<IScopeResolver<DHCPv4Packet, IPv4Address>Manager>(MockBehavior.Strict);
//            managerMock.Setup(x => x.ValidateDHCPv4Resolver(information)).Returns(true);
//            managerMock.Setup(x => x.InitializeResolver(information)).Returns(resolverMock.Object);
//            managerMock.Setup(x => x.InitializeResolver(initalInformation)).Returns<IScopeResolver<DHCPv4Packet, IPv4Address>>(null);

//            DHCPv4RootScope rootScope = GetRootScope(managerMock);
//            rootScope.Load(new List<DomainEvent> {
//                new DHCPv4ScopeAddedEvent(new DHCPv4ScopeCreateInstruction
//            {
//                Id = scopeId,
//                ResolverInformations = initalInformation,
//            })
//            });

//            Boolean result = rootScope.UpdateScopeResolver(scopeId, information);
//            Assert.True(result);

//            DHCPv4Scope scope = rootScope.GetScopeById(scopeId);
//            Assert.Equal(resolverMock.Object, scope.Resolver);

//            CheckScopeChangesEvent<DHCPv4ScopeResolverUpdatedEvent>(rootScope, scopeId,
//                (castedEvent) =>
//                {
//                    Assert.Equal(information, castedEvent.ResolverInformationen);
//                }
//            );
//        }

//        [Fact]
//        public void DHCPv4RootScope_UpdateScopeResolver_WithLeases()
//        {
//            Random random = new Random();

//            String initialDescription = random.GetAlphanumericString(30);
//            Guid scopeId = Guid.NewGuid();

//            IScopeResolverManager<DHCPv4Packet, IPv4Address> initalInformation = new IScopeResolverManager<DHCPv4Packet, IPv4Address>
//            {
//                Typename = "my initial resolver",
//            };

//            IScopeResolverManager<DHCPv4Packet, IPv4Address> information = new IScopeResolverManager<DHCPv4Packet, IPv4Address>
//            {
//                Typename = "my mocked resolver",
//            };

//            Mock<IScopeResolver<DHCPv4Packet, IPv4Address>> resolverMock = new Mock<IScopeResolver<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);

//            Mock<IScopeResolver<DHCPv4Packet, IPv4Address>Manager> managerMock = new Mock<IScopeResolver<DHCPv4Packet, IPv4Address>Manager>(MockBehavior.Strict);
//            managerMock.Setup(x => x.ValidateDHCPv4Resolver(information)).Returns(true);
//            managerMock.Setup(x => x.InitializeResolver(information)).Returns(resolverMock.Object);
//            managerMock.Setup(x => x.InitializeResolver(initalInformation)).Returns<IScopeResolver<DHCPv4Packet, IPv4Address>>(null);

//            DHCPv4RootScope rootScope = GetRootScope(managerMock);

//            List<DomainEvent> events = new List<DomainEvent>();
//            events.Add(new DHCPv4ScopeAddedEvent(new DHCPv4ScopeCreateInstruction
//            {
//                Id = scopeId,
//                ResolverInformations = initalInformation,
//            }));

//            Dictionary<Guid, Boolean> expectedResults =
//                    DHCPv4LeaseTester.AddEventsForCancelableLeases(random, scopeId, events);

//            rootScope.Load(events);

//            Boolean result = rootScope.UpdateScopeResolver(scopeId, information);
//            Assert.True(result);

//            DHCPv4Scope scope = rootScope.GetScopeById(scopeId);
//            Assert.Equal(resolverMock.Object, scope.Resolver);

//            IEnumerable<DomainEvent> changes = rootScope.GetChanges();

//            Int32 expectedAmountOfEvents = 1 + expectedResults.Where(x => x.Value == true).Count();
//            Assert.Equal(expectedAmountOfEvents, changes.Count());

//            DomainEvent firstEvent = changes.First();
//            Assert.IsAssignableFrom<DHCPv4ScopeResolverUpdatedEvent>(firstEvent);

//            DHCPv4ScopeResolverUpdatedEvent castedFirstEvent = (DHCPv4ScopeResolverUpdatedEvent)firstEvent;
//            Assert.Equal(scopeId, castedFirstEvent.EntityId);
//            Assert.Equal(information, castedFirstEvent.ResolverInformationen);

//            HashSet<Guid> blub = expectedResults.Where(x => x.Value == true).Select(x => x.Key).ToHashSet();
//            foreach (DomainEvent item in changes.Skip(1))
//            {
//                Assert.IsAssignableFrom<DHCPv4LeaseCanceledEvent>(item);

//                DHCPv4LeaseCanceledEvent castedEvent = (DHCPv4LeaseCanceledEvent)item;
//                Assert.Contains(castedFirstEvent.EntityId, blub);

//                Assert.Equal(DHCPv4LeaseCancelReasons.ResolverChanged, castedEvent.Reason);
//            }

//            foreach (var item in expectedResults)
//            {
//                DHCPv4Lease lease = scope.Leases.GetLeaseById(item.Key);
//                if (item.Value == true)
//                {
//                    Assert.Equal(LeaseStates.Canceled, lease.State);
//                }
//                else
//                {
//                    Assert.NotEqual(LeaseStates.Canceled, lease.State);
//                }
//            }
//        }

//        [Fact]
//        public void DHCPv4RootScope_UpdateScopeResolver_Failed_InvalidResolver()
//        {
//            Random random = new Random();

//            DHCPv4RootScope rootScope = GetRootScope();
//            Guid scopeId = Guid.NewGuid();

//            IScopeResolverManager<DHCPv4Packet, IPv4Address> initalInformation = new IScopeResolverManager<DHCPv4Packet, IPv4Address>
//            {
//                Typename = "my initial resolver",
//            };

//            IScopeResolverManager<DHCPv4Packet, IPv4Address> information = new IScopeResolverManager<DHCPv4Packet, IPv4Address>
//            {
//                Typename = "my mocked resolver",
//            };

//            Mock<IScopeResolver<DHCPv4Packet, IPv4Address>Manager> managerMock = new Mock<IScopeResolver<DHCPv4Packet, IPv4Address>Manager>(MockBehavior.Strict);
//            managerMock.Setup(x => x.ValidateDHCPv4Resolver(information)).Returns(false);
//            managerMock.Setup(x => x.InitializeResolver(initalInformation)).Returns<IScopeResolver<DHCPv4Packet, IPv4Address>>(null);

//            rootScope.Load(new List<DomainEvent>
//            {
//                new DHCPv4ScopeAddedEvent(new DHCPv4ScopeCreateInstruction
//            {
//                Id = scopeId,
//                ResolverInformations = initalInformation,
//            })

//            });

//            ScopeException exp = Assert.Throws<ScopeException>(
//                () => rootScope.UpdateScopeResolver(scopeId, information));

//            Assert.Equal(DHCPv4ScopeExceptionReasons.InvalidResolver, exp.Reason);
//        }

//        [Fact]
//        public void DHCPv4RootScope_UpdateScopeResolver_Failed_NoInformation()
//        {
//            Random random = new Random();

//            DHCPv4RootScope rootScope = GetRootScope();
//            Guid scopeId = Guid.NewGuid();

//            rootScope.Load(new List<DomainEvent>
//            {
//                new DHCPv4ScopeAddedEvent(new DHCPv4ScopeCreateInstruction
//            {
//                Id = scopeId,
//            })

//            });

//            ScopeException exp = Assert.Throws<ScopeException>(
//                () => rootScope.UpdateScopeResolver(scopeId, null));

//            Assert.Equal(DHCPv4ScopeExceptionReasons.NoInput, exp.Reason);
//        }

//        [Fact]
//        public void DHCPv4RootScope_UpdateScopeResolver_Failed_ScopeNotFound()
//        {
//            Random random = new Random();

//            DHCPv4RootScope rootScope = GetRootScope();
//            Guid scopeId = Guid.NewGuid();

//            IScopeResolverManager<DHCPv4Packet, IPv4Address> initalInformation = new IScopeResolverManager<DHCPv4Packet, IPv4Address>
//            {
//                Typename = "my initial resolver",
//            };

//            IScopeResolverManager<DHCPv4Packet, IPv4Address> information = new IScopeResolverManager<DHCPv4Packet, IPv4Address>
//            {
//                Typename = "my mocked resolver",
//            };

//            Mock<IScopeResolver<DHCPv4Packet, IPv4Address>Manager> managerMock = new Mock<IScopeResolver<DHCPv4Packet, IPv4Address>Manager>(MockBehavior.Strict);
//            managerMock.Setup(x => x.InitializeResolver(initalInformation)).Returns<IScopeResolver<DHCPv4Packet, IPv4Address>>(null);

//            rootScope.Load(new List<DomainEvent>
//            {
//                new DHCPv4ScopeAddedEvent(new DHCPv4ScopeCreateInstruction
//            {
//                Id = scopeId,
//                ResolverInformations = initalInformation,
//            })

//            });

//            ScopeException exp = Assert.Throws<ScopeException>(
//                () => rootScope.UpdateScopeResolver(Guid.NewGuid(), information));

//            Assert.Equal(DHCPv4ScopeExceptionReasons.ScopeNotFound, exp.Reason);
//        }

//        [Fact]
//        public void DHCPv4RootScope_UpdateScopePropertiesn()
//        {
//            Random random = new Random();

//            String initialDescription = random.GetAlphanumericString(30);
//            Guid scopeId = Guid.NewGuid();

//            DHCPv4RootScope rootScope = GetRootScope();
//            rootScope.Load(new List<DomainEvent> {
//                new DHCPv4ScopeAddedEvent(new DHCPv4ScopeCreateInstruction
//            {
//                Id = scopeId,
//            })
//            });

//            IEnumerable<DHCPv4ScopeProperty> propertyItems = random.GenerateProperties();
//            DHCPv4ScopeProperties properties = new DHCPv4ScopeProperties(propertyItems);

//            Boolean result = rootScope.UpdateScopeProperties(scopeId, properties);
//            Assert.True(result);

//            DHCPv4Scope scope = rootScope.GetScopeById(scopeId);
//            Assert.NotEqual(DHCPv4Scope.NotFound, scope);

//            Assert.Equal(properties, scope.Properties);

//            CheckScopeChangesEvent<DHCPv4ScopePropertiesUpdatedEvent>(rootScope, scopeId, (castedEvent) =>
//            {
//                Assert.Equal(properties, castedEvent.Properties);
//            });
//        }

//        [Fact]
//        public void DHCPv4RootScope_UpdateScopePropertiesn_Failed_ScopeNotFound()
//        {
//            Random random = new Random();

//            String initialDescription = random.GetAlphanumericString(30);
//            Guid scopeId = Guid.NewGuid();

//            DHCPv4RootScope rootScope = GetRootScope();
//            rootScope.Load(new List<DomainEvent> {
//                new DHCPv4ScopeAddedEvent(new DHCPv4ScopeCreateInstruction
//            {
//                Id = scopeId,
//            })
//            });

//            IEnumerable<DHCPv4ScopeProperty> propertyItems = random.GenerateProperties();
//            DHCPv4ScopeProperties properties = new DHCPv4ScopeProperties(propertyItems);

//            ScopeException exception = Assert.Throws<ScopeException>(
//                () => rootScope.UpdateScopeProperties(Guid.NewGuid(), properties));

//            Assert.Equal(DHCPv4ScopeExceptionReasons.ScopeNotFound, exception.Reason);
//        }

//        [Fact]
//        public void DHCPv4RootScope_UpdateScopePropertiesn_Failed_PropertiesAreNull()
//        {
//            Random random = new Random();

//            String initialDescription = random.GetAlphanumericString(30);
//            Guid scopeId = Guid.NewGuid();

//            DHCPv4RootScope rootScope = GetRootScope();
//            rootScope.Load(new List<DomainEvent> {
//                new DHCPv4ScopeAddedEvent(new DHCPv4ScopeCreateInstruction
//            {
//                Id = scopeId,
//            })
//            });

//            ScopeException exception = Assert.Throws<ScopeException>(
//                () => rootScope.UpdateScopeProperties(scopeId, null));

//            Assert.Equal(DHCPv4ScopeExceptionReasons.NoInput, exception.Reason);
//        }

//        [Fact]
//        public void DHCPv4RootScope_UpdateAddressProperties_ForRootScope()
//        {
//            Random random = new Random();

//            String initialDescription = random.GetAlphanumericString(30);
//            Guid scopeId = Guid.NewGuid();

//            IPv4Address start = random.GetIPv4Address();
//            IPv4Address end = random.GetIPv4AddressGreaterThan(start);

//            DHCPv4ScopeAddressProperties properties = new DHCPv4ScopeAddressProperties
//            (start, end, new IPv4Address[0],
//            TimeSpan.FromMinutes(random.Next(20, 30)), TimeSpan.FromMinutes(random.Next(40, 60)), TimeSpan.FromMinutes(random.Next(70, 90)),
//            random.NextBoolean(), DHCPv4ScopeAddressProperties.DHCPv4AddressAllocationStrategies.Next,
//            random.NextBoolean(), random.NextBoolean(), random.NextBoolean()
//                );

//            DHCPv4RootScope rootScope = GetRootScope();
//            rootScope.Load(new List<DomainEvent> {
//                new DHCPv4ScopeAddedEvent(new DHCPv4ScopeCreateInstruction
//            {
//                Id = scopeId,
//            })
//            });

//            Boolean result = rootScope.UpdateAddressProperties(scopeId, properties);
//            Assert.True(result);

//            DHCPv4Scope scope = rootScope.GetScopeById(scopeId);
//            Assert.NotEqual(DHCPv4Scope.NotFound, scope);

//            Assert.Equal(properties, scope.AddressRelatedProperties);

//            CheckScopeChangesEvent<DHCPv4ScopeAddressPropertiesUpdatedEvent>(rootScope, scopeId, (castedEvent) =>
//            {
//                Assert.Equal(properties, castedEvent.AddressProperties);
//            });
//        }

//        [Fact]
//        public void DHCPv4RootScope_UpdateAddressProperties_ForNonRootScope()
//        {
//            Random random = new Random();

//            String initialDescription = random.GetAlphanumericString(30);
//            Guid parentId = Guid.NewGuid();
//            Guid scopeId = Guid.NewGuid();

//            IPv4Address start = random.GetIPv4Address();
//            IPv4Address end = random.GetIPv4AddressGreaterThan(start);

//            DHCPv4ScopeAddressProperties properties = new DHCPv4ScopeAddressProperties
//            (start, end, new IPv4Address[0],
//            TimeSpan.FromMinutes(random.Next(20, 30)), TimeSpan.FromMinutes(random.Next(40, 60)), TimeSpan.FromMinutes(random.Next(70, 90)),
//            random.NextBoolean(), DHCPv4ScopeAddressProperties.DHCPv4AddressAllocationStrategies.Next,
//            random.NextBoolean(), random.NextBoolean(), random.NextBoolean()
//                );

//            DHCPv4ScopeAddressProperties childProperties = new DHCPv4ScopeAddressProperties
//            (start + random.Next(3, 10), end - random.Next(2, 4), new IPv4Address[0]);

//            DHCPv4RootScope rootScope = GetRootScope();
//            rootScope.Load(new List<DomainEvent> {
//                new DHCPv4ScopeAddedEvent(new DHCPv4ScopeCreateInstruction
//            {
//                Id = parentId,
//                AddressProperties = properties,
//            }),
//             new DHCPv4ScopeAddedEvent(new DHCPv4ScopeCreateInstruction
//            {
//                Id = scopeId,
//            })
//            });

//            Boolean result = rootScope.UpdateAddressProperties(scopeId, childProperties);
//            Assert.True(result);

//            DHCPv4Scope scope = rootScope.GetScopeById(scopeId);
//            Assert.NotEqual(DHCPv4Scope.NotFound, scope);

//            Assert.Equal(childProperties, scope.AddressRelatedProperties);

//            CheckScopeChangesEvent<DHCPv4ScopeAddressPropertiesUpdatedEvent>(rootScope, scopeId, (castedEvent) =>
//            {
//                Assert.Equal(childProperties, castedEvent.AddressProperties);
//            });
//        }

//        [Fact]
//        public void DHCPv4RootScope_UpdateAddressProperties_Failed_InvalidForRoot()
//        {
//            Random random = new Random();

//            Guid scopeId = Guid.NewGuid();

//            IPv4Address start = random.GetIPv4Address();
//            IPv4Address end = random.GetIPv4AddressGreaterThan(start);

//            List<DHCPv4ScopeAddressProperties> invalidProperties = new List<DHCPv4ScopeAddressProperties>
//                {
//            new DHCPv4ScopeAddressProperties
//            (start, end, new IPv4Address[0],
//            TimeSpan.FromMinutes(random.Next(20, 30)), TimeSpan.FromMinutes(random.Next(40, 60)), TimeSpan.FromMinutes(random.Next(70, 90)),
//            random.NextBoolean(), DHCPv4ScopeAddressProperties.DHCPv4AddressAllocationStrategies.Next,
//            random.NextBoolean(), random.NextBoolean(), null
//                ),
//            new DHCPv4ScopeAddressProperties
//            (start, end, new IPv4Address[0],
//            TimeSpan.FromMinutes(random.Next(20, 30)), TimeSpan.FromMinutes(random.Next(40, 60)), TimeSpan.FromMinutes(random.Next(70, 90)),
//            random.NextBoolean(), DHCPv4ScopeAddressProperties.DHCPv4AddressAllocationStrategies.Next,
//            random.NextBoolean(), null, random.NextBoolean()
//                ),
//            new DHCPv4ScopeAddressProperties
//            (start, end, new IPv4Address[0],
//            TimeSpan.FromMinutes(random.Next(20, 30)), TimeSpan.FromMinutes(random.Next(40, 60)), TimeSpan.FromMinutes(random.Next(70, 90)),
//            random.NextBoolean(), DHCPv4ScopeAddressProperties.DHCPv4AddressAllocationStrategies.Next,
//            null, random.NextBoolean(), random.NextBoolean()
//                ),
//            new DHCPv4ScopeAddressProperties
//            (start, end, new IPv4Address[0],
//            TimeSpan.FromMinutes(random.Next(20, 30)), TimeSpan.FromMinutes(random.Next(40, 60)), TimeSpan.FromMinutes(random.Next(70, 90)),
//            random.NextBoolean(), null,
//            random.NextBoolean(), random.NextBoolean(), random.NextBoolean()
//                ),
//            new DHCPv4ScopeAddressProperties
//            (start, end, new IPv4Address[0],
//            TimeSpan.FromMinutes(random.Next(20, 30)), TimeSpan.FromMinutes(random.Next(40, 60)), TimeSpan.FromMinutes(random.Next(70, 90)),
//            null, DHCPv4ScopeAddressProperties.DHCPv4AddressAllocationStrategies.Next,
//            random.NextBoolean(), random.NextBoolean(), random.NextBoolean()
//                ),
//            new DHCPv4ScopeAddressProperties
//            (start, end, new IPv4Address[0],
//            TimeSpan.FromMinutes(random.Next(20, 30)), TimeSpan.FromMinutes(random.Next(40, 60)), null,
//            random.NextBoolean(), DHCPv4ScopeAddressProperties.DHCPv4AddressAllocationStrategies.Next,
//            random.NextBoolean(), random.NextBoolean(), random.NextBoolean()
//                ),
//            new DHCPv4ScopeAddressProperties
//            (start, end, new IPv4Address[0],
//            TimeSpan.FromMinutes(random.Next(20, 30)), null, TimeSpan.FromMinutes(random.Next(70, 90)),
//            random.NextBoolean(), DHCPv4ScopeAddressProperties.DHCPv4AddressAllocationStrategies.Next,
//            random.NextBoolean(), random.NextBoolean(), random.NextBoolean()
//                ),
//            new DHCPv4ScopeAddressProperties
//            (start, end, new IPv4Address[0],
//            null, TimeSpan.FromMinutes(random.Next(40, 60)), TimeSpan.FromMinutes(random.Next(70, 90)),
//            random.NextBoolean(), DHCPv4ScopeAddressProperties.DHCPv4AddressAllocationStrategies.Next,
//            random.NextBoolean(), random.NextBoolean(), random.NextBoolean()
//                ),
//            new DHCPv4ScopeAddressProperties
//            (start, end, new IPv4Address[0],
//            TimeSpan.FromMinutes(random.Next(40, 60)), TimeSpan.FromMinutes(random.Next(20, 30)), TimeSpan.FromMinutes(random.Next(70, 90)),
//            random.NextBoolean(), DHCPv4ScopeAddressProperties.DHCPv4AddressAllocationStrategies.Next,
//            random.NextBoolean(), random.NextBoolean(), random.NextBoolean()
//                ),
//            new DHCPv4ScopeAddressProperties
//            (start, end, new IPv4Address[0],
//            TimeSpan.FromMinutes(random.Next(20, 30)), TimeSpan.FromMinutes(random.Next(40, 60)), TimeSpan.FromMinutes(random.Next(30, 40)),
//            random.NextBoolean(), DHCPv4ScopeAddressProperties.DHCPv4AddressAllocationStrategies.Next,
//            random.NextBoolean(), random.NextBoolean(), random.NextBoolean()
//                ),
//            };

//            foreach (var item in invalidProperties)
//            {

//                DHCPv4RootScope rootScope = GetRootScope();
//                rootScope.Load(new List<DomainEvent> {
//                new DHCPv4ScopeAddedEvent(new DHCPv4ScopeCreateInstruction
//                {
//                    Id = scopeId,
//                    AddressProperties = item
//                })
//                });

//                ScopeException exp = Assert.Throws<ScopeException>(() => rootScope.UpdateAddressProperties(scopeId, item));

//                Assert.Equal(DHCPv4ScopeExceptionReasons.AddressPropertiesInvalidForParents, exp.Reason);
//            }
//        }

//        [Theory]
//        [InlineData("192.168.178.1", "192.168.178.1", "192.168.178.1", "192.168.178.1", true)]
//        [InlineData("192.168.178.0", "192.168.178.255", "192.168.178.10", "192.168.178.255", true)]
//        [InlineData("192.168.178.0", "192.168.178.255", "192.168.178.10", "192.168.178.100", true)]
//        [InlineData("192.168.178.0", "192.168.178.255", "192.168.178.0", "192.168.178.100", true)]
//        [InlineData("192.168.178.0", "192.168.178.255", "192.168.178.0", "192.168.178.255", true)]

//        [InlineData("192.168.178.1", "192.168.178.1", "192.168.178.2", "192.168.178.3", false)]
//        [InlineData("192.168.178.0", "192.168.178.255", "192.168.177.0", "192.168.178.100", false)]
//        [InlineData("192.168.178.0", "192.168.178.255", "192.168.177.0", "192.168.177.255", false)]
//        [InlineData("192.168.178.0", "192.168.178.255", "192.168.178.10", "192.168.179.13", false)]
//        public void DHCPv4RootScope_UpdateAddressProperties_AddressRangeOfParen(
//            String rawParentStart, String rawParentEnd,
//            String rawChildStart, String rawChildEnd,
//            Boolean shouldBeInRange
//            )
//        {
//            Random random = new Random();

//            String initialDescription = random.GetAlphanumericString(30);
//            Guid parentId = Guid.NewGuid();
//            Guid scopeId = Guid.NewGuid();

//            IPv4Address start = IPv4Address.FromString(rawParentStart);
//            IPv4Address end = IPv4Address.FromString(rawParentEnd);

//            DHCPv4ScopeAddressProperties properties = new DHCPv4ScopeAddressProperties
//            (start, end, new IPv4Address[0],
//            TimeSpan.FromMinutes(random.Next(20, 30)), TimeSpan.FromMinutes(random.Next(40, 60)), TimeSpan.FromMinutes(random.Next(70, 90)),
//            random.NextBoolean(), DHCPv4ScopeAddressProperties.DHCPv4AddressAllocationStrategies.Next,
//            random.NextBoolean(), random.NextBoolean(), random.NextBoolean()
//                );

//            IPv4Address startChild = IPv4Address.FromString(rawChildStart);
//            IPv4Address endChild = IPv4Address.FromString(rawChildEnd);

//            DHCPv4ScopeAddressProperties childProperties = new DHCPv4ScopeAddressProperties
//            (startChild, endChild, new IPv4Address[0]);

//            DHCPv4RootScope rootScope = GetRootScope();
//            rootScope.Load(new List<DomainEvent> {
//                new DHCPv4ScopeAddedEvent(new DHCPv4ScopeCreateInstruction
//            {
//                Id = parentId,
//                AddressProperties = properties,
//            }),
//             new DHCPv4ScopeAddedEvent(new DHCPv4ScopeCreateInstruction
//            {
//                Id = scopeId,
//            })
//            });

//            if (shouldBeInRange == false)
//            {

//                ScopeException exp = Assert.Throws<ScopeException>(() => rootScope.UpdateAddressProperties(scopeId, childProperties));
//                Assert.Equal(DHCPv4ScopeExceptionReasons.NotInParentRange, exp.Reason);
//            }
//            else
//            {
//                Boolean result = rootScope.UpdateAddressProperties(scopeId, childProperties);
//                Assert.True(result);
//            }
//        }

//        [Theory]
//        [InlineData(10, 20, 30, null, null, null, true)]
//        [InlineData(10, 20, 30, null, null, 35, true)]
//        [InlineData(10, 20, 30, null, 25, null, true)]
//        [InlineData(10, 20, 30, 15, null, null, true)]
//        [InlineData(10, 20, 30, 2, null, null, true)]
//        [InlineData(10, 20, 30, null, 30, 40, true)]
//        [InlineData(10, 20, 30, 15, 30, 40, true)]
//        [InlineData(10, 20, 30, 10, 20, 30, true)]

//        [InlineData(10, 20, 30, null, 35, null, false)]
//        [InlineData(10, 20, 30, null, null, 15, false)]
//        [InlineData(10, 20, 30, 21, null, null, false)]
//        [InlineData(10, 20, 30, 20, null, null, false)]
//        [InlineData(10, 20, 30, 5, 3, 2, false)]
//        [InlineData(10, 20, 30, 2, 5, 3, false)]
//        [InlineData(10, 20, 30, 3, 2, 5, false)]
//        public void DHCPv4RootScope_UpdateAddressProperties_TimeRange(
//            Int32 parentRenewalTime, Int32 parentPreferredLifetime, Int32 parentValidLifetime,
//            Int32? childRenewalTime, Int32? childPreferredLifetime, Int32? childValidLifetime,
//            Boolean shouldBeInRange
//        )
//        {
//            Random random = new Random();

//            String initialDescription = random.GetAlphanumericString(30);
//            Guid parentId = Guid.NewGuid();
//            Guid scopeId = Guid.NewGuid();

//            IPv4Address start = random.GetIPv4Address();
//            IPv4Address end = random.GetIPv4AddressGreaterThan(start);

//            DHCPv4ScopeAddressProperties properties = new DHCPv4ScopeAddressProperties
//            (start, end, new IPv4Address[0],
//             renewalTime: TimeSpan.FromMinutes(parentRenewalTime),
//             preferredLifetime: TimeSpan.FromMinutes(parentPreferredLifetime),
//             validLifetime: TimeSpan.FromMinutes(parentValidLifetime)
//                );

//            DHCPv4ScopeAddressProperties childProperties = new DHCPv4ScopeAddressProperties
//            (start + random.Next(3, 10), end - random.Next(2, 4), new IPv4Address[0],
//             renewalTime: childRenewalTime.HasValue == false ? new TimeSpan?() : TimeSpan.FromMinutes(childRenewalTime.Value),
//             preferredLifetime: childPreferredLifetime.HasValue == false ? new TimeSpan?() : TimeSpan.FromMinutes(childPreferredLifetime.Value),
//             validLifetime: childValidLifetime.HasValue == false ? new TimeSpan?() : TimeSpan.FromMinutes(childValidLifetime.Value)
//            );

//            DHCPv4RootScope rootScope = GetRootScope();
//            rootScope.Load(new List<DomainEvent> {
//                new DHCPv4ScopeAddedEvent(new DHCPv4ScopeCreateInstruction
//            {
//                Id = parentId,
//                AddressProperties = properties,
//            }),
//             new DHCPv4ScopeAddedEvent(new DHCPv4ScopeCreateInstruction
//            {
//                Id = scopeId,
//            })
//            });

//            if (shouldBeInRange == false)
//            {
//                ScopeException exp = Assert.Throws<ScopeException>(() => rootScope.UpdateAddressProperties(scopeId, childProperties));
//                Assert.Equal(DHCPv4ScopeExceptionReasons.InvalidTimeRanges, exp.Reason);
//            }
//            else
//            {
//                Boolean result = rootScope.UpdateAddressProperties(scopeId, childProperties);
//                Assert.True(result);
//            }
//        }

//        [Fact]
//        public void DHCPv4RootScope_DeleteScope_NonRootScope_IncludeChildren()
//        {
//            Random random = new Random();

//            Guid parentId = Guid.NewGuid();
//            Guid scopeId = Guid.NewGuid();

//            DHCPv4RootScope rootScope = GetRootScope();
//            List<DomainEvent> events = new List<DomainEvent> {
//                new DHCPv4ScopeAddedEvent(new DHCPv4ScopeCreateInstruction
//            {
//                Id = parentId,
//            }),
//             new DHCPv4ScopeAddedEvent(new DHCPv4ScopeCreateInstruction
//            {
//                 ParentId = parentId,
//                Id = scopeId,
//            }) };

//            Int32 subChildAmount = random.Next(3, 10);
//            List<Guid> subChildIds = new List<Guid>(subChildAmount);
//            for (int i = 0; i < subChildAmount; i++)
//            {
//                Guid subScopeId = Guid.NewGuid();
//                subChildIds.Add(subScopeId);
//                events.Add(new DHCPv4ScopeAddedEvent(new DHCPv4ScopeCreateInstruction
//                {
//                    ParentId = scopeId,
//                    Id = subScopeId,
//                }));
//            }

//            rootScope.Load(events);

//            Boolean result = rootScope.DeleteScope(scopeId, true);
//            Assert.True(result);

//            DHCPv4Scope scope = rootScope.GetScopeById(scopeId);
//            Assert.Equal(DHCPv4Scope.NotFound, scope);

//            foreach (Guid subChildId in subChildIds)
//            {
//                DHCPv4Scope subchildScope = rootScope.GetScopeById(subChildId);
//                Assert.Equal(DHCPv4Scope.NotFound, subchildScope);
//            }

//            IEnumerable<DomainEvent> changes = rootScope.GetChanges();
//            Assert.NotEmpty(changes);
//            Assert.Equal(subChildIds.Count + 1, changes.Count());

//            foreach (DomainEvent item in changes)
//            {
//                Assert.IsAssignableFrom<DHCPv4ScopeDeletedEvent>(item);

//                DHCPv4ScopeDeletedEvent castedEvent = (DHCPv4ScopeDeletedEvent)item;
//                Assert.True(castedEvent.IncludeChildren);

//                if (item == changes.First())
//                {
//                    Assert.Equal(scopeId, castedEvent.EntityId);
//                }
//                else
//                {
//                    Assert.Contains(castedEvent.EntityId, subChildIds);
//                    subChildIds.Remove(castedEvent.EntityId);
//                }
//            }
//        }

//        [Fact]
//        public void DHCPv4RootScope_DeleteScope_NonRootScope_NotIncludeChildren()
//        {
//            Random random = new Random();

//            Guid parentId = Guid.NewGuid();
//            Guid scopeId = Guid.NewGuid();

//            DHCPv4RootScope rootScope = GetRootScope();
//            List<DomainEvent> events = new List<DomainEvent> {
//                new DHCPv4ScopeAddedEvent(new DHCPv4ScopeCreateInstruction
//            {
//                Id = parentId,
//            }),
//             new DHCPv4ScopeAddedEvent(new DHCPv4ScopeCreateInstruction
//            {
//                 ParentId = parentId,
//                Id = scopeId,
//            }) };

//            Int32 subChildAmount = random.Next(3, 10);
//            List<Guid> subChildIds = new List<Guid>(subChildAmount);
//            for (int i = 0; i < subChildAmount; i++)
//            {
//                Guid subScopeId = Guid.NewGuid();
//                subChildIds.Add(subScopeId);
//                events.Add(new DHCPv4ScopeAddedEvent(new DHCPv4ScopeCreateInstruction
//                {
//                    ParentId = scopeId,
//                    Id = subScopeId,
//                }));
//            }

//            rootScope.Load(events);

//            Boolean result = rootScope.DeleteScope(scopeId, false);
//            Assert.True(result);

//            DHCPv4Scope scope = rootScope.GetScopeById(scopeId);
//            Assert.Equal(DHCPv4Scope.NotFound, scope);

//            Assert.Empty(scope.GetChildScopes());

//            foreach (Guid subChildId in subChildIds)
//            {
//                DHCPv4Scope subchildScope = rootScope.GetScopeById(subChildId);
//                Assert.NotEqual(DHCPv4Scope.NotFound, subchildScope);
//                Assert.NotEqual(DHCPv4Scope.NotFound, subchildScope.ParentScope);
//                Assert.Equal(parentId, subchildScope.ParentScope.Id);
//            }

//            IEnumerable<DomainEvent> changes = rootScope.GetChanges();
//            Assert.NotEmpty(changes);
//            Assert.Equal(subChildIds.Count + 1, changes.Count());

//            foreach (DomainEvent item in changes)
//            {
//                if (item == changes.First())
//                {
//                    Assert.IsAssignableFrom<DHCPv4ScopeDeletedEvent>(item);

//                    DHCPv4ScopeDeletedEvent castedEvent = (DHCPv4ScopeDeletedEvent)item;
//                    Assert.True(castedEvent.IncludeChildren);

//                    Assert.Equal(scopeId, castedEvent.EntityId);
//                }
//                else
//                {
//                    Assert.IsAssignableFrom<DHCPv4ScopeParentUpdatedEvent>(item);
//                    DHCPv4ScopeParentUpdatedEvent castedEvent = (DHCPv4ScopeParentUpdatedEvent)item;
//                    Assert.Equal(parentId, castedEvent.ParentId);
//                    Assert.Contains(castedEvent.EntityId, subChildIds);
//                    subChildIds.Remove(castedEvent.EntityId);
//                }
//            }
//        }

//        [Fact]
//        public void DHCPv4RootScope_DeleteScope_RootScope_IncludeChildren()
//        {
//            Random random = new Random();

//            Guid parentId = Guid.NewGuid();
//            Guid scopeId = Guid.NewGuid();

//            DHCPv4RootScope rootScope = GetRootScope();
//            List<DomainEvent> events = new List<DomainEvent> {
//                new DHCPv4ScopeAddedEvent(new DHCPv4ScopeCreateInstruction
//            {
//                Id = parentId,
//            }),
//             new DHCPv4ScopeAddedEvent(new DHCPv4ScopeCreateInstruction
//            {
//                 ParentId = parentId,
//                Id = scopeId,
//            }) };

//            Int32 subChildAmount = random.Next(3, 10);
//            List<Guid> subChildIds = new List<Guid>(subChildAmount);
//            for (int i = 0; i < subChildAmount; i++)
//            {
//                Guid subScopeId = Guid.NewGuid();
//                subChildIds.Add(subScopeId);
//                events.Add(new DHCPv4ScopeAddedEvent(new DHCPv4ScopeCreateInstruction
//                {
//                    ParentId = scopeId,
//                    Id = subScopeId,
//                }));
//            }

//            rootScope.Load(events);

//            Boolean result = rootScope.DeleteScope(parentId, true);
//            Assert.True(result);

//            DHCPv4Scope parent = rootScope.GetScopeById(parentId);
//            Assert.Equal(DHCPv4Scope.NotFound, parent);

//            DHCPv4Scope scope = rootScope.GetScopeById(scopeId);
//            Assert.Equal(DHCPv4Scope.NotFound, scope);

//            foreach (Guid subChildId in subChildIds)
//            {
//                DHCPv4Scope subchildScope = rootScope.GetScopeById(subChildId);
//                Assert.Equal(DHCPv4Scope.NotFound, subchildScope);
//            }

//            IEnumerable<DomainEvent> changes = rootScope.GetChanges();
//            Assert.NotEmpty(changes);
//            Assert.Equal(subChildIds.Count + 2, changes.Count());

//            foreach (DomainEvent item in changes)
//            {
//                Assert.IsAssignableFrom<DHCPv4ScopeDeletedEvent>(item);

//                DHCPv4ScopeDeletedEvent castedEvent = (DHCPv4ScopeDeletedEvent)item;
//                Assert.True(castedEvent.IncludeChildren);

//                if (item == changes.First())
//                {
//                    Assert.Equal(parentId, castedEvent.EntityId);
//                }
//                else if (item == changes.ElementAt(1))
//                {
//                    Assert.Equal(scopeId, castedEvent.EntityId);
//                }
//                else
//                {
//                    Assert.Contains(castedEvent.EntityId, subChildIds);
//                    subChildIds.Remove(castedEvent.EntityId);
//                }
//            }
//        }

//        [Fact]
//        public void DHCPv4RootScope_DeleteScope_RootScope_NotIncludeChildren()
//        {
//            Random random = new Random();

//            Guid parentId = Guid.NewGuid();
//            Guid scopeId = Guid.NewGuid();

//            DHCPv4RootScope rootScope = GetRootScope();
//            List<DomainEvent> events = new List<DomainEvent> {
//                new DHCPv4ScopeAddedEvent(new DHCPv4ScopeCreateInstruction
//            {
//                Id = parentId,
//            }),
//             new DHCPv4ScopeAddedEvent(new DHCPv4ScopeCreateInstruction
//            {
//                 ParentId = parentId,
//                Id = scopeId,
//            }) };

//            Int32 subChildAmount = random.Next(3, 10);
//            List<Guid> subChildIds = new List<Guid>(subChildAmount);
//            for (int i = 0; i < subChildAmount; i++)
//            {
//                Guid subScopeId = Guid.NewGuid();
//                subChildIds.Add(subScopeId);
//                events.Add(new DHCPv4ScopeAddedEvent(new DHCPv4ScopeCreateInstruction
//                {
//                    ParentId = scopeId,
//                    Id = subScopeId,
//                }));
//            }

//            rootScope.Load(events);

//            Boolean result = rootScope.DeleteScope(parentId, false);
//            Assert.True(result);

//            DHCPv4Scope parentScope = rootScope.GetScopeById(parentId);
//            Assert.Equal(DHCPv4Scope.NotFound, parentScope);

//            DHCPv4Scope scope = rootScope.GetScopeById(scopeId);
//            Assert.NotEqual(DHCPv4Scope.NotFound, scope);

//            Assert.Equal(subChildIds, scope.GetChildIds(true));

//            Assert.Equal(DHCPv4Scope.NotFound, scope.ParentScope);

//            foreach (Guid subChildId in subChildIds)
//            {
//                DHCPv4Scope subchildScope = rootScope.GetScopeById(subChildId);
//                Assert.Equal(scope, subchildScope.ParentScope);
//            }

//            IEnumerable<DomainEvent> changes = rootScope.GetChanges();
//            Assert.NotEmpty(changes);
//            Assert.Equal(2, changes.Count());

//            foreach (DomainEvent item in changes)
//            {
//                if (item == changes.First())
//                {
//                    Assert.IsAssignableFrom<DHCPv4ScopeDeletedEvent>(item);

//                    DHCPv4ScopeDeletedEvent castedEvent = (DHCPv4ScopeDeletedEvent)item;
//                    Assert.False(castedEvent.IncludeChildren);

//                    Assert.Equal(parentId, castedEvent.EntityId);
//                }
//                else
//                {
//                    Assert.IsAssignableFrom<DHCPv4ScopeParentUpdatedEvent>(item);
//                    DHCPv4ScopeParentUpdatedEvent castedEvent = (DHCPv4ScopeParentUpdatedEvent)item;
//                    Assert.Null(castedEvent.ParentId);

//                    Assert.Equal(scopeId, castedEvent.EntityId);
//                }
//            }
//        }

//        [Fact]
//        public void DHCPv4RootScope_DeleteScope_Failed_ScopeNotFound()
//        {
//            Random random = new Random();

//            DHCPv4RootScope rootScope = GetRootScope();
//            Guid scopeId = Guid.NewGuid();

//            rootScope.Load(new List<DomainEvent>
//            {
//                new DHCPv4ScopeAddedEvent(new DHCPv4ScopeCreateInstruction
//            {
//                Id = scopeId,
//            })

//            });

//            ScopeException exp = Assert.Throws<ScopeException>(
//                () => rootScope.DeleteScope(Guid.NewGuid(), random.NextBoolean()));

//            Assert.Equal(DHCPv4ScopeExceptionReasons.ScopeNotFound, exp.Reason);
//        }

//        [Fact]
//        public void DHCPv4RootScope_UpdateParent_NonRootToRoot()
//        {
//            Random random = new Random();

//            DHCPv4RootScope rootScope = GetRootScope();
//            Guid parentId = Guid.NewGuid();
//            Guid scopeId = Guid.NewGuid();

//            rootScope.Load(new List<DomainEvent>
//            {
//            new DHCPv4ScopeAddedEvent(new DHCPv4ScopeCreateInstruction
//            {
//                Id = parentId,
//            }),
//            new DHCPv4ScopeAddedEvent(new DHCPv4ScopeCreateInstruction
//            {
//               ParentId = parentId,
//                Id = scopeId,
//            }),

//            });

//            Boolean actual = rootScope.UpdateParent(scopeId, null);
//            Assert.True(actual);

//            DHCPv4Scope parentScope = rootScope.GetScopeById(parentId);
//            DHCPv4Scope childScope = rootScope.GetScopeById(scopeId);

//            Assert.DoesNotContain(childScope, parentScope.GetChildScopes());
//            Assert.Equal(DHCPv4Scope.NotFound, childScope.ParentScope);
//            Assert.Empty(childScope.GetChildScopes());

//            CheckScopeChangesEvent<DHCPv4ScopeParentUpdatedEvent>(rootScope, scopeId, (castedEvent) =>
//            {
//                Assert.Null(castedEvent.ParentId);
//            });
//        }

//        [Fact]
//        public void DHCPv4RootScope_UpdateParent_RootToNonRoot()
//        {
//            Random random = new Random();

//            DHCPv4RootScope rootScope = GetRootScope();
//            Guid parentId = Guid.NewGuid();
//            Guid scopeId = Guid.NewGuid();

//            rootScope.Load(new List<DomainEvent>
//            {
//            new DHCPv4ScopeAddedEvent(new DHCPv4ScopeCreateInstruction
//            {
//                Id = parentId,
//            }),
//            new DHCPv4ScopeAddedEvent(new DHCPv4ScopeCreateInstruction
//            {
//                Id = scopeId,
//            }),

//            });

//            Boolean actual = rootScope.UpdateParent(scopeId, parentId);
//            Assert.True(actual);

//            DHCPv4Scope parentScope = rootScope.GetScopeById(parentId);
//            DHCPv4Scope childScope = rootScope.GetScopeById(scopeId);

//            Assert.Contains(childScope, parentScope.GetChildScopes());
//            Assert.Equal(parentScope, childScope.ParentScope);
//            Assert.Empty(childScope.GetChildScopes());

//            CheckScopeChangesEvent<DHCPv4ScopeParentUpdatedEvent>(rootScope, scopeId, (castedEvent) =>
//            {
//                Assert.NotNull(castedEvent.ParentId);
//                Assert.Equal(parentId, castedEvent.ParentId.Value);
//            });
//        }

//        [Fact]
//        public void DHCPv4RootScope_UpdateParent_NonRootToAnotherNonRoot()
//        {
//            Random random = new Random();

//            DHCPv4RootScope rootScope = GetRootScope();
//            Guid originalParentId = Guid.NewGuid();
//            Guid newParentId = Guid.NewGuid();

//            Guid scopeId = Guid.NewGuid();

//            rootScope.Load(new List<DomainEvent>
//            {
//            new DHCPv4ScopeAddedEvent(new DHCPv4ScopeCreateInstruction
//            {
//                Id = originalParentId,
//            }),
//            new DHCPv4ScopeAddedEvent(new DHCPv4ScopeCreateInstruction
//            {
//               ParentId = originalParentId,
//                Id = scopeId,
//            }),
//            new DHCPv4ScopeAddedEvent(new DHCPv4ScopeCreateInstruction
//            {
//                Id = newParentId,
//            }),
//            });

//            Boolean actual = rootScope.UpdateParent(scopeId, newParentId);
//            Assert.True(actual);

//            DHCPv4Scope originalParentScope = rootScope.GetScopeById(originalParentId);
//            DHCPv4Scope newParentScope = rootScope.GetScopeById(newParentId);
//            DHCPv4Scope childScope = rootScope.GetScopeById(scopeId);

//            Assert.DoesNotContain(childScope, originalParentScope.GetChildScopes());
//            Assert.Equal(newParentScope, childScope.ParentScope);
//            Assert.Contains(childScope, newParentScope.GetChildScopes());

//            Assert.Empty(childScope.GetChildScopes());

//            CheckScopeChangesEvent<DHCPv4ScopeParentUpdatedEvent>(rootScope, scopeId, (castedEvent) =>
//            {
//                Assert.NotNull(castedEvent.ParentId);
//                Assert.Equal(newParentId, castedEvent.ParentId.Value);
//            });
//        }

//        [Fact]
//        public void DHCPv4RootScope_UpdateParent_Failed_ScopeNotFound()
//        {
//            Random random = new Random();

//            DHCPv4RootScope rootScope = GetRootScope();
//            Guid scopeId = Guid.NewGuid();

//            rootScope.Load(new List<DomainEvent>
//            {
//                new DHCPv4ScopeAddedEvent(new DHCPv4ScopeCreateInstruction
//            {
//                Id = scopeId,
//            })

//            });

//            ScopeException exp = Assert.Throws<ScopeException>(
//                () => rootScope.UpdateParent(Guid.NewGuid(), null));

//            Assert.Equal(DHCPv4ScopeExceptionReasons.ScopeNotFound, exp.Reason);
//        }

//        [Fact]
//        public void DHCPv4RootScope_UpdateParent_Failed_ParentNotFound()
//        {
//            Random random = new Random();

//            DHCPv4RootScope rootScope = GetRootScope();
//            Guid scopeId = Guid.NewGuid();

//            rootScope.Load(new List<DomainEvent>
//            {
//                new DHCPv4ScopeAddedEvent(new DHCPv4ScopeCreateInstruction
//            {
//                Id = scopeId,
//            })

//            });

//            ScopeException exp = Assert.Throws<ScopeException>(
//                () => rootScope.UpdateParent(scopeId, Guid.NewGuid()));

//            Assert.Equal(DHCPv4ScopeExceptionReasons.ScopeParentNotFound, exp.Reason);
//        }

//        [Fact]
//        public void DHCPv4RootScope_UpdateParent_Failed_ParentAddedAsChild()
//        {
//            Random random = new Random();

//            DHCPv4RootScope rootScope = GetRootScope();
//            Guid scopeId = Guid.NewGuid();
//            Guid parentId = Guid.NewGuid();

//            rootScope.Load(new List<DomainEvent>
//            {
//                   new DHCPv4ScopeAddedEvent(new DHCPv4ScopeCreateInstruction
//            {
//                Id = parentId,
//            }),
//                new DHCPv4ScopeAddedEvent(new DHCPv4ScopeCreateInstruction
//            {
//                Id = scopeId,
//                ParentId = parentId
//            })
//            });

//            ScopeException exp = Assert.Throws<ScopeException>(
//                () => rootScope.UpdateParent(parentId, scopeId));

//            //Assert.Equal(DHCPv4ScopeExceptionReasons.ParentNotMoveable, exp.Reason);
//        }

//        [Fact]
//        public void DHCPv4RootScope_AddScope_RootScope()
//        {
//            Random random = new Random();

//            DHCPv4RootScope rootScope = GetRootScope();

//            Guid scopeId = Guid.NewGuid();
//            String description = random.GetAlphanumericString(30);
//            String name = random.GetAlphanumericString(20);

//            IScopeResolverManager<DHCPv4Packet, IPv4Address> information = new IScopeResolverManager<DHCPv4Packet, IPv4Address>
//            {
//                Typename = "my mocked resolver",
//            };

//            Mock<IScopeResolver<DHCPv4Packet, IPv4Address>> resolverMock = new Mock<IScopeResolver<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);
//            IScopeResolver<DHCPv4Packet, IPv4Address> mockedResolver = resolverMock.Object;

//            Mock<IScopeResolver<DHCPv4Packet, IPv4Address>Manager> managerMock = new Mock<IScopeResolver<DHCPv4Packet, IPv4Address>Manager>(MockBehavior.Strict);
//            managerMock.Setup(x => x.ValidateDHCPv4Resolver(information)).Returns(true);
//            managerMock.Setup(x => x.InitializeResolver(information)).Returns(mockedResolver);

//            var properties = new DHCPv4ScopeProperties(random.GenerateProperties());
//            DHCPv4ScopeAddressProperties addressProperties = null;

//            DHCPv4ScopeCreateInstruction instruction = new DHCPv4ScopeCreateInstruction
//            {
//                AddressProperties = addressProperties,
//                Description = ScopeDescription.FromString(description),
//                Name = ScopeName.FromString(name),
//                Properties = properties,
//                ResolverInformations = information,
//            };

//            Boolean actual = rootScope.AddScope(instruction);
//            Assert.True(actual);

//            DHCPv4Scope scope = rootScope.GetScopeById(scopeId);

//            Assert.Equal(scopeId, scope.Id);
//            Assert.Equal(name, scope.Name);
//            Assert.Equal(description, scope.Description);
//            Assert.Equal(properties, scope.Properties);
//            Assert.Equal(addressProperties, scope.AddressRelatedProperties);
//            Assert.Equal(mockedResolver, scope.Resolver);
//            Assert.Equal(DHCPv4Scope.NotFound, scope.ParentScope);

//            CheckScopeChangesDomainEvent<DHCPv4ScopeAddedEvent>(rootScope, scopeId, (castedEvent) =>
//            {
//                Assert.Equal(scopeId, castedEvent.Instructions.Id);
//                Assert.Equal(addressProperties, castedEvent.Instructions.AddressProperties);
//                Assert.Equal(description, castedEvent.Instructions.Description);
//                Assert.Equal(name, castedEvent.Instructions.Name);
//                Assert.Null(castedEvent.Instructions.ParentId);
//                Assert.Equal(properties, castedEvent.Instructions.Properties);
//                Assert.Equal(information, castedEvent.Instructions.ResolverInformations);
//            });
//        }
//    }
//}
