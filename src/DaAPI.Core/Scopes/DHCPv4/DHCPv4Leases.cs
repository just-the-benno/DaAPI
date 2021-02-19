using DaAPI.Core.Common;
using DaAPI.Core.Scopes.DHCPv4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using static DaAPI.Core.Scopes.DHCPv4.DHCPv4Lease;
using static DaAPI.Core.Scopes.DHCPv4.DHCPv4LeaseEvents;

namespace DaAPI.Core.Scopes.DHCPv4
{
    public class DHCPv4Leases : Leases<DHCPv4Leases, DHCPv4Lease, IPv4Address>
    {
        internal DHCPv4Leases(Guid id,
              Action<DomainEvent> additonalApplier) : base(id, additonalApplier)
        {

        }

        #region When and apply

        internal DHCPv4Lease AddLease(
            Guid id,
            IPv4Address address,
            TimeSpan lifetime,
            DHCPv4ClientIdentifier identifier,
            Byte[] uniqueIdentifier,
            DHCPv4Lease ancestor
            )
        {
            if (address == IPv4Address.Empty)
            {
                throw new ArgumentException("the ip address ::0 is invalid for a lease", nameof(address));
            }
            if (lifetime.TotalSeconds < 0)
            {
                throw new ArgumentException("the timespan has to be postive", nameof(lifetime));
            }

            Apply(new DHCPv4LeaseCreatedEvent
            {
                EntityId = id,
                Address = address,
                HardwareAddress = identifier.HwAddress ?? Array.Empty<Byte>(),
                ClientDUID = identifier.DUID,
                UniqueIdentifier = uniqueIdentifier,
                StartedAt = DateTime.UtcNow,
                ValidUntil = DateTime.UtcNow + lifetime,
                AncestorId = ancestor != null && ancestor.IsActive() ? ancestor.Id : new Guid?(),
            });

            var lease = GetLeaseById(id);
            return lease;
        }

        protected override void When(DomainEvent domainEvent)
        {
            DHCPv4Lease lease = null;
            switch (domainEvent)
            {
                case DHCPv4LeaseCreatedEvent e:
                    DHCPv4Lease leaseToAdd = new DHCPv4Lease(
                        e.EntityId,
                        e.Address,
                        e.StartedAt,
                        e.ValidUntil,
                        e.ClientDUID != DUID.Empty ? DHCPv4ClientIdentifier.FromDuid(e.ClientDUID, e.HardwareAddress) : DHCPv4ClientIdentifier.FromHwAddress(e.HardwareAddress),
                        e.UniqueIdentifier,
                        e.AncestorId,
                        _additonalApplier
                        );

                    if (Entries.ContainsKey(e.EntityId) == false)
                    {
                        AddEntry(e.EntityId, leaseToAdd);
                        CleanEntries();
                    }

                    break;
                case DHCPv4ScopeRelatedEvent e:
                    lease = GetLeaseById(e.EntityId);
                    break;
                default:
                    break;
            }

            if (domainEvent?.IsHandled() == false)
            {
                ApplyToEnity(lease, domainEvent);
            }
        }

        internal DHCPv4Lease GetLeaseByClientIdentifier(DHCPv4ClientIdentifier clientIdentifier) => GetLeaseByExpression(x => x.Identifier == clientIdentifier);


        #endregion

    }
}
