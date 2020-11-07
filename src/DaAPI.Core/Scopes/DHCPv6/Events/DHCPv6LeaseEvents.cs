using DaAPI.Core.Common;
using DaAPI.Core.Common.DHCPv6;
using System;
using System.Collections.Generic;
using System.Text;
using static DaAPI.Core.Scopes.DHCPv6.DHCPv6Lease;

namespace DaAPI.Core.Scopes.DHCPv6
{
    public static class DHCPv6LeaseEvents
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "<Pending>")]
        public abstract class DHCPv6ScopeRelatedEvent : EntityBasedDomainEvent
        {
            public Guid ScopeId { get; set; }

            protected DHCPv6ScopeRelatedEvent()
            {
            }

            protected DHCPv6ScopeRelatedEvent(Guid id) : base(id)
            {
            }
        }

        public class DHCPv6AddressSuspendedEvent : DHCPv6ScopeRelatedEvent
        {
            public IPv6Address Address { get; set; }
            public DateTime SuspendedTill { get; set; }

            public DHCPv6AddressSuspendedEvent()
            {

            }

            public DHCPv6AddressSuspendedEvent(Guid leaseId, IPv6Address address, DateTime suspendTill) : base(leaseId)
            {
                Address = address;
                SuspendedTill = suspendTill;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "<Pending>")]
        public class DHCPv6LeaseActivatedEvent : DHCPv6ScopeRelatedEvent
        {
            public DHCPv6LeaseActivatedEvent()
            {

            }

            public DHCPv6LeaseActivatedEvent(Guid leaseId) : base(leaseId)
            {

            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "<Pending>")]
        public class DHCPv6LeaseExpiredEvent : DHCPv6ScopeRelatedEvent
        {
            public DHCPv6LeaseExpiredEvent()
            {

            }

            public DHCPv6LeaseExpiredEvent(Guid leaseId) : base(leaseId)
            {

            }
        }

        public class DHCPv6LeasePrefixAddedEvent : DHCPv6ScopeRelatedEvent
        {
            public Byte PrefixLength { get; set; }
            public UInt32 PrefixAssociationId { get; set; }
            public IPv6Address NetworkAddress { get; set; }
            public DateTime Started { get; set; }

            public DHCPv6LeasePrefixAddedEvent()
            {

            }

            public DHCPv6LeasePrefixAddedEvent(Guid leaseId) : base(leaseId)
            {

            }
        }

        public class DHCPv6LeasePrefixActvatedEvent : DHCPv6ScopeRelatedEvent
        {
            public Byte PrefixLength { get; set; }
            public UInt32 PrefixAssociationId { get; set; }
            public IPv6Address NetworkAddress { get; set; }

            public DHCPv6LeasePrefixActvatedEvent()
            {

            }

            public DHCPv6LeasePrefixActvatedEvent(Guid leaseId) : base(leaseId)
            {

            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "<Pending>")]
        public class DHCPv6LeaseCanceledEvent : DHCPv6ScopeRelatedEvent
        {
            public LeaseCancelReasons Reason { get; set; }

            public DHCPv6LeaseCanceledEvent()
            {

            }

            public DHCPv6LeaseCanceledEvent(Guid leaseId) : this(leaseId, LeaseCancelReasons.NotSpecified)
            {

            }

            public DHCPv6LeaseCanceledEvent(Guid leaseId, LeaseCancelReasons reason) : base(leaseId)
            {
                Reason = reason;
            }

            public DHCPv6LeaseCanceledEvent(Guid leaseId, Guid scopeId, LeaseCancelReasons reason) : this(leaseId, reason)
            {
                ScopeId = scopeId;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "<Pending>")]
        public class DHCPv6LeaseCreatedEvent : DHCPv6ScopeRelatedEvent
        {
            #region Properties

            public IPv6Address Address { get; set; }
            public DUID ClientIdentifier { get; set; }
            public DateTime StartedAt { get; set; }
            public DateTime ValidUntil { get; set; }
            public Byte[] UniqueIdentiifer { get; set; }
            public UInt32 IdentityAssocationId { get; set; }
            public Boolean HasPrefixDelegation { get; set; }
            public IPv6Address DelegatedNetworkAddress { get; set; }
            public Byte PrefixLength { get; set; }
            public UInt32 IdentityAssocationIdForPrefix { get; set; }

            public Guid? AncestorId { get; set; }

            #endregion

            #region Constructor

            #endregion

        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "<Pending>")]

        public class DHCPv6LeaseReleasedEvent : DHCPv6ScopeRelatedEvent
        {
            public bool OnlyPrefix { get; set; }

            public DHCPv6LeaseReleasedEvent()
            {

            }

            public DHCPv6LeaseReleasedEvent(Guid leaseId, Boolean onlyPrefix) : base(leaseId)
            {
                OnlyPrefix = onlyPrefix;
            }

        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "<Pending>")]
        public class DHCPv6LeaseRenewedEvent : DHCPv6ScopeRelatedEvent
        {
            public DateTime End { get; set; }
            public Boolean Reset { get; set; }
            public Boolean ResetPrefix { get; set; }

            public DHCPv6LeaseRenewedEvent()
            {

            }

            public DHCPv6LeaseRenewedEvent(Guid leaseId, DateTime end, Boolean reset, Boolean resetPrefix)
            {
                EntityId = leaseId;
                End = end;
                Reset = reset;
                ResetPrefix = resetPrefix;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "<Pending>")]
        public class DHCPv6LeaseRevokedEvent : DHCPv6ScopeRelatedEvent
        {
            public DHCPv6LeaseRevokedEvent()
            {

            }

            public DHCPv6LeaseRevokedEvent(Guid leaseId) : base(leaseId)
            {

            }
        }

    }
}
