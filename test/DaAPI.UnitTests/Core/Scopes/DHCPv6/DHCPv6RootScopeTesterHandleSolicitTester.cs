using DaAPI.Core.Common;
using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Notifications.Triggers;
using DaAPI.Core.Packets.DHCPv6;
using DaAPI.Core.Scopes;
using DaAPI.Core.Scopes.DHCPv6;
using DaAPI.Core.Scopes.DHCPv6.ScopeProperties;
using DaAPI.Core.Services;
using DaAPI.TestHelper;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using static DaAPI.Core.Scopes.DHCPv6.DHCPv6LeaseEvents;
using static DaAPI.Core.Scopes.DHCPv6.DHCPv6PacketHandledEvents;

namespace DaAPI.UnitTests.Core.Scopes.DHCPv6
{
    public class DHCPv6RootScopeTesterHandleSolicitTester : DHCPv6RootSoopeTesterBase
    {
        private void CheckHandeledEvent(Int32 index, DHCPv6Packet discoverPacket, DHCPv6Packet result, DHCPv6RootScope rootScope, Guid scopeId)
        {
            IEnumerable<DomainEvent> changes = rootScope.GetChanges();

            Assert.IsAssignableFrom<DHCPv6SolicitHandledEvent>(changes.ElementAt(index));

            DHCPv6SolicitHandledEvent handeledEvent = (DHCPv6SolicitHandledEvent)changes.ElementAt(index);
            Assert.Equal(discoverPacket, handeledEvent.Request);
            Assert.Equal(result, handeledEvent.Response);
            Assert.Equal(scopeId, handeledEvent.ScopeId);
            Assert.True(handeledEvent.WasSuccessfullHandled);
        }

        private void CheckHandeledEventNotSuccessfull(Int32 index, DHCPv6Packet solicitPacket, DHCPv6RootScope rootScope, Guid scopeId)
        {
            IEnumerable<DomainEvent> changes = rootScope.GetChanges();

            Assert.IsAssignableFrom<DHCPv6SolicitHandledEvent>(changes.ElementAt(index));

            DHCPv6SolicitHandledEvent handeledEvent = (DHCPv6SolicitHandledEvent)changes.ElementAt(index);
            Assert.Equal(solicitPacket, handeledEvent.Request);
            Assert.Equal(DHCPv6Packet.Empty, handeledEvent.Response);
            Assert.Equal(scopeId, handeledEvent.ScopeId);
            Assert.False(handeledEvent.WasSuccessfullHandled);
        }

        private DHCPv6Packet GetSolicitPacket(
            Random random, out DUID clientDuid, out UInt32 iaId, Boolean createBNullIaid = false, params DHCPv6PacketOption[] options)
        {
            IPv6HeaderInformation headerInformation =
               new IPv6HeaderInformation(IPv6Address.FromString("fe80::1"), IPv6Address.FromString("fe80::2"));

            clientDuid = new UUIDDUID(random.NextGuid());
            if (createBNullIaid == true)
            {
                iaId = 0;
            }
            else
            {
                iaId = random.NextUInt32();
            }

            var packetOptions = new List<DHCPv6PacketOption>(options)
                {
                    new DHCPv6PacketIdentifierOption(DHCPv6PacketOptionTypes.ClientIdentifier,clientDuid),
                    new DHCPv6PacketIdentityAssociationNonTemporaryAddressesOption(iaId,TimeSpan.Zero,TimeSpan.Zero,Array.Empty<DHCPv6PacketSuboption>())
                };

            DHCPv6Packet innerPacket = DHCPv6Packet.AsInner(random.NextUInt16(),
                DHCPv6PacketTypes.Solicit, packetOptions);

            DHCPv6Packet packet = DHCPv6RelayPacket.AsOuterRelay(headerInformation, true, 1, random.GetIPv6Address(), random.GetIPv6Address(),
    new DHCPv6PacketOption[] { new DHCPv6PacketByteArrayOption(DHCPv6PacketOptionTypes.InterfaceId, random.NextBytes(10)) }, innerPacket);

            return packet;
        }

