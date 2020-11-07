using DaAPI.Core.Common;
using DaAPI.Core.Scopes.DHCPv4;
using DaAPI.Core.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using static DaAPI.Core.Scopes.DHCPv4.DHCPv4ScopeEvents;
using static DaAPI.Core.Scopes.DHCPv4.DHCPv4LeaseEvents;
using DaAPI.TestHelper;
using System.Linq;
using static DaAPI.Core.Scopes.DHCPv4.DHCPv4Lease;

namespace DaAPI.UnitTests.Core.Scopes.DHCPv4
{
    public class DHCPv4LeaseTester
    {
        public DHCPv4RootScope GetRootScope() =>
            new DHCPv4RootScope(Guid.NewGuid(), Mock.Of<IDHCPv4ScopeResolverManager>());

        [Fact]
        public void DHCPv4Lease_MatchesUniqueIdentiifer()
        {
            Random random = new Random();
            DHCPv4RootScope rootScope = GetRootScope();

            Guid scopeId = Guid.NewGuid();
            Byte[] uniqueIdentifier = random.NextBytes(10);

            List<DomainEvent> events = new List<DomainEvent>
            {
                 new DHCPv4ScopeAddedEvent(new DHCPv4ScopeCreateInstruction
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

                events.Add(new DHCPv4LeaseCreatedEvent
                {
                    ScopeId = scopeId,
                    EntityId = leaseId,
                    UniqueIdentiifer = identifier,
                    Address = random.GetIPv4Address(),
                });

                expectedResults.Add(leaseId, matches);
            }

            rootScope.Load(events);

            DHCPv4Scope scope = rootScope.GetRootScopes().First();
            foreach (var item in expectedResults)
            {
                DHCPv4Lease lease = scope.Leases.GetLeaseById(item.Key);
                Boolean actual = lease.MatchesUniqueIdentiifer(uniqueIdentifier);
                Assert.Equal(item.Value, actual);
            }
        }

        [Fact]
        public void DHCPv4Lease_AddressesAreInUse()
        {
            Random random = new Random();
            DHCPv4RootScope rootScope = GetRootScope();

            Guid scopeId = Guid.NewGuid();

            List<DomainEvent> events = new List<DomainEvent>
            {
                 new DHCPv4ScopeAddedEvent(new DHCPv4ScopeCreateInstruction
                 {
                     Id = scopeId,
                 }),
            };

            Int32 leaseAmount = random.Next(10, 30);
            Dictionary<Guid, Boolean> expectedResults = new Dictionary<Guid, bool>();
            for (int i = 0; i < leaseAmount; i++)
            {
                Guid leaseId = Guid.NewGuid();

                events.Add(new DHCPv4LeaseCreatedEvent
                {
                    ScopeId = scopeId,
                    EntityId = leaseId,
                    Address = random.GetIPv4Address(),
                });

                DomainEvent eventToAdd = null;
                Boolean addressIsInUse = true;
                Double randomValue = random.NextDouble();
                Double possiblities = 5.0;
                if(randomValue < 1 / possiblities)
                {
                    eventToAdd = new DHCPv4LeaseReleasedEvent(leaseId);
                    addressIsInUse = false;
                }
                else if(randomValue < 2 / possiblities)
                {
                    eventToAdd = new DHCPv4LeaseRevokedEvent(leaseId);
                    addressIsInUse = false;

                }
                else if(randomValue < 3 / possiblities)
                {
                    eventToAdd = new DHCPv4AddressSuspendedEvent(leaseId, random.GetIPv4Address(), DateTime.UtcNow.AddHours(12));
                    addressIsInUse = false;
                }

                if(eventToAdd != null)
                {
                    events.Add(eventToAdd);
                }

                expectedResults.Add(leaseId, addressIsInUse);
            }

            rootScope.Load(events);

            DHCPv4Scope scope = rootScope.GetRootScopes().First();
            foreach (var item in expectedResults)
            {
                DHCPv4Lease lease = scope.Leases.GetLeaseById(item.Key);
                Boolean actual = lease.AddressIsInUse();
                Assert.Equal(item.Value, actual);
            }
        }

        [Fact]
        public void DHCPv4Lease_IsPending()
        {
            Random random = new Random();
            DHCPv4RootScope rootScope = GetRootScope();

            Guid scopeId = Guid.NewGuid();

            List<DomainEvent> events = new List<DomainEvent>
            {
                 new DHCPv4ScopeAddedEvent(new DHCPv4ScopeCreateInstruction
                 {
                     Id = scopeId,
                 }),
            };

            Int32 leaseAmount = random.Next(10, 30);
            Dictionary<Guid, Boolean> expectedResults = new Dictionary<Guid, bool>();
            for (int i = 0; i < leaseAmount; i++)
            {
                Guid leaseId = Guid.NewGuid();

                events.Add(new DHCPv4LeaseCreatedEvent
                {
                    ScopeId = scopeId,
                    EntityId = leaseId,
                    Address = random.GetIPv4Address(),
                });

                Boolean addressIsInPending = random.NextDouble() > 0.5;
                if(addressIsInPending == false)
                {
                    events.Add(new DHCPv4LeaseActivatedEvent(leaseId));
                }

                expectedResults.Add(leaseId, addressIsInPending);
            }

            rootScope.Load(events);

            DHCPv4Scope scope = rootScope.GetRootScopes().First();
            foreach (var item in expectedResults)
            {
                DHCPv4Lease lease = scope.Leases.GetLeaseById(item.Key);
                Boolean actual = lease.IsPending();
                Assert.Equal(item.Value, actual);
            }
        }

        [Fact]
        public void DHCPv4Lease_IsActive()
        {
            Random random = new Random();
            DHCPv4RootScope rootScope = GetRootScope();

            Guid scopeId = Guid.NewGuid();

            List<DomainEvent> events = new List<DomainEvent>
            {
                 new DHCPv4ScopeAddedEvent(new DHCPv4ScopeCreateInstruction
                 {
                     Id = scopeId,
                 }),
            };

            Int32 leaseAmount = random.Next(10, 30);
            Dictionary<Guid, Boolean> expectedResults = new Dictionary<Guid, bool>();
            for (int i = 0; i < leaseAmount; i++)
            {
                Guid leaseId = Guid.NewGuid();

                events.Add(new DHCPv4LeaseCreatedEvent
                {
                    ScopeId = scopeId,
                    EntityId = leaseId,
                    Address = random.GetIPv4Address(),
                });

                Boolean addressIsActive = random.NextDouble() > 0.5;
                if (addressIsActive == true)
                {
                    events.Add(new DHCPv4LeaseActivatedEvent(leaseId));
                }

                expectedResults.Add(leaseId, addressIsActive);
            }

            rootScope.Load(events);

            DHCPv4Scope scope = rootScope.GetRootScopes().First();
            foreach (var item in expectedResults)
            {
                DHCPv4Lease lease = scope.Leases.GetLeaseById(item.Key);
                Boolean actual = lease.IsActive();
                Assert.Equal(item.Value, actual);
            }
        }

        public static Dictionary<Guid, Boolean> AddEventsForCancelableLeases(
            Random random,
            Guid scopeId,
            ICollection<DomainEvent> events)
        {
            Dictionary<DHCPv4LeaseStates, Func<Guid, DHCPv4ScopeRelatedEvent>> cancalableStateBuilder = new Dictionary<DHCPv4LeaseStates, Func<Guid, DHCPv4ScopeRelatedEvent>>
            {
                { DHCPv4LeaseStates.Pending, (id) => null  },
                { DHCPv4LeaseStates.Active, (id) => new DHCPv4LeaseActivatedEvent(id)  },
            };

            Dictionary<DHCPv4LeaseStates, Func<Guid, DHCPv4ScopeRelatedEvent>> nonCancalableStateBuilder = new Dictionary<DHCPv4LeaseStates, Func<Guid, DHCPv4ScopeRelatedEvent>>
            {
                { DHCPv4LeaseStates.Inactive, (id) => new DHCPv4LeaseExpiredEvent(id)  },
                { DHCPv4LeaseStates.Canceled, (id) => new DHCPv4LeaseCanceledEvent(id)  },
                { DHCPv4LeaseStates.Released, (id) => new DHCPv4LeaseReleasedEvent(id)  },
                { DHCPv4LeaseStates.Revoked, (id) => new DHCPv4LeaseRevokedEvent(id)  },
            };

            Int32 leaseAmount = random.Next(20, 40);
            Dictionary<Guid, Boolean> expectedCancallations = new Dictionary<Guid, bool>();
            for (int i = 0; i < leaseAmount; i++)
            {
                Guid leaseId = Guid.NewGuid();

                events.Add(new DHCPv4LeaseCreatedEvent
                {
                    ScopeId = scopeId,
                    EntityId = leaseId,
                    Address = random.GetIPv4Address(),
                });

                Boolean shouldBeCancelable = random.NextDouble() > 0.5;
                Dictionary<DHCPv4LeaseStates, Func<Guid, DHCPv4ScopeRelatedEvent>> eventCreatorDict = null;
                if (shouldBeCancelable == true)
                {
                    eventCreatorDict = cancalableStateBuilder;
                }
                else
                {
                    eventCreatorDict = nonCancalableStateBuilder;
                }

                var entry = eventCreatorDict.ElementAt(random.Next(0, eventCreatorDict.Count));
                DHCPv4ScopeRelatedEvent stateChangingEvent = entry.Value(leaseId);
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
        public void DHCPv4Lease_IsCancelable()
        {
            Random random = new Random();
            DHCPv4RootScope rootScope = GetRootScope();

            Guid scopeId = Guid.NewGuid();

            List<DomainEvent> events = new List<DomainEvent>
            {
                 new DHCPv4ScopeAddedEvent(new DHCPv4ScopeCreateInstruction
                 {
                     Id = scopeId,
                 }),
            };

            Dictionary<Guid, Boolean> expectedResults = 
                AddEventsForCancelableLeases(random,scopeId,events);

            rootScope.Load(events);

            DHCPv4Scope scope = rootScope.GetRootScopes().First();
            foreach (var item in expectedResults)
            {
                DHCPv4Lease lease = scope.Leases.GetLeaseById(item.Key);
                Boolean actual = lease.IsCancelable();
                Assert.Equal(item.Value, actual);
            }
        }
    }
}
