using DaAPI.Core.Common;
using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Notifications.Triggers;
using DaAPI.Core.Packets.DHCPv6;
using DaAPI.Core.Scopes;
using DaAPI.Core.Scopes.DHCPv6;
using DaAPI.Core.Services;
using DaAPI.TestHelper;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using static DaAPI.Core.Scopes.DHCPv6.DHCPv6LeaseEvents;

namespace DaAPI.UnitTests.Core.Scopes.DHCPv6
{
    public class DHCPv6RootScopeTesterHandleRebindTester_PrefixBinding : DHCPv6RootSoopeTesterBase
    {
        [Theory]
        [InlineData(true, true, true, false, false)]
        [InlineData(true, true, false, false, true)]
        [InlineData(true, false, true, true, false)]
        [InlineData(true, false, false, false, false)]

        [InlineData(false, true, true, true, true)]
        [InlineData(false, true, false, false, true)]
        [InlineData(false, false, true, true, false)]
        [InlineData(false, false, false, false, false)]

        public void TestNotifcationTriggerForSolicitMessages(Boolean reuse, Boolean prefixRequest, Boolean hadPrefix, Boolean shouldHaveOldBinding, Boolean shouldHaveNewBinding)
        {
            Random random = new Random();
            IPv6HeaderInformation headerInformation =
            new IPv6HeaderInformation(IPv6Address.FromString("fe80::1"), IPv6Address.FromString("fe80::2"));

            DUID clientDuid = new UUIDDUID(random.NextGuid());

            UInt32 iaId = random.NextBoolean() == false ? random.NextUInt32() : 0;
            IPv6Address leasedAddress = IPv6Address.FromString("fe80::5");

            UInt32 prefixIaId = random.NextBoolean() == false ? random.NextUInt32() : 0;
            Byte exisitngPrefixLength = 62;
            DHCPv6PrefixDelegation existingDelegation = DHCPv6PrefixDelegation.FromValues(
                IPv6Address.FromString("2a4c::0"), new IPv6SubnetMask(new IPv6SubnetMaskIdentifier(exisitngPrefixLength)), prefixIaId);

            var packetOptions = new List<DHCPv6PacketOption>
                {
                    new DHCPv6PacketIdentifierOption(DHCPv6PacketOptionTypes.ClientIdentifier,clientDuid),
                    new DHCPv6PacketIdentityAssociationNonTemporaryAddressesOption(iaId,TimeSpan.Zero,TimeSpan.Zero,Array.Empty<DHCPv6PacketSuboption>()),
                };
            if (prefixRequest == true)
            {
                packetOptions.Add(
                    new DHCPv6PacketIdentityAssociationPrefixDelegationOption(prefixIaId, TimeSpan.Zero, TimeSpan.Zero, Array.Empty<DHCPv6PacketSuboption>()));
            }

            DHCPv6Packet innerPacket = DHCPv6Packet.AsInner(random.NextUInt16(),
                DHCPv6PacketTypes.REBIND, packetOptions);

            DHCPv6Packet packet = DHCPv6RelayPacket.AsOuterRelay(headerInformation, true, 1, random.GetIPv6Address(), random.GetIPv6Address(),
                new DHCPv6PacketOption[] { new DHCPv6PacketByteArrayOption(DHCPv6PacketOptionTypes.InterfaceId, random.NextBytes(10)) }, innerPacket);

            CreateScopeResolverInformation resolverInformations = new CreateScopeResolverInformation { Typename = "something" };

            Mock<IScopeResolver<DHCPv6Packet, IPv6Address>> resolverMock = new Mock<IScopeResolver<DHCPv6Packet, IPv6Address>>(MockBehavior.Strict);
            resolverMock.Setup(x => x.PacketMeetsCondition(packet)).Returns(true);
            resolverMock.SetupGet(x => x.HasUniqueIdentifier).Returns(false);

            var scopeResolverMock = new Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>>(MockBehavior.Strict);
            scopeResolverMock.Setup(x => x.InitializeResolver(resolverInformations)).Returns(resolverMock.Object);

            Mock<ILoggerFactory> factoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);
            factoryMock.Setup(x => x.CreateLogger(It.IsAny<String>())).Returns(Mock.Of<ILogger<DHCPv6RootScope>>());

