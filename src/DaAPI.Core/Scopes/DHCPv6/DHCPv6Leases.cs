using DaAPI.Core.Common;
using DaAPI.Core.Common.DHCPv6;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static DaAPI.Core.Scopes.DHCPv6.DHCPv6LeaseEvents;

namespace DaAPI.Core.Scopes.DHCPv6
{
    public class DHCPv6Leases : Leases<DHCPv6Leases, DHCPv6Lease, IPv6Address>
    {
        internal DHCPv6Leases(Guid id,
            Action<DomainEvent> additonalApplier) : base(id, additonalApplier)
        {

        }

        #region When and apply

        internal DHCPv6Lease AddLease(
            Guid id,
            IPv6Address address,
            TimeSpan lifetime,
            UInt32 identityAssociationId,
            DUID clientIdentifier,
            Byte[] uniqueIdentifier,
            DHCPv6PrefixDelegation prefixDelegation,
            DHCPv6Lease ancestor
            )
        {
            if (address == IPv6Address.Empty)
            {
                throw new ArgumentException("the ip address ::0 is invalid for a lease", nameof(address));
            }
            if (lifetime.TotalSeconds < 0)
            {
                throw new ArgumentException("the timespan has to be postive", nameof(lifetime));
            }

            Apply(new DHCPv6LeaseCreatedEvent
            {
                EntityId = id,
                Address = address,
                ClientIdentifier = clientIdentifier,
                IdentityAssocationId = identityAssociationId,
                UniqueIdentiifer = uniqueIdentifier,
                StartedAt = DateTime.UtcNow,
                ValidUntil = DateTime.UtcNow + lifetime,
                HasPrefixDelegation = prefixDelegation != DHCPv6PrefixDelegation.None,
                PrefixLength = prefixDelegation.Mask.Identifier,
                DelegatedNetworkAddress = prefixDelegation.NetworkAddress,
                IdentityAssocationIdForPrefix = prefixDelegation.IdentityAssociation,
                AncestorId = ancestor != null && ancestor.IsActive() ? ancestor.Id : new Guid?(),
            });

            var lease = GetLeaseById(id);
            return lease;
        }

        protected override void When(DomainEvent domainEvent)
        {
            DHCPv6Lease lease = null;
            switch (domainEvent)
            {
                case DHCPv6LeaseCreatedEvent e:
                    DHCPv6Lease leaseToAdd = new DHCPv6Lease(
                        e.EntityId,
                        e.Address,
                        e.StartedAt,
                        e.ValidUntil,
                        e.ClientIdentifier,
                        e.IdentityAssocationId,
                        e.UniqueIdentiifer,
                        e.HasPrefixDelegation == true ? new DHCPv6PrefixDelegation(e.DelegatedNetworkAddress, e.PrefixLength, e.IdentityAssocationIdForPrefix, e.StartedAt) : DHCPv6PrefixDelegation.None,
                        e.AncestorId,
                        _additonalApplier
                        );

                    if (Entries.ContainsKey(e.EntityId) == false)
                    {
                        AddEntry(e.EntityId, leaseToAdd);
                        CleanEntries();
                    }

                    break;
                case DHCPv6ScopeRelatedEvent e:
                    lease = GetLeaseById(e.EntityId);
                    break;
                default:
                    break;
            }

            if (lease != null)
            {
                if (domainEvent.IsHandled() == false)
                {
                    ApplyToEnity(lease, domainEvent);
                }
            }
        }

        internal DHCPv6Lease GetLeaseAssociationIdAndDuid(UInt32 associationId, DUID clientId) =>
            GetLeaseByExpression(x => x.IdentityAssocicationId == associationId && x.ClientDUID == clientId);

        internal DHCPv6Lease GetLeaseByClientDuid(DUID clientIdentifier) =>
            GetLeaseByExpression(x => x.ClientDUID == clientIdentifier);

        public IReadOnlyList<IPv6Address> GetUsedPrefixes() =>
          Entries.Values.Where(x => x.AddressIsInUse() == true && x.PrefixDelegation != DHCPv6PrefixDelegation.None).Select(x => x.PrefixDelegation.NetworkAddress).ToList().AsReadOnly();

        #endregion
    }
}
