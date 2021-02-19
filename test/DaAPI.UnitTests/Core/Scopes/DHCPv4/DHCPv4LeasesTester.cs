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
using DaAPI.Core.Packets.DHCPv4;
using DaAPI.Core.Scopes;
using Microsoft.Extensions.Logging;

namespace DaAPI.UnitTests.Core.Scopes.DHCPv4
{
    public class DHCPv4LeasesTester
    {
        public DHCPv4RootScope GetRootScope() =>
            new DHCPv4RootScope(Guid.NewGuid(), Mock.Of<IScopeResolverManager<DHCPv4Packet, IPv4Address>>(), Mock.Of<ILoggerFactory>());

        [Fact]
        public void DHCPv4Leases_Contains()
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

            Int32 leaseAmount = random.Next(3, 10);
            HashSet<Guid> existingIds = new HashSet<Guid>();
            for (int i = 0; i < leaseAmount; i++)
            {
                Guid leaseId = Guid.NewGuid();

                events.Add(new DHCPv4LeaseCreatedEvent
                {
                    ScopeId = scopeId,
                    EntityId = leaseId,
                    Address = random.GetIPv4Address(),
                    HardwareAddress = random.NextBytes(6),
                });

                existingIds.Add(leaseId);
            }

            rootScope.Load(events);

            DHCPv4Scope scope = rootScope.GetRootScopes().First();
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
        public void DHCPv4Leases_GetLeaseById()
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

            Int32 leaseAmount = random.Next(3, 10);
            HashSet<Guid> existingIds = new HashSet<Guid>();
            for (int i = 0; i < leaseAmount; i++)
            {
                Guid leaseId = Guid.NewGuid();

                events.Add(new DHCPv4LeaseCreatedEvent
                {
                    ScopeId = scopeId,
                    EntityId = leaseId,
                    Address = random.GetIPv4Address(),
                    HardwareAddress = random.NextBytes(6),
                });

                existingIds.Add(leaseId);
            }

            rootScope.Load(events);

            DHCPv4Scope scope = rootScope.GetRootScopes().First();
            foreach (Guid leaseId in existingIds)
            {
                DHCPv4Lease lease = scope.Leases.GetLeaseById(leaseId);
                Assert.True(lease != DHCPv4Lease.Empty);
            }

            Int32 notExisitngAmount = random.Next(30, 100);
            for (int i = 0; i < notExisitngAmount; i++)
            {
                Guid id = Guid.NewGuid();
                if (existingIds.Contains(id) == true) { continue; }

                DHCPv4Lease lease = scope.Leases.GetLeaseById(id);
                Assert.True(lease == DHCPv4Lease.Empty);
            }
        }

        [Fact]
        public void DHCPv4Leases_GetUsedAddresses()
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

            Int32 leaseAmount = random.Next(30, 60);
            List<IPv4Address> expectedUsedAddress = new List<IPv4Address>();
            for (int i = 0; i < leaseAmount; i++)
            {
                Guid leaseId = Guid.NewGuid();
                IPv4Address address = random.GetIPv4Address();

                events.Add(new DHCPv4LeaseCreatedEvent
                {
                    ScopeId = scopeId,
                    EntityId = leaseId,
                    Address = address,
                    HardwareAddress = random.NextBytes(6),
                });

                DomainEvent eventToAdd = null;
                Boolean addressIsInUse = true;
                Double randomValue = random.NextDouble();
                Double possiblities = 5.0;
                if (randomValue < 1 / possiblities)
                {
                    eventToAdd = new DHCPv4LeaseReleasedEvent(leaseId);
                    addressIsInUse = false;
                }
                else if (randomValue < 2 / possiblities)
                {
                    eventToAdd = new DHCPv4LeaseRevokedEvent(leaseId);
                    addressIsInUse = false;

                }
                else if (randomValue < 3 / possiblities)
                {
                    eventToAdd = new DHCPv4AddressSuspendedEvent(leaseId, random.GetIPv4Address(), DateTime.UtcNow.AddHours(12));
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

            DHCPv4Scope scope = rootScope.GetRootScopes().First();
            List<IPv4Address> actualAddresses = scope.Leases.GetUsedAddresses().ToList();

            Assert.Equal(expectedUsedAddress, actualAddresses);
        }

        [Fact]
        public void DHCPv4Leases_GetSuspenedAddresses()
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

            Int32 leaseAmount = random.Next(30, 60);
            List<IPv4Address> expectedUsedAddress = new List<IPv4Address>();
            for (int i = 0; i < leaseAmount; i++)
            {
                Guid leaseId = Guid.NewGuid();
                IPv4Address address = random.GetIPv4Address();

                events.Add(new DHCPv4LeaseCreatedEvent
                {
                    ScopeId = scopeId,
                    EntityId = leaseId,
                    Address = address,
                    HardwareAddress = random.NextBytes(6),
                });

                Boolean shouldBeSuspended = random.NextDouble() > 0.5;
                if (shouldBeSuspended == true)
                {
                    events.Add(new DHCPv4AddressSuspendedEvent(
                        leaseId,
                        address, DateTime.UtcNow.AddHours(12)));

                    expectedUsedAddress.Add(address);
                }
            }

            rootScope.Load(events);

            DHCPv4Scope scope = rootScope.GetRootScopes().First();
            List<IPv4Address> actualAddresses = scope.Leases.GetSuspendedAddresses().ToList();

            Assert.Equal(expectedUsedAddress.OrderBy(x => x), actualAddresses.OrderBy(x => x));
        }

