using DaAPI.Core.Common;
using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Packets.DHCPv6;
using DaAPI.Core.Scopes;
using DaAPI.Core.Scopes.DHCPv6;
using DaAPI.TestHelper;
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
    public class DHCPv6LeaseTester
    {
        public DHCPv6RootScope GetRootScope()
        {
            Mock<ILoggerFactory> factoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);
            factoryMock.Setup(x => x.CreateLogger(It.IsAny<String>())).Returns(Mock.Of<ILogger<DHCPv6RootScope>>());

            var scope = new DHCPv6RootScope(Guid.NewGuid(), Mock.Of<IScopeResolverManager<DHCPv6Packet, IPv6Address>>(), factoryMock.Object);
            return scope;

        }

        [Fact]
        public void MatchesUniqueIdentiifer()
        {
            Random random = new Random();
            DHCPv6RootScope rootScope = GetRootScope();

            Guid scopeId = Guid.NewGuid();
            Byte[] uniqueIdentifier = random.NextBytes(10);

            List<DomainEvent> events = new List<DomainEvent>
            {
                 new DHCPv6ScopeAddedEvent(new DHCPv6ScopeCreateInstruction
                 {
                     Id = scopeId,
                 }),
            };

            Int32 leaseAmount = random.Next(3, 10);
            Dictionary<Guid, Boolean> expectedResults = new Dictionary<Guid, bool>();
            for (int i = 0; i < leaseAmount; i++)
            {
                Guid leaseId = Guid.NewGuid();

                Byte[] identifier = null;
                Boolean matches = false;
                Double randomValue = random.NextDouble();
                if (randomValue > 0.75)
                {
                    identifier = uniqueIdentifier;
                    matches = true;
                }
                else if (randomValue > 0.5)
                {
                    identifier = random.NextBytes(12);
                }
                else if (randomValue > 0.25)
                {
                    identifier = new byte[0];
                }

                events.Add(new DHCPv6LeaseCreatedEvent
                {
                    ScopeId = scopeId,
                    EntityId = leaseId,
                    UniqueIdentiifer = identifier,
                    Address = random.GetIPv6Address(),
                    ClientIdentifier = new UUIDDUID(random.NextGuid()),
                });

                expectedResults.Add(leaseId, matches);
            }

            rootScope.Load(events);

            DHCPv6Scope scope = rootScope.GetRootScopes().First();
            foreach (var item in expectedResults)
            {
                DHCPv6Lease lease = scope.Leases.GetLeaseById(item.Key);
                Boolean actual = lease.MatchesUniqueIdentiifer(uniqueIdentifier);
                Assert.Equal(item.Value, actual);
            }
        }

        [Fact]
        public void AddressesAreInUse()
        {
            Random random = new Random();
            DHCPv6RootScope rootScope = GetRootScope();

            Guid scopeId = Guid.NewGuid();

            List<DomainEvent> events = new List<DomainEvent>
            {
                 new DHCPv6ScopeAddedEvent(new DHCPv6ScopeCreateInstruction
                 {
                     Id = scopeId,
                 }),
            };

            Int32 leaseAmount = random.Next(10, 30);
            Dictionary<Guid, Boolean> expectedResults = new Dictionary<Guid, bool>();
            for (int i = 0; i < leaseAmount; i++)
            {
                Guid leaseId = Guid.NewGuid();

                events.Add(new DHCPv6LeaseCreatedEvent
                {
                    ScopeId = scopeId,
                    EntityId = leaseId,
                    Address = random.GetIPv6Address(),
                    ClientIdentifier = new UUIDDUID(random.NextGuid()),
                });

                DomainEvent eventToAdd = null;
                Boolean addressIsInUse = true;
                Double randomValue = random.NextDouble();
                Double possiblities = 5.0;
                if (randomValue < 1 / possiblities)
                {
                    eventToAdd = new DHCPv6LeaseReleasedEvent(leaseId, false);
                    addressIsInUse = false;
                }
                else if (randomValue < 2 / possiblities)
                {
                    eventToAdd = new DHCPv6LeaseRevokedEvent(leaseId);
                    addressIsInUse = false;

                }
                else if (randomValue < 3 / possiblities)
                {
                    eventToAdd = new DHCPv6AddressSuspendedEvent(leaseId, random.GetIPv6Address(), DateTime.UtcNow.AddHours(12));
                    addressIsInUse = false;
                }
                else if (randomValue < 4 / possiblities)
                {
                    eventToAdd = new DHCPv6LeaseCanceledEvent(leaseId, LeaseCancelReasons.NotSpecified);
                    addressIsInUse = false;
                }
                if (eventToAdd != null)
                {
                    events.Add(eventToAdd);
                }

                expectedResults.Add(leaseId, addressIsInUse);
            }

            rootScope.Load(events);

            DHCPv6Scope scope = rootScope.GetRootScopes().First();
            foreach (var item in expectedResults)
            {
                DHCPv6Lease lease = scope.Leases.GetLeaseById(item.Key);
                Boolean actual = lease.AddressIsInUse();
                Assert.Equal(item.Value, actual);
            }
        }

        [Fact]
        public void IsPending()
        {
            Random random = new Random();
            DHCPv6RootScope rootScope = GetRootScope();

            Guid scopeId = Guid.NewGuid();

            List<DomainEvent> events = new List<DomainEvent>
            {
                 new DHCPv6ScopeAddedEvent(new DHCPv6ScopeCreateInstruction
                 {
                     Id = scopeId,
                 }),
            };

            Int32 leaseAmount = random.Next(10, 30);
            Dictionary<Guid, Boolean> expectedResults = new Dictionary<Guid, bool>();
            for (int i = 0; i < leaseAmount; i++)
            {
                Guid leaseId = Guid.NewGuid();

                events.Add(new DHCPv6LeaseCreatedEvent
                {
                    ScopeId = scopeId,
                    EntityId = leaseId,
                    Address = random.GetIPv6Address(),
                    ClientIdentifier = new UUIDDUID(random.NextGuid()),
                });

                Boolean addressIsInPending = random.NextDouble() > 0.5;
                if (addressIsInPending == false)
                {
                    events.Add(new DHCPv6LeaseActivatedEvent(leaseId));
                }

                expectedResults.Add(leaseId, addressIsInPending);
            }

            rootScope.Load(events);

            DHCPv6Scope scope = rootScope.GetRootScopes().First();
            foreach (var item in expectedResults)
            {
                DHCPv6Lease lease = scope.Leases.GetLeaseById(item.Key);
                Boolean actual = lease.IsPending();
                Assert.Equal(item.Value, actual);
            }
        }

        [Fact]
        public void IsActive()
        {
            Random random = new Random();
            DHCPv6RootScope rootScope = GetRootScope();

            Guid scopeId = Guid.NewGuid();

            List<DomainEvent> events = new List<DomainEvent>
            {
                 new DHCPv6ScopeAddedEvent(new DHCPv6ScopeCreateInstruction
                 {
                     Id = scopeId,
                 }),
            };

            Int32 leaseAmount = random.Next(10, 30);
            Dictionary<Guid, Boolean> expectedResults = new Dictionary<Guid, bool>();
            for (int i = 0; i < leaseAmount; i++)
            {
                Guid leaseId = Guid.NewGuid();

                events.Add(new DHCPv6LeaseCreatedEvent
                {
                    ScopeId = scopeId,
                    EntityId = leaseId,
                    Address = random.GetIPv6Address(),
                    ClientIdentifier = new UUIDDUID(random.NextGuid()),
                });

                Boolean addressIsActive = random.NextDouble() > 0.5;
                if (addressIsActive == true)
                {
                    events.Add(new DHCPv6LeaseActivatedEvent(leaseId));
                }

                expectedResults.Add(leaseId, addressIsActive);
            }

            rootScope.Load(events);

            DHCPv6Scope scope = rootScope.GetRootScopes().First();
            foreach (var item in expectedResults)
            {
                DHCPv6Lease lease = scope.Leases.GetLeaseById(item.Key);
                Boolean actual = lease.IsActive();
                Assert.Equal(item.Value, actual);
            }
        }

        public static Dictionary<Guid, Boolean> AddEventsForCancelableLeases(
            Random random,
            Guid scopeId,
            ICollection<DomainEvent> events)
        {
            Dictionary<LeaseStates, Func<Guid, DHCPv6ScopeRelatedEvent>> cancalableStateBuilder = new Dictionary<LeaseStates, Func<Guid, DHCPv6ScopeRelatedEvent>>
            {
                { LeaseStates.Pending, (id) => null  },
                { LeaseStates.Active, (id) => new DHCPv6LeaseActivatedEvent(id)  },
            };

            Dictionary<LeaseStates, Func<Guid, DHCPv6ScopeRelatedEvent>> nonCancalableStateBuilder = new Dictionary<LeaseStates, Func<Guid, DHCPv6ScopeRelatedEvent>>
            {
                //{ LeaseStates.Inactive, (id) => new DHCPv6LeaseExpiredEvent(id)  },
                //{ LeaseStates.Canceled, (id) => new DHCPv6LeaseCanceledEvent(id)  },
                { LeaseStates.Released, (id) => new DHCPv6LeaseReleasedEvent(id,false)  },
                { LeaseStates.Revoked, (id) => new DHCPv6LeaseRevokedEvent(id)  },
            };

            Int32 leaseAmount = random.Next(20, 40);
            Dictionary<Guid, Boolean> expectedCancallations = new Dictionary<Guid, bool>();
            for (int i = 0; i < leaseAmount; i++)
            {
                Guid leaseId = random.NextGuid();

                events.Add(new DHCPv6LeaseCreatedEvent
                {
                    ScopeId = scopeId,
                    EntityId = leaseId,
                    Address = random.GetIPv6Address(),
                    ClientIdentifier = new UUIDDUID(random.NextGuid()),
                });

                Boolean shouldBeCancelable = random.NextDouble() > 0.5;
                Dictionary<LeaseStates, Func<Guid, DHCPv6ScopeRelatedEvent>> eventCreatorDict = null;
                if (shouldBeCancelable == true)
                {
                    eventCreatorDict = cancalableStateBuilder;
                }
                else
                {
                    eventCreatorDict = nonCancalableStateBuilder;
                }

                var entry = eventCreatorDict.ElementAt(random.Next(0, eventCreatorDict.Count));
                DHCPv6ScopeRelatedEvent stateChangingEvent = entry.Value(leaseId);
                if (stateChangingEvent != null)
                {
                    stateChangingEvent.ScopeId = scopeId;
                    events.Add(stateChangingEvent);
                }

                expectedCancallations.Add(leaseId, shouldBeCancelable);
            }

            return expectedCancallations;
        }

        [Fact]
        public void IsCancelable()
        {
            Random random = new Random();
            DHCPv6RootScope rootScope = GetRootScope();

            Guid scopeId = Guid.NewGuid();

            List<DomainEvent> events = new List<DomainEvent>
            {
                 new DHCPv6ScopeAddedEvent(new DHCPv6ScopeCreateInstruction
                 {
                     Id = scopeId,
                 }),
            };

            Dictionary<Guid, Boolean> expectedResults =
                AddEventsForCancelableLeases(random, scopeId, events);

            rootScope.Load(events);

            DHCPv6Scope scope = rootScope.GetRootScopes().First();
            foreach (var item in expectedResults)
            {
                DHCPv6Lease lease = scope.Leases.GetLeaseById(item.Key);
                Boolean actual = lease.IsCancelable();
                Assert.Equal(item.Value, actual);
            }
        }

        [Fact]
        public void CanBeExtended()
        {
            Random random = new Random(1345);
            DHCPv6RootScope rootScope = GetRootScope();

            Guid scopeId = Guid.NewGuid();

            List<DomainEvent> events = new List<DomainEvent>
            {
                 new DHCPv6ScopeAddedEvent(new DHCPv6ScopeCreateInstruction
                 {
                     Id = scopeId,
                 }),
            };

            Int32 leaseAmount = random.Next(10, 30);
            Dictionary<Guid, Boolean> expectedResults = new Dictionary<Guid, bool>();
            for (int i = 0; i < leaseAmount; i++)
            {
                Guid leaseId = Guid.NewGuid();

                events.Add(new DHCPv6LeaseCreatedEvent
                {
                    ScopeId = scopeId,
                    EntityId = leaseId,
                    Address = random.GetIPv6Address(),
                    ClientIdentifier = new UUIDDUID(random.NextGuid()),
                });

                Boolean shouldBeExtentable = random.NextBoolean();
                DomainEvent @event = null;
                if (shouldBeExtentable == true)
                {
                    Int32 nextValue = random.Next(0, 4);
                    switch (nextValue)
                    {
                        case 0:
                            @event = new DHCPv6LeaseActivatedEvent(leaseId);
                            break;
                        case 1:
                            @event = new DHCPv6LeaseReleasedEvent(leaseId, false);
                            break;
                        case 2:
                            @event = new DHCPv6LeaseRenewedEvent(leaseId, DateTime.UtcNow.AddHours(3), false, false);
                            break;
                        case 3:
                            @event = new DHCPv6LeaseExpiredEvent(leaseId);
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    Int32 nextValue = random.Next(0, 3);
                    switch (nextValue)
                    {
                        case 0:
                            @event = new DHCPv6LeaseCanceledEvent(leaseId);
                            break;
                        case 1:
                            @event = new DHCPv6LeaseRevokedEvent(leaseId);
                            break;
                        case 2:
                            @event = new DHCPv6AddressSuspendedEvent(leaseId, random.GetIPv6Address(),DateTime.UtcNow.AddHours(3));
                            break;
                        default:
                            break;
                    }
                }

                events.Add(@event);

                expectedResults.Add(leaseId, shouldBeExtentable);
            }

            rootScope.Load(events);

            DHCPv6Scope scope = rootScope.GetRootScopes().First();
            foreach (var item in expectedResults)
            {
                DHCPv6Lease lease = scope.Leases.GetLeaseById(item.Key);
                Boolean actual = lease.CanBeExtended();
                Assert.Equal(item.Value, actual);
            }
        }
    }
}
