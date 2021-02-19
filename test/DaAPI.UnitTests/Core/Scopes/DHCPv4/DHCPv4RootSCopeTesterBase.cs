using DaAPI.Core.Common;
using DaAPI.Core.Packets.DHCPv4;
using DaAPI.Core.Scopes;
using DaAPI.Core.Scopes.DHCPv4;
using DaAPI.Core.Services;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using static DaAPI.Core.Scopes.DHCPv4.DHCPv4LeaseEvents;
using static DaAPI.Core.Scopes.DHCPv4.DHCPv4ScopeEvents;

namespace DaAPI.UnitTests.Core.Scopes.DHCPv4
{
    public abstract class DHCPv4RootScopeTesterBase
    {
        protected DHCPv4RootScope GetRootScope(Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>> scopeResolverMock)
        {
            Mock<ILoggerFactory> factoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);
            factoryMock.Setup(x => x.CreateLogger(It.IsAny<String>())).Returns(Mock.Of<ILogger<DHCPv4RootScope>>());

            var result = new DHCPv4RootScope(Guid.NewGuid(), scopeResolverMock.Object, factoryMock.Object);
            return result;
        }

        protected void CheckEventAmount(int expectedAmount, DHCPv4RootScope rootScope)
        {
            var changes = rootScope.GetChanges();
            Assert.Equal(expectedAmount, changes.Count());
        }

        protected static void CheckRevokedEvent(Int32 index, Guid scopeId, Guid leaseId, DHCPv4RootScope rootScope)
        {
            IEnumerable<DomainEvent> changes = rootScope.GetChanges();
            var @event = changes.ElementAt(index);
            Assert.NotNull(@event);
            Assert.IsAssignableFrom<DHCPv4LeaseRevokedEvent>(@event);

            DHCPv4LeaseRevokedEvent revokedEvent = (DHCPv4LeaseRevokedEvent)@event;

            Assert.Equal(scopeId, revokedEvent.ScopeId);
            Assert.Equal(leaseId, revokedEvent.EntityId);
        }
        protected static DHCPv4Lease CheckLease(
            Int32 index, Int32 expectedAmount, IPv4Address expectedAdress,
            Guid scopeId, DHCPv4RootScope rootScope, DateTime expectedCreationData,
            Byte[] uniqueIdentifier = null)
        {
            DHCPv4Scope scope = rootScope.GetScopeById(scopeId);
            var leases = scope.Leases.GetAllLeases();
            Assert.Equal(expectedAmount, leases.Count());

            DHCPv4Lease lease = leases.ElementAt(index);
            Assert.NotNull(lease);
            Assert.Equal(expectedAdress, lease.Address);
            Int32 expiresInMinutes = (Int32)(lease.End - DateTime.UtcNow).TotalMinutes;
            Assert.True(expiresInMinutes >= 60 * 24 - 4 && expiresInMinutes <= 60 * 24);
            Assert.True((expectedCreationData - lease.Start).TotalMinutes < 2);
            Assert.True(lease.IsPending());
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
         Int32 index, byte[] clientMacAdress,
         Guid scopeId, DHCPv4RootScope rootScope,
         IPv4Address expectedAdress, DHCPv4Lease lease,
         Byte[] uniqueIdentifier = null
         )
        {
            IEnumerable<DomainEvent> changes = rootScope.GetChanges();
            Assert.NotNull(changes);

            Assert.IsAssignableFrom<DHCPv4LeaseCreatedEvent>(changes.ElementAt(index));

            DHCPv4LeaseCreatedEvent createdEvent = (DHCPv4LeaseCreatedEvent)changes.ElementAt(index);
            Assert.NotNull(createdEvent);

            Assert.Equal(scopeId, createdEvent.ScopeId);
            Assert.Equal(expectedAdress, createdEvent.Address);
            Assert.Equal(clientMacAdress, createdEvent.HardwareAddress);
            Assert.Equal(lease.Id, createdEvent.EntityId);
            if (uniqueIdentifier == null)
            {
                Assert.Null(createdEvent.UniqueIdentifier);
            }
            else
            {
                Assert.Equal(uniqueIdentifier, createdEvent.UniqueIdentifier);
            }
            Assert.Equal(lease.Start, createdEvent.StartedAt);
            Assert.Equal(lease.End, createdEvent.ValidUntil);
        }
        protected static void DHCPv4ScopeAddressesAreExhaustedEvent(int index, DHCPv4RootScope rootScope, Guid scopeId)
        {
            IEnumerable<DomainEvent> changes = rootScope.GetChanges();

            Assert.IsAssignableFrom<DHCPv4ScopeAddressesAreExhaustedEvent>(changes.ElementAt(index));

            DHCPv4ScopeAddressesAreExhaustedEvent castedEvent = (DHCPv4ScopeAddressesAreExhaustedEvent)changes.ElementAt(index);

            Assert.Equal(castedEvent.EntityId, scopeId);
        }

        protected void CheckPacketOptions(Guid scopeId, DHCPv4RootScope rootScope, DHCPv4Packet result)
        {
            var scope = rootScope.GetScopeById(scopeId);

            if(scope.Properties != null)
            {     
                foreach (var item in scope.Properties.Properties)
                {

                }
            }

            var subnetOption = result.GetOptionByIdentifier((Byte)DHCPv4OptionTypes.SubnetMask) as DHCPv4PacketAddressOption;
            if(result.YourIPAdress == IPv4Address.Empty)
            {
                Assert.Null(subnetOption);
            }
            else
            {
                Assert.NotNull(subnetOption);
                Assert.True(ByteHelper.AreEqual(subnetOption.Address.GetBytes(), scope.AddressRelatedProperties.Mask.GetBytes()));
            }
        }
    }
}
