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
    public class DHCPv4Leases : AggregateRoot
    {
        #region Fields

        private readonly Dictionary<Guid, DHCPv4Lease> _leases = new Dictionary<Guid, DHCPv4Lease>();
        private readonly Action<DomainEvent> _additonalApplier;

        #endregion

        #region constructor

        internal DHCPv4Leases(
            Guid id,
            Action<DomainEvent> additonalApplier) : base(id, additonalApplier)
        {
            EventHandlingStrategy = EventHandlingStratgies.Multiple;
            _additonalApplier = additonalApplier;

        }

        #endregion

        #region When and apply

        protected override void When(DomainEvent domainEvent)
        {
            DHCPv4Lease lease = null;
            switch (domainEvent)
            {
                case DHCPv4LeaseRenewedEvent e:
                    lease = GetLeaseById(e.EntityId);
                    break;
                case DHCPv4LeaseRevokedEvent e:
                    lease = GetLeaseById(e.EntityId);
                    break;
                case DHCPv4LeaseReleasedEvent e:
                    lease = GetLeaseById(e.EntityId);
                    break;
                case DHCPv4AddressSuspendedEvent e:
                    lease = GetLeaseById(e.EntityId);
                    break;
                case DHCPv4LeaseActivatedEvent e:
                    lease = GetLeaseById(e.EntityId);
                    break;
                case DHCPv4LeaseExpiredEvent e:
                    lease = GetLeaseById(e.EntityId);
                    break;
                case DHCPv4LeaseCanceledEvent e:
                    lease = GetLeaseById(e.EntityId);
                    break;
                case DHCPv4LeaseCreatedEvent e:
                    DHCPv4Lease leaseToAdd = new DHCPv4Lease(
                        e.EntityId,
                        e.Address,
                        e.StartedAt,
                        e.ValidUntil,
                        e.ClientIdentifier,
                        e.UniqueIdentiifer,
                        _additonalApplier
                        );

                    _leases.Add(e.EntityId, leaseToAdd);

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

        #endregion
        internal void AddLease(
            Guid id,
            IPv4Address address,
            TimeSpan lifetime,
            DHCPv4ClientIdentifier clientIdentifier,
            Byte[] uniqueIdentifier
            )
        {
            if(address == IPv4Address.Empty)
            {
                throw new ArgumentException("the ip address 0.0.0.0 is invalid for an lease", nameof(address));
            }
            if(lifetime.TotalSeconds < 0)
            {
                throw new ArgumentException("the timespan has to be postive", nameof(lifetime));
            }

            Apply(new DHCPv4LeaseCreatedEvent
            {
                EntityId = id,
                Address = address,
                ClientIdentifier = clientIdentifier,
                UniqueIdentiifer = uniqueIdentifier,
                StartedAt = DateTime.UtcNow,
                ValidUntil = DateTime.UtcNow + lifetime
            });
        }

        #region queries

        public Boolean Contains(Guid leaseId)
        {
            return _leases.ContainsKey(leaseId);
        }

        public DHCPv4Lease GetLeaseById(Guid id)
        {
            if(Contains(id) == false) { return DHCPv4Lease.Empty; }
            return _leases[id];
        }

        public IEnumerable<DHCPv4Lease> GetAllLeases() => _leases.Values.AsEnumerable();

        private DHCPv4Lease GetLeaseByExpression(Func<DHCPv4Lease,Boolean> expression)
        {
            DHCPv4Lease lease = _leases.Values.FirstOrDefault(expression);
            if (lease == null)
            {
                return DHCPv4Lease.Empty;
            }
            else
            {
                return lease;
            }
        }

        internal DHCPv4Lease GetLeaseByClientIdentifier(DHCPv4ClientIdentifier clientIdentifier)
        {
            return GetLeaseByExpression(x => x.ClientIdentifier == clientIdentifier);
        }

        internal DHCPv4Lease GetLeaseByUniqueIdentifier(byte[] value)
        {
            return GetLeaseByExpression(x => x.MatchesUniqueIdentiifer(value));
        }

        internal DHCPv4Lease GetLeaseByIPAddress(IPv4Address address)
        {
            return GetLeaseByExpression(x => x.Address == address);
        }

        public IReadOnlyList<IPv4Address> GetUsedAddresses()
        {
            return _leases.Values.Where(x => x.AddressIsInUse() == true).Select(x => x.Address).ToList().AsReadOnly();
        }

        public IReadOnlyList<IPv4Address> GetSuspendedAddresses()
        {
            return _leases.Values.Where(x => x.State == DHCPv4Lease.DHCPv4LeaseStates.Suspended).Select(x => x.Address).ToList().AsReadOnly();
        }

        public Boolean IsAddressSuspended(IPv4Address address)
        {
            Int32 amount = _leases.Values.Count(x => x.State == DHCPv4Lease.DHCPv4LeaseStates.Suspended && x.Address == address);
            return amount > 0;
        }

        public Boolean IsAddressActive(IPv4Address address)
        {
            Int32 amount = _leases.Values.Count(x => x.IsActive() == true && x.Address == address);
            return amount > 0;
        }

        internal void CancelAllLeases(DHCPv4LeaseCancelReasons reason)
        {
            foreach (DHCPv4Lease item in _leases.Values.Where(x => x.IsCancelable() == true))
            {
                item.Cancel(reason);
            }
        }

        #endregion

    }
}
