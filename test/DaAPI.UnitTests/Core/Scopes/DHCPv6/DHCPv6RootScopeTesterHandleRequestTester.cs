using DaAPI.Core.Common;
using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Packets.DHCPv6;
using DaAPI.Core.Scopes;
using DaAPI.Core.Scopes.DHCPv6;
using DaAPI.TestHelper;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using static DaAPI.Core.Scopes.DHCPv6.DHCPv6LeaseEvents;
using static DaAPI.Core.Scopes.DHCPv6.DHCPv6PacketHandledEvents;

namespace DaAPI.UnitTests.Core.Scopes.DHCPv6
{
    public class DHCPv6RootScopeTesterHandleRequestTester : DHCPv6RootSoopeTesterBase
    {
        private void CheckHandeledEvent(Int32 index, DHCPv6Packet requestPacket, DHCPv6Packet result, DHCPv6RootScope rootScope, Guid? scopeId, DHCPv6RequestHandledEvent.RequestErrors error)
        {
            IEnumerable<DomainEvent> changes = rootScope.GetChanges();

            Assert.IsAssignableFrom<DHCPv6RequestHandledEvent>(changes.ElementAt(index));

            DHCPv6RequestHandledEvent handeledEvent = (DHCPv6RequestHandledEvent)changes.ElementAt(index);
            Assert.Equal(requestPacket, handeledEvent.Request);
            Assert.Equal(result, handeledEvent.Response);
            Assert.Equal(scopeId, handeledEvent.ScopeId);
            Assert.Equal(error == DHCPv6RequestHandledEvent.RequestErrors.NoError, handeledEvent.WasSuccessfullHandled);
            Assert.Equal(error, handeledEvent.Error);
        }

