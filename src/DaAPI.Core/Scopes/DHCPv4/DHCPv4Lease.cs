using DaAPI.Core.Common;
using DaAPI.Core.Scopes.DHCPv4;
using System;
using System.Collections.Generic;
using System.Text;
using static DaAPI.Core.Scopes.DHCPv4.DHCPv4LeaseEvents;

namespace DaAPI.Core.Scopes.DHCPv4
{
    public class DHCPv4Lease : Entity
    {
        public enum DHCPv4LeaseCancelReasons
        {
            NotSpecified = 0,
            ResolverChanged = 1,
        }

        public enum DHCPv4LeaseStates
        {
            Pending = 1,
            Active = 2,
            Revoked = 3,
            Released = 4,
            Inactive = 5,
            Suspended = 6,
            Canceled = 7,
        }

        private static readonly TimeSpan _defaultSuspendTime = TimeSpan.FromMinutes(5);

        #region Properties

        public DHCPv4LeaseStates State { get; private set; }
        public IPv4Address Address { get; private set; }
        public Byte[] UniqueIdentifier { get; private set; }
        public DHCPv4ClientIdentifier ClientIdentifier { get; set; }
        public DateTime Start { get; private set; }
        public DateTime End { get; private set; }

        #endregion

        #region constructor and factories

        internal DHCPv4Lease(
            Guid id,
            IPv4Address address,
            DateTime start,
            DateTime end,
            DHCPv4ClientIdentifier clientIdentifier,
            Byte[] uniqueIdentifier,
            Action<DomainEvent> addtionalApplier
             ) : base(id, addtionalApplier)
        {
            State = DHCPv4LeaseStates.Pending;
            Address = IPv4Address.FromAddress(address);
            Start = start;
            End = end;

            UniqueIdentifier = uniqueIdentifier == null ? Array.Empty<Byte>() : ByteHelper.CopyData(uniqueIdentifier);
            ClientIdentifier = clientIdentifier;
        }

        public static DHCPv4Lease Empty => null;

        #endregion

        #region applies and when

        internal void Renew(TimeSpan value, Boolean reset)
        {
            if (value.TotalMinutes < 0)
            {
                throw new ArgumentException("the renew time should be positive", nameof(value));
            }

            if (State != DHCPv4LeaseStates.Active && State != DHCPv4LeaseStates.Pending)
            {
                throw new InvalidOperationException("only an lease within state 'active' or 'pending' could be renewd");
            }

            base.Apply(new DHCPv4LeaseRenewedEvent(this.Id, DateTime.UtcNow + value, reset));
        }

        internal void Reactived(TimeSpan value)
        {
            if (value.TotalMinutes < 0)
            {
                throw new ArgumentException("the renew time should be positive", nameof(value));
            }

            List<DHCPv4LeaseStates> expectedStates = new List<DHCPv4LeaseStates>
            {
            DHCPv4LeaseStates.Canceled, 
                DHCPv4LeaseStates.Released, 
                DHCPv4LeaseStates.Revoked, 
                DHCPv4LeaseStates.Suspended,
                DHCPv4LeaseStates.Inactive,

            };

            if (expectedStates.Contains(State) == false)
            {
                throw new InvalidOperationException("unable to reactive the leases based on its current state");
            }

            base.Apply(new DHCPv4LeaseRenewedEvent(this.Id, DateTime.UtcNow + value, false));
        }

        internal void Revoke()
        {
            base.Apply(new DHCPv4LeaseRevokedEvent(this.Id));
        }

        internal void Release()
        {
            base.Apply(new DHCPv4LeaseReleasedEvent(this.Id));
        }

        internal void Suspend(TimeSpan? suspentionTime)
        {
            TimeSpan timeToAdd = _defaultSuspendTime;
            if (suspentionTime.HasValue == true)
            {
                timeToAdd = suspentionTime.Value;
            }

            base.Apply(new DHCPv4AddressSuspendedEvent(this.Id, this.Address, DateTime.UtcNow + timeToAdd));
        }

        internal void RemovePendingState()
        {
            if (IsPending() == false)
            {
                throw new InvalidOperationException("the pending state can not remove if the lease is not pending andymore");
            }

            base.Apply(new DHCPv4LeaseActivatedEvent(this.Id));
        }

        internal void Cancel(DHCPv4LeaseCancelReasons reason)
        {
            if (IsCancelable() == false)
            {
                throw new InvalidOperationException();
            }

            base.Apply(new DHCPv4LeaseCanceledEvent(Id, reason));
        }

        protected override void When(DomainEvent domainEvent)
        {
            switch (domainEvent)
            {
                case DHCPv4LeaseRenewedEvent e:
                    End = e.End;
                    if (e.Reset == true)
                    {
                        State = DHCPv4LeaseStates.Pending;
                    }
                    else
                    {
                        State = DHCPv4LeaseStates.Active;
                    }
                    break;
                case DHCPv4LeaseRevokedEvent _:
                    State = DHCPv4LeaseStates.Revoked;
                    break;
                case DHCPv4LeaseReleasedEvent _:
                    State = DHCPv4LeaseStates.Released;
                    break;
                case DHCPv4AddressSuspendedEvent _:
                    State = DHCPv4LeaseStates.Suspended;
                    break;
                case DHCPv4LeaseActivatedEvent _:
                    State = DHCPv4LeaseStates.Active;
                    break;
                case DHCPv4LeaseCanceledEvent _:
                    State = DHCPv4LeaseStates.Canceled;
                    break;
                case DHCPv4LeaseExpiredEvent _:
                    State = DHCPv4LeaseStates.Inactive;
                    break;
                default:
                    break;
            }
        }

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
                State != DHCPv4LeaseStates.Revoked &&
                State != DHCPv4LeaseStates.Released &&
                State != DHCPv4LeaseStates.Inactive &&
                State != DHCPv4LeaseStates.Suspended;
        }

        public Boolean IsPending()
        {
            return State == DHCPv4LeaseStates.Pending;
        }

        public Boolean IsActive()
        {
            return State == DHCPv4LeaseStates.Active;
        }

        public Boolean IsCancelable()
        {
            return State == DHCPv4LeaseStates.Pending || State == DHCPv4LeaseStates.Active;
        }

        #endregion
    }
}
