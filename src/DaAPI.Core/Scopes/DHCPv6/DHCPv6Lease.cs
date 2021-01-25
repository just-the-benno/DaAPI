using DaAPI.Core.Common;
using DaAPI.Core.Common.DHCPv6;
using System;
using System.Collections.Generic;
using System.Text;
using static DaAPI.Core.Scopes.DHCPv6.DHCPv6LeaseEvents;

namespace DaAPI.Core.Scopes.DHCPv6
{
    public class DHCPv6Lease : Lease<DHCPv6Lease, IPv6Address>
    {
        #region Properties

        public DUID ClientDUID { get; private set; }
        public override Byte[] ClientUniqueIdentifier => ClientDUID.GetAsByteStream();
        public UInt32 IdentityAssocicationId { get; internal set; }
        public DHCPv6PrefixDelegation PrefixDelegation { get; private set; }


        #endregion

        #region constructor and factories

        internal DHCPv6Lease(
            Guid id,
            IPv6Address address,
            DateTime start,
            DateTime end,
            DUID clientIdentifier,
            UInt32 identityAssociationId,
            Byte[] uniqueIdentifier,
            DHCPv6PrefixDelegation prefix,
            Guid? ancestorId,
            Action<DomainEvent> addtionalApplier
             ) : base(id, address, start, end, uniqueIdentifier, ancestorId, addtionalApplier)
        {
            ClientDUID = clientIdentifier;
            IdentityAssocicationId = identityAssociationId;
            PrefixDelegation = prefix;
        }

        #endregion

        internal override void Renew(TimeSpan value, Boolean reset) => Renew(value, reset, true);

        internal void Renew(TimeSpan value, Boolean reset, Boolean resetPrefix)
        {
            CanRenew(value);

            if (resetPrefix == true && PrefixDelegation == DHCPv6PrefixDelegation.None)
            {
                resetPrefix = false;
            }

            base.Apply(new DHCPv6LeaseRenewedEvent(this.Id, DateTime.UtcNow + value, reset, resetPrefix));
        }

        internal override void Reactived(TimeSpan value)
        {
            CanReactived(value);

            base.Apply(new DHCPv6LeaseRenewedEvent(this.Id, DateTime.UtcNow + value, false, true));
        }

        internal override void Revoke() => base.Apply(new DHCPv6LeaseRevokedEvent(this.Id));
        internal void Release(Boolean onlyPrefix) => base.Apply(new DHCPv6LeaseReleasedEvent(this.Id, onlyPrefix));

        internal override void Release() => Release(false);

        internal override void Suspend(TimeSpan? suspentionTime)
        {
            TimeSpan timeToAdd = GetSuspensionTime(suspentionTime);

            base.Apply(new DHCPv6AddressSuspendedEvent(this.Id, this.Address, DateTime.UtcNow + timeToAdd));
        }

        internal override void Cancel(LeaseCancelReasons reason)
        {
            CanCancel();
            base.Apply(new DHCPv6LeaseCanceledEvent(Id, reason));
        }

        internal override void RemovePendingState()
        {
            CanRemovePendingState();
            base.Apply(new DHCPv6LeaseActivatedEvent(this.Id));
        }

        internal override void Expired()
        {
            CanExpire();
            base.Apply(new DHCPv6LeaseExpiredEvent(this.Id));
        }

        internal void UpdateAddressPrefix(DHCPv6PrefixDelegation prefix, Boolean acceptPendingState)
        {

            if (((acceptPendingState == true && IsPending()) || IsActive() == true) == false)
            {
                throw new InvalidOperationException("the pending state can not remove if the lease is not pending andymore");
            }

            base.Apply(new DHCPv6LeasePrefixAddedEvent(Id)
            {
                NetworkAddress = prefix.NetworkAddress,
                PrefixAssociationId = prefix.IdentityAssociation,
                PrefixLength = prefix.Mask.Identifier,
                Started = DateTime.UtcNow,
            });
        }

        internal void ActivateAddressPrefix()
        {
            if (IsActive() == false)
            {
                throw new InvalidOperationException("activating a prefix is only possible if the lease is current active");
            }

            base.Apply(new DHCPv6LeasePrefixActvatedEvent(Id)
            {
                NetworkAddress = PrefixDelegation.NetworkAddress,
                PrefixAssociationId = PrefixDelegation.IdentityAssociation,
                PrefixLength = PrefixDelegation.Mask.Identifier,
            });
        }

        protected override void When(DomainEvent domainEvent)
        {
            switch (domainEvent)
            {
                case DHCPv6LeaseRenewedEvent e:
                    HandleRenew(e.End, e.Reset);
                    if (e.ResetPrefix == true)
                    {
                        PrefixDelegation = DHCPv6PrefixDelegation.None;
                    }
                    break;
                case DHCPv6LeaseRevokedEvent _:
                    HandleRevoked();
                    break;
                case DHCPv6LeaseReleasedEvent e:
                    if (e.OnlyPrefix == true)
                    {
                        PrefixDelegation = DHCPv6PrefixDelegation.None;
                    }
                    else
                    {
                        HandleReleased();
                    }
                    break;
                case DHCPv6AddressSuspendedEvent _:
                    HandleSuspended();
                    break;
                case DHCPv6LeaseActivatedEvent _:
                    HandleActivated();
                    break;
                case DHCPv6LeaseCanceledEvent _:
                    HandleCanceled();
                    break;
                case DHCPv6LeaseExpiredEvent _:
                    HandleExpire();
                    break;
                case DHCPv6LeasePrefixAddedEvent e:
                    PrefixDelegation = new DHCPv6PrefixDelegation(e.NetworkAddress, e.PrefixLength, e.PrefixAssociationId,e.Started);
                    break;
                default:
                    break;
            }
        }


    }
}
