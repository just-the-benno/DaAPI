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
    public class DHCPv6LeasesTester
    {
        public DHCPv6RootScope GetRootScope()
        {
            Mock<ILoggerFactory> factoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);
            factoryMock.Setup(x => x.CreateLogger(It.IsAny<String>())).Returns(Mock.Of<ILogger<DHCPv6RootScope>>());

            var result = new DHCPv6RootScope(Guid.NewGuid(),
               Mock.Of<IScopeResolverManager<DHCPv6Packet, IPv6Address>>(), factoryMock.Object);

            return result;
        }


        [Fact]
        public void Contains()
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

            Int32 leaseAmount = random.Next(3, 10);
            HashSet<Guid> existingIds = new HashSet<Guid>();
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

                existingIds.Add(leaseId);
            }

            rootScope.Load(events);

            DHCPv6Scope scope = rootScope.GetRootScopes().First();
            foreach (Guid leaseId in existingIds)
            {
                Boolean actual = scope.Leases.Contains(leaseId);
                Assert.True(actual);
            }

            Int32 notExisitngAmount = random.Next(30, 100);
            for (int i = 0; i < notExisitngAmount; i++)
            {
                Guid id = Guid.NewGuid();
                if (existingIds.Contains(id) == true) { continue; }

                Boolean actual = scope.Leases.Contains(id);
                Assert.False(actual);
            }
        }

        [Fact]
        public void GetLeaseById()
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

            Int32 leaseAmount = random.Next(3, 10);
            HashSet<Guid> existingIds = new HashSet<Guid>();
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

                existingIds.Add(leaseId);
            }

            rootScope.Load(events);

            DHCPv6Scope scope = rootScope.GetRootScopes().First();
            foreach (Guid leaseId in existingIds)
            {
                DHCPv6Lease lease = scope.Leases.GetLeaseById(leaseId);
                Assert.True(lease != DHCPv6Lease.Empty);
            }

            Int32 notExisitngAmount = random.Next(30, 100);
            for (int i = 0; i < notExisitngAmount; i++)
            {
                Guid id = Guid.NewGuid();
                if (existingIds.Contains(id) == true) { continue; }

                DHCPv6Lease lease = scope.Leases.GetLeaseById(id);
                Assert.True(lease == DHCPv6Lease.Empty);
            }
        }

        [Fact]
        public void GetUsedAddresses()
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

            Int32 leaseAmount = random.Next(30, 60);
            List<IPv6Address> expectedUsedAddress = new List<IPv6Address>();
            for (int i = 0; i < leaseAmount; i++)
            {
                Guid leaseId = Guid.NewGuid();
                IPv6Address address = random.GetIPv6Address();

                events.Add(new DHCPv6LeaseCreatedEvent
                {
                    ScopeId = scopeId,
                    EntityId = leaseId,
                    Address = address,
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

                if (eventToAdd != null)
                {
                    events.Add(eventToAdd);
                }

                if (addressIsInUse == true)
                {
                    expectedUsedAddress.Add(address);
                }
            }

            rootScope.Load(events);

            DHCPv6Scope scope = rootScope.GetRootScopes().First();
            List<IPv6Address> actualAddresses = scope.Leases.GetUsedAddresses().ToList();

            Assert.Equal(expectedUsedAddress, actualAddresses);
        }

        [Fact]
        public void GetSuspenedAddresses()
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

            Int32 leaseAmount = random.Next(30, 60);
            List<IPv6Address> expectedUsedAddress = new List<IPv6Address>();
            for (int i = 0; i < leaseAmount; i++)
            {
                Guid leaseId = Guid.NewGuid();
                IPv6Address address = random.GetIPv6Address();

                events.Add(new DHCPv6LeaseCreatedEvent
                {
                    ScopeId = scopeId,
                    EntityId = leaseId,
                    Address = address,
                    ClientIdentifier = new UUIDDUID(random.NextGuid()),
                });

                Boolean shouldBeSuspended = random.NextDouble() > 0.5;
                if (shouldBeSuspended == true)
                {
                    events.Add(new DHCPv6AddressSuspendedEvent(
                        leaseId,
                        address, DateTime.UtcNow.AddHours(12)));

                    expectedUsedAddress.Add(address);
                }
            }

            rootScope.Load(events);

            DHCPv6Scope scope = rootScope.GetRootScopes().First();
            List<IPv6Address> actualAddresses = scope.Leases.GetSuspendedAddresses().ToList();

            Assert.Equal(expectedUsedAddress.OrderBy(x => x), actualAddresses.OrderBy(x => x));
        }

        [Fact]
        public void IsAddressSuspended()
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

            Int32 leaseAmount = random.Next(30, 60);
            Dictionary<IPv6Address, Boolean> expectedUsedAddress = new Dictionary<IPv6Address, bool>();

            for (int i = 0; i < leaseAmount; i++)
            {
                Guid leaseId = Guid.NewGuid();
                IPv6Address address = random.GetIPv6Address();

                events.Add(new DHCPv6LeaseCreatedEvent
                {
                    ScopeId = scopeId,
                    EntityId = leaseId,
                    Address = address,
                    ClientIdentifier = new UUIDDUID(random.NextGuid()),
                });

                Boolean shouldBeSuspended = random.NextDouble() > 0.5;
                if (shouldBeSuspended == true)
                {
                    events.Add(new DHCPv6AddressSuspendedEvent(
                        leaseId,
                        address, DateTime.UtcNow.AddHours(12)));
                }

                expectedUsedAddress.Add(address, shouldBeSuspended);
            }

            rootScope.Load(events);

            DHCPv6Scope scope = rootScope.GetRootScopes().First();

            foreach (var item in expectedUsedAddress)
            {
                Boolean actual = scope.Leases.IsAddressSuspended(item.Key);
                Assert.Equal(item.Value, actual);
            }

            Int32 notExistingAddressesAmount = random.Next(30, 50);
            for (int i = 0; i < notExistingAddressesAmount; i++)
            {
                IPv6Address notExisitingAddress = random.GetIPv6Address();
                if (expectedUsedAddress.ContainsKey(notExisitingAddress) == true) { continue; }

                Boolean actual = scope.Leases.IsAddressSuspended(notExisitingAddress);
                Assert.False(actual);
            }
        }

        [Fact]
        public void IsAddressActive()
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

            Int32 leaseAmount = random.Next(30, 60);
            Dictionary<IPv6Address, Boolean> expectedUsedAddress = new Dictionary<IPv6Address, bool>();

            for (int i = 0; i < leaseAmount; i++)
            {
                Guid leaseId = Guid.NewGuid();
                IPv6Address address = random.GetIPv6Address();

                events.Add(new DHCPv6LeaseCreatedEvent
                {
                    ScopeId = scopeId,
                    EntityId = leaseId,
                    Address = address,
                    ClientIdentifier = new UUIDDUID(random.NextGuid()),
                });

                Boolean addressIsInUse = random.NextDouble() > 0.5;
                if (addressIsInUse == true)
                {
                    events.Add(new DHCPv6LeaseActivatedEvent(leaseId));
                }

                expectedUsedAddress.Add(address, addressIsInUse);
            }

            rootScope.Load(events);

            DHCPv6Scope scope = rootScope.GetRootScopes().First();

            foreach (var item in expectedUsedAddress)
            {
                Boolean actual = scope.Leases.IsAddressActive(item.Key);
                Assert.Equal(item.Value, actual);
            }

            Int32 notExistingAddressesAmount = random.Next(30, 50);
            for (int i = 0; i < notExistingAddressesAmount; i++)
            {
                IPv6Address notExisitingAddress = random.GetIPv6Address();
                if (expectedUsedAddress.ContainsKey(notExisitingAddress) == true) { continue; }

                Boolean actual = scope.Leases.IsAddressActive(notExisitingAddress);
                Assert.False(actual);
            }
        }

        [Fact]
        public void LimitLeasesPerClient()
        {
            Random random = new Random();
            DHCPv6RootScope rootScope = GetRootScope();

            Guid scopeId = Guid.NewGuid();
            DUID clientIdentifier = new UUIDDUID(random.NextGuid());

            List<DomainEvent> events = new List<DomainEvent>
            {
                 new DHCPv6ScopeAddedEvent(new DHCPv6ScopeCreateInstruction
                 {
                     Id = scopeId,
                 }),
            };

            Int32 leaseAmount = random.Next(20, 40);
            for (int i = 0; i < leaseAmount; i++)
            {
                Guid leaseId = Guid.NewGuid();

                events.Add(new DHCPv6LeaseCreatedEvent
                {
                    ScopeId = scopeId,
                    EntityId = leaseId,
                    Address = random.GetIPv6Address(),
                    ClientIdentifier = clientIdentifier
                });
                events.Add(new DHCPv6LeaseActivatedEvent
                {
                    ScopeId = scopeId,
                    EntityId = leaseId,
                });
                events.Add(new DHCPv6LeaseRevokedEvent
                {
                    ScopeId = scopeId,
                    EntityId = leaseId,
                });
            }

            rootScope.Load(events);

            DHCPv6Scope scope = rootScope.GetRootScopes().First();
            var leases = scope.Leases.GetAllLeases();

            Assert.Equal(5 + 1, leases.Count());
        }

    }
}