        private DHCPv6Packet GetRequestPacket(
          Random random, out IPv6Address usedAddress, out DUID clientDuid, out UInt32 iaId, Boolean withIdentity, params DHCPv6PacketOption[] options)
        {
            IPv6HeaderInformation headerInformation =
               new IPv6HeaderInformation(IPv6Address.FromString("fe80::1"), IPv6Address.FromString("fe80::2"));

            clientDuid = new UUIDDUID(random.NextGuid());
            iaId = random.NextBoolean() == false ? random.NextUInt32() : 0;
            usedAddress = random.GetIPv6Address();

            var packetOptions = new List<DHCPv6PacketOption>(options)
                {
                    new DHCPv6PacketIdentifierOption(DHCPv6PacketOptionTypes.ClientIdentifier,clientDuid),
                };

            if (withIdentity == true)
            {
                packetOptions.Add(new DHCPv6PacketIdentityAssociationNonTemporaryAddressesOption(iaId, TimeSpan.FromSeconds(random.Next()), TimeSpan.FromSeconds(random.Next()), new DHCPv6PacketSuboption[]
                    {
                     new DHCPv6PacketIdentityAssociationAddressSuboption(usedAddress,TimeSpan.FromSeconds(random.Next()),TimeSpan.FromSeconds(random.Next()),Array.Empty<DHCPv6PacketSuboption>())
                    }));
            }

            DHCPv6Packet packet = DHCPv6Packet.AsOuter(headerInformation, random.NextUInt16(),
                DHCPv6PacketTypes.REQUEST, packetOptions);

            return packet;
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void HandleRequest_LeaseFound_LeaseIsPending(Boolean withPrefixDelegation)
        {
            Random random = new Random();

            UInt32 prefixId = random.NextUInt32();
            Byte prefixLength = (Byte)random.Next(20, 60);
            IPv6Address prefixNetworkAddress = IPv6Address.FromString("fd01::0");

            DHCPv6PacketOption[] options = Array.Empty<DHCPv6PacketOption>();
            if (withPrefixDelegation == true)
            {
                options = new DHCPv6PacketOption[] {
                new DHCPv6PacketIdentityAssociationPrefixDelegationOption(prefixId,TimeSpan.FromSeconds(random.Next()),TimeSpan.FromSeconds(random.Next()),Array.Empty<DHCPv6PacketSuboption>())
                };
            }

            var packet = GetRequestPacket(random, out IPv6Address _, out DUID clientDuid, out UInt32 iaId, true, options);
            var resolverInformations = GetMockupResolver(packet, out Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>> scopeResolverMock);

            Guid scopeId = random.NextGuid();
            Guid leaseId = random.NextGuid();
            IPv6Address leasedAddress = IPv6Address.FromString("fe80::2");

            DHCPv6RootScope rootScope = GetRootScope(scopeResolverMock);
            rootScope.Load(new List<DomainEvent>{ new DHCPv6ScopeEvents.DHCPv6ScopeAddedEvent(
                new DHCPv6ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv6ScopeAddressProperties(
                        IPv6Address.FromString("fe80::0"),
                        IPv6Address.FromString("fe80::ff"),
                        new List<IPv6Address>{IPv6Address.FromString("fe80::1") },
                        t1: DHCPv6TimeScale.FromDouble(0.5),
                        t2: DHCPv6TimeScale.FromDouble(0.75),
                        preferredLifeTime: TimeSpan.FromDays(0.5),
                        validLifeTime: TimeSpan.FromDays(1),
                        addressAllocationStrategy: DHCPv6ScopeAddressProperties.AddressAllocationStrategies.Next),
                    ResolverInformation = resolverInformations,
                    Name = "Testscope",
                    Id = scopeId,
                }),
                new DHCPv6LeaseCreatedEvent
                {
                    EntityId = leaseId,
                    Address = leasedAddress,
                    ClientIdentifier = clientDuid,
                    IdentityAssocationId = iaId,
                    ScopeId = scopeId,
                    StartedAt = DateTime.UtcNow,
                    ValidUntil = DateTime.UtcNow.AddDays(1),
                    HasPrefixDelegation = withPrefixDelegation,
                    PrefixLength = prefixLength,
                    IdentityAssocationIdForPrefix = prefixId,
                    DelegatedNetworkAddress = prefixNetworkAddress,
                }
            });

            DHCPv6Packet result = rootScope.HandleRequest(packet, GetServerPropertiesResolver());
            CheckPacket(packet, leasedAddress, iaId, result, DHCPv6PacketTypes.REPLY, withPrefixDelegation == false ? DHCPv6PrefixDelegation.None : DHCPv6PrefixDelegation.FromValues(prefixNetworkAddress, new IPv6SubnetMask(new IPv6SubnetMaskIdentifier(prefixLength)), prefixId));

            CheckEventAmount(2, rootScope);
            CheckLeaseActivatedEvent(0, scopeId, leaseId, rootScope);
            CheckHandeledEvent(1, packet, result, rootScope, scopeId, DHCPv6RequestHandledEvent.RequestErrors.NoError);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void HandleRequest_LeaseFound_LeaseIsActive(Boolean withPrefixDelegation)
        {
            Random random = new Random();

            UInt32 prefixId = random.NextUInt32();
            Byte prefixLength = (Byte)random.Next(20, 60);
            IPv6Address prefixNetworkAddress = IPv6Address.FromString("fd01::0");

            DHCPv6PacketOption[] options = Array.Empty<DHCPv6PacketOption>();
            if (withPrefixDelegation == true)
            {
                options = new DHCPv6PacketOption[] {
                new DHCPv6PacketIdentityAssociationPrefixDelegationOption(prefixId,TimeSpan.FromSeconds(random.Next()),TimeSpan.FromSeconds(random.Next()),Array.Empty<DHCPv6PacketSuboption>())
                };
            }

            var packet = GetRequestPacket(random, out IPv6Address _, out DUID clientDuid, out UInt32 iaId, true, options);
            var resolverInformations = GetMockupResolver(packet, out Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>> scopeResolverMock);

            Guid scopeId = random.NextGuid();
            Guid leaseId = random.NextGuid();
            IPv6Address leasedAddress = IPv6Address.FromString("fe80::2");

            DHCPv6RootScope rootScope = GetRootScope(scopeResolverMock);
            rootScope.Load(new List<DomainEvent>{ new DHCPv6ScopeEvents.DHCPv6ScopeAddedEvent(
                new DHCPv6ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv6ScopeAddressProperties(
                        IPv6Address.FromString("fe80::0"),
                        IPv6Address.FromString("fe80::ff"),
                        new List<IPv6Address>{IPv6Address.FromString("fe80::1") },
                        t1: DHCPv6TimeScale.FromDouble(0.5),
                        t2: DHCPv6TimeScale.FromDouble(0.75),
                        preferredLifeTime: TimeSpan.FromDays(0.5),
                        validLifeTime: TimeSpan.FromDays(1),
                        addressAllocationStrategy: DHCPv6ScopeAddressProperties.AddressAllocationStrategies.Next),
                    ResolverInformation = resolverInformations,
                    Name = "Testscope",
                    Id = scopeId,
                }),
                new DHCPv6LeaseCreatedEvent
                {
                    EntityId = leaseId,
                    Address = leasedAddress,
                    ClientIdentifier = clientDuid,
                    IdentityAssocationId = iaId,
                    ScopeId = scopeId,
                    StartedAt = DateTime.UtcNow,
                    ValidUntil = DateTime.UtcNow.AddDays(1),
                    HasPrefixDelegation = withPrefixDelegation,
                    PrefixLength = prefixLength,
                    IdentityAssocationIdForPrefix = prefixId,
                    DelegatedNetworkAddress = prefixNetworkAddress,
                },
                new DHCPv6LeaseActivatedEvent
                {
                    EntityId = leaseId,
                    ScopeId = scopeId,
                },

            });

            DHCPv6Packet result = rootScope.HandleRequest(packet, GetServerPropertiesResolver());
            CheckPacket(packet, leasedAddress, iaId, result, DHCPv6PacketTypes.REPLY, withPrefixDelegation == false ? DHCPv6PrefixDelegation.None : DHCPv6PrefixDelegation.FromValues(prefixNetworkAddress, new IPv6SubnetMask(new IPv6SubnetMaskIdentifier(prefixLength)), prefixId));

            CheckEventAmount(1, rootScope);
            CheckHandeledEvent(0, packet, result, rootScope, scopeId, DHCPv6RequestHandledEvent.RequestErrors.NoError);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void HandleRequest_LeaseFound_LeaseIsInInvalidState(Boolean withPrefixDelegation)
        {
            Random random = new Random();

            UInt32 prefixId = random.NextUInt32();
            Byte prefixLength = (Byte)random.Next(20, 60);
            IPv6Address prefixNetworkAddress = IPv6Address.FromString("fd01::0");

            DHCPv6PacketOption[] options = Array.Empty<DHCPv6PacketOption>();
            if (withPrefixDelegation == true)
            {
                options = new DHCPv6PacketOption[] {
                new DHCPv6PacketIdentityAssociationPrefixDelegationOption(prefixId,TimeSpan.FromSeconds(random.Next()),TimeSpan.FromSeconds(random.Next()),new DHCPv6PacketSuboption[]{
                new DHCPv6PacketIdentityAssociationPrefixDelegationSuboption(TimeSpan.FromSeconds(random.Next()),TimeSpan.FromSeconds(random.Next()),prefixLength,prefixNetworkAddress,Array.Empty<DHCPv6PacketSuboption>())
                })
                };
            }

            var packet = GetRequestPacket(random, out IPv6Address leasedAddress, out DUID clientDuid, out UInt32 iaId, true, options);
            var resolverInformations = GetMockupResolver(packet, out Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>> scopeResolverMock);

            Guid scopeId = random.NextGuid();
            Guid leaseId = random.NextGuid();

            DHCPv6RootScope rootScope = GetRootScope(scopeResolverMock);
            rootScope.Load(new List<DomainEvent>{ new DHCPv6ScopeEvents.DHCPv6ScopeAddedEvent(
                new DHCPv6ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv6ScopeAddressProperties(
                        IPv6Address.FromString("fe80::0"),
                        IPv6Address.FromString("fe80::ff"),
                        new List<IPv6Address>{IPv6Address.FromString("fe80::1") },
                        preferredLifeTime: TimeSpan.FromDays(0.5),
                        validLifeTime: TimeSpan.FromDays(1),
                        addressAllocationStrategy: DHCPv6ScopeAddressProperties.AddressAllocationStrategies.Next),
                    ResolverInformation = resolverInformations,
                    Name = "Testscope",
                    Id = scopeId,
                }),
                new DHCPv6LeaseCreatedEvent
                {
                    EntityId = leaseId,
                    Address = leasedAddress,
                    ClientIdentifier = clientDuid,
                    IdentityAssocationId = iaId,
                    ScopeId = scopeId,
                    StartedAt = DateTime.UtcNow,
                    ValidUntil = DateTime.UtcNow.AddDays(1),
                    HasPrefixDelegation = withPrefixDelegation,
                    PrefixLength = prefixLength,
                    IdentityAssocationIdForPrefix = prefixId,
                    DelegatedNetworkAddress = prefixNetworkAddress,
                },
                new DHCPv6LeaseActivatedEvent
                {
                    EntityId = leaseId,
                    ScopeId = scopeId,
                },
                new DHCPv6LeaseExpiredEvent
                {
                    EntityId = leaseId,
                    ScopeId = scopeId,
                }

            });

            DHCPv6Packet result = rootScope.HandleRequest(packet, GetServerPropertiesResolver());
            CheckErrorPacket(packet, leasedAddress, iaId, result, withPrefixDelegation == false ? DHCPv6PrefixDelegation.None : DHCPv6PrefixDelegation.FromValues(prefixNetworkAddress, new IPv6SubnetMask(new IPv6SubnetMaskIdentifier(prefixLength)), prefixId), DHCPv6StatusCodes.NoAddrsAvail);

            CheckEventAmount(1, rootScope);
            CheckHandeledEvent(0, packet, result, rootScope, scopeId, DHCPv6RequestHandledEvent.RequestErrors.LeaseNotInCorrectState);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void HandleRequest_LeaseNotFound(Boolean withPrefixDelegation)
        {
            Random random = new Random();

            UInt32 prefixId = random.NextUInt32();
            Byte prefixLength = (Byte)random.Next(20, 60);
            IPv6Address prefixNetworkAddress = IPv6Address.FromString("fd01::0");

            DHCPv6PacketOption[] options = Array.Empty<DHCPv6PacketOption>();
            if (withPrefixDelegation == true)
            {
                options = new DHCPv6PacketOption[] {
                new DHCPv6PacketIdentityAssociationPrefixDelegationOption(prefixId,TimeSpan.FromSeconds(random.Next()),TimeSpan.FromSeconds(random.Next()),new DHCPv6PacketSuboption[]{
                new DHCPv6PacketIdentityAssociationPrefixDelegationSuboption(TimeSpan.FromSeconds(random.Next()),TimeSpan.FromSeconds(random.Next()),prefixLength,prefixNetworkAddress,Array.Empty<DHCPv6PacketSuboption>())
                })
                };
            }

            var packet = GetRequestPacket(random, out IPv6Address leasedAddress, out DUID _, out UInt32 iaId, true, options);
            var resolverInformations = GetMockupResolver(packet, out Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>> scopeResolverMock);

            Guid scopeId = random.NextGuid();
            Guid leaseId = random.NextGuid();

            DHCPv6RootScope rootScope = GetRootScope(scopeResolverMock);
            rootScope.Load(new List<DomainEvent>{ new DHCPv6ScopeEvents.DHCPv6ScopeAddedEvent(
                new DHCPv6ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv6ScopeAddressProperties(
                        IPv6Address.FromString("fe80::0"),
                        IPv6Address.FromString("fe80::ff"),
                        new List<IPv6Address>{IPv6Address.FromString("fe80::1") },
                        preferredLifeTime: TimeSpan.FromDays(0.5),
                        validLifeTime: TimeSpan.FromDays(1),
                        addressAllocationStrategy: DHCPv6ScopeAddressProperties.AddressAllocationStrategies.Next),
                    ResolverInformation = resolverInformations,
                    Name = "Testscope",
                    Id = scopeId,
                }),
                new DHCPv6LeaseCreatedEvent
                {
                    EntityId = leaseId,
                    Address = leasedAddress,
                    ClientIdentifier = new UUIDDUID(random.NextGuid()),
                    IdentityAssocationId = iaId,
                    ScopeId = scopeId,
                    StartedAt = DateTime.UtcNow,
                    ValidUntil = DateTime.UtcNow.AddDays(1),
                    HasPrefixDelegation = withPrefixDelegation,
                    PrefixLength = prefixLength,
                    IdentityAssocationIdForPrefix = prefixId,
                    DelegatedNetworkAddress = prefixNetworkAddress,
                },
            });

            DHCPv6Packet result = rootScope.HandleRequest(packet, GetServerPropertiesResolver());
            CheckErrorPacket(packet, leasedAddress, iaId, result, withPrefixDelegation == false ? DHCPv6PrefixDelegation.None : DHCPv6PrefixDelegation.FromValues(prefixNetworkAddress, new IPv6SubnetMask(new IPv6SubnetMaskIdentifier(prefixLength)), prefixId), DHCPv6StatusCodes.NoAddrsAvail);

            CheckEventAmount(1, rootScope);
            CheckHandeledEvent(0, packet, result, rootScope, scopeId, DHCPv6RequestHandledEvent.RequestErrors.LeaseNotFound);
        }

