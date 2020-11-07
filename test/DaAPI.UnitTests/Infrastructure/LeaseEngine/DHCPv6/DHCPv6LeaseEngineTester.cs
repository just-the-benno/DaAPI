using DaAPI.Core.Common;
using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Packets.DHCPv6;
using DaAPI.Core.Scopes;
using DaAPI.Core.Scopes.DHCPv6;
using DaAPI.Core.Services;
using DaAPI.Infrastructure.LeaseEngines.DHCPv6;
using DaAPI.Infrastructure.ServiceBus;
using DaAPI.Infrastructure.ServiceBus.Messages;
using DaAPI.Infrastructure.StorageEngine;
using DaAPI.Infrastructure.StorageEngine.DHCPv6;
using DaAPI.TestHelper;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DaAPI.UnitTests.Infrastructure.LeaseEngine.DHCPv6
{
    public class DHCPv6LeaseEngineTester
    {
        private DHCPv6Packet GetSolicitPacket(
         Random random, out DUID clientDuid, out UInt32 iaId, params DHCPv6PacketOption[] options)
        {
            IPv6HeaderInformation headerInformation =
               new IPv6HeaderInformation(IPv6Address.FromString("fe80::1"), IPv6Address.FromString("fe80::1"));

            clientDuid = new UUIDDUID(random.NextGuid());
            iaId = random.NextUInt32();

            var packetOptions = new List<DHCPv6PacketOption>(options)
                {
                    new DHCPv6PacketIdentifierOption(DHCPv6PacketOptionTypes.ClientIdentifier,clientDuid),
                    new DHCPv6PacketIdentityAssociationNonTemporaryAddressesOption(iaId,TimeSpan.Zero,TimeSpan.Zero,Array.Empty<DHCPv6PacketSuboption>()),
                    new DHCPv6PacketTrueOption(DHCPv6PacketOptionTypes.RapitCommit),
            };

            DHCPv6Packet packet = DHCPv6Packet.AsOuter(headerInformation, random.NextUInt16(),
                DHCPv6PacketTypes.Solicit, packetOptions);

            return packet;
        }

        private static CreateScopeResolverInformation GetMockupResolver(
            DHCPv6Packet packet,
            out Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>> scopeResolverMock
            )
        {
            CreateScopeResolverInformation resolverInformations = new CreateScopeResolverInformation { Typename = "something" };
            scopeResolverMock = new Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>>(MockBehavior.Strict);

            Mock<IScopeResolver<DHCPv6Packet, IPv6Address>> resolverMock = new Mock<IScopeResolver<DHCPv6Packet, IPv6Address>>(MockBehavior.Strict);
            resolverMock.Setup(x => x.PacketMeetsCondition(packet)).Returns(true);
            resolverMock.SetupGet(x => x.HasUniqueIdentifier).Returns(false);
            scopeResolverMock.Setup(x => x.InitializeResolver(resolverInformations)).Returns(resolverMock.Object);

            return resolverInformations;
        }

        protected DHCPv6RootScope GetRootScope(Random random, out DHCPv6Packet packet)
        {
            packet = GetSolicitPacket(random, out _, out _);
            var resolverInformations = GetMockupResolver(packet, out Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>> scopeResolverMock);

            Guid scopeId = random.NextGuid();

            Mock<ILoggerFactory> factoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);
            factoryMock.Setup(x => x.CreateLogger(It.IsAny<String>())).Returns(Mock.Of<ILogger<DHCPv6RootScope>>());

            var rootScope = new DHCPv6RootScope(Guid.NewGuid(), scopeResolverMock.Object, factoryMock.Object);

            rootScope.Load(new List<DomainEvent>{ new DHCPv6ScopeEvents.DHCPv6ScopeAddedEvent(
                new DHCPv6ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv6ScopeAddressProperties(
                        IPv6Address.FromString("fe80::0"),
                        IPv6Address.FromString("fe80::ff"),
                        new List<IPv6Address>{IPv6Address.FromString("fe80::1") },
                        t1: DHCPv6TimeScale.FromDouble(0.25),
                        t2: DHCPv6TimeScale.FromDouble(0.75),
                        preferredLifeTime: TimeSpan.FromDays(0.5),
                        validLifeTime: TimeSpan.FromDays(1),
                        rapitCommitEnabled: true,
                        addressAllocationStrategy: DHCPv6ScopeAddressProperties.AddressAllocationStrategies.Next,
                        prefixDelgationInfo: DHCPv6PrefixDelgationInfo.FromValues(
                            IPv6Address.FromString("2a64:40::0"),new IPv6SubnetMaskIdentifier(64),new IPv6SubnetMaskIdentifier(64))
                        ),
                    ResolverInformation = resolverInformations,
                    Name = "Testscope",
                    Id = scopeId,
                })
            });

            return rootScope;
        }

        [Fact]
        public async Task HandlePacket_Solicit()
        {
            Random random = new Random();

            DHCPv6RootScope rootScope = GetRootScope(random, out DHCPv6Packet request);

            Mock<IDHCPv6StorageEngine> storageMock = new Mock<IDHCPv6StorageEngine>(MockBehavior.Strict);
            storageMock.Setup(x => x.Save(rootScope)).ReturnsAsync(true).Verifiable();

            Mock<IDHCPv6ServerPropertiesResolver> propertyResolver = new Mock<IDHCPv6ServerPropertiesResolver>(MockBehavior.Strict);
            propertyResolver.Setup(x => x.GetServerDuid()).Returns(new UUIDDUID(Guid.NewGuid())).Verifiable();

            Mock<IServiceBus> serviceBusMock = new Mock<IServiceBus>();
            serviceBusMock.Setup(x => x.Publish(It.Is<NewTriggerHappendMessage>(y =>
            y.Triggers.Count() == 1))).Returns(Task.FromResult(true));

            DHCPv6LeaseEngine engine = new DHCPv6LeaseEngine(
                storageMock.Object,
                rootScope,
                propertyResolver.Object,
                serviceBusMock.Object,
                Mock.Of<ILogger<DHCPv6LeaseEngine>>());

            var response = await engine.HandlePacket(request);

            Assert.NotEqual(DHCPv6Packet.Empty, response);

            storageMock.Verify();
            propertyResolver.Verify();
            serviceBusMock.Verify();
        }
    }
}