            var rootScope = new DHCPv6RootScope(Guid.NewGuid(), scopeResolverMock.Object, factoryMock.Object);

            Guid scopeId = random.NextGuid();
            Guid leaseId = random.NextGuid();

            var expetecNewLeaseAddress = IPv6Address.FromString("fe80::0");
            var events = new List<DomainEvent> { new DHCPv6ScopeEvents.DHCPv6ScopeAddedEvent(
                    new DHCPv6ScopeCreateInstruction
                    {
                        AddressProperties = new DHCPv6ScopeAddressProperties(
                            IPv6Address.FromString("fe80::0"),
                            IPv6Address.FromString("fe80::ff"),
                            new List<IPv6Address> { IPv6Address.FromString("fe80::1") },
                            t1: DHCPv6TimeScale.FromDouble(0.5),
                            t2: DHCPv6TimeScale.FromDouble(0.75),
                            preferredLifeTime: TimeSpan.FromDays(0.5),
                            validLifeTime: TimeSpan.FromDays(1),
                            reuseAddressIfPossible: reuse,
                            prefixDelgationInfo: DHCPv6PrefixDelgationInfo.FromValues(existingDelegation.NetworkAddress,new IPv6SubnetMaskIdentifier(40),new IPv6SubnetMaskIdentifier(64)),
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
                        HasPrefixDelegation = hadPrefix,
                        UniqueIdentiifer = Array.Empty<Byte>(),
                        PrefixLength = hadPrefix == true ? exisitngPrefixLength : (Byte)0,
                        IdentityAssocationIdForPrefix = hadPrefix == true ? prefixIaId : (UInt32)0,
                        DelegatedNetworkAddress = hadPrefix == true ? existingDelegation.NetworkAddress : IPv6Address.Empty,
                        StartedAt = DateTime.UtcNow.AddDays(-1),
                        ValidUntil = DateTime.UtcNow.AddDays(1),
                    },
                    new DHCPv6LeaseActivatedEvent
                    {
                        EntityId = leaseId,
                        ScopeId = scopeId,
                    }
            };

            rootScope.Load(events);

            var serverPropertiesResolverMock = new Mock<IDHCPv6ServerPropertiesResolver>(MockBehavior.Strict);
            serverPropertiesResolverMock.Setup(x => x.GetServerDuid()).Returns(new UUIDDUID(Guid.NewGuid()));

            var _ = rootScope.HandleRebind(packet, serverPropertiesResolverMock.Object);

            if (shouldHaveNewBinding == false && shouldHaveOldBinding == false)
            {
                CheckEmptyTrigger(rootScope);
            }
            else
            {
                var trigger = CheckTrigger<PrefixEdgeRouterBindingUpdatedTrigger>(rootScope);

                Assert.Equal(scopeId, trigger.ScopeId);

                if (shouldHaveNewBinding == true)
                {
                    Assert.NotNull(trigger.NewBinding);

                    Assert.Equal(64, trigger.NewBinding.Mask.Identifier);
                    if (reuse == true)
                    {
                        Assert.Equal(leasedAddress, trigger.NewBinding.Host);
                    }
                    else
                    {
                        Assert.Equal(expetecNewLeaseAddress, trigger.NewBinding.Host);
                    }
                }
                else
                {
                    Assert.Null(trigger.NewBinding);
                }

                if (shouldHaveOldBinding == true)
                {
                    Assert.NotNull(trigger.OldBinding);

                    Assert.Equal(exisitngPrefixLength, trigger.OldBinding.Mask.Identifier);
                    Assert.Equal(existingDelegation.NetworkAddress, trigger.OldBinding.Prefix);
                    Assert.Equal(leasedAddress, trigger.OldBinding.Host);
                }
                else
                {
                    Assert.Null(trigger.OldBinding);
                }
            }
        }
    }
}