        [Fact]
        public void HandleRequest_OnlyPrefix_LeaseFound_LeaseIsActive()
        {
            Random random = new Random();

            UInt32 prefixId = random.NextUInt32();
            Byte prefixLength = (Byte)random.Next(20, 60);
            IPv6Address prefixNetworkAddress = IPv6Address.FromString("fd01::0");

            var packet = GetRequestPacket(random, out IPv6Address leasedAddress, out DUID clientDuid, out UInt32 iaId, false, new DHCPv6PacketIdentityAssociationPrefixDelegationOption(prefixId, TimeSpan.FromSeconds(random.Next()), TimeSpan.FromSeconds(random.Next()), new DHCPv6PacketSuboption[]{
                new DHCPv6PacketIdentityAssociationPrefixDelegationSuboption(TimeSpan.FromSeconds(random.Next()),TimeSpan.FromSeconds(random.Next()),prefixLength,prefixNetworkAddress,Array.Empty<DHCPv6PacketSuboption>())
                }));

            var resolverInformations = GetMockupResolver(packet, out Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>> scopeResolverMock);

            Guid scopeId = random.NextGuid();
            Guid leaseId = random.NextGuid();

            DHCPv6RootScope rootScope = GetRootScope(scopeResolverMock);
            rootScope.Load(new List<DomainEvent>{ new DHCPv6ScopeEvents.DHCPv6ScopeAddedEvent(
                new DHCPv6ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv6ScopeAddressProperties(
                        IPv6Address.FromString("fe80::0"),
                        IPv6Address.FromString("fe80::ff"),
                        new List<IPv6Address>{IPv6Address.FromString("fe80::1") },
                        t1: DHCPv6TimeScale.FromDouble(0.5),
                        t2: DHCPv6TimeScale.FromDouble(0.75),
                        preferredLifeTime: TimeSpan.FromDays(0.5),
                        validLifeTime: TimeSpan.FromDays(1),
                        addressAllocationStrategy: DHCPv6ScopeAddressProperties.AddressAllocationStrategies.Next),
                    ResolverInformation = resolverInformations,
                    Name = "Testscope",
                    Id = scopeId,
                }),
                new DHCPv6LeaseCreatedEvent
                {
                    EntityId = leaseId,
                    Address = leasedAddress,
                    ClientIdentifier = clientDuid,
                    IdentityAssocationId = iaId,
                    ScopeId = scopeId,
                    StartedAt = DateTime.UtcNow,
                    ValidUntil = DateTime.UtcNow.AddDays(1),
                    HasPrefixDelegation = true,
                    PrefixLength = prefixLength,
                    IdentityAssocationIdForPrefix = prefixId,
                    DelegatedNetworkAddress = prefixNetworkAddress,
                },
                new DHCPv6LeaseActivatedEvent
                {
                    EntityId = leaseId,
                    ScopeId = scopeId,
                },

            });

