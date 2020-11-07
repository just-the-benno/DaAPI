using DaAPI.Core.Common;
using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Notifications;
using DaAPI.Core.Packets.DHCPv6;
using DaAPI.Core.Scopes;
using DaAPI.Core.Scopes.DHCPv6;
using DaAPI.Core.Scopes.DHCPv6.Resolvers;
using DaAPI.Core.Services;
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
    public abstract class DHCPv6RootSoopeTesterBase
    {
        protected DHCPv6RootScope GetRootScope(Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>> scopeResolverMock)
        {
            Mock<ILoggerFactory> factoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);
            factoryMock.Setup(x => x.CreateLogger(It.IsAny<String>())).Returns(Mock.Of<ILogger<DHCPv6RootScope>>());

            var scope = new DHCPv6RootScope(Guid.NewGuid(), scopeResolverMock.Object, factoryMock.Object);

            return scope;
        }


        protected void CheckEventAmount(int expectedAmount, DHCPv6RootScope rootScope)
        {
            var changes = rootScope.GetChanges();
            Assert.Equal(expectedAmount, changes.Count());
        }

        protected static IDHCPv6ServerPropertiesResolver GetServerPropertiesResolver() => GetServerPropertiesResolver(Guid.NewGuid());

        protected static IDHCPv6ServerPropertiesResolver GetServerPropertiesResolver(Guid id)
        {
            var mock = new Mock<IDHCPv6ServerPropertiesResolver>(MockBehavior.Strict);
            mock.Setup(x => x.GetServerDuid()).Returns(new UUIDDUID(id));

            return mock.Object;
        }

        protected static CreateScopeResolverInformation GetMockupResolver(
           DHCPv6Packet packet,
           out Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>> scopeResolverMock,
           Byte[] uniqueIdentifierValue = null
           )
        {
            CreateScopeResolverInformation resolverInformations = new CreateScopeResolverInformation { Typename = "something" };
            scopeResolverMock = new Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>>(MockBehavior.Strict);

            Mock<IScopeResolver<DHCPv6Packet, IPv6Address>> resolverMock = new Mock<IScopeResolver<DHCPv6Packet, IPv6Address>>(MockBehavior.Strict);
            resolverMock.Setup(x => x.PacketMeetsCondition(packet)).Returns(true);
            resolverMock.SetupGet(x => x.HasUniqueIdentifier).Returns(uniqueIdentifierValue != null);
            if (uniqueIdentifierValue != null)
            {
                resolverMock.Setup(x => x.GetUniqueIdentifier(packet)).Returns(uniqueIdentifierValue);
            }
            scopeResolverMock.Setup(x => x.InitializeResolver(resolverInformations)).Returns(resolverMock.Object);

            return resolverInformations;
        }

        protected static CreateScopeResolverInformation GetMockupResolver(
           IEnumerable<DHCPv6Packet> packets,
           out Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>> scopeResolverMock,
           Byte[] uniqueIdentifierValue = null
           )
        {
            CreateScopeResolverInformation resolverInformations = new CreateScopeResolverInformation { Typename = "something" };
            scopeResolverMock = new Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>>(MockBehavior.Strict);

            Mock<IScopeResolver<DHCPv6Packet, IPv6Address>> resolverMock = new Mock<IScopeResolver<DHCPv6Packet, IPv6Address>>(MockBehavior.Strict);
            foreach (var packet in packets)
            {
                resolverMock.Setup(x => x.PacketMeetsCondition(packet)).Returns(true);
                resolverMock.SetupGet(x => x.HasUniqueIdentifier).Returns(uniqueIdentifierValue != null);
                if (uniqueIdentifierValue != null)
                {
                    resolverMock.Setup(x => x.GetUniqueIdentifier(packet)).Returns(uniqueIdentifierValue);
                }
            }

            scopeResolverMock.Setup(x => x.InitializeResolver(resolverInformations)).Returns(resolverMock.Object);

            return resolverInformations;
        }

        protected static CreateScopeResolverInformation GetMockupPseudoResolver(out Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>> scopeResolverMock)
        {
            CreateScopeResolverInformation resolverInformations = new CreateScopeResolverInformation { Typename = "something" };
            scopeResolverMock = new Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>>(MockBehavior.Strict);

            scopeResolverMock.Setup(x => x.InitializeResolver(resolverInformations)).Returns(new DHCPv6PseudoResolver());

            return resolverInformations;
        }

        protected static void CheckRevokedEvent(Int32 index, Guid scopeId, Guid leaseId, DHCPv6RootScope rootScope)
        {
            IEnumerable<DomainEvent> changes = rootScope.GetChanges();
            var @event = changes.ElementAt(index);
            Assert.NotNull(@event);
            Assert.IsAssignableFrom<DHCPv6LeaseRevokedEvent>(@event);

            DHCPv6LeaseRevokedEvent revokedEvent = (DHCPv6LeaseRevokedEvent)@event;

            Assert.Equal(scopeId, revokedEvent.ScopeId);
            Assert.Equal(leaseId, revokedEvent.EntityId);
        }

        protected static void CheckLeaseActivatedEvent(Int32 index, Guid scopeId, Guid leaseId, DHCPv6RootScope rootScope)
        {
            IEnumerable<DomainEvent> changes = rootScope.GetChanges();
            var @event = changes.ElementAt(index);
            Assert.NotNull(@event);
            Assert.IsAssignableFrom<DHCPv6LeaseActivatedEvent>(@event);

            DHCPv6LeaseActivatedEvent castedEvent = (DHCPv6LeaseActivatedEvent)@event;

            Assert.Equal(scopeId, castedEvent.ScopeId);
            Assert.Equal(leaseId, castedEvent.EntityId);
        }

        protected static void CheckLeaseRenewdEvent(
           Guid scopeId,
           DHCPv6RootScope rootScope, DHCPv6Lease lease, Boolean expectReset, Boolean expectedPrefixReset)
        {
            IEnumerable<DomainEvent> changes = rootScope.GetChanges();
            Assert.NotNull(changes);
            Assert.Equal(2, changes.Count());

            Assert.IsAssignableFrom<DHCPv6LeaseRenewedEvent>(changes.First());

            DHCPv6LeaseRenewedEvent createdEvent = (DHCPv6LeaseRenewedEvent)changes.First();
            Assert.NotNull(createdEvent);

            Assert.Equal(scopeId, createdEvent.ScopeId);
            Assert.NotEqual(Guid.Empty, lease.Id);

            Assert.Equal(lease.Id, createdEvent.EntityId);
            Assert.Equal(expectReset, createdEvent.Reset);

            Assert.Equal(expectedPrefixReset, createdEvent.ResetPrefix);

            Assert.Equal(lease.End, createdEvent.End);
        }

        protected static DHCPv6Lease CheckLease(
            Int32 index, Int32 expectedAmount, IPv6Address expectedAdress,
            Guid scopeId, DHCPv6RootScope rootScope, DateTime expectedCreationData,
            DUID clientDuid, UInt32 iaId, Boolean shouldBePending, Boolean shouldHavePrefix, Byte[] uniqueIdentifier = null)
        {
            DHCPv6Scope scope = rootScope.GetScopeById(scopeId);
            var leases = scope.Leases.GetAllLeases();
            Assert.Equal(expectedAmount, leases.Count());

            DHCPv6Lease lease = leases.ElementAt(index);
            Assert.NotNull(lease);
            Assert.Equal(expectedAdress, lease.Address);
            Int32 expiresInMinutes = (Int32)(lease.End - DateTime.UtcNow).TotalMinutes;
            Assert.True(expiresInMinutes >= 60 * 24 - 4 && expiresInMinutes <= 60 * 24);
            Assert.True((expectedCreationData - lease.Start).TotalMinutes < 2);
            if (shouldBePending == true)
            {
                Assert.True(lease.IsPending());
            }
            else
            {
                Assert.True(lease.IsActive());
            }

            Assert.Equal(clientDuid, lease.ClientDUID);
            Assert.Equal(iaId, lease.IdentityAssocicationId);


            if (uniqueIdentifier == null)
            {
                Assert.Empty(lease.UniqueIdentifier);
            }
            else
            {
                Assert.NotNull(lease.UniqueIdentifier);
                Assert.Equal(uniqueIdentifier, lease.UniqueIdentifier);
            }

            if (shouldHavePrefix == false)
            {
                Assert.Equal(DHCPv6PrefixDelegation.None, lease.PrefixDelegation);

            }

            return lease;
        }

        protected static DHCPv6Lease CheckLeaseForPrefix(
           Int32 index, Int32 expectedAmount, IPv6Address minAddress, IPv6Address maxAddress,
           Guid scopeId, DHCPv6RootScope rootScope,
           DUID clientDuid, UInt32 prefixIaId, Byte prefixLength, Boolean shouldBePending, Byte[] uniqueIdentifier = null)
        {
            DHCPv6Scope scope = rootScope.GetScopeById(scopeId);
            var leases = scope.Leases.GetAllLeases();
            Assert.Equal(expectedAmount, leases.Count());

            DHCPv6Lease lease = leases.ElementAt(index);
            Assert.Equal(clientDuid, lease.ClientDUID);

            Assert.NotNull(lease);
            Assert.NotEqual(DHCPv6PrefixDelegation.None, lease.PrefixDelegation);
            Assert.Equal(prefixIaId, lease.PrefixDelegation.IdentityAssociation);
            Assert.Equal(prefixLength, lease.PrefixDelegation.Mask.Identifier);

            Assert.True((new IPv6SubnetMask(new IPv6SubnetMaskIdentifier(prefixLength))).IsIPv6AdressANetworkAddress(lease.PrefixDelegation.NetworkAddress));
            Assert.True(lease.PrefixDelegation.NetworkAddress.IsBetween(minAddress, maxAddress));

            if (shouldBePending == true)
            {
                Assert.True(lease.IsPending());
            }
            else
            {
                Assert.True(lease.IsActive());
            }

            if (uniqueIdentifier == null)
            {
                Assert.Empty(lease.UniqueIdentifier);
            }
            else
            {
                Assert.NotNull(lease.UniqueIdentifier);
                Assert.Equal(uniqueIdentifier, lease.UniqueIdentifier);
            }

            return lease;
        }

        protected static void CheckLeaseCreatedEvent(
         Int32 index, DUID clientDuid, UInt32 iaId,
         Guid scopeId, DHCPv6RootScope rootScope,
         IPv6Address expectedAdress, DHCPv6Lease lease,
          Byte[] uniqueIdentifier = null,
          Guid? ancestorId = null
         )
        {
            IEnumerable<DomainEvent> changes = rootScope.GetChanges();
            Assert.NotNull(changes);

            Assert.IsAssignableFrom<DHCPv6LeaseCreatedEvent>(changes.ElementAt(index));

            DHCPv6LeaseCreatedEvent createdEvent = (DHCPv6LeaseCreatedEvent)changes.ElementAt(index);
            Assert.NotNull(createdEvent);

            Assert.Equal(scopeId, createdEvent.ScopeId);
            Assert.Equal(expectedAdress, createdEvent.Address);
            Assert.Equal(clientDuid, createdEvent.ClientIdentifier);
            Assert.Equal(iaId, createdEvent.IdentityAssocationId);
            Assert.Equal(lease.Id, createdEvent.EntityId);
            Assert.Equal(ancestorId, createdEvent.AncestorId);
            if (uniqueIdentifier == null)
            {
                Assert.Null(createdEvent.UniqueIdentiifer);
            }
            else
            {
                Assert.Equal(uniqueIdentifier, createdEvent.UniqueIdentiifer);
            }
            Assert.Equal(lease.Start, createdEvent.StartedAt);
            Assert.Equal(lease.End, createdEvent.ValidUntil);

            if (lease.PrefixDelegation == DHCPv6PrefixDelegation.None)
            {
                Assert.Equal(IPv6Address.Empty, createdEvent.DelegatedNetworkAddress);
                Assert.Equal(0, createdEvent.PrefixLength);
                Assert.False(createdEvent.HasPrefixDelegation);
            }
        }

        protected static void CheckDHCPv6LeasePrefixAddedEvent(
             Int32 index, UInt32 prefixIaId,
             Guid scopeId, DHCPv6RootScope rootScope,
              DHCPv6Lease lease
 )
        {
            IEnumerable<DomainEvent> changes = rootScope.GetChanges();
            Assert.NotNull(changes);

            Assert.IsAssignableFrom<DHCPv6LeasePrefixAddedEvent>(changes.ElementAt(index));

            DHCPv6LeasePrefixAddedEvent createdEvent = (DHCPv6LeasePrefixAddedEvent)changes.ElementAt(index);
            Assert.NotNull(createdEvent);

            Assert.Equal(scopeId, createdEvent.ScopeId);
            Assert.Equal(lease.PrefixDelegation.NetworkAddress, createdEvent.NetworkAddress);
            Assert.Equal(lease.PrefixDelegation.IdentityAssociation, createdEvent.PrefixAssociationId);
            Assert.Equal(lease.PrefixDelegation.Mask.Identifier, createdEvent.PrefixLength);
            Assert.Equal(lease.PrefixDelegation.IdentityAssociation, prefixIaId);
        }

        protected static void DHCPv6ScopeAddressesAreExhaustedEvent(int index, DHCPv6RootScope rootScope, Guid scopeId)
        {
            IEnumerable<DomainEvent> changes = rootScope.GetChanges();

            Assert.IsAssignableFrom<DHCPv6ScopeAddressesAreExhaustedEvent>(changes.ElementAt(index));

            DHCPv6ScopeAddressesAreExhaustedEvent castedEvent = (DHCPv6ScopeAddressesAreExhaustedEvent)changes.ElementAt(index);

            Assert.Equal(castedEvent.EntityId, scopeId);
        }

        protected static void CheckPacket(DHCPv6Packet request,
            IPv6Address expectedAdress, UInt32 iaId, DHCPv6Packet response, DHCPv6PacketTypes type, DHCPv6PrefixDelegation prefixDelegation)
        {
            Assert.NotNull(response);
            Assert.NotEqual(DHCPv6Packet.Empty, response);
            Assert.True(response.IsValid);

            Assert.Equal(type, response.GetInnerPacket().PacketType);

            CheckIfPacketIsCorrectReply(request, response);

            if (iaId != 0)
            {
                var iaOption = response.GetInnerPacket().GetNonTemporaryIdentiyAssocation(iaId);
                Assert.NotNull(iaOption);

                var uncastedSuboption = iaOption.Suboptions.First();
                Assert.NotNull(uncastedSuboption);
                Assert.IsAssignableFrom<DHCPv6PacketIdentityAssociationAddressSuboption>(uncastedSuboption);

                var addressSubOption = (DHCPv6PacketIdentityAssociationAddressSuboption)uncastedSuboption;

                Assert.Equal(expectedAdress, addressSubOption.Address);

                Assert.NotEmpty(addressSubOption.Suboptions);
                Assert.Single(addressSubOption.Suboptions);

                var uncastedSubSuboption = addressSubOption.Suboptions.First();
                Assert.NotNull(uncastedSubSuboption);
                Assert.IsAssignableFrom<DHCPv6PacketStatusCodeSuboption>(uncastedSubSuboption);

                var statusSubOption = (DHCPv6PacketStatusCodeSuboption)uncastedSubSuboption;
                Assert.Equal((UInt16)DHCPv6StatusCodes.Success, statusSubOption.StatusCode);
                Assert.False(String.IsNullOrEmpty(statusSubOption.Message));
            }
            if (prefixDelegation != DHCPv6PrefixDelegation.None)
            {
                var iaOption = response.GetInnerPacket().GetPrefixDelegationIdentiyAssocation(prefixDelegation.IdentityAssociation);
                Assert.NotNull(iaOption);

                Assert.Equal(prefixDelegation.IdentityAssociation, iaOption.Id);

                Assert.NotEmpty(iaOption.Suboptions);
                Assert.Single(iaOption.Suboptions);

                var uncastedSuboption = iaOption.Suboptions.First();
                Assert.NotNull(uncastedSuboption);
                Assert.IsAssignableFrom<DHCPv6PacketIdentityAssociationPrefixDelegationSuboption>(uncastedSuboption);

                var addressSubOption = (DHCPv6PacketIdentityAssociationPrefixDelegationSuboption)uncastedSuboption;

                Assert.Equal(prefixDelegation.Mask.Identifier, addressSubOption.PrefixLength);

                Assert.NotEmpty(addressSubOption.Suboptions);
                Assert.Single(addressSubOption.Suboptions);

                var uncastedSubSuboption = addressSubOption.Suboptions.First();
                Assert.NotNull(uncastedSubSuboption);
                Assert.IsAssignableFrom<DHCPv6PacketStatusCodeSuboption>(uncastedSubSuboption);

                var statusSubOption = (DHCPv6PacketStatusCodeSuboption)uncastedSubSuboption;
                Assert.Equal((UInt16)DHCPv6StatusCodes.Success, statusSubOption.StatusCode);
                Assert.False(String.IsNullOrEmpty(statusSubOption.Message));
            }
        }

        private static void CheckIfPacketIsCorrectReply(DHCPv6Packet request, DHCPv6Packet response)
        {
            Assert.Equal(request.Header.Source, response.Header.Destionation);
            Assert.Equal(request.Header.Destionation, response.Header.Source);

            if (response is DHCPv6RelayPacket)
            {
                Assert.IsAssignableFrom<DHCPv6RelayPacket>(response);

                Assert.Equal(DHCPv6PacketTypes.RELAY_REPL, response.PacketType);

                if (request.HasOption(DHCPv6PacketOptionTypes.InterfaceId) == true)
                {
                    var requestInterfaceIdOption = request.GetOption<DHCPv6PacketByteArrayOption>(DHCPv6PacketOptionTypes.InterfaceId);
                    Assert.True(request.HasOption(DHCPv6PacketOptionTypes.InterfaceId));
                    var responseInterfaceIdOption = response.GetOption<DHCPv6PacketByteArrayOption>(DHCPv6PacketOptionTypes.InterfaceId);

                    Assert.Equal(requestInterfaceIdOption, responseInterfaceIdOption);
                }
            }
        }

        protected static void CheckErrorPacket(DHCPv6Packet request,
           IPv6Address expectedAdress, UInt32 iaId, DHCPv6Packet response, DHCPv6PrefixDelegation prefixDelegation, DHCPv6StatusCodes expectedStatusCode)
        {
            Assert.NotNull(response);
            Assert.NotEqual(DHCPv6Packet.Empty, response);
            Assert.True(response.IsValid);

            Assert.Equal(DHCPv6PacketTypes.REPLY, response.GetInnerPacket().PacketType);

            CheckIfPacketIsCorrectReply(request, response);

            if (iaId != 0)
            {
                var iaOption = response.GetInnerPacket().GetNonTemporaryIdentiyAssocation(iaId);
                Assert.NotNull(iaOption);

                Assert.Equal(TimeSpan.Zero, iaOption.T1);
                Assert.Equal(TimeSpan.Zero, iaOption.T2);

                var uncastedSuboption = iaOption.Suboptions.First();
                Assert.NotNull(uncastedSuboption);
                Assert.IsAssignableFrom<DHCPv6PacketIdentityAssociationAddressSuboption>(uncastedSuboption);

                var addressSubOption = (DHCPv6PacketIdentityAssociationAddressSuboption)uncastedSuboption;

                Assert.Equal(expectedAdress, addressSubOption.Address);
                Assert.Equal(TimeSpan.Zero, addressSubOption.PreferredLifetime);
                Assert.Equal(TimeSpan.Zero, addressSubOption.ValidLifetime);

                Assert.NotEmpty(addressSubOption.Suboptions);
                Assert.Single(addressSubOption.Suboptions);

                var uncastedSubSuboption = addressSubOption.Suboptions.First();
                Assert.NotNull(uncastedSubSuboption);
                Assert.IsAssignableFrom<DHCPv6PacketStatusCodeSuboption>(uncastedSubSuboption);

                var statusSubOption = (DHCPv6PacketStatusCodeSuboption)uncastedSubSuboption;
                Assert.Equal((UInt16)expectedStatusCode, statusSubOption.StatusCode);
                //Assert.False(String.IsNullOrEmpty(statusSubOption.Message));
            }
            if (prefixDelegation != DHCPv6PrefixDelegation.None)
            {
                var iaOption = response.GetInnerPacket().GetPrefixDelegationIdentiyAssocation(prefixDelegation.IdentityAssociation);
                Assert.NotNull(iaOption);

                Assert.Equal(prefixDelegation.IdentityAssociation, iaOption.Id);

                Assert.Equal(TimeSpan.Zero, iaOption.T1);
                Assert.Equal(TimeSpan.Zero, iaOption.T2);

                Assert.NotEmpty(iaOption.Suboptions);
                Assert.Single(iaOption.Suboptions);

                var uncastedSuboption = iaOption.Suboptions.First();
                Assert.NotNull(uncastedSuboption);
                Assert.IsAssignableFrom<DHCPv6PacketIdentityAssociationPrefixDelegationSuboption>(uncastedSuboption);

                var addressSubOption = (DHCPv6PacketIdentityAssociationPrefixDelegationSuboption)uncastedSuboption;

                Assert.Equal(prefixDelegation.Mask.Identifier, addressSubOption.PrefixLength);

                Assert.NotEmpty(addressSubOption.Suboptions);
                Assert.Single(addressSubOption.Suboptions);

                Assert.Equal(TimeSpan.Zero, addressSubOption.PreferredLifetime);
                Assert.Equal(TimeSpan.Zero, addressSubOption.ValidLifetime);

                var uncastedSubSuboption = addressSubOption.Suboptions.First();
                Assert.NotNull(uncastedSubSuboption);
                Assert.IsAssignableFrom<DHCPv6PacketStatusCodeSuboption>(uncastedSubSuboption);


                var statusSubOption = (DHCPv6PacketStatusCodeSuboption)uncastedSubSuboption;
                Assert.Equal((UInt16)(expectedStatusCode == DHCPv6StatusCodes.NoAddrsAvail ? DHCPv6StatusCodes.NoPrefixAvail : expectedStatusCode), statusSubOption.StatusCode);
                //Assert.False(String.IsNullOrEmpty(statusSubOption.Message));
            }
        }

        protected static void CheckPrefixPacket(
            DHCPv6Packet result, DHCPv6PacketTypes type, DHCPv6PrefixDelegation prefixDelegation)
        {
            Assert.NotNull(result);
            Assert.NotEqual(DHCPv6Packet.Empty, result);
            Assert.True(result.IsValid);

            Assert.Equal(type, result.PacketType);

            var iaOption = result.GetPrefixDelegationIdentiyAssocation(prefixDelegation.IdentityAssociation);
            Assert.NotNull(iaOption);

            var uncastedSuboption = iaOption.Suboptions.First();
            Assert.NotNull(uncastedSuboption);
            Assert.IsAssignableFrom<DHCPv6PacketIdentityAssociationPrefixDelegationSuboption>(uncastedSuboption);

            var addressSubOption = (DHCPv6PacketIdentityAssociationPrefixDelegationSuboption)uncastedSuboption;

            Assert.Equal(prefixDelegation.Mask.Identifier, addressSubOption.PrefixLength);
            Assert.Equal(prefixDelegation.NetworkAddress, addressSubOption.Address);

            Assert.NotEmpty(addressSubOption.Suboptions);
            Assert.Single(addressSubOption.Suboptions);

            var uncastedSubSuboption = addressSubOption.Suboptions.First();
            Assert.NotNull(uncastedSubSuboption);
            Assert.IsAssignableFrom<DHCPv6PacketStatusCodeSuboption>(uncastedSubSuboption);

            var statusSubOption = (DHCPv6PacketStatusCodeSuboption)uncastedSubSuboption;
            Assert.Equal((UInt16)DHCPv6StatusCodes.Success, statusSubOption.StatusCode);
            Assert.False(String.IsNullOrEmpty(statusSubOption.Message));
        }

        protected void CheckEmptyTrigger(INotificationTriggerSource triggerSource)
        {
            var triggers = triggerSource.GetTriggers();
            Assert.NotNull(triggers);
            Assert.Empty(triggers);
        }

        protected T CheckTrigger<T>(INotificationTriggerSource triggerSource) where T : NotifcationTrigger
        {
            var triggers = triggerSource.GetTriggers();
            Assert.NotNull(triggers);
            Assert.Single(triggers);

            Assert.IsAssignableFrom<T>(triggers.First());

            var trigger = (T)triggers.First();
            Assert.NotNull(trigger);

            return trigger;
        }
    }
}