        private DHCPv6Packet GetSolicitPacketWithPrefixIdOnly(
    Random random, out DUID clientDuid, out UInt32 prefixIaId, params DHCPv6PacketOption[] options)
        {
            IPv6HeaderInformation headerInformation =
               new IPv6HeaderInformation(IPv6Address.FromString("fe80::1"), IPv6Address.FromString("fe80::1"));

            clientDuid = new UUIDDUID(random.NextGuid());
            prefixIaId = random.NextUInt32();

            var packetOptions = new List<DHCPv6PacketOption>(options)
                {
                    new DHCPv6PacketIdentifierOption(DHCPv6PacketOptionTypes.ClientIdentifier,clientDuid),
                    new DHCPv6PacketIdentityAssociationPrefixDelegationOption(prefixIaId,TimeSpan.Zero,TimeSpan.Zero,Array.Empty<DHCPv6PacketSuboption>())
                };

            DHCPv6Packet packet = DHCPv6Packet.AsOuter(headerInformation, random.NextUInt16(),
                DHCPv6PacketTypes.Solicit, packetOptions);

            return packet;
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void HandleSolicit_NoLeaseFound_AddressAvaiable(Boolean withNullIaid)
        {
            Random random = new Random();

            var packet = GetSolicitPacket(random, out DUID clientDuid, out UInt32 iaId, withNullIaid);
            var resolverInformations = GetMockupResolver(packet, out Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>> scopeResolverMock);

            Guid scopeId = random.NextGuid();

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
                })
            });

            IPv6Address expectedAdress = IPv6Address.FromString("fe80::0");

            DHCPv6Packet result = rootScope.HandleSolicit(packet, GetServerPropertiesResolver());
            CheckPacket(packet, expectedAdress, iaId, result, DHCPv6PacketTypes.ADVERTISE, DHCPv6PrefixDelegation.None);

            DHCPv6Lease lease = CheckLease(0, 1, expectedAdress, scopeId, rootScope, DateTime.UtcNow, clientDuid, iaId, true, false);

            CheckEventAmount(2, rootScope);
            CheckLeaseCreatedEvent(0, clientDuid, iaId, scopeId, rootScope, expectedAdress, lease);
            CheckHandeledEvent(1, packet, result, rootScope, scopeId);

            CheckEmptyTrigger(rootScope);
        }

        [Fact]
        public void HandleSolicit_TestRandomizeOfAddresses()
        {
            Random random = new Random();

            var packet = GetSolicitPacket(random, out DUID clientDuid, out UInt32 iaId);
            var resolverInformations = GetMockupResolver(packet, out Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>> scopeResolverMock);

            Guid scopeId = random.NextGuid();

            DHCPv6RootScope rootScope = GetRootScope(scopeResolverMock);
            rootScope.Load(new List<DomainEvent>{ new DHCPv6ScopeEvents.DHCPv6ScopeAddedEvent(
                new DHCPv6ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv6ScopeAddressProperties(
                        IPv6Address.FromString("fe80::0"),
                        IPv6Address.FromString("fe80:ffff:ffff:ffff:ffff:ffff:ffff:ffff"),
                        new List<IPv6Address>{IPv6Address.FromString("fe80::1") },
                        t1: DHCPv6TimeScale.FromDouble(0.5),
                        t2: DHCPv6TimeScale.FromDouble(0.75),
                        preferredLifeTime: TimeSpan.FromDays(0.5),
                        validLifeTime: TimeSpan.FromDays(1),
                        addressAllocationStrategy: DHCPv6ScopeAddressProperties.AddressAllocationStrategies.Random),
                    ResolverInformation = resolverInformations,
                    Name = "Testscope",
                    Id = scopeId,
                })
            });


            DHCPv6Packet result = rootScope.HandleSolicit(packet, GetServerPropertiesResolver());

            IPv6Address address = GetLeasedIPv6AddresFromPacket(result, iaId);
            Int32 zeroCounter = address.GetBytes().Count(x => x == 0);

            Assert.True(zeroCounter <= 3);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void HandleSolicit_NoLeaseFound_AddressAvaiable_WithPrefix(Boolean rapitCommitEnabled)
        {
            Random random = new Random();

            UInt32 prefixId = random.NextUInt32();
            IPv6Address prefixNetworkAddress = IPv6Address.FromString("2001:e68:5423::0");
            IPv6Address maxNetworkAddress = IPv6Address.FromString("2001:0e68:54ff::0");

            Byte prefixLength = 48;
            Byte assingedPrefixLength = 64;

            var packet = GetSolicitPacket(random, out DUID clientDuid, out UInt32 iaId, false,
                new DHCPv6PacketIdentityAssociationPrefixDelegationOption(prefixId, TimeSpan.Zero, TimeSpan.Zero, Array.Empty<DHCPv6PacketByteValueSuboption>()),
                new DHCPv6PacketTrueOption(DHCPv6PacketOptionTypes.RapitCommit)
                );

            var resolverInformations = GetMockupResolver(packet, out Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>> scopeResolverMock);

            Guid scopeId = random.NextGuid();

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
                        rapitCommitEnabled: rapitCommitEnabled,
                        prefixDelgationInfo: DHCPv6PrefixDelgationInfo.FromValues(prefixNetworkAddress,new IPv6SubnetMaskIdentifier(prefixLength),new IPv6SubnetMaskIdentifier(assingedPrefixLength)),
                        addressAllocationStrategy: DHCPv6ScopeAddressProperties.AddressAllocationStrategies.Next),
                    ResolverInformation = resolverInformations,
                    Name = "Testscope",
                    Id = scopeId,
                })
            });

            IPv6Address expectedAdress = IPv6Address.FromString("fe80::0");

            DHCPv6Packet result = rootScope.HandleSolicit(packet, GetServerPropertiesResolver());
            CheckPacket(packet, expectedAdress, iaId, result, rapitCommitEnabled == true ? DHCPv6PacketTypes.REPLY : DHCPv6PacketTypes.ADVERTISE, DHCPv6PrefixDelegation.None);

            DHCPv6Lease lease = CheckLease(0, 1, expectedAdress, scopeId, rootScope, DateTime.UtcNow, clientDuid, iaId, !rapitCommitEnabled, true);

            Assert.NotEqual(DHCPv6PrefixDelegation.None, lease.PrefixDelegation);
            Assert.Equal(assingedPrefixLength, lease.PrefixDelegation.Mask.Identifier);
            Assert.Equal(prefixId, lease.PrefixDelegation.IdentityAssociation);
            Assert.True((new IPv6SubnetMask(new IPv6SubnetMaskIdentifier(64))).IsIPv6AdressANetworkAddress(lease.PrefixDelegation.NetworkAddress));

            Assert.True(lease.PrefixDelegation.NetworkAddress.IsBetween(prefixNetworkAddress, maxNetworkAddress));

            CheckEventAmount(rapitCommitEnabled == true ? 3 : 2, rootScope);
            CheckLeaseCreatedEvent(0, clientDuid, iaId, scopeId, rootScope, expectedAdress, lease);
            CheckHandeledEvent(rapitCommitEnabled == true ? 2 : 1, packet, result, rootScope, scopeId);

            var createdEvent = rootScope.GetChanges().ElementAt(0) as DHCPv6LeaseCreatedEvent;
            Assert.True(createdEvent.HasPrefixDelegation);
            Assert.Equal(lease.PrefixDelegation.NetworkAddress, createdEvent.DelegatedNetworkAddress);
            Assert.Equal(assingedPrefixLength, createdEvent.PrefixLength);
            Assert.Equal(prefixId, createdEvent.IdentityAssocationIdForPrefix);

            if (rapitCommitEnabled == true)
            {
                var trigger = CheckTrigger<PrefixEdgeRouterBindingUpdatedTrigger>(rootScope);

                Assert.Equal(scopeId, trigger.ScopeId);
                Assert.Null(trigger.OldBinding);
                Assert.NotNull(trigger.NewBinding);

                Assert.Equal(lease.PrefixDelegation.NetworkAddress, trigger.NewBinding.Prefix);
                Assert.Equal(assingedPrefixLength, trigger.NewBinding.Mask.Identifier);
                Assert.Equal(expectedAdress, trigger.NewBinding.Host);
            }
        }

        [Fact]
        public void HandleSolicit_NoLeaseFound_AddressAvaiable_WithPrefix_BothIAIds0()
        {
            Random random = new Random();

            UInt32 prefixId = 0;
            IPv6Address prefixNetworkAddress = IPv6Address.FromString("2001:e68:5423::0");
            IPv6Address maxNetworkAddress = IPv6Address.FromString("2001:0e68:54ff::0");

            Byte prefixLength = 48;
            Byte assingedPrefixLength = 64;

            var packet = GetSolicitPacket(random, out DUID clientDuid, out UInt32 iaId, true,
                new DHCPv6PacketIdentityAssociationPrefixDelegationOption(prefixId, TimeSpan.Zero, TimeSpan.Zero, Array.Empty<DHCPv6PacketByteValueSuboption>()),
                new DHCPv6PacketTrueOption(DHCPv6PacketOptionTypes.RapitCommit)
                );

            var resolverInformations = GetMockupResolver(packet, out Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>> scopeResolverMock);

            Guid scopeId = random.NextGuid();

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
                        rapitCommitEnabled: true,
                        prefixDelgationInfo: DHCPv6PrefixDelgationInfo.FromValues(prefixNetworkAddress,new IPv6SubnetMaskIdentifier(prefixLength),new IPv6SubnetMaskIdentifier(assingedPrefixLength)),
                        addressAllocationStrategy: DHCPv6ScopeAddressProperties.AddressAllocationStrategies.Next),
                    ResolverInformation = resolverInformations,
                    Name = "Testscope",
                    Id = scopeId,
                })
            });

            IPv6Address expectedAdress = IPv6Address.FromString("fe80::0");

            DHCPv6Packet result = rootScope.HandleSolicit(packet, GetServerPropertiesResolver());
            CheckPacket(packet, expectedAdress, iaId, result, DHCPv6PacketTypes.REPLY, DHCPv6PrefixDelegation.None);

            DHCPv6Lease lease = CheckLease(0, 1, expectedAdress, scopeId, rootScope, DateTime.UtcNow, clientDuid, iaId, false, true);

            Assert.NotEqual(DHCPv6PrefixDelegation.None, lease.PrefixDelegation);
            Assert.Equal(assingedPrefixLength, lease.PrefixDelegation.Mask.Identifier);
            Assert.Equal(prefixId, lease.PrefixDelegation.IdentityAssociation);
            Assert.True((new IPv6SubnetMask(new IPv6SubnetMaskIdentifier(64))).IsIPv6AdressANetworkAddress(lease.PrefixDelegation.NetworkAddress));

            Assert.True(lease.PrefixDelegation.NetworkAddress.IsBetween(prefixNetworkAddress, maxNetworkAddress));

            CheckEventAmount(3, rootScope);
            CheckLeaseCreatedEvent(0, clientDuid, iaId, scopeId, rootScope, expectedAdress, lease);
            CheckHandeledEvent(2, packet, result, rootScope, scopeId);

            var createdEvent = rootScope.GetChanges().ElementAt(0) as DHCPv6LeaseCreatedEvent;
            Assert.True(createdEvent.HasPrefixDelegation);
            Assert.Equal(lease.PrefixDelegation.NetworkAddress, createdEvent.DelegatedNetworkAddress);
            Assert.Equal(assingedPrefixLength, createdEvent.PrefixLength);
            Assert.Equal(prefixId, createdEvent.IdentityAssocationIdForPrefix);

            var trigger = CheckTrigger<PrefixEdgeRouterBindingUpdatedTrigger>(rootScope);

            Assert.Equal(scopeId, trigger.ScopeId);
            Assert.Null(trigger.OldBinding);
            Assert.NotNull(trigger.NewBinding);

            Assert.Equal(lease.PrefixDelegation.NetworkAddress, trigger.NewBinding.Prefix);
            Assert.Equal(assingedPrefixLength, trigger.NewBinding.Mask.Identifier);
            Assert.Equal(expectedAdress, trigger.NewBinding.Host);
        }

        [Fact]
        public void HandleSolicit_NoLeaseFound_AddressAvaiable_WithPrefix_UnevenLength()
        {
            Random random = new Random();

            UInt32 prefixId = random.NextUInt32();
            IPv6Address prefixNetworkAddress = IPv6Address.FromString("2001:0e68:5423:C000::0");
            IPv6Address maxNetworkAddress = IPv6Address.FromString("2001:0e68:5423:CC00::0");

            Byte prefixLength = 52;
            Byte assingedPrefixLength = 54;

            var packet = GetSolicitPacket(random, out DUID clientDuid, out UInt32 iaId, false,
                new DHCPv6PacketIdentityAssociationPrefixDelegationOption(prefixId, TimeSpan.Zero, TimeSpan.Zero, Array.Empty<DHCPv6PacketByteValueSuboption>()));

            var resolverInformations = GetMockupResolver(packet, out Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>> scopeResolverMock);

            Guid scopeId = random.NextGuid();

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
                        prefixDelgationInfo: DHCPv6PrefixDelgationInfo.FromValues(prefixNetworkAddress,new IPv6SubnetMaskIdentifier(prefixLength),new IPv6SubnetMaskIdentifier(assingedPrefixLength)),
                        addressAllocationStrategy: DHCPv6ScopeAddressProperties.AddressAllocationStrategies.Next),
                    ResolverInformation = resolverInformations,
                    Name = "Testscope",
                    Id = scopeId,
                })
            });

            IPv6Address expectedAdress = IPv6Address.FromString("fe80::0");

            DHCPv6Packet result = rootScope.HandleSolicit(packet, GetServerPropertiesResolver());
            CheckPacket(packet, expectedAdress, iaId, result, DHCPv6PacketTypes.ADVERTISE, DHCPv6PrefixDelegation.None);

            DHCPv6Lease lease = CheckLease(0, 1, expectedAdress, scopeId, rootScope, DateTime.UtcNow, clientDuid, iaId, true, true);

            Assert.NotEqual(DHCPv6PrefixDelegation.None, lease.PrefixDelegation);
            Assert.Equal(assingedPrefixLength, lease.PrefixDelegation.Mask.Identifier);
            Assert.Equal(prefixId, lease.PrefixDelegation.IdentityAssociation);
            Assert.True((new IPv6SubnetMask(new IPv6SubnetMaskIdentifier(64))).IsIPv6AdressANetworkAddress(lease.PrefixDelegation.NetworkAddress));

            Assert.True(lease.PrefixDelegation.NetworkAddress.IsBetween(prefixNetworkAddress, maxNetworkAddress));

            CheckEventAmount(2, rootScope);
            CheckLeaseCreatedEvent(0, clientDuid, iaId, scopeId, rootScope, expectedAdress, lease);
            CheckHandeledEvent(1, packet, result, rootScope, scopeId);

            CheckEmptyTrigger(rootScope);
        }

        [Fact]
        public void HandleSolicit_NoLeaseFound_AddressAvaiable_WithRapitCommit()
        {
            Random random = new Random();

            var packet = GetSolicitPacket(random, out DUID clientDuid, out UInt32 iaId, false, new DHCPv6PacketTrueOption(DHCPv6PacketOptionTypes.RapitCommit));
            var resolverInformations = GetMockupResolver(packet, out Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>> scopeResolverMock);

            Guid scopeId = random.NextGuid();

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
                        rapitCommitEnabled: true,
                        addressAllocationStrategy: DHCPv6ScopeAddressProperties.AddressAllocationStrategies.Next),
                    ResolverInformation = resolverInformations,
                    Name = "Testscope",
                    Id = scopeId,
                })
            });

            IPv6Address expectedAdress = IPv6Address.FromString("fe80::0");

            DHCPv6Packet result = rootScope.HandleSolicit(packet, GetServerPropertiesResolver());
            CheckPacket(packet, expectedAdress, iaId, result, DHCPv6PacketTypes.REPLY, DHCPv6PrefixDelegation.None);

            DHCPv6Lease lease = CheckLease(0, 1, expectedAdress, scopeId, rootScope, DateTime.UtcNow, clientDuid, iaId, false, false);

            CheckEventAmount(3, rootScope);
            CheckLeaseCreatedEvent(0, clientDuid, iaId, scopeId, rootScope, expectedAdress, lease);
            CheckLeaseActivatedEvent(1, scopeId, lease.Id, rootScope);
            CheckHandeledEvent(2, packet, result, rootScope, scopeId);

            CheckEmptyTrigger(rootScope);
        }

        [Fact]
        public async Task HandleSolicit_NoLeaseFound_AddressAvaiable_WithRapitCommit_TwoPackets()
        {
            Random random = new Random();

            var packet = GetSolicitPacket(random, out DUID clientDuid, out UInt32 iaId, false, new DHCPv6PacketTrueOption(DHCPv6PacketOptionTypes.RapitCommit));
            var resolverInformations = GetMockupResolver(packet, out Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>> scopeResolverMock);

            Guid scopeId = random.NextGuid();

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
                        rapitCommitEnabled: true,
                        reuseAddressIfPossible: false,
                        addressAllocationStrategy: DHCPv6ScopeAddressProperties.AddressAllocationStrategies.Next),
                    ResolverInformation = resolverInformations,
                    Name = "Testscope",
                    Id = scopeId,
                })
            }); ;

            Guid serverId = random.NextGuid();

            IPv6Address expectedAdress = IPv6Address.FromString("fe80::0");

            DHCPv6Packet result = rootScope.HandleSolicit(packet, GetServerPropertiesResolver(serverId));
            await Task.Delay(200);
            DHCPv6Packet secondResult = rootScope.HandleSolicit(packet, GetServerPropertiesResolver(serverId));
            //Assert.Equal(result, secondResult);

            CheckPacket(packet, expectedAdress, iaId, result, DHCPv6PacketTypes.REPLY, DHCPv6PrefixDelegation.None);
            CheckPacket(packet, expectedAdress, iaId, secondResult, DHCPv6PacketTypes.REPLY, DHCPv6PrefixDelegation.None);

            DHCPv6Lease lease = CheckLease(0, 1, expectedAdress, scopeId, rootScope, DateTime.UtcNow, clientDuid, iaId, false, false);

            CheckEventAmount(4, rootScope);
            CheckLeaseCreatedEvent(0, clientDuid, iaId, scopeId, rootScope, expectedAdress, lease);
            CheckLeaseActivatedEvent(1, scopeId, lease.Id, rootScope);
            CheckHandeledEvent(2, packet, result, rootScope, scopeId);
            CheckHandeledEvent(3, packet, secondResult, rootScope, scopeId);

            CheckEmptyTrigger(rootScope);
        }

        [Theory]
        [InlineData(DHCPv6ScopeAddressProperties.AddressAllocationStrategies.Next)]
        [InlineData(DHCPv6ScopeAddressProperties.AddressAllocationStrategies.Random)]
        public void HandleSolicit_NoLeaseFound_AddressAvaiable_SingleAddressPool(
            DHCPv6ScopeAddressProperties.AddressAllocationStrategies allocationStrategy
            )
        {
            Random random = new Random();

            var packet = GetSolicitPacket(random, out DUID clientDuid, out UInt32 iaId);
            var resolverInformations = GetMockupResolver(packet, out Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>> scopeResolverMock);

            Guid scopeId = random.NextGuid();

            DHCPv6RootScope rootScope = GetRootScope(scopeResolverMock);
            rootScope.Load(new List<DomainEvent>{ new DHCPv6ScopeEvents.DHCPv6ScopeAddedEvent(
                new DHCPv6ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv6ScopeAddressProperties(
                        IPv6Address.FromString("fe80::0"),
                        IPv6Address.FromString("fe80::0"),
                        new List<IPv6Address>{ },
                        t1: DHCPv6TimeScale.FromDouble(0.5),
                        t2: DHCPv6TimeScale.FromDouble(0.75),
                        preferredLifeTime: TimeSpan.FromDays(0.5),
                        validLifeTime: TimeSpan.FromDays(1),
                        addressAllocationStrategy: allocationStrategy),
                    ResolverInformation = resolverInformations,
                    Name = "Testscope",
                    Id = scopeId,
                })
            });

            IPv6Address expectedAdress = IPv6Address.FromString("fe80::0");

            DHCPv6Packet result = rootScope.HandleSolicit(packet, GetServerPropertiesResolver());
            CheckPacket(packet, expectedAdress, iaId, result, DHCPv6PacketTypes.ADVERTISE, DHCPv6PrefixDelegation.None);

            DHCPv6Lease lease = CheckLease(0, 1, expectedAdress, scopeId, rootScope, DateTime.UtcNow, clientDuid, iaId, true, false);

            CheckEventAmount(2, rootScope);
            CheckLeaseCreatedEvent(0, clientDuid, iaId, scopeId, rootScope, expectedAdress, lease);
            CheckHandeledEvent(1, packet, result, rootScope, scopeId);

            CheckEmptyTrigger(rootScope);
        }

        [Theory]
        [InlineData(DHCPv6ScopeAddressProperties.AddressAllocationStrategies.Next)]
        [InlineData(DHCPv6ScopeAddressProperties.AddressAllocationStrategies.Random)]
        public void HandleSolicit_NoLeaseFound_AddressAvaiable_FirstFreeAddress(
            DHCPv6ScopeAddressProperties.AddressAllocationStrategies allocationStrategy)
        {
            Random random = new Random();

            var packet = GetSolicitPacket(random, out DUID clientDuid, out UInt32 iaId);
            var resolverInformations = GetMockupResolver(packet, out Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>> scopeResolverMock);

            Guid scopeId = random.NextGuid();

            Byte avaiableAddress = random.NextByte();
            List<IPv6Address> excludedAddress = new List<IPv6Address>(255);
            for (int i = 0; i <= Byte.MaxValue; i++)
            {
                if (i != avaiableAddress)
                {
                    excludedAddress.Add(IPv6Address.FromByteArray(new byte[16] { 254, 128, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, (Byte)i }));
                }
            }

            DHCPv6RootScope rootScope = GetRootScope(scopeResolverMock);
            rootScope.Load(new List<DomainEvent>{ new DHCPv6ScopeEvents.DHCPv6ScopeAddedEvent(
                new DHCPv6ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv6ScopeAddressProperties(
                        IPv6Address.FromString("fe80::0"),
                        IPv6Address.FromString("fe80::ff"),
                        excludedAddress,
                        t1: DHCPv6TimeScale.FromDouble(0.5),
                        t2: DHCPv6TimeScale.FromDouble(0.75),
                        preferredLifeTime: TimeSpan.FromDays(0.5),
                        validLifeTime: TimeSpan.FromDays(1),
                        addressAllocationStrategy: allocationStrategy),
                    ResolverInformation = resolverInformations,
                    Name = "Testscope",
                    Id = scopeId,
                })
            });

            IPv6Address expectedAdress = IPv6Address.FromByteArray(new byte[16] { 254, 128, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, avaiableAddress });

            DHCPv6Packet result = rootScope.HandleSolicit(packet, GetServerPropertiesResolver());
            CheckPacket(packet, expectedAdress, iaId, result, DHCPv6PacketTypes.ADVERTISE, DHCPv6PrefixDelegation.None);

            DHCPv6Lease lease = CheckLease(0, 1, expectedAdress, scopeId, rootScope, DateTime.UtcNow, clientDuid, iaId, true, false);

            CheckEventAmount(2, rootScope);
            CheckLeaseCreatedEvent(0, clientDuid, iaId, scopeId, rootScope, expectedAdress, lease);
            CheckHandeledEvent(1, packet, result, rootScope, scopeId);

            CheckEmptyTrigger(rootScope);
        }

        [Theory]
        [InlineData(DHCPv6ScopeAddressProperties.AddressAllocationStrategies.Next)]
        [InlineData(DHCPv6ScopeAddressProperties.AddressAllocationStrategies.Random)]
        public void HandleSolicit_LeaseFound_ReuseEnabled(DHCPv6ScopeAddressProperties.AddressAllocationStrategies allocationStrategy)
        {
            Random random = new Random();

            var packet = GetSolicitPacket(random, out DUID clientDuid, out UInt32 iaId);
            var resolverInformations = GetMockupResolver(packet, out Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>> scopeResolverMock);

            Guid scopeId = random.NextGuid();
            Guid leaseId = random.NextGuid();

            IPv6Address leasedAddress = IPv6Address.FromString("fe80::A");
            DateTime leaseCreatedAt = DateTime.UtcNow.AddHours(-1);
            DHCPv6RootScope rootScope = GetRootScope(scopeResolverMock);
            rootScope.Load(new List<DomainEvent>{ new DHCPv6ScopeEvents.DHCPv6ScopeAddedEvent(
                new DHCPv6ScopeCreateInstruction
                {
                  AddressProperties = new DHCPv6ScopeAddressProperties(
                        IPv6Address.FromString("fe80::0"),
                        IPv6Address.FromString("fe80::ff"),
                        Array.Empty<IPv6Address>(),
                        t1: DHCPv6TimeScale.FromDouble(0.5),
                        t2: DHCPv6TimeScale.FromDouble(0.75),
                        preferredLifeTime: TimeSpan.FromDays(0.5),
                        validLifeTime: TimeSpan.FromDays(1),
                        reuseAddressIfPossible: true,
                        addressAllocationStrategy: allocationStrategy),
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
                    StartedAt = leaseCreatedAt,
                    ValidUntil = DateTime.UtcNow.AddDays(1),
                 },
                 new DHCPv6LeaseActivatedEvent
                 {
                    EntityId = leaseId,
                    ScopeId = scopeId,
                 }
            }); ;

            IPv6Address expectedAdress = leasedAddress;

            DHCPv6Packet result = rootScope.HandleSolicit(packet, GetServerPropertiesResolver());
            CheckPacket(packet, expectedAdress, iaId, result, DHCPv6PacketTypes.ADVERTISE, DHCPv6PrefixDelegation.None);

            DHCPv6Lease lease = CheckLease(1, 2, expectedAdress, scopeId, rootScope, leaseCreatedAt, clientDuid, iaId, true, false);
            CheckEventAmount(2, rootScope);

            CheckLeaseCreatedEvent(0, clientDuid, iaId, scopeId, rootScope, expectedAdress, lease, null, leaseId);
            CheckHandeledEvent(1, packet, result, rootScope, scopeId);

            CheckEmptyTrigger(rootScope);
        }

        [Fact]
        public void HandleSolicit_LeaseFound_ReuseEnabled_ButLeaseNotExtentable()
        {
            Random random = new Random();

            var packet = GetSolicitPacket(random, out DUID clientDuid, out UInt32 iaId);
            var resolverInformations = GetMockupResolver(packet, out Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>> scopeResolverMock);

            Guid scopeId = random.NextGuid();
            Guid leaseId = random.NextGuid();

            IPv6Address leasedAddress = IPv6Address.FromString("fe80::A");
            DateTime leaseCreatedAt = DateTime.UtcNow.AddHours(-1);
            DHCPv6RootScope rootScope = GetRootScope(scopeResolverMock);
            rootScope.Load(new List<DomainEvent>{ new DHCPv6ScopeEvents.DHCPv6ScopeAddedEvent(
                new DHCPv6ScopeCreateInstruction
                {
                  AddressProperties = new DHCPv6ScopeAddressProperties(
                        IPv6Address.FromString("fe80::8"),
                        IPv6Address.FromString("fe80::ff"),
                        Array.Empty<IPv6Address>(),
                        t1: DHCPv6TimeScale.FromDouble(0.5),
                        t2: DHCPv6TimeScale.FromDouble(0.75),
                        preferredLifeTime: TimeSpan.FromDays(0.5),
                        validLifeTime: TimeSpan.FromDays(1),
                        reuseAddressIfPossible: true,
                        addressAllocationStrategy: ScopeAddressProperties<DHCPv6ScopeAddressProperties, IPv6Address>.AddressAllocationStrategies.Next),
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
                    StartedAt = leaseCreatedAt,
                    ValidUntil = DateTime.UtcNow.AddDays(1),
                 },
                 new DHCPv6LeaseActivatedEvent
                 {
                    EntityId = leaseId,
                    ScopeId = scopeId,
                 },
                 new DHCPv6LeaseCanceledEvent
                 {
                    EntityId = leaseId,
                    ScopeId = scopeId,
                 }
            }); ;

            IPv6Address expectedAddress = IPv6Address.FromString("fe80::8");

            DHCPv6Packet result = rootScope.HandleSolicit(packet, GetServerPropertiesResolver());
            CheckPacket(packet, expectedAddress, iaId, result, DHCPv6PacketTypes.ADVERTISE, DHCPv6PrefixDelegation.None);

            DHCPv6Lease lease = CheckLease(1, 2, expectedAddress, scopeId, rootScope, DateTime.UtcNow, clientDuid, iaId, true, false);
            CheckEventAmount(2, rootScope);

            CheckLeaseCreatedEvent(0, clientDuid, iaId, scopeId, rootScope, expectedAddress, lease, null);

            CheckHandeledEvent(1, packet, result, rootScope, scopeId);

            CheckEmptyTrigger(rootScope);
        }

        [Fact]
        public void HandleSolicit_MultipleLeaseFound_ReuseEnabled_ButLeaseNotExtentable_WithRapitCommit()
        {
            Random random = new Random();

            var packet = GetSolicitPacket(random, out DUID clientDuid, out UInt32 iaId, false, new[] { new DHCPv6PacketTrueOption(DHCPv6PacketOptionTypes.RapitCommit) });
            var resolverInformations = GetMockupResolver(packet, out Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>> scopeResolverMock);

            Guid scopeId = random.NextGuid();

            IPv6Address leasedAddress = IPv6Address.FromString("fe80::8");
            DateTime leaseCreatedAt = DateTime.UtcNow.AddHours(-1);
            DHCPv6RootScope rootScope = GetRootScope(scopeResolverMock);

            List<DomainEvent> events =
            new List<DomainEvent>{ new DHCPv6ScopeEvents.DHCPv6ScopeAddedEvent(
                new DHCPv6ScopeCreateInstruction
                {
                  AddressProperties = new DHCPv6ScopeAddressProperties(
                        IPv6Address.FromString("fe80::8"),
                        IPv6Address.FromString("fe80::8"),
                        Array.Empty<IPv6Address>(),
                        t1: DHCPv6TimeScale.FromDouble(0.5),
                        t2: DHCPv6TimeScale.FromDouble(0.75),
                        preferredLifeTime: TimeSpan.FromDays(0.5),
                        validLifeTime: TimeSpan.FromDays(1),
                        reuseAddressIfPossible: true,
                        rapitCommitEnabled: true,
                        addressAllocationStrategy: ScopeAddressProperties<DHCPv6ScopeAddressProperties, IPv6Address>.AddressAllocationStrategies.Next),
                    ResolverInformation = resolverInformations,
                    Name = "Testscope",
                    Id = scopeId,
                }),

            };

            for (int i = 0; i < 10; i++)
            {
                Guid leaseId = random.NextGuid();

                events.AddRange(new DomainEvent[]
                {
                    new DHCPv6LeaseCreatedEvent
                    {
                        EntityId = leaseId,
                        Address = leasedAddress,
                        ClientIdentifier = clientDuid,
                        IdentityAssocationId = iaId,
                        ScopeId = scopeId,
                        StartedAt = leaseCreatedAt.AddHours(-(10-i)).AddMinutes(-random.Next(3,10)),
                        ValidUntil = DateTime.UtcNow.AddDays(1),
                    },
                     new DHCPv6LeaseActivatedEvent
                     {
                         EntityId = leaseId,
                         ScopeId = scopeId,
                     },
                     new DHCPv6LeaseCanceledEvent
                     {
                         EntityId = leaseId,
                         ScopeId = scopeId,
                     }
                });
            }

            rootScope.Load(events);
            IPv6Address expectedAddress = IPv6Address.FromString("fe80::8");

            DHCPv6Packet result = rootScope.HandleSolicit(packet, GetServerPropertiesResolver());
            CheckPacket(packet, expectedAddress, iaId, result, DHCPv6PacketTypes.REPLY, DHCPv6PrefixDelegation.None);

            DHCPv6Lease lease = CheckLease(5, 6, expectedAddress, scopeId, rootScope, DateTime.UtcNow, clientDuid, iaId, false, false);
            CheckEventAmount(3, rootScope);

            CheckLeaseCreatedEvent(0, clientDuid, iaId, scopeId, rootScope, expectedAddress, lease, null);
            CheckLeaseActivatedEvent(1, scopeId, lease.Id, rootScope);
            CheckHandeledEvent(2, packet, result, rootScope, scopeId);

            CheckEmptyTrigger(rootScope);
        }

        [Fact]
        public void HandleSolicit_LeaseFound_ButPending_ReuseEnabled_ResetPrefix()
        {
            Random random = new Random();

            var packet = GetSolicitPacket(random, out DUID clientDuid, out UInt32 iaId);
            var resolverInformations = GetMockupResolver(packet, out Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>> scopeResolverMock);

            Guid scopeId = random.NextGuid();
            Guid leaseId = random.NextGuid();

            IPv6Address leasedAddress = IPv6Address.FromString("fe80::A");
            DateTime leaseCreatedAt = DateTime.UtcNow.AddHours(-1);
            DHCPv6RootScope rootScope = GetRootScope(scopeResolverMock);
            rootScope.Load(new List<DomainEvent>{ new DHCPv6ScopeEvents.DHCPv6ScopeAddedEvent(
                new DHCPv6ScopeCreateInstruction
                {
                  AddressProperties = new DHCPv6ScopeAddressProperties(
                        IPv6Address.FromString("fe80::0"),
                        IPv6Address.FromString("fe80::ff"),
                        Array.Empty<IPv6Address>(),
                        t1: DHCPv6TimeScale.FromDouble(0.5),
                        t2: DHCPv6TimeScale.FromDouble(0.75),
                        preferredLifeTime: TimeSpan.FromDays(0.5),
                        validLifeTime: TimeSpan.FromDays(1),
                        reuseAddressIfPossible: true,
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
                    StartedAt = leaseCreatedAt,
                    ValidUntil = DateTime.UtcNow.AddDays(1),
                    HasPrefixDelegation = true,
                    DelegatedNetworkAddress = IPv6Address.FromString("fe90::0"),
                    IdentityAssocationIdForPrefix = random.NextUInt32(),
                    PrefixLength = 64,
                 },
            }); ;

            IPv6Address expectedAdress = leasedAddress;

            DHCPv6Packet result = rootScope.HandleSolicit(packet, GetServerPropertiesResolver());
            CheckPacket(packet, expectedAdress, iaId, result, DHCPv6PacketTypes.ADVERTISE, DHCPv6PrefixDelegation.None);

            DHCPv6Lease lease = CheckLease(1, 2, expectedAdress, scopeId, rootScope, leaseCreatedAt, clientDuid, iaId, true, false);
            CheckEventAmount(2, rootScope);
            CheckLeaseCreatedEvent(0, clientDuid, iaId, scopeId, rootScope, expectedAdress, lease, null);
            CheckHandeledEvent(1, packet, result, rootScope, scopeId);
        }

        [Theory]
        [InlineData(DHCPv6ScopeAddressProperties.AddressAllocationStrategies.Next)]
        [InlineData(DHCPv6ScopeAddressProperties.AddressAllocationStrategies.Random)]
        public void HandleSolicit_LeaseFound_ReuseEnabled_WithRapitCommit(DHCPv6ScopeAddressProperties.AddressAllocationStrategies allocationStrategy)
        {
            Random random = new Random();

            var packet = GetSolicitPacket(random, out DUID clientDuid, out UInt32 iaId, false, new DHCPv6PacketTrueOption(DHCPv6PacketOptionTypes.RapitCommit));
            var resolverInformations = GetMockupResolver(packet, out Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>> scopeResolverMock);

            Guid scopeId = random.NextGuid();
            Guid leaseId = random.NextGuid();

            IPv6Address leasedAddress = IPv6Address.FromString("fe80::A");
            DateTime leaseCreatedAt = DateTime.UtcNow.AddHours(-1);
            DHCPv6RootScope rootScope = GetRootScope(scopeResolverMock);
            rootScope.Load(new List<DomainEvent>{ new DHCPv6ScopeEvents.DHCPv6ScopeAddedEvent(
                new DHCPv6ScopeCreateInstruction
                {
                  AddressProperties = new DHCPv6ScopeAddressProperties(
                        IPv6Address.FromString("fe80::0"),
                        IPv6Address.FromString("fe80::ff"),
                        Array.Empty<IPv6Address>(),
                        t1: DHCPv6TimeScale.FromDouble(0.5),
                        t2: DHCPv6TimeScale.FromDouble(0.75),
                        preferredLifeTime: TimeSpan.FromDays(0.5),
                        validLifeTime: TimeSpan.FromDays(1),
                        reuseAddressIfPossible: true,
                        rapitCommitEnabled: true,
                        addressAllocationStrategy: allocationStrategy),
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
                    StartedAt = leaseCreatedAt,
                    ValidUntil = DateTime.UtcNow.AddDays(1),
                 },
                 new DHCPv6LeaseActivatedEvent
                {
                    EntityId = leaseId,
                    ScopeId = scopeId,
                }
            }); ;

            IPv6Address expectedAdress = leasedAddress;

            DHCPv6Packet result = rootScope.HandleSolicit(packet, GetServerPropertiesResolver());
            CheckPacket(packet, expectedAdress, iaId, result, DHCPv6PacketTypes.REPLY, DHCPv6PrefixDelegation.None);

            DHCPv6Lease lease = CheckLease(1, 2, expectedAdress, scopeId, rootScope, leaseCreatedAt, clientDuid, iaId, false, false);
            CheckEventAmount(4, rootScope);

            CheckLeaseCreatedEvent(0, clientDuid, iaId, scopeId, rootScope, expectedAdress, lease, null, leaseId);
            CheckLeaseActivatedEvent(1, scopeId, lease.Id, rootScope);
            CheckRevokedEvent(2, scopeId, leaseId, rootScope);

            CheckHandeledEvent(3, packet, result, rootScope, scopeId);

            CheckEmptyTrigger(rootScope);
        }

        [Fact]
        public void HandleSolit_LeaseFound_ReuseNotEnabled()
        {
            Random random = new Random();

            var packet = GetSolicitPacket(random, out DUID clientDuid, out UInt32 iaId);
            var resolverInformations = GetMockupResolver(packet, out Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>> scopeResolverMock);

            Guid scopeId = random.NextGuid();
            Guid leaseId = random.NextGuid();

            IPv6Address leasedAddress = IPv6Address.FromString("fe80::A");
            IPv6Address expectedAddress = IPv6Address.FromString("fe80::2");

            DateTime leaseCreatedAt = DateTime.UtcNow.AddHours(-1);
            DHCPv6RootScope rootScope = GetRootScope(scopeResolverMock);
            rootScope.Load(new List<DomainEvent>{ new DHCPv6ScopeEvents.DHCPv6ScopeAddedEvent(
                new DHCPv6ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv6ScopeAddressProperties(
                        IPv6Address.FromString("fe80::1"),
                        IPv6Address.FromString("fe80::FF"),
                        new List<IPv6Address>{
                            IPv6Address.FromString("fe80::1")
                        },
                        t1: DHCPv6TimeScale.FromDouble(0.5),
                        t2: DHCPv6TimeScale.FromDouble(0.75),
                        preferredLifeTime: TimeSpan.FromDays(0.5),
                        validLifeTime: TimeSpan.FromDays(1),
                        reuseAddressIfPossible: false,
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
                    StartedAt = leaseCreatedAt,
                    ValidUntil = DateTime.UtcNow.AddDays(1),
                 },
                new DHCPv6LeaseActivatedEvent
                {
                    EntityId = leaseId,
                    ScopeId = scopeId,
                 },
            });

            DHCPv6Packet result = rootScope.HandleSolicit(packet, GetServerPropertiesResolver());
            CheckPacket(packet, expectedAddress, iaId, result, DHCPv6PacketTypes.ADVERTISE, DHCPv6PrefixDelegation.None);

            DHCPv6Lease lease = CheckLease(1, 2, expectedAddress, scopeId, rootScope, DateTime.UtcNow, clientDuid, iaId, true, false);

            var oldLease = rootScope.GetScopeById(scopeId).Leases.GetLeaseById(leaseId);
            Assert.True(oldLease.IsActive());
            Assert.Equal(LeaseStates.Active, oldLease.State);

            CheckEventAmount(2, rootScope);

            CheckLeaseCreatedEvent(0, clientDuid, iaId, scopeId, rootScope, expectedAddress, lease, null, leaseId);

            CheckHandeledEvent(1, packet, result, rootScope, scopeId);

            CheckEmptyTrigger(rootScope);
        }

        [Fact]
        public void HandleSolit_LeaseFound_ReuseNotEnabled_WithRapitCommit()
        {
            Random random = new Random();

            var packet = GetSolicitPacket(random, out DUID clientDuid, out UInt32 iaId, false, new DHCPv6PacketTrueOption(DHCPv6PacketOptionTypes.RapitCommit));
            var resolverInformations = GetMockupResolver(packet, out Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>> scopeResolverMock);

            Guid scopeId = random.NextGuid();
            Guid leaseId = random.NextGuid();

            IPv6Address leasedAddress = IPv6Address.FromString("fe80::A");
            IPv6Address expectedAddress = IPv6Address.FromString("fe80::2");

            DateTime leaseCreatedAt = DateTime.UtcNow.AddHours(-1);
            DHCPv6RootScope rootScope = GetRootScope(scopeResolverMock);
            rootScope.Load(new List<DomainEvent>{ new DHCPv6ScopeEvents.DHCPv6ScopeAddedEvent(
                new DHCPv6ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv6ScopeAddressProperties(
                        IPv6Address.FromString("fe80::1"),
                        IPv6Address.FromString("fe80::FF"),
                        new List<IPv6Address>{
                            IPv6Address.FromString("fe80::1")

                        },
                        t1: DHCPv6TimeScale.FromDouble(0.5),
                        t2: DHCPv6TimeScale.FromDouble(0.75),
                        preferredLifeTime: TimeSpan.FromDays(0.5),
                        validLifeTime: TimeSpan.FromDays(1),
                        reuseAddressIfPossible: false,
                        rapitCommitEnabled: true,
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
                    StartedAt = leaseCreatedAt,
                    ValidUntil = DateTime.UtcNow.AddDays(1),
                 },
                 new DHCPv6LeaseActivatedEvent
                {
                    EntityId = leaseId,
                    ScopeId = scopeId,
                 },
            });

            DHCPv6Packet result = rootScope.HandleSolicit(packet, GetServerPropertiesResolver());
            CheckPacket(packet, expectedAddress, iaId, result, DHCPv6PacketTypes.REPLY, DHCPv6PrefixDelegation.None);

            DHCPv6Lease lease = CheckLease(1, 2, expectedAddress, scopeId, rootScope, DateTime.UtcNow, clientDuid, iaId, false, false);

            var revokedLease = rootScope.GetScopeById(scopeId).Leases.GetLeaseById(leaseId);
            Assert.False(revokedLease.IsActive());
            Assert.Equal(LeaseStates.Revoked, revokedLease.State);

            CheckEventAmount(4, rootScope);

            CheckLeaseCreatedEvent(0, clientDuid, iaId, scopeId, rootScope, expectedAddress, lease, null, leaseId);
            CheckLeaseActivatedEvent(1, scopeId, lease.Id, rootScope);
            CheckRevokedEvent(2, scopeId, leaseId, rootScope);

            CheckHandeledEvent(3, packet, result, rootScope, scopeId);

            CheckEmptyTrigger(rootScope);
        }

        [Fact]
        public void HandleSolicit_LeaseFound_ReuseNotEnabled_ExcludedFormerLeaseAddress()
        {
            Random random = new Random();

            var packet = GetSolicitPacket(random, out DUID clientDuid, out UInt32 iaId);
            var resolverInformations = GetMockupResolver(packet, out Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>> scopeResolverMock);

            Guid scopeId = random.NextGuid();
            Guid leaseId = random.NextGuid();

            IPv6Address leasedAddress = IPv6Address.FromString("fe80::2");
            IPv6Address expectedAddress = IPv6Address.FromString("fe80::3");

            DateTime leaseCreatedAt = DateTime.UtcNow.AddHours(-1);
            DHCPv6RootScope rootScope = GetRootScope(scopeResolverMock);
            rootScope.Load(new List<DomainEvent>{ new DHCPv6ScopeEvents.DHCPv6ScopeAddedEvent(
                new DHCPv6ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv6ScopeAddressProperties(
                        IPv6Address.FromString("fe80::1"),
                        IPv6Address.FromString("fe80::FF"),
                        new List<IPv6Address>{
                            IPv6Address.FromString("fe80::1")
                        },
                        t1: DHCPv6TimeScale.FromDouble(0.5),
                        t2: DHCPv6TimeScale.FromDouble(0.75),
                        preferredLifeTime: TimeSpan.FromDays(0.5),
                        validLifeTime: TimeSpan.FromDays(1),
                        reuseAddressIfPossible: false,
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
                    StartedAt = leaseCreatedAt,
                    ValidUntil = DateTime.UtcNow.AddDays(1),
                 },
                new DHCPv6LeaseActivatedEvent
                {
                    EntityId = leaseId,
                    ScopeId = scopeId,
                }
            });

            DHCPv6Packet result = rootScope.HandleSolicit(packet, GetServerPropertiesResolver());
            CheckPacket(packet, expectedAddress, iaId, result, DHCPv6PacketTypes.ADVERTISE, DHCPv6PrefixDelegation.None);

            DHCPv6Lease lease = CheckLease(1, 2, expectedAddress, scopeId, rootScope, DateTime.UtcNow, clientDuid, iaId, true, false);

            var revokedLease = rootScope.GetScopeById(scopeId).Leases.GetLeaseById(leaseId);
            Assert.True(revokedLease.IsActive());
            Assert.Equal(LeaseStates.Active, revokedLease.State);

            CheckEventAmount(2, rootScope);

            CheckLeaseCreatedEvent(0, clientDuid, iaId, scopeId, rootScope, expectedAddress, lease, null, leaseId);
            CheckHandeledEvent(1, packet, result, rootScope, scopeId);

            CheckEmptyTrigger(rootScope);
        }

        [Fact]
        public void HandleSolitcit_LeaseFoundByUniqueIdentifier_NoReuse()
        {
            Random random = new Random();
            Byte[] uniqueIdentifier = random.NextBytes(20);

            var packet = GetSolicitPacket(random, out DUID newClientDuid, out UInt32 newIaId);
            var resolverInformations = GetMockupResolver(packet, out Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>> scopeResolverMock, uniqueIdentifier);

            Guid scopeId = random.NextGuid();
            Guid leaseId = random.NextGuid();

            UInt32 oldIaId = random.NextUInt32();
            DUID oldClientId = new UUIDDUID(Guid.NewGuid());

            IPv6Address leasedAddress = IPv6Address.FromString("fe::3");
            IPv6Address expectedAddress = IPv6Address.FromString("fe::4");

            DateTime leaseCreatedAt = DateTime.UtcNow.AddHours(-1);
            DHCPv6RootScope rootScope = GetRootScope(scopeResolverMock);
            rootScope.Load(new List<DomainEvent>{ new DHCPv6ScopeEvents.DHCPv6ScopeAddedEvent(
                new DHCPv6ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv6ScopeAddressProperties(
                        IPv6Address.FromString("fe::1"),
                        IPv6Address.FromString("fe::FF"),
                        new List<IPv6Address>{
                            IPv6Address.FromString("fe::1"),
                            IPv6Address.FromString("fe::2"),
                        },
                        t1: DHCPv6TimeScale.FromDouble(0.5),
                        t2: DHCPv6TimeScale.FromDouble(0.75),
                        preferredLifeTime: TimeSpan.FromDays(0.5),
                        validLifeTime: TimeSpan.FromDays(1),
                        reuseAddressIfPossible: false,
                        addressAllocationStrategy: DHCPv6ScopeAddressProperties.AddressAllocationStrategies.Next),
                    ResolverInformation = resolverInformations,
                    Name = "Testscope",
                    Id = scopeId,
                }),
                new DHCPv6LeaseCreatedEvent
                {
                    EntityId = leaseId,
                    Address = leasedAddress,
                    ClientIdentifier = oldClientId,
                    IdentityAssocationId = oldIaId,
                    ScopeId = scopeId,
                    UniqueIdentiifer = uniqueIdentifier,
                    StartedAt = leaseCreatedAt,
                    ValidUntil = DateTime.UtcNow.AddDays(1),
                 },
                new DHCPv6LeaseActivatedEvent
                {
                    EntityId = leaseId,
                    ScopeId = scopeId,
                 },
            }); ;

            DHCPv6Packet result = rootScope.HandleSolicit(packet, GetServerPropertiesResolver());
            CheckPacket(packet, expectedAddress, newIaId, result, DHCPv6PacketTypes.ADVERTISE, DHCPv6PrefixDelegation.None);

            DHCPv6Lease lease = CheckLease(1, 2, expectedAddress, scopeId, rootScope, DateTime.UtcNow, newClientDuid, newIaId, true, false, uniqueIdentifier);

            var oldLease = rootScope.GetScopeById(scopeId).Leases.GetLeaseById(leaseId);
            Assert.True(oldLease.IsActive());
            Assert.Equal(LeaseStates.Active, oldLease.State);

            CheckEventAmount(2, rootScope);

            CheckLeaseCreatedEvent(0, newClientDuid, newIaId, scopeId, rootScope, expectedAddress, lease, uniqueIdentifier, leaseId);
            CheckHandeledEvent(1, packet, result, rootScope, scopeId);

            CheckEmptyTrigger(rootScope);
        }

        [Fact]
        public void HandleSolitcit_LeaseFoundByUniqueIdentifier_NoReuse_WithRappitCommit()
        {
            Random random = new Random();
            Byte[] uniqueIdentifier = random.NextBytes(20);

            var packet = GetSolicitPacket(random, out DUID newClientDuid, out UInt32 newIaId, false, new DHCPv6PacketTrueOption(DHCPv6PacketOptionTypes.RapitCommit));
            var resolverInformations = GetMockupResolver(packet, out Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>> scopeResolverMock, uniqueIdentifier);

            Guid scopeId = random.NextGuid();
            Guid leaseId = random.NextGuid();

            UInt32 oldIaId = random.NextUInt32();
            DUID oldClientId = new UUIDDUID(Guid.NewGuid());

            IPv6Address leasedAddress = IPv6Address.FromString("fe::3");
            IPv6Address expectedAddress = IPv6Address.FromString("fe::4");

            DateTime leaseCreatedAt = DateTime.UtcNow.AddHours(-1);
            DHCPv6RootScope rootScope = GetRootScope(scopeResolverMock);
            rootScope.Load(new List<DomainEvent>{ new DHCPv6ScopeEvents.DHCPv6ScopeAddedEvent(
                new DHCPv6ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv6ScopeAddressProperties(
                        IPv6Address.FromString("fe::1"),
                        IPv6Address.FromString("fe::FF"),
                        new List<IPv6Address>{
                            IPv6Address.FromString("fe::1"),
                            IPv6Address.FromString("fe::2"),
                        },
                        t1: DHCPv6TimeScale.FromDouble(0.5),
                        t2: DHCPv6TimeScale.FromDouble(0.75),
                        preferredLifeTime: TimeSpan.FromDays(0.5),
                        validLifeTime: TimeSpan.FromDays(1),
                        reuseAddressIfPossible: false,
                        rapitCommitEnabled: true,
                        addressAllocationStrategy: DHCPv6ScopeAddressProperties.AddressAllocationStrategies.Next),
                    ResolverInformation = resolverInformations,
                    Name = "Testscope",
                    Id = scopeId,
                }),
                new DHCPv6LeaseCreatedEvent
                {
                    EntityId = leaseId,
                    Address = leasedAddress,
                    ClientIdentifier = oldClientId,
                    IdentityAssocationId = oldIaId,
                    ScopeId = scopeId,
                    UniqueIdentiifer = uniqueIdentifier,
                    StartedAt = leaseCreatedAt,
                    ValidUntil = DateTime.UtcNow.AddDays(1),
                 },
                 new DHCPv6LeaseActivatedEvent
                {
                    EntityId = leaseId,
                    ScopeId = scopeId,
                 },
            }); ;

            DHCPv6Packet result = rootScope.HandleSolicit(packet, GetServerPropertiesResolver());
            CheckPacket(packet, expectedAddress, newIaId, result, DHCPv6PacketTypes.REPLY, DHCPv6PrefixDelegation.None);

            DHCPv6Lease lease = CheckLease(1, 2, expectedAddress, scopeId, rootScope, DateTime.UtcNow, newClientDuid, newIaId, false, false, uniqueIdentifier);

            var revokedLease = rootScope.GetScopeById(scopeId).Leases.GetLeaseById(leaseId);
            Assert.False(revokedLease.IsActive());
            Assert.Equal(LeaseStates.Revoked, revokedLease.State);

            CheckEventAmount(4, rootScope);

            CheckLeaseCreatedEvent(0, newClientDuid, newIaId, scopeId, rootScope, expectedAddress, lease, uniqueIdentifier, leaseId);
            CheckLeaseActivatedEvent(1, scopeId, lease.Id, rootScope);
            CheckRevokedEvent(2, scopeId, leaseId, rootScope);

            CheckHandeledEvent(3, packet, result, rootScope, scopeId);

            CheckEmptyTrigger(rootScope);
        }

        [Fact]
        public void HandleSolicit_LeaseFoundByUniqueIdentifier_Reuse()
        {
            Random random = new Random();
            Byte[] uniqueIdentifier = random.NextBytes(20);

            var packet = GetSolicitPacket(random, out DUID newClientDuid, out UInt32 newIaId);
            var resolverInformations = GetMockupResolver(packet, out Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>> scopeResolverMock, uniqueIdentifier);

            Guid scopeId = random.NextGuid();
            Guid leaseId = random.NextGuid();

            UInt32 oldIaId = random.NextUInt32();
            DUID oldClientId = new UUIDDUID(Guid.NewGuid());

            IPv6Address leasedAddress = IPv6Address.FromString("fe::3");
            IPv6Address expectedAddress = leasedAddress;

            DateTime leaseCreatedAt = DateTime.UtcNow.AddHours(-1);
            DHCPv6RootScope rootScope = GetRootScope(scopeResolverMock);
            rootScope.Load(new List<DomainEvent>{ new DHCPv6ScopeEvents.DHCPv6ScopeAddedEvent(
                new DHCPv6ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv6ScopeAddressProperties(
                        IPv6Address.FromString("fe::1"),
                        IPv6Address.FromString("fe::FF"),
                        new List<IPv6Address>{
                            IPv6Address.FromString("fe::1"),
                            IPv6Address.FromString("fe::2"),
                        },
                        t1: DHCPv6TimeScale.FromDouble(0.5),
                        t2: DHCPv6TimeScale.FromDouble(0.75),
                        preferredLifeTime: TimeSpan.FromDays(0.5),
                        validLifeTime: TimeSpan.FromDays(1),
                        reuseAddressIfPossible: true,
                        rapitCommitEnabled: true,
                        supportDirectUnicast: true,
                        addressAllocationStrategy: DHCPv6ScopeAddressProperties.AddressAllocationStrategies.Next),
                    ResolverInformation = resolverInformations,
                    Name = "Testscope",
                    Id = scopeId,
                }),
                new DHCPv6LeaseCreatedEvent
                {
                    EntityId = leaseId,
                    Address = leasedAddress,
                    ClientIdentifier = oldClientId,
                    IdentityAssocationId = oldIaId,
                    ScopeId = scopeId,
                    UniqueIdentiifer = uniqueIdentifier,
                    StartedAt = leaseCreatedAt,
                    ValidUntil = DateTime.UtcNow.AddDays(1),
                 },
                new DHCPv6LeaseActivatedEvent
                {
                    EntityId = leaseId,
                    ScopeId = scopeId,
                 },
            }); ;

            DHCPv6Packet result = rootScope.HandleSolicit(packet, GetServerPropertiesResolver());
            CheckPacket(packet, expectedAddress, newIaId, result, DHCPv6PacketTypes.ADVERTISE, DHCPv6PrefixDelegation.None);

            DHCPv6Lease lease = CheckLease(1, 2, expectedAddress, scopeId, rootScope, DateTime.UtcNow, newClientDuid, newIaId, true, false, uniqueIdentifier);

            var oldLease = rootScope.GetScopeById(scopeId).Leases.GetLeaseById(leaseId);
            Assert.True(oldLease.IsActive());
            Assert.Equal(LeaseStates.Active, oldLease.State);
            Assert.True(lease.HasAncestor());
            Assert.True(lease.AncestorId.HasValue);
            Assert.Equal(lease.AncestorId.Value, oldLease.Id);

            CheckEventAmount(2, rootScope);

            CheckLeaseCreatedEvent(0, newClientDuid, newIaId, scopeId, rootScope, expectedAddress, lease, uniqueIdentifier, leaseId);

            CheckHandeledEvent(1, packet, result, rootScope, scopeId);

            CheckEmptyTrigger(rootScope);
        }

        [Fact]
        public void HandleSolicit_LeaseFoundByUniqueIdentifier_Reuse_WithRapitCommt()
        {
            Random random = new Random();
            Byte[] uniqueIdentifier = random.NextBytes(20);

            var packet = GetSolicitPacket(random, out DUID newClientDuid, out UInt32 newIaId, false, new DHCPv6PacketTrueOption(DHCPv6PacketOptionTypes.RapitCommit));
            var resolverInformations = GetMockupResolver(packet, out Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>> scopeResolverMock, uniqueIdentifier);

            Guid scopeId = random.NextGuid();
            Guid leaseId = random.NextGuid();

            UInt32 oldIaId = random.NextUInt32();
            DUID oldClientId = new UUIDDUID(Guid.NewGuid());

            IPv6Address leasedAddress = IPv6Address.FromString("fe::3");
            IPv6Address expectedAddress = leasedAddress;

            DateTime leaseCreatedAt = DateTime.UtcNow.AddHours(-1);
            DHCPv6RootScope rootScope = GetRootScope(scopeResolverMock);
            rootScope.Load(new List<DomainEvent>{ new DHCPv6ScopeEvents.DHCPv6ScopeAddedEvent(
                new DHCPv6ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv6ScopeAddressProperties(
                        IPv6Address.FromString("fe::1"),
                        IPv6Address.FromString("fe::FF"),
                        new List<IPv6Address>{
                            IPv6Address.FromString("fe::1"),
                            IPv6Address.FromString("fe::2"),
                        },
                        t1: DHCPv6TimeScale.FromDouble(0.5),
                        t2: DHCPv6TimeScale.FromDouble(0.75),
                        preferredLifeTime: TimeSpan.FromDays(0.5),
                        validLifeTime: TimeSpan.FromDays(1),
                        reuseAddressIfPossible: true,
                        rapitCommitEnabled: true,
                        addressAllocationStrategy: DHCPv6ScopeAddressProperties.AddressAllocationStrategies.Next),
                    ResolverInformation = resolverInformations,
                    Name = "Testscope",
                    Id = scopeId,
                }),
                new DHCPv6LeaseCreatedEvent
                {
                    EntityId = leaseId,
                    Address = leasedAddress,
                    ClientIdentifier = oldClientId,
                    IdentityAssocationId = oldIaId,
                    ScopeId = scopeId,
                    UniqueIdentiifer = uniqueIdentifier,
                    StartedAt = leaseCreatedAt,
                    ValidUntil = DateTime.UtcNow.AddDays(1),
                 },
                 new DHCPv6LeaseActivatedEvent
                {
                    EntityId = leaseId,
                    ScopeId = scopeId,
                 },
            }); ;

            DHCPv6Packet result = rootScope.HandleSolicit(packet, GetServerPropertiesResolver());
            CheckPacket(packet, expectedAddress, newIaId, result, DHCPv6PacketTypes.REPLY, DHCPv6PrefixDelegation.None);

            DHCPv6Lease lease = CheckLease(1, 2, expectedAddress, scopeId, rootScope, DateTime.UtcNow, newClientDuid, newIaId, false, false, uniqueIdentifier);

            var oldLease = rootScope.GetScopeById(scopeId).Leases.GetLeaseById(leaseId);
            Assert.False(oldLease.IsActive());
            Assert.Equal(LeaseStates.Revoked, oldLease.State);

            CheckEventAmount(4, rootScope);

            CheckLeaseCreatedEvent(0, newClientDuid, newIaId, scopeId, rootScope, expectedAddress, lease, uniqueIdentifier, leaseId);
            CheckLeaseActivatedEvent(1, scopeId, lease.Id, rootScope);
            CheckRevokedEvent(2, scopeId, leaseId, rootScope);

            CheckHandeledEvent(3, packet, result, rootScope, scopeId);

            CheckEmptyTrigger(rootScope);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void HandleSolicit_HasUniqueIdentifier_ButNoLeaseFound(Boolean reuseAddress)
        {
            Random random = new Random();
            Byte[] uniqueIdentifier = random.NextBytes(20);

            var packet = GetSolicitPacket(random, out DUID newClientDuid, out UInt32 newIaId);
            var resolverInformations = GetMockupResolver(packet, out Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>> scopeResolverMock, uniqueIdentifier);

            Guid scopeId = random.NextGuid();
            Guid leaseId = random.NextGuid();

            UInt32 oldIaId = random.NextUInt32();
            DUID oldClientId = new UUIDDUID(Guid.NewGuid());

            IPv6Address leasedAddress = IPv6Address.FromString("fe::3");
            IPv6Address expectedAddress = IPv6Address.FromString("fe::4");

            DateTime leaseCreatedAt = DateTime.UtcNow.AddHours(-1);
            DHCPv6RootScope rootScope = GetRootScope(scopeResolverMock);
            rootScope.Load(new List<DomainEvent>{ new DHCPv6ScopeEvents.DHCPv6ScopeAddedEvent(
                new DHCPv6ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv6ScopeAddressProperties(
                        IPv6Address.FromString("fe::1"),
                        IPv6Address.FromString("fe::FF"),
                        new List<IPv6Address>{
                            IPv6Address.FromString("fe::1"),
                            IPv6Address.FromString("fe::2"),
                        },
                        t1: DHCPv6TimeScale.FromDouble(0.5),
                        t2: DHCPv6TimeScale.FromDouble(0.75),
                        preferredLifeTime: TimeSpan.FromDays(0.5),
                        validLifeTime: TimeSpan.FromDays(1),
                        reuseAddressIfPossible: reuseAddress,
                        addressAllocationStrategy: DHCPv6ScopeAddressProperties.AddressAllocationStrategies.Next),
                    ResolverInformation = resolverInformations,
                    Name = "Testscope",
                    Id = scopeId,
                }),
                new DHCPv6LeaseCreatedEvent
                {
                    EntityId = leaseId,
                    Address = leasedAddress,
                    ClientIdentifier = oldClientId,
                    IdentityAssocationId = oldIaId,
                    ScopeId = scopeId,
                    UniqueIdentiifer = random.NextBytes(20),
                    StartedAt = leaseCreatedAt,
                    ValidUntil = DateTime.UtcNow.AddDays(1),
                 },
            }); ;

            DHCPv6Packet result = rootScope.HandleSolicit(packet, GetServerPropertiesResolver());
            CheckPacket(packet, expectedAddress, newIaId, result, DHCPv6PacketTypes.ADVERTISE, DHCPv6PrefixDelegation.None);

            DHCPv6Lease lease = CheckLease(1, 2, expectedAddress, scopeId, rootScope, DateTime.UtcNow, newClientDuid, newIaId, true, false, uniqueIdentifier);

            var firstLease = rootScope.GetScopeById(scopeId).Leases.GetLeaseById(leaseId);
            Assert.True(firstLease.IsPending());

            CheckEventAmount(2, rootScope);

            CheckLeaseCreatedEvent(0, newClientDuid, newIaId, scopeId, rootScope, expectedAddress, lease, uniqueIdentifier);
            CheckHandeledEvent(1, packet, result, rootScope, scopeId);

            CheckEmptyTrigger(rootScope);
        }

        [Theory]
        [InlineData(DHCPv6ScopeAddressProperties.AddressAllocationStrategies.Next)]
        [InlineData(DHCPv6ScopeAddressProperties.AddressAllocationStrategies.Random)]
        public void HandleSolicit_NoLeaseFound_NoAddressAvaiable(
           DHCPv6ScopeAddressProperties.AddressAllocationStrategies allocationStrategy)
        {
            Random random = new Random();

            var packet = GetSolicitPacket(random, out DUID _, out UInt32 _);
            var resolverInformations = GetMockupResolver(packet, out Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>> scopeResolverMock);

            Guid scopeId = random.NextGuid();

            Byte avaiableAddress = random.NextByte();
            List<IPv6Address> excludedAddress = new List<IPv6Address>(255);
            for (int i = 0; i <= Byte.MaxValue; i++)
            {
                if (i != avaiableAddress)
                {
                    excludedAddress.Add(IPv6Address.FromByteArray(new byte[16] { 254, 128, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, (Byte)i }));
                }
            }

            DHCPv6RootScope rootScope = GetRootScope(scopeResolverMock);
            rootScope.Load(new List<DomainEvent>{
                new DHCPv6ScopeEvents.DHCPv6ScopeAddedEvent(
                new DHCPv6ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv6ScopeAddressProperties(
                        IPv6Address.FromString("fe80::0"),
                        IPv6Address.FromString("fe80::ff"),
                        excludedAddress,
                        preferredLifeTime: TimeSpan.FromDays(0.5),
                        validLifeTime: TimeSpan.FromDays(1),
                        addressAllocationStrategy: allocationStrategy),
                    ResolverInformation = resolverInformations,
                    Name = "Testscope",
                    Id = scopeId,
                }),
                new DHCPv6LeaseCreatedEvent
                {
                    EntityId = random.NextGuid(),
                    Address = IPv6Address.FromByteArray(new byte[16] { 254, 128, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, (Byte)avaiableAddress }),
                    ClientIdentifier = new UUIDDUID(Guid.NewGuid()),
                    IdentityAssocationId = random.NextUInt32(),
                    ScopeId = scopeId,
                    StartedAt = DateTime.UtcNow,
                    ValidUntil = DateTime.UtcNow.AddDays(1),
                 }
            });

            DHCPv6Packet result = rootScope.HandleSolicit(packet, GetServerPropertiesResolver());
            Assert.Equal(DHCPv6Packet.Empty, result);

            CheckEventAmount(2, rootScope);
            DHCPv6ScopeAddressesAreExhaustedEvent(0, rootScope, scopeId);
            CheckHandeledEventNotSuccessfull(1, packet, rootScope, scopeId);

            CheckEmptyTrigger(rootScope);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void HandleSolicitWithPrefixDelegation_LeaseFound_PrefixDelegationAvaiable(Boolean rapitCommitEnabled)
        {
            Random random = new Random();

            DHCPv6PacketOption[] options = Array.Empty<DHCPv6PacketOption>();
            if (rapitCommitEnabled == true)
            {
                options = new DHCPv6PacketOption[] {
                new DHCPv6PacketTrueOption(DHCPv6PacketOptionTypes.RapitCommit)
                };
            }

            var packet = GetSolicitPacketWithPrefixIdOnly(random, out DUID clientDuid, out UInt32 prefixIaId, options);
            var resolverInformations = GetMockupResolver(packet, out Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>> scopeResolverMock);

            Guid scopeId = random.NextGuid();
            Guid leaseId = random.NextGuid();

            IPv6Address leasedAddress = random.GetIPv6Address();

            Byte leasedPrefixLength = 58;
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
                        rapitCommitEnabled: rapitCommitEnabled,
                        addressAllocationStrategy: DHCPv6ScopeAddressProperties.AddressAllocationStrategies.Next,
                        prefixDelgationInfo: DHCPv6PrefixDelgationInfo.FromValues(IPv6Address.FromString("fd80::0"),new IPv6SubnetMaskIdentifier(56),new IPv6SubnetMaskIdentifier(leasedPrefixLength))
                        ),
                    ResolverInformation = resolverInformations,
                    Name = "Testscope",
                    Id = scopeId,
                }),
                new DHCPv6LeaseCreatedEvent
                {
                    EntityId = leaseId,
                    ScopeId = scopeId,
                    Address = leasedAddress,
                    HasPrefixDelegation = false,
                    ClientIdentifier = clientDuid,
                    StartedAt = DateTime.UtcNow.AddMinutes(-10)
                },
                new DHCPv6LeaseActivatedEvent
                {
                    EntityId = leaseId,
                    ScopeId = scopeId,
                }
            });

            DHCPv6Packet result = rootScope.HandleSolicit(packet, GetServerPropertiesResolver());
            DHCPv6Lease lease = CheckLeaseForPrefix(0, 1, IPv6Address.FromString("fd80::0"), IPv6Address.FromString("fd80:0:0:c0::0"), scopeId, rootScope, clientDuid, prefixIaId, leasedPrefixLength, false);

            CheckPrefixPacket(result, rapitCommitEnabled == true ? DHCPv6PacketTypes.REPLY : DHCPv6PacketTypes.ADVERTISE, lease.PrefixDelegation);

            CheckEventAmount(2, rootScope);
            CheckDHCPv6LeasePrefixAddedEvent(0, prefixIaId, scopeId, rootScope, lease);
            CheckHandeledEvent(1, packet, result, rootScope, scopeId);

            if (rapitCommitEnabled == true)
            {
                var trigger = CheckTrigger<PrefixEdgeRouterBindingUpdatedTrigger>(rootScope);
                Assert.Equal(scopeId, trigger.ScopeId);
                Assert.Null(trigger.OldBinding);
                Assert.NotNull(trigger.NewBinding);

                Assert.Equal(lease.PrefixDelegation.NetworkAddress, trigger.NewBinding.Prefix);
                Assert.Equal(leasedPrefixLength, trigger.NewBinding.Mask.Identifier);
                Assert.Equal(leasedAddress, trigger.NewBinding.Host);
            }
            else
            {
                CheckEmptyTrigger(rootScope);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void HandleSolicitWithPrefixDelegation_NoLeaseFound_PrefixDelegationAvaiable(Boolean rapitCommitEnabled)
        {
            Random random = new Random();

            DHCPv6PacketOption[] options = Array.Empty<DHCPv6PacketOption>();
            if (rapitCommitEnabled == true)
            {
                options = new DHCPv6PacketOption[] {
                new DHCPv6PacketTrueOption(DHCPv6PacketOptionTypes.RapitCommit)
                };
            }

            var packet = GetSolicitPacketWithPrefixIdOnly(random, out DUID clientDuid, out UInt32 prefixIaId, options);
            var resolverInformations = GetMockupResolver(packet, out Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>> scopeResolverMock);

            Guid scopeId = random.NextGuid();
            Guid leaseId = random.NextGuid();

            IPv6Address leasedAddress = random.GetIPv6Address();

            Byte leasedPrefixLength = 58;
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
                        rapitCommitEnabled: rapitCommitEnabled,
                        addressAllocationStrategy: DHCPv6ScopeAddressProperties.AddressAllocationStrategies.Random,
                        prefixDelgationInfo: DHCPv6PrefixDelgationInfo.FromValues(IPv6Address.FromString("fd80::0"),new IPv6SubnetMaskIdentifier(56),new IPv6SubnetMaskIdentifier(leasedPrefixLength))
                        ),
                    ResolverInformation = resolverInformations,
                    Name = "Testscope",
                    Id = scopeId,
                }),
                new DHCPv6LeaseCreatedEvent
                {
                    EntityId = leaseId,
                    ScopeId = scopeId,
                    Address = leasedAddress,
                    HasPrefixDelegation = false,
                    ClientIdentifier = clientDuid,
                    StartedAt = DateTime.UtcNow.AddMinutes(-10)
                },
                new DHCPv6LeaseActivatedEvent
                {
                    EntityId = leaseId,
                    ScopeId = scopeId,
                }
            });

            DHCPv6Packet result = rootScope.HandleSolicit(packet, GetServerPropertiesResolver());
            DHCPv6Lease lease = CheckLeaseForPrefix(0, 1, IPv6Address.FromString("fd80::0"), IPv6Address.FromString("fd80:0:0:c0::0"), scopeId, rootScope, clientDuid, prefixIaId, leasedPrefixLength, false);

            CheckPrefixPacket(result, rapitCommitEnabled == true ? DHCPv6PacketTypes.REPLY : DHCPv6PacketTypes.ADVERTISE, lease.PrefixDelegation);

            CheckEventAmount(2, rootScope);
            CheckDHCPv6LeasePrefixAddedEvent(0, prefixIaId, scopeId, rootScope, lease);
            CheckHandeledEvent(1, packet, result, rootScope, scopeId);

            if (rapitCommitEnabled == true)
            {
                var trigger = CheckTrigger<PrefixEdgeRouterBindingUpdatedTrigger>(rootScope);
                Assert.Equal(scopeId, trigger.ScopeId);
                Assert.Null(trigger.OldBinding);
                Assert.NotNull(trigger.NewBinding);

                Assert.Equal(lease.PrefixDelegation.NetworkAddress, trigger.NewBinding.Prefix);
                Assert.Equal(leasedPrefixLength, trigger.NewBinding.Mask.Identifier);
                Assert.Equal(leasedAddress, trigger.NewBinding.Host);
            }
            else
            {
                CheckEmptyTrigger(rootScope);
            }
        }

        private class CustomDHCPv6RootScope : DHCPv6RootScope
        {
            private Timer _timer;

            public CustomDHCPv6RootScope(Guid id, IScopeResolverManager<DHCPv6Packet, IPv6Address> resolverManager,
            ILoggerFactory factory) : base(id, resolverManager, factory)
            {
            }

            public void ProcessEventsDeferred(IList<DomainEvent> events, TimeSpan span)
            {
                Int32 index = 0;
                _timer = new Timer((state) =>
                {
                    if (index >= events.Count)
                    {
                        _timer.Dispose();
                    }
                    else
                    {
                        base.Load(new[] { events[index++] });
                    }
                }, index, span, span);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void HandleSolicitWithPrefixDelegation_LeaseNotImmediatelyAvailable_PrefixDelegationAvaiable(Boolean rapitCommitEnabled)
        {
            Random random = new Random();

            DHCPv6PacketOption[] options = Array.Empty<DHCPv6PacketOption>();
            if (rapitCommitEnabled == true)
            {
                options = new DHCPv6PacketOption[] {
                new DHCPv6PacketTrueOption(DHCPv6PacketOptionTypes.RapitCommit)
                };
            }

            var packet = GetSolicitPacketWithPrefixIdOnly(random, out DUID clientDuid, out UInt32 prefixIaId, options);
            var resolverInformations = GetMockupResolver(packet, out Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>> scopeResolverMock);

            Mock<ILoggerFactory> factoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);
            factoryMock.Setup(x => x.CreateLogger(It.IsAny<String>())).Returns(Mock.Of<ILogger<DHCPv6RootScope>>());

            var rootScope = new CustomDHCPv6RootScope(Guid.NewGuid(), scopeResolverMock.Object, factoryMock.Object);

            Guid scopeId = random.NextGuid();
            Guid leaseId = random.NextGuid();

            IPv6Address leasedAddress = random.GetIPv6Address();

            Byte leasedPrefixLength = 58;
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
                        rapitCommitEnabled: rapitCommitEnabled,
                        addressAllocationStrategy: DHCPv6ScopeAddressProperties.AddressAllocationStrategies.Random,
                        prefixDelgationInfo: DHCPv6PrefixDelgationInfo.FromValues(IPv6Address.FromString("fd80::0"),new IPv6SubnetMaskIdentifier(56),new IPv6SubnetMaskIdentifier(leasedPrefixLength))
                        ),
                    ResolverInformation = resolverInformations,
                    Name = "Testscope",
                    Id = scopeId,
                }),

            });

            rootScope.ProcessEventsDeferred(new List<DomainEvent> {
             new DHCPv6LeaseCreatedEvent
                {
                    EntityId = leaseId,
                    ScopeId = scopeId,
                    Address = leasedAddress,
                    HasPrefixDelegation = false,
                    ClientIdentifier = clientDuid,
                    StartedAt = DateTime.UtcNow.AddMinutes(-10)
                },
                new DHCPv6LeaseActivatedEvent
                {
                    EntityId = leaseId,
                    ScopeId = scopeId,
                }
            }, TimeSpan.FromMilliseconds(175));

            DHCPv6Packet result = rootScope.HandleSolicit(packet, GetServerPropertiesResolver());
            DHCPv6Lease lease = CheckLeaseForPrefix(0, 1, IPv6Address.FromString("fd80::0"), IPv6Address.FromString("fd80:0:0:c0::0"), scopeId, rootScope, clientDuid, prefixIaId, leasedPrefixLength, false);

            CheckPrefixPacket(result, rapitCommitEnabled == true ? DHCPv6PacketTypes.REPLY : DHCPv6PacketTypes.ADVERTISE, lease.PrefixDelegation);

            CheckEventAmount(2, rootScope);
            CheckDHCPv6LeasePrefixAddedEvent(0, prefixIaId, scopeId, rootScope, lease);
            CheckHandeledEvent(1, packet, result, rootScope, scopeId);

            if (rapitCommitEnabled == true)
            {
                var trigger = CheckTrigger<PrefixEdgeRouterBindingUpdatedTrigger>(rootScope);
                Assert.Equal(scopeId, trigger.ScopeId);
                Assert.Null(trigger.OldBinding);
                Assert.NotNull(trigger.NewBinding);

                Assert.Equal(lease.PrefixDelegation.NetworkAddress, trigger.NewBinding.Prefix);
                Assert.Equal(leasedPrefixLength, trigger.NewBinding.Mask.Identifier);
                Assert.Equal(leasedAddress, trigger.NewBinding.Host);
            }
            else
            {
                CheckEmptyTrigger(rootScope);
            }
        }

        [Fact]
        public void HandleSolicitWithPrefixDelegation_NoLeaseFound()
        {
            Random random = new Random();

            var packet = GetSolicitPacketWithPrefixIdOnly(random, out DUID _, out _);
            var resolverInformations = GetMockupResolver(packet, out Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>> scopeResolverMock);

            Guid scopeId = random.NextGuid();
            Guid leaseId = random.NextGuid();

            Byte leasedPrefixLength = 58;
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
                        prefixDelgationInfo: DHCPv6PrefixDelgationInfo.FromValues(IPv6Address.FromString("fd80::0"),new IPv6SubnetMaskIdentifier(56),new IPv6SubnetMaskIdentifier(leasedPrefixLength))
                        ),
                    ResolverInformation = resolverInformations,
                    Name = "Testscope",
                    Id = scopeId,
                }),
                new DHCPv6LeaseCreatedEvent
                {
                    EntityId = leaseId,
                    ScopeId = scopeId,
                    HasPrefixDelegation = false,
                    ClientIdentifier = new UUIDDUID(random.NextGuid()),
                    StartedAt = DateTime.UtcNow.AddMinutes(-10)
                },
                new DHCPv6LeaseActivatedEvent
                {
                    EntityId = leaseId,
                    ScopeId = scopeId,
                }
            });

            DHCPv6Packet result = rootScope.HandleSolicit(packet, GetServerPropertiesResolver());
            Assert.Equal(DHCPv6Packet.Empty, result);

            CheckEventAmount(1, rootScope);
            CheckHandeledEventNotSuccessfull(0, packet, rootScope, scopeId);

            CheckEmptyTrigger(rootScope);
        }

        [Fact]
        public void HandleSolicitWithPrefixDelegation_LeaseNotActive()
        {
            Random random = new Random();

            var packet = GetSolicitPacketWithPrefixIdOnly(random, out DUID clientDuid, out _);
            var resolverInformations = GetMockupResolver(packet, out Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>> scopeResolverMock);

            Guid scopeId = random.NextGuid();
            Guid leaseId = random.NextGuid();

            Byte leasedPrefixLength = 58;
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
                        prefixDelgationInfo: DHCPv6PrefixDelgationInfo.FromValues(IPv6Address.FromString("fd80::0"),new IPv6SubnetMaskIdentifier(56),new IPv6SubnetMaskIdentifier(leasedPrefixLength))
                        ),
                    ResolverInformation = resolverInformations,
                    Name = "Testscope",
                    Id = scopeId,
                }),
                new DHCPv6LeaseCreatedEvent
                {
                    EntityId = leaseId,
                    ScopeId = scopeId,
                    HasPrefixDelegation = false,
                    ClientIdentifier = clientDuid,
                    StartedAt = DateTime.UtcNow.AddMinutes(-10)
                }
            });

            DHCPv6Packet result = rootScope.HandleSolicit(packet, GetServerPropertiesResolver());
            Assert.Equal(DHCPv6Packet.Empty, result);

            CheckEventAmount(1, rootScope);
            CheckHandeledEventNotSuccessfull(0, packet, rootScope, scopeId);

            CheckEmptyTrigger(rootScope);
        }

        [Fact]
        public void HandleSolicitWithPrefixDelegation_AddressPropertiesNoPrefix()
        {
            Random random = new Random();

            var packet = GetSolicitPacketWithPrefixIdOnly(random, out DUID clientDuid, out _);
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
                        validLifeTime: TimeSpan.FromDays(1)
                        ),
                    ResolverInformation = resolverInformations,
                    Name = "Testscope",
                    Id = scopeId,
                }),
                new DHCPv6LeaseCreatedEvent
                {
                    EntityId = leaseId,
                    ScopeId = scopeId,
                    HasPrefixDelegation = false,
                    ClientIdentifier = clientDuid,
                    StartedAt = DateTime.UtcNow.AddMinutes(-10)
                },
                 new DHCPv6LeaseActivatedEvent
                {
                    EntityId = leaseId,
                    ScopeId = scopeId,
                }
            });

            DHCPv6Packet result = rootScope.HandleSolicit(packet, GetServerPropertiesResolver());
            Assert.Equal(DHCPv6Packet.Empty, result);

            CheckEventAmount(1, rootScope);
            CheckHandeledEventNotSuccessfull(0, packet, rootScope, scopeId);

            CheckEmptyTrigger(rootScope);
        }

        private IPv6Address GetLeasedIPv6AddresFromPacket(DHCPv6Packet packet, UInt32 id) =>
            packet.GetInnerPacket().GetNonTemporaryIdentiyAssocation(id)
            .Suboptions.Cast<DHCPv6PacketIdentityAssociationAddressSuboption>()
            .Select(x => x.Address).FirstOrDefault();

        private IPv6Address GetLeasedIPv6PreifxAddresFromPacket(DHCPv6Packet packet, UInt32 id) =>
            packet.GetInnerPacket().GetPrefixDelegationIdentiyAssocation(id)
            .Suboptions.Cast<DHCPv6PacketIdentityAssociationPrefixDelegationSuboption>()
            .Select(x => x.Address).FirstOrDefault();

        [Theory]
        [InlineData(DHCPv6ScopeAddressProperties.AddressAllocationStrategies.Next)]
        [InlineData(DHCPv6ScopeAddressProperties.AddressAllocationStrategies.Random)]

        public void HandleSolicit_NoLeaseFound_AddressAvaiable_WithRapitCommit_TwoPackets_AndScopeWithTwoAddresses(
            DHCPv6ScopeAddressProperties.AddressAllocationStrategies allocationStrategies)
        {
            Random random = new Random();

            UInt32 firstClientPrefixIaId = random.NextUInt32();
            UInt32 secondClientPrefixIaId = random.NextUInt32();

            var firstPacket = GetSolicitPacket(random, out  _, out UInt32 firstClientIaId, false,
                new DHCPv6PacketTrueOption(DHCPv6PacketOptionTypes.RapitCommit),
                new DHCPv6PacketIdentityAssociationPrefixDelegationOption(firstClientPrefixIaId, TimeSpan.Zero, TimeSpan.Zero, Array.Empty<DHCPv6PacketSuboption>())
                );

            var secondPacket = GetSolicitPacket(random, out _, out UInt32 secondClientIaId, false,
                new DHCPv6PacketTrueOption(DHCPv6PacketOptionTypes.RapitCommit),
                new DHCPv6PacketIdentityAssociationPrefixDelegationOption(secondClientPrefixIaId, TimeSpan.Zero, TimeSpan.Zero, Array.Empty<DHCPv6PacketSuboption>())
                );

            var resolverInformations = GetMockupResolver(new[] { firstPacket, secondPacket }, out Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>> scopeResolverMock);

            Guid scopeId = random.NextGuid();

            DHCPv6RootScope rootScope = GetRootScope(scopeResolverMock);
            rootScope.Load(new List<DomainEvent>{ new DHCPv6ScopeEvents.DHCPv6ScopeAddedEvent(
                new DHCPv6ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv6ScopeAddressProperties(
                        IPv6Address.FromString("2a0c:cac5:102:97::666"),
                        IPv6Address.FromString("2a0c:cac5:102:97::667"),
                        new List<IPv6Address>(),
                        t1: DHCPv6TimeScale.FromDouble(0.5),
                        t2: DHCPv6TimeScale.FromDouble(0.75),
                        preferredLifeTime: TimeSpan.FromDays(0.5),
                        validLifeTime: TimeSpan.FromDays(1),
                        rapitCommitEnabled: true,
                        reuseAddressIfPossible: false,
                        prefixDelgationInfo: DHCPv6PrefixDelgationInfo.FromValues(
                            IPv6Address.FromString("2a0c:cac6:2001:0066::0"),
                            new IPv6SubnetMaskIdentifier(63),
                            new IPv6SubnetMaskIdentifier(64)
                            ),
                        addressAllocationStrategy: allocationStrategies),
                    ResolverInformation = resolverInformations,
                    Name = "Testscope",
                    Id = scopeId,
                })
            }); ;

            IPv6Address firstAvailaibeAddress = IPv6Address.FromString("2a0c:cac5:102:97::666");
            IPv6Address secondAvailaibeAddress = IPv6Address.FromString("2a0c:cac5:102:97::667");

            IPv6Address firstAvaiblePrefixAddress = IPv6Address.FromString("2a0c:cac6:2001:0066::0");
            IPv6Address secondAvaiblePrefixAddress = IPv6Address.FromString("2a0c:cac6:2001:0067::0");

            Guid serverId = random.NextGuid();

            DHCPv6Packet firstResponse = rootScope.HandleSolicit(firstPacket, GetServerPropertiesResolver(serverId));
            DHCPv6Packet secondResponse = rootScope.HandleSolicit(secondPacket, GetServerPropertiesResolver(serverId));

            IPv6Address firstLeasedAddress = GetLeasedIPv6AddresFromPacket(firstResponse, firstClientIaId);
            IPv6Address secondLeasedAddress = GetLeasedIPv6AddresFromPacket(secondResponse, secondClientIaId);

            IPv6Address firstPrefixAddress = GetLeasedIPv6PreifxAddresFromPacket(firstResponse, firstClientPrefixIaId);
            IPv6Address secondPrefixAddress = GetLeasedIPv6PreifxAddresFromPacket(secondResponse, secondClientPrefixIaId);

            Assert.NotEqual(firstLeasedAddress, secondLeasedAddress);
            Assert.NotEqual(firstPrefixAddress, secondPrefixAddress);

            if (allocationStrategies == DHCPv6ScopeAddressProperties.AddressAllocationStrategies.Next)
            {
                Assert.Equal(firstLeasedAddress, firstAvailaibeAddress);
                Assert.Equal(secondLeasedAddress, secondAvailaibeAddress);

                Assert.Equal(firstAvaiblePrefixAddress, firstPrefixAddress);
                Assert.Equal(secondAvaiblePrefixAddress, secondPrefixAddress);
            }
            else
            {
                Assert.True(firstLeasedAddress == firstAvailaibeAddress || firstLeasedAddress == secondAvailaibeAddress);
                Assert.True(secondLeasedAddress == firstAvailaibeAddress || secondLeasedAddress == secondAvailaibeAddress);

                Assert.True(firstPrefixAddress == firstAvaiblePrefixAddress || firstPrefixAddress == secondAvaiblePrefixAddress);
                Assert.True(secondPrefixAddress == firstAvaiblePrefixAddress || secondPrefixAddress == secondAvaiblePrefixAddress);
            }
        }

        [Fact]
        public void HandleSolicit_NoLeaseFound_AddressAvaiable_WithOptions()
        {
            Random random = new Random();

            var packet = GetSolicitPacket(random, out DUID clientDuid, out UInt32 iaId);
            var resolverInformations = GetMockupResolver(packet, out Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>> scopeResolverMock);

            Guid scopeId = random.NextGuid();

            var addressListOption = new DHCPv6AddressListScopeProperty(random.NextUInt16(), random.GetIPv6Addresses(3, 8));
            var textOption = new DHCPv6TextScopeProperty(random.NextUInt16(), random.GetAlphanumericString());
            var byteNumericOption = new DHCPv6NumericValueScopeProperty(random.NextUInt16(), random.NextByte(), NumericScopePropertiesValueTypes.Byte, DHCPv6ScopePropertyType.Byte);
            var uint16NumericOption = new DHCPv6NumericValueScopeProperty(random.NextUInt16(), random.NextUInt16(), NumericScopePropertiesValueTypes.UInt16, DHCPv6ScopePropertyType.UInt16);
            var uint32NumericOption = new DHCPv6NumericValueScopeProperty(random.NextUInt16(), random.NextUInt32(), NumericScopePropertiesValueTypes.UInt32, DHCPv6ScopePropertyType.UInt32);

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
                    ScopeProperties = new DHCPv6ScopeProperties(addressListOption,textOption,
                    byteNumericOption,uint16NumericOption,uint32NumericOption),
                })
            });

            IPv6Address expectedAdress = IPv6Address.FromString("fe80::0");

            DHCPv6Packet result = rootScope.HandleSolicit(packet, GetServerPropertiesResolver());
            CheckPacket(packet, expectedAdress, iaId, result, DHCPv6PacketTypes.ADVERTISE, DHCPv6PrefixDelegation.None);

            DHCPv6Lease lease = CheckLease(0, 1, expectedAdress, scopeId, rootScope, DateTime.UtcNow, clientDuid, iaId, true, false);

            CheckEventAmount(2, rootScope);
            CheckLeaseCreatedEvent(0, clientDuid, iaId, scopeId, rootScope, expectedAdress, lease);
            CheckHandeledEvent(1, packet, result, rootScope, scopeId);

            CheckEmptyTrigger(rootScope);

            var innerPacket = result.GetInnerPacket();

            var actualAddressListOption = innerPacket.GetOption<DHCPv6PacketIPAddressListOption>(addressListOption.OptionIdentifier);
            // var actualtextOption = innerPacket.GetOption<DHCPv6PacketTextOption>(textOption.OptionIdentifier);
            var actualByteNumericOption = innerPacket.GetOption<DHCPv6PacketByteOption>(byteNumericOption.OptionIdentifier);
            var actualUint16NumericOption = innerPacket.GetOption<DHCPv6PacketUInt16Option>(uint16NumericOption.OptionIdentifier);
            var actualUint32NumericOption = innerPacket.GetOption<DHCPv6PacketUInt32Option>(uint32NumericOption.OptionIdentifier);

            Assert.Equal(addressListOption.Addresses, actualAddressListOption.Addresses);
            Assert.Equal(byteNumericOption.Value, actualByteNumericOption.Value);
            Assert.Equal(byteNumericOption.Value, actualByteNumericOption.Value);
            Assert.Equal(uint16NumericOption.Value, actualUint16NumericOption.Value);
            Assert.Equal(uint32NumericOption.Value, actualUint32NumericOption.Value);

        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task HandleSolicitWithPrefixDelegation_LeaseFound_PrefixDelegationAvaiable_CheckRebound(Boolean rapitCommitEnabled)
        {
            Random random = new Random();

            DHCPv6PacketOption[] options = Array.Empty<DHCPv6PacketOption>();
            if (rapitCommitEnabled == true)
            {
                options = new DHCPv6PacketOption[] {
                new DHCPv6PacketTrueOption(DHCPv6PacketOptionTypes.RapitCommit)
                };
            }

            var firstPacket = GetSolicitPacketWithPrefixIdOnly(random, out DUID clientDuid, out UInt32 prefixIaId, options);
            var resolverInformations = GetMockupResolver(firstPacket, out Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>> scopeResolverMock);

            Guid scopeId = random.NextGuid();
            Guid leaseId = random.NextGuid();

            IPv6Address leasedAddress = random.GetIPv6Address();

            Byte leasedPrefixLength = 58;
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
                        rapitCommitEnabled: rapitCommitEnabled,
                        addressAllocationStrategy: DHCPv6ScopeAddressProperties.AddressAllocationStrategies.Next,
                        prefixDelgationInfo: DHCPv6PrefixDelgationInfo.FromValues(IPv6Address.FromString("fd80::0"),new IPv6SubnetMaskIdentifier(56),new IPv6SubnetMaskIdentifier(leasedPrefixLength))
                        ),
                    ResolverInformation = resolverInformations,
                    Name = "Testscope",
                    Id = scopeId,
                }),
                new DHCPv6LeaseCreatedEvent
                {
                    EntityId = leaseId,
                    ScopeId = scopeId,
                    Address = leasedAddress,
                    HasPrefixDelegation = false,
                    ClientIdentifier = clientDuid,
                    StartedAt = DateTime.UtcNow.AddMinutes(-10)
                },
                new DHCPv6LeaseActivatedEvent
                {
                    EntityId = leaseId,
                    ScopeId = scopeId,
                }
            });

            DHCPv6Packet firstResult = rootScope.HandleSolicit(firstPacket, GetServerPropertiesResolver());

            DHCPv6Lease lease = CheckLeaseForPrefix(0, 1, IPv6Address.FromString("fd80::0"), IPv6Address.FromString("fd80:0:0:c0::0"), scopeId, rootScope, clientDuid, prefixIaId, leasedPrefixLength, false);

            CheckPrefixPacket(firstResult, rapitCommitEnabled == true ? DHCPv6PacketTypes.REPLY : DHCPv6PacketTypes.ADVERTISE, lease.PrefixDelegation);

            CheckEventAmount(2, rootScope);
            CheckDHCPv6LeasePrefixAddedEvent(0, prefixIaId, scopeId, rootScope, lease);
            CheckHandeledEvent(1, firstPacket, firstResult, rootScope, scopeId);

            if (rapitCommitEnabled == true)
            {
                var trigger = CheckTrigger<PrefixEdgeRouterBindingUpdatedTrigger>(rootScope);
                Assert.Equal(scopeId, trigger.ScopeId);
                Assert.Null(trigger.OldBinding);
                Assert.NotNull(trigger.NewBinding);

                Assert.Equal(lease.PrefixDelegation.NetworkAddress, trigger.NewBinding.Prefix);
                Assert.Equal(leasedPrefixLength, trigger.NewBinding.Mask.Identifier);
                Assert.Equal(leasedAddress, trigger.NewBinding.Host);
            }
            else
            {
                CheckEmptyTrigger(rootScope);
            }

            await Task.Delay(1000);
            DHCPv6Packet secondResult = rootScope.HandleSolicit(firstPacket, GetServerPropertiesResolver());

            Assert.Null(secondResult);
        }
    }
}