            DHCPv6Packet result = rootScope.HandleRequest(packet, GetServerPropertiesResolver());
            CheckPacket(packet, leasedAddress, 0, result, DHCPv6PacketTypes.REPLY,  DHCPv6PrefixDelegation.FromValues(prefixNetworkAddress, new IPv6SubnetMask(new IPv6SubnetMaskIdentifier(prefixLength)), prefixId));

            CheckEventAmount(2, rootScope);

            DHCPv6LeasePrefixActvatedEvent @event = rootScope.GetChanges().First() as DHCPv6LeasePrefixActvatedEvent;
            Assert.NotNull(@event);

            Assert.Equal(leaseId, @event.EntityId);
            Assert.Equal(scopeId, @event.ScopeId);

            Assert.Equal(prefixNetworkAddress, @event.NetworkAddress);
            Assert.Equal(prefixLength, @event.PrefixLength);
            Assert.Equal(prefixId, @event.PrefixAssociationId);

            CheckHandeledEvent(1, packet, result, rootScope, scopeId, DHCPv6RequestHandledEvent.RequestErrors.NoError);
        }

        [Fact]
        public void HandleRequest_OnlyPrefix_LeaseFound_LeaseIsPending()
        {
            Random random = new Random();

            UInt32 prefixId = random.NextUInt32();
            Byte prefixLength = (Byte)random.Next(20, 60);
            IPv6Address prefixNetworkAddress = IPv6Address.FromString("fd01::0");

            var packet = GetRequestPacket(random, out IPv6Address leasedAddress, out DUID clientDuid, out UInt32 iaId, false, new DHCPv6PacketIdentityAssociationPrefixDelegationOption(prefixId, TimeSpan.FromSeconds(random.Next()), TimeSpan.FromSeconds(random.Next()), new DHCPv6PacketSuboption[]{
                new DHCPv6PacketIdentityAssociationPrefixDelegationSuboption(TimeSpan.FromSeconds(random.Next()),TimeSpan.FromSeconds(random.Next()),prefixLength,prefixNetworkAddress,Array.Empty<DHCPv6PacketSuboption>())
                }));

            var resolverInformations = GetMockupResolver(packet, out Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>> scopeResolverMock);

            Guid scopeId = random.NextGuid();
            Guid leaseId = random.NextGuid();

            DHCPv6RootScope rootScope = GetRootScope(scopeResolverMock);
            rootScope.Load(new List<DomainEvent>{ new DHCPv6ScopeEvents.DHCPv6ScopeAddedEvent(
                new DHCPv6ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv6ScopeAddressProperties(
                        IPv6Address.FromString("fe80::0"),
                        IPv6Address.FromString("fe80::ff"),
                        new List<IPv6Address>{IPv6Address.FromString("fe80::1") },
                        preferredLifeTime: TimeSpan.FromDays(0.5),
                        validLifeTime: TimeSpan.FromDays(1),
                        addressAllocationStrategy: DHCPv6ScopeAddressProperties.AddressAllocationStrategies.Next),
                    ResolverInformation = resolverInformations,
                    Name = "Testscope",
                    Id = scopeId,
                }),
                new DHCPv6LeaseCreatedEvent
                {
                    EntityId = leaseId,
                    Address = leasedAddress,
                    ClientIdentifier = clientDuid,
                    IdentityAssocationId = iaId,
                    ScopeId = scopeId,
                    StartedAt = DateTime.UtcNow,
                    ValidUntil = DateTime.UtcNow.AddDays(1),
                    HasPrefixDelegation = true,
                    PrefixLength = prefixLength,
                    IdentityAssocationIdForPrefix = prefixId,
                    DelegatedNetworkAddress = prefixNetworkAddress,
                },
            });

