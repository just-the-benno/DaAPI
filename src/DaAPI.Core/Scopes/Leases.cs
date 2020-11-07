using DaAPI.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace DaAPI.Core.Scopes
{
    public abstract class Leases<TLeases, TLease, TAddress> : AggregateRoot
        where TLeases : Leases<TLeases, TLease, TAddress>
        where TAddress : IPAddress<TAddress>
        where TLease : Lease<TLease, TAddress>
    {
        #region Fields

        protected Dictionary<Guid, TLease> Entries { get; } = new Dictionary<Guid, TLease>();
        protected List<TLease> LatestLeases { get; } = new List<TLease>();

        private const int _maxAmountOfLeasesPerClient = 5;
        protected readonly Action<DomainEvent> _additonalApplier;

        #endregion

        #region constructor

        internal Leases(
            Guid id,
            Action<DomainEvent> additonalApplier) : base(id, additonalApplier)
        {
            EventHandlingStrategy = EventHandlingStratgies.Multiple;
            _additonalApplier = additonalApplier;

        }

        #endregion

        #region When and apply

        #endregion

        #region queries

        protected void AddEntry(Guid id, TLease lease)
        {
            Entries.Add(id, lease);
            LatestLeases.Add(lease);
        }

        protected void RemoveEntry(Guid id)
        {
            if(Entries.ContainsKey(id) == true)
            {
                TLease lease = Entries[id];
                LatestLeases.Remove(lease);
                Entries.Remove(id);
            }
        }

        public TLease GetAncestor(TLease lease)
        {
            if (lease.HasAncestor() == false) { return null; }

            if (Entries.ContainsKey(lease.AncestorId.Value) == false)
            {
                return null;
            }
            else
            {
                return Entries[lease.AncestorId.Value];
            }
        }

        protected void RemoveLease(TLease lease)
        {
            if (Entries.ContainsKey(lease.Id) == true)
            {
                Entries.Remove(lease.Id);
            }
        }

        public Boolean Contains(Guid leaseId)
        {
            return Entries.ContainsKey(leaseId);
        }

        public TLease GetLeaseById(Guid id)
        {
            if (Contains(id) == false) { return null; }
            return Entries[id];
        }

        public IEnumerable<TLease> GetAllLeases() => LatestLeases.AsEnumerable();

        protected TLease GetLeaseByExpression(Func<TLease, Boolean> expression)
        {
            TLease lease =  LatestLeases.LastOrDefault(expression);
            if (lease == null)
            {
                return null;
            }
            else
            {
                return lease;
            }
        }

        internal TLease GetLeaseByUniqueIdentifier(Byte[] value) =>
            GetLeaseByExpression(x => x.MatchesUniqueIdentiifer(value));

        internal TLease GetLeaseByAddress(TAddress address) =>
            GetLeaseByExpression(x => x.Address == address);

        public IReadOnlyList<TAddress> GetUsedAddresses() =>
            Entries.Values.Where(x => x.AddressIsInUse() == true).Select(x => x.Address).ToList().AsReadOnly();

        internal IEnumerable<TLease> CleanupExpiredLeases()
        {
            DateTime now = DateTime.UtcNow;
            List<TLease> result = new List<TLease>();
            foreach (var item in Entries.Values)
            {
                if (item.IsActive() && item.End <= now)
                {
                    item.Expired();
                    result.Add(item);
                }
            }

            return result;
        }

        internal int CleanupPendingLeases()
        {
            DateTime now = DateTime.UtcNow;
            Int32 amount = 0;
            foreach (var item in Entries.Values)
            {
                if (item.IsPending() == true && (now - item.Start).TotalMinutes > 5)
                {
                    item.Cancel(LeaseCancelReasons.ToLongPending);
                    amount += 1;
                }
            }

            return amount;
        }

        internal void DropUnusedLeasesOlderThan(DateTime threshold)
        {
            var keysToRemove = Entries.Where(x => x.Value.IsActive() == false && x.Value.End < threshold).Select(x => x.Key).ToList();
            foreach (var key in keysToRemove)
            {
                Entries.Remove(key);
            }
        }

        public IReadOnlyList<TAddress> GetSuspendedAddresses() =>
            Entries.Values.Where(x => x.State == LeaseStates.Suspended).Select(x => x.Address).ToList().AsReadOnly();

        public Boolean IsAddressSuspended(TAddress address) =>
            Entries.Values.Count(x => x.State == LeaseStates.Suspended && x.Address == address) > 0;

        public Boolean IsAddressActive(TAddress address) =>
            Entries.Values.Count(x => x.IsActive() == true && x.Address == address) > 0;

        internal void CancelAllLeases(LeaseCancelReasons reason)
        {
            foreach (TLease item in Entries.Values.Where(x => x.IsCancelable() == true))
            {
                item.Cancel(reason);
            }
        }

        private class ArrayComparer<T> : IEqualityComparer<T[]>
        {
            public bool Equals(T[] x, T[] y)
            {
                return x.SequenceEqual(y);
            }

            public int GetHashCode(T[] obj)
            {
                return obj.Aggregate(string.Empty, (s, i) => s + i.GetHashCode(), s => s.GetHashCode());
            }
        }

        protected void CleanEntries()
        {
            List<Guid> leaseIdsToRemove = new List<Guid>();

            foreach (var item in Entries.GroupBy(x => x.Value.ClientUniqueIdentifier,new ArrayComparer<Byte>()))
            {
                if(item.Count() <= _maxAmountOfLeasesPerClient)
                {
                    continue;
                }

                var nonActiveLeases = item.Where(x => x.Value.State != LeaseStates.Pending && x.Value.State != LeaseStates.Active);
                Int32 diff = nonActiveLeases.Count() - _maxAmountOfLeasesPerClient;
                if(diff > 0)
                {
                    leaseIdsToRemove.AddRange(nonActiveLeases.OrderBy(x => x.Value.Start).Select(x => x.Value.Id).Take(diff));
                }
            }

            foreach (var item in leaseIdsToRemove)
            {
                RemoveEntry(item);
            }
        }

        public IEnumerable<TLease> GetAllLeasesNotInRange(TAddress start, TAddress end) => Entries.Values.Where(x => x.Address.IsBetween(start, end) == false);

        #endregion

    }
}

