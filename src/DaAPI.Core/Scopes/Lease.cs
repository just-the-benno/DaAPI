using DaAPI.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Scopes
{
    public enum LeaseCancelReasons
    {
        NotSpecified = 0,
        ResolverChanged = 1,
        ToLongPending = 2,
        AddressRangeChanged = 3,
    }

    public enum LeaseStates
    {
        Pending = 1,
        Active = 2,
        Revoked = 3,
        Released = 4,
        Inactive = 5,
        Suspended = 6,
        Canceled = 7,
    }

    public abstract class Lease<TLease, TAddress> : Entity
        where TLease : Lease<TLease, TAddress>
        where TAddress : IPAddress<TAddress>
    {
        protected static readonly TimeSpan DefaultSuspendTime = TimeSpan.FromMinutes(5);

        #region Properties

        public LeaseStates State { get; private set; }
        public TAddress Address { get; private set; }
        public Byte[] UniqueIdentifier { get; private set; }
        public DateTime Start { get; private set; }
        public DateTime End { get; private set; }
        public Guid? AncestorId { get; private set; }


        #endregion

        #region constructor and factories

        internal Lease(
            Guid id,
            TAddress address,
            DateTime start,
            DateTime end,
            Byte[] uniqueIdentifier,
            Guid? ancestorId,
            Action<DomainEvent> addtionalApplier
             ) : base(id, addtionalApplier)
        {
            State = LeaseStates.Pending;
            Address = address;
            Start = start;
            End = end;
            AncestorId = ancestorId;

            UniqueIdentifier = uniqueIdentifier == null ? Array.Empty<Byte>() : ByteHelper.CopyData(uniqueIdentifier);
        }

        public static TLease Empty => null;
        public static TLease NotFound => null;

        public abstract Byte[] ClientUniqueIdentifier { get; }

        #endregion

        #region applies and when

        public Boolean HasAncestor() => AncestorId.HasValue == true;

        internal abstract void Renew(TimeSpan value, Boolean reset);

        protected void CanRenew(TimeSpan value)
        {
            if (value.TotalMinutes < 0)
            {
                throw new ArgumentException("the renew time should be positive", nameof(value));
            }

            if (State != LeaseStates.Active && State != LeaseStates.Pending)
            {
                throw new InvalidOperationException("only an lease within state 'active' or 'pending' could be renewd");
            }
        }

        internal abstract void Reactived(TimeSpan value);

        protected void CanReactived(TimeSpan value)
        {
            if (value.TotalMinutes < 0)
            {
                throw new ArgumentException("the renew time should be positive", nameof(value));
            }

            List<LeaseStates> expectedStates = new List<LeaseStates>
            {
                LeaseStates.Canceled,
                LeaseStates.Released,
                LeaseStates.Revoked,
                LeaseStates.Suspended,
                LeaseStates.Inactive,
            };

            if (expectedStates.Contains(State) == false)
            {
                throw new InvalidOperationException("unable to reactive the leases based on its current state");
            }
        }


        internal abstract void Revoke();
        internal abstract void Release();
        internal abstract void Suspend(TimeSpan? suspentionTime);
        internal abstract void RemovePendingState();
        internal abstract void Expired();

        protected void CanRemovePendingState()
        {
            if (IsPending() == false)
            {
                throw new InvalidOperationException("the pending state can not remove if the lease is not pending andymore");
            }
        }

        protected void CanExpire()
        {
            if ((IsActive() || IsPending()) == false)
            {
                throw new InvalidOperationException("only active leases can be expiring");
            }
        }

        internal abstract void Cancel(LeaseCancelReasons reason);

        protected void CanCancel()
        {
            if (IsCancelable() == false)
            {
                throw new InvalidOperationException();
            }
        }

        protected void HandleRenew(DateTime end, Boolean reset)
        {
            End = end;
            if (reset == true)
            {
                State = LeaseStates.Pending;
            }
            else
            {
                State = LeaseStates.Active;
            }
        }

        protected TimeSpan GetSuspensionTime(TimeSpan? suspentionTime) => suspentionTime.HasValue == true ? suspentionTime.Value : DefaultSuspendTime;

        protected void HandleRevoked() => State = LeaseStates.Revoked;
        protected void HandleReleased() => State = LeaseStates.Released;
        protected void HandleSuspended() => State = LeaseStates.Suspended;
        protected void HandleActivated() => State = LeaseStates.Active;
        protected void HandleCanceled() => State = LeaseStates.Canceled;
        protected void HandleExpire() => State = LeaseStates.Inactive;


        #endregion

        #region queries

        public Boolean MatchesUniqueIdentiifer(byte[] value)
        {
            if (UniqueIdentifier == null || UniqueIdentifier.Length == 0)
            {
                return false;
            }

            return ByteHelper.AreEqual(value, UniqueIdentifier);
        }

        public Boolean AddressIsInUse()
        {
            return
                State != LeaseStates.Revoked &&
                State != LeaseStates.Released &&
                State != LeaseStates.Inactive &&
                State != LeaseStates.Canceled &&
                State != LeaseStates.Suspended;
        }

        public Boolean CanBeExtended() => State != LeaseStates.Revoked && State != LeaseStates.Suspended && State != LeaseStates.Canceled;

        public Boolean IsPending()
        {
            return State == LeaseStates.Pending;
        }

        public Boolean IsActive()
        {
            return State == LeaseStates.Active;
        }

        public Boolean IsCancelable()
        {
            return State == LeaseStates.Pending || State == LeaseStates.Active;
        }

        #endregion
    }
}