        [Fact]
        public void DHCPv4Leases_IsAddressSuspended()
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

            Int32 leaseAmount = random.Next(30, 60);
            Dictionary<IPv4Address, Boolean> expectedUsedAddress = new Dictionary<IPv4Address, bool>();

            for (int i = 0; i < leaseAmount; i++)
            {
                Guid leaseId = Guid.NewGuid();
                IPv4Address address = random.GetIPv4Address();

                events.Add(new DHCPv4LeaseCreatedEvent
                {
                    ScopeId = scopeId,
                    EntityId = leaseId,
                    Address = address,
                    HardwareAddress = random.NextBytes(6),
                });

                Boolean shouldBeSuspended = random.NextDouble() > 0.5;
                if (shouldBeSuspended == true)
                {
                    events.Add(new DHCPv4AddressSuspendedEvent(
                        leaseId,
                        address, DateTime.UtcNow.AddHours(12)));
                }

                expectedUsedAddress.Add(address, shouldBeSuspended);
            }

            rootScope.Load(events);

            DHCPv4Scope scope = rootScope.GetRootScopes().First();

            foreach (var item in expectedUsedAddress)
            {
                Boolean actual = scope.Leases.IsAddressSuspended(item.Key);
                Assert.Equal(item.Value, actual);
            }

            Int32 notExistingAddressesAmount = random.Next(30, 50);
            for (int i = 0; i < notExistingAddressesAmount; i++)
            {
                IPv4Address notExisitingAddress = random.GetIPv4Address();
                if (expectedUsedAddress.ContainsKey(notExisitingAddress) == true) { continue; }

                Boolean actual = scope.Leases.IsAddressSuspended(notExisitingAddress);
                Assert.False(actual);
            }
        }

        [Fact]
        public void DHCPv4Leases_IsAddressActive()
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

            Int32 leaseAmount = random.Next(30, 60);
            Dictionary<IPv4Address, Boolean> expectedUsedAddress = new Dictionary<IPv4Address, bool>();

            for (int i = 0; i < leaseAmount; i++)
            {
                Guid leaseId = Guid.NewGuid();
                IPv4Address address = random.GetIPv4Address();

                events.Add(new DHCPv4LeaseCreatedEvent
                {
                    ScopeId = scopeId,
                    EntityId = leaseId,
                    Address = address,
                    HardwareAddress = random.NextBytes(6),
                });

                Boolean addressIsInUse = random.NextDouble() > 0.5;
                if (addressIsInUse == true)
                {
                    events.Add(new DHCPv4LeaseActivatedEvent(leaseId));
                }

                expectedUsedAddress.Add(address, addressIsInUse);
            }

            rootScope.Load(events);

            DHCPv4Scope scope = rootScope.GetRootScopes().First();

            foreach (var item in expectedUsedAddress)
            {
                Boolean actual = scope.Leases.IsAddressActive(item.Key);
                Assert.Equal(item.Value, actual);
            }

            Int32 notExistingAddressesAmount = random.Next(30, 50);
            for (int i = 0; i < notExistingAddressesAmount; i++)
            {
                IPv4Address notExisitingAddress = random.GetIPv4Address();
                if (expectedUsedAddress.ContainsKey(notExisitingAddress) == true) { continue; }

                Boolean actual = scope.Leases.IsAddressActive(notExisitingAddress);
                Assert.False(actual);
            }
        }

    }
}