            DHCPv6Packet result = rootScope.HandleRequest(packet, GetServerPropertiesResolver());
            CheckErrorPacket(packet, leasedAddress, 0, result, DHCPv6PrefixDelegation.FromValues(prefixNetworkAddress, new IPv6SubnetMask(new IPv6SubnetMaskIdentifier(prefixLength)), prefixId), DHCPv6StatusCodes.NoAddrsAvail);

            CheckEventAmount(1, rootScope);
            CheckHandeledEvent(0, packet, result, rootScope, scopeId, DHCPv6RequestHandledEvent.RequestErrors.LeasePendingButOnlyPrefixRequested);
        }

        [Fact]
        public void HandleRequest_OnlyPrefix_LeaseNotFound()
        {
            Random random = new Random();

            UInt32 prefixId = random.NextUInt32();
            Byte prefixLength = (Byte)random.Next(20, 60);
            IPv6Address prefixNetworkAddress = IPv6Address.FromString("fd01::0");

            var packet = GetRequestPacket(random, out IPv6Address leasedAddress, out DUID _, out UInt32 iaId, false, new DHCPv6PacketIdentityAssociationPrefixDelegationOption(prefixId, TimeSpan.FromSeconds(random.Next()), TimeSpan.FromSeconds(random.Next()), new DHCPv6PacketSuboption[]{
                new DHCPv6PacketIdentityAssociationPrefixDelegationSuboption(TimeSpan.FromSeconds(random.Next()),TimeSpan.FromSeconds(random.Next()),prefixLength,prefixNetworkAddress,Array.Empty<DHCPv6PacketSuboption>())
                }));
            var resolverInformations = GetMockupResolver(packet, out Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>> scopeResolverMock);

            Guid scopeId = random.NextGuid();
            Guid leaseId = random.NextGuid();

            DHCPv6RootScope rootScope = GetRootScope(scopeResolverMock);
            rootScope.Load(new List<DomainEvent>{ new DHCPv6ScopeEvents.DHCPv6ScopeAddedEvent(
                new DHCPv6ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv6ScopeAddressProperties(
                        IPv6Address.FromString("fe80::0"),
                        IPv6Address.FromString("fe80::ff"),
                        new List<IPv6Address>{IPv6Address.FromString("fe80::1") },
                        preferredLifeTime: TimeSpan.FromDays(0.5),
                        validLifeTime: TimeSpan.FromDays(1),
                        addressAllocationStrategy: DHCPv6ScopeAddressProperties.AddressAllocationStrategies.Next),
                    ResolverInformation = resolverInformations,
                    Name = "Testscope",
                    Id = scopeId,
                }),
                new DHCPv6LeaseCreatedEvent
                {
                    EntityId = leaseId,
                    Address = leasedAddress,
                    ClientIdentifier = new UUIDDUID(random.NextGuid()),
                    IdentityAssocationId = iaId,
                    ScopeId = scopeId,
                    StartedAt = DateTime.UtcNow,
                    ValidUntil = DateTime.UtcNow.AddDays(1),
                    HasPrefixDelegation = false,
                    PrefixLength = prefixLength,
                    IdentityAssocationIdForPrefix = prefixId,
                    DelegatedNetworkAddress = prefixNetworkAddress,
                },
            });

            DHCPv6Packet result = rootScope.HandleRequest(packet, GetServerPropertiesResolver());
            CheckErrorPacket(packet, leasedAddress, 0, result, DHCPv6PrefixDelegation.FromValues(prefixNetworkAddress, new IPv6SubnetMask(new IPv6SubnetMaskIdentifier(prefixLength)), prefixId), DHCPv6StatusCodes.NoAddrsAvail);

            CheckEventAmount(1, rootScope);
            CheckHandeledEvent(0, packet, result, rootScope, scopeId, DHCPv6RequestHandledEvent.RequestErrors.LeaseNotFound);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void HandleRequest_ScopeNotFound(Boolean withPrefixDelegation)
        {
            Random random = new Random();

            UInt32 prefixId = random.NextUInt32();
            Byte prefixLength = (Byte)random.Next(20, 60);
            IPv6Address prefixNetworkAddress = IPv6Address.FromString("fd01::0");

            DHCPv6PacketOption[] options = Array.Empty<DHCPv6PacketOption>();
            if (withPrefixDelegation == true)
            {
                options = new DHCPv6PacketOption[] {
                new DHCPv6PacketIdentityAssociationPrefixDelegationOption(prefixId,TimeSpan.FromSeconds(random.Next()),TimeSpan.FromSeconds(random.Next()),new DHCPv6PacketSuboption[]{
                new DHCPv6PacketIdentityAssociationPrefixDelegationSuboption(TimeSpan.FromSeconds(random.Next()),TimeSpan.FromSeconds(random.Next()),prefixLength,prefixNetworkAddress,Array.Empty<DHCPv6PacketSuboption>())
                })
                };
            }

            var packet = GetRequestPacket(random, out _, out DUID _, out UInt32 _, true, options);
             GetMockupResolver(DHCPv6Packet.Empty, out Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>> scopeResolverMock);

            DHCPv6RootScope rootScope = GetRootScope(scopeResolverMock);

            DHCPv6Packet result = rootScope.HandleRequest(packet, GetServerPropertiesResolver());
            Assert.Equal(DHCPv6Packet.Empty, result);

            CheckEventAmount(1, rootScope);
            CheckHandeledEvent(0, packet, result, rootScope, null, DHCPv6RequestHandledEvent.RequestErrors.ScopeNotFound);
        }
    }
}
