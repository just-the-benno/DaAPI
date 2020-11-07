using DaAPI.Core.Common;
using DaAPI.Core.Packets.DHCPv4;
using DaAPI.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static DaAPI.Core.Packets.DHCPv4.DHCPv4Packet;
using static DaAPI.Core.Scopes.DHCPv4.DHCPv4LeaseEvents;
using static DaAPI.Core.Scopes.DHCPv4.DHCPv4PacketHandledEvents;
using static DaAPI.Core.Scopes.DHCPv4.DHCPv4ScopeEvents;

namespace DaAPI.Core.Scopes.DHCPv4
{
    public class DHCPv4Scope : AggregateRoot
    {
        #region Fields

        private readonly List<DHCPv4Scope> _subscopes;
        private readonly IReadOnlyList<DHCPv4Scope> _returnScopes;

        #endregion

        #region Properties

        public ScopeName Name { get; private set; }
        public ScopeDescription Description { get; private set; }
        public Boolean IsSuspendend { get; private set; }

        public IDHCPv4ScopeResolver Resolver { get; private set; }
        public DHCPv4Scope ParentScope { get; private set; }
        public IReadOnlyList<DHCPv4Scope> Subscopes => _returnScopes;

        public DHCPv4ScopeProperties Properties { get; private set; }
        public DHCPv4ScopeAddressProperties AddressRelatedProperties { get; private set; }

        public DHCPv4Leases Leases { get; private set; }

        #endregion

        #region constructor and factories

        private DHCPv4Scope(Guid id, Action<DomainEvent> addtionalApplier) : base(id, addtionalApplier)
        {
            Properties = new DHCPv4ScopeProperties();
            _subscopes = new List<DHCPv4Scope>();
            _returnScopes = _subscopes.AsReadOnly();
            Leases = new DHCPv4Leases(Guid.NewGuid(), (e) =>
            {
                switch (e)
                {
                    case DHCPv4ScopeRelatedEvent ce:
                        if (ce.ScopeId == Guid.Empty)
                        {
                            ce.ScopeId = Id;
                        }
                        break;
                    default:
                        break;
                }
                addtionalApplier(e);
            });

            base.EventHandlingStrategy = EventHandlingStratgies.Multiple;
        }

        internal static DHCPv4Scope FromInstructions(
            DHCPv4ScopeCreateInstruction instructions,
            IDHCPv4ScopeResolverManager resolverManager,
            Action<DomainEvent> rootApplier
            )
        {
            DHCPv4Scope scope = new DHCPv4Scope(instructions.Id, rootApplier)
            {
                Name = new ScopeName(instructions.Name),
                Description = new ScopeDescription(instructions.Description),
                Properties = instructions.Properties,
                AddressRelatedProperties = instructions.AddressProperties,
            };

            IDHCPv4ScopeResolver resolver = resolverManager.InitializeResolver(instructions.ResolverInformations);
            scope.Resolver = resolver;

            return scope;
        }

        public static DHCPv4Scope NotFound => null;


        #endregion

        #region queries

        private void GetProperties(DHCPv4ScopeProperties result)
        {
            if (ParentScope != null)
            {
                ParentScope.GetProperties(result);
            }

            result.OverrideProperties(Properties);
        }

        public DHCPv4ScopeProperties GetScopeProperties()
        {
            DHCPv4ScopeProperties result = DHCPv4ScopeProperties.Empty;
            GetProperties(result);

            return result;
        }

        private void GetAddressProperties(DHCPv4ScopeAddressProperties result)
        {
            if (ParentScope != null)
            {
                ParentScope.GetAddressProperties(result);
            }

            result.OverrideProperties(AddressRelatedProperties);
        }

        public DHCPv4ScopeAddressProperties GetAddressProperties()
        {
            DHCPv4ScopeAddressProperties result = DHCPv4ScopeAddressProperties.Empty;
            GetAddressProperties(result);

            return result;
        }

        public IEnumerable<Guid> GetChildIds(Boolean onlyDirectChildren)
        {
            if (onlyDirectChildren == true)
            {
                return new List<Guid>(_subscopes.Select(x => x.Id));
            }

            List<Guid> result = new List<Guid>();

            foreach (DHCPv4Scope item in _subscopes)
            {
                item.GetChildIds(result, true);
            }

            return result;
        }

        private void GetChildIds(ICollection<Guid> ids, Boolean includeChildren)
        {
            ids.Add(this.Id);
            if (includeChildren == true)
            {
                foreach (DHCPv4Scope child in _subscopes)
                {
                    child.GetChildIds(ids, true);
                }
            }
        }

        public IEnumerable<DHCPv4Scope> GetChildScopes() => _subscopes.AsEnumerable();

        #endregion

        #region packet handling

        internal DHCPv4Packet HandleDiscover(DHCPv4Packet packet)
        {
            DHCPv4ScopeAddressProperties addressProperties = GetAddressProperties();

            IPv4Address leaseAddress = IPv4Address.Empty;
            Boolean newLeaseNeeded = true;

            DHCPv4ClientIdentifier clientIdentifier = packet.GetClientIdentifier();
            DHCPv4Lease currentLease = Leases.GetLeaseByClientIdentifier(clientIdentifier);

            IPv4Address excludeFromLease = IPv4Address.Empty;

            if (currentLease != DHCPv4Lease.Empty)
            {
                if (addressProperties.ReuseAddressIfPossible == true)
                {
                    currentLease.Renew(addressProperties.ValidLifetime.Value, true);
                    leaseAddress = IPv4Address.FromAddress(currentLease.Address);
                    newLeaseNeeded = false;
                }
                else
                {
                    excludeFromLease = IPv4Address.FromByteArray(currentLease.Address.GetBytes());
                    currentLease.Revoke();
                }
            }
            else
            {
                if (Resolver.HasUniqueIdentifier == true)
                {
                    currentLease = Leases.GetLeaseByUniqueIdentifier(Resolver.GetUniqueIdentifier(packet));
                    if (currentLease != DHCPv4Lease.Empty)
                    {
                        currentLease.Revoke();
                        excludeFromLease = IPv4Address.FromAddress(currentLease.Address);

                        if (addressProperties.ReuseAddressIfPossible == true)
                        {
                            leaseAddress = IPv4Address.FromAddress(currentLease.Address);
                        }
                    }
                }
            }

            if (leaseAddress == IPv4Address.Empty)
            {
                leaseAddress = addressProperties.GetValidAddresses(Leases.GetUsedAddresses(), excludeFromLease);

                if (leaseAddress == IPv4Address.Empty)
                {
                    base.Apply(new DHCPv4ScopeAddressesAreExhaustedEvent(Id));
                    base.Apply(new DHCPv4DiscoverHandledEvent(this.Id, packet, DHCPv4DiscoverHandledEvent.DisoverErros.NoAddressesLeft));
                    return DHCPv4Packet.Empty;
                }
            }

            if (newLeaseNeeded == true)
            {
                Leases.AddLease(
                    Guid.NewGuid(),
                    leaseAddress,
                    addressProperties.ValidLifetime.Value,
                    clientIdentifier,
                    Resolver.HasUniqueIdentifier == true ? Resolver.GetUniqueIdentifier(packet) : null
                    );
            }

            DHCPv4ScopeProperties scopeProperties = GetScopeProperties();

            DHCPv4Packet response = DHCPv4Packet.AsDiscoverResponse(
                   packet,
                   leaseAddress,
                   addressProperties,
                   scopeProperties.Properties
                );

            base.Apply(new DHCPv4DiscoverHandledEvent(this.Id, packet, response));
            return response;
        }
        internal DHCPv4Packet HandleRequest(DHCPv4Packet packet)
        {
            DHCPv4PacketRequestType requestType = packet.GetRequestType();
            DHCPv4ScopeAddressProperties addressProperties = GetAddressProperties();
            DHCPv4ScopeProperties scopeProperties = GetScopeProperties();

            DHCPv4ClientIdentifier identifier = packet.GetClientIdentifier();
            DHCPv4Lease lease = Leases.GetLeaseByClientIdentifier(identifier);

            Boolean newLeaseNeeded = false;
            IPv4Address leaseAddress = IPv4Address.Empty;
            DHCPv4Packet answer = DHCPv4Packet.Empty;
            var requestError = DHCPv4RequestHandledEvent.RequestErros.NoError;

            IPv4Address excludedAddressForNewAddress = IPv4Address.Empty;

            if (requestType == DHCPv4PacketRequestType.AnswerToOffer)
            {
                if (lease == DHCPv4Lease.Empty)
                {
                    answer = DHCPv4Packet.AsNonAcknowledgeResponse(packet, "no lease found");
                    requestError = DHCPv4RequestHandledEvent.RequestErros.LeaseNotFound;
                }
                else
                {
                    if (lease.IsPending() == false)
                    {
                        answer = DHCPv4Packet.AsNonAcknowledgeResponse(packet, "the requested lease is in use");
                        requestError = DHCPv4RequestHandledEvent.RequestErros.LeaseNotPending;
                    }
                    else
                    {
                        lease.RemovePendingState();
                        leaseAddress = lease.Address;
                    }
                }
            }
            else if (requestType == DHCPv4PacketRequestType.Renewing ||
                requestType == DHCPv4PacketRequestType.Rebinding
                )
            {
                if (
                    (requestType == DHCPv4PacketRequestType.Renewing &&
                    addressProperties.SupportDirectUnicast == true) ||
                    requestType == DHCPv4PacketRequestType.Rebinding)
                {
                    if (lease == DHCPv4Lease.Empty)
                    {
                        requestError = DHCPv4RequestHandledEvent.RequestErros.LeaseNotFound;

                        if (requestType == DHCPv4PacketRequestType.Renewing)
                        {
                            answer = DHCPv4Packet.AsNonAcknowledgeResponse(packet, "no lease found");
                        }
                    }
                    else
                    {
                        if (lease.IsActive() == false)
                        {
                            answer = DHCPv4Packet.AsNonAcknowledgeResponse(packet, "lease not active anymore");
                            requestError = DHCPv4RequestHandledEvent.RequestErros.LeaseNotActive;
                        }
                        else
                        {
                            if (addressProperties.ReuseAddressIfPossible == true)
                            {
                                lease.Renew(addressProperties.ValidLifetime.Value, false);
                                leaseAddress = lease.Address;
                            }
                            else
                            {
                                lease.Revoke();
                                newLeaseNeeded = true;
                                excludedAddressForNewAddress = lease.Address;
                            }
                        }
                    }
                }
                else
                {
                    answer = DHCPv4Packet.AsNonAcknowledgeResponse(packet, "the renewing of an ip address is not allowed");
                    requestError = DHCPv4RequestHandledEvent.RequestErros.RenewingNotAllowed;
                }
            }
            else if (requestType == DHCPv4PacketRequestType.Initializing)
            {
                if (lease == DHCPv4Lease.Empty)
                {
                    requestError = DHCPv4RequestHandledEvent.RequestErros.LeaseNotFound;
                    //answer = DHCPv4Packet.AsNonAcknowledgeResponse(packet, "no lease found");
                }
                else
                {
                    if (lease.IsActive() == true)
                    {
                        if (addressProperties.ReuseAddressIfPossible == true)
                        {
                            lease.Renew(addressProperties.ValidLifetime.Value, false);
                            leaseAddress = lease.Address;
                        }
                        else
                        {
                            lease.Revoke();
                            newLeaseNeeded = true;
                            excludedAddressForNewAddress = lease.Address;
                        }
                    }
                    else
                    {
                        if (addressProperties.ReuseAddressIfPossible == true)
                        {
                            if (Leases.IsAddressActive(lease.Address) == true)
                            {
                                newLeaseNeeded = true;
                            }
                            else
                            {
                                lease.Reactived(addressProperties.ValidLifetime.Value);
                                leaseAddress = lease.Address;
                            }
                        }
                        else
                        {
                            newLeaseNeeded = true;
                        }
                    }
                }
            }

            if (requestError == DHCPv4RequestHandledEvent.RequestErros.NoError)
            {
                if (leaseAddress == IPv4Address.Empty)
                {
                    leaseAddress = addressProperties.GetValidAddresses(Leases.GetUsedAddresses(), excludedAddressForNewAddress);

                    if (leaseAddress == IPv4Address.Empty)
                    {
                        base.Apply(new DHCPv4ScopeAddressesAreExhaustedEvent(Id));
                        answer = DHCPv4Packet.AsNonAcknowledgeResponse(packet, "no addresses left");
                        requestError = DHCPv4RequestHandledEvent.RequestErros.NoAddressAvaiable;
                    }
                }
                if (answer == DHCPv4Packet.Empty)
                {
                    if (newLeaseNeeded == true)
                    {
                        Leases.AddLease(
                            Guid.NewGuid(),
                            leaseAddress,
                            addressProperties.ValidLifetime.Value,
                            identifier,
                            Resolver.HasUniqueIdentifier == true ? Resolver.GetUniqueIdentifier(packet) : null
                            );
                    }

                    answer = DHCPv4Packet.AsRequestResponse(
                       packet,
                       leaseAddress,
                       addressProperties,
                       scopeProperties.Properties
                    );
                }
            }

            base.Apply(new DHCPv4RequestHandledEvent(this.Id, packet, answer, requestError));
            return answer;
        }

        internal DHCPv4Packet HandleDecline(DHCPv4Packet packet)
        {
            DHCPv4ScopeAddressProperties addressProperties = GetAddressProperties();

            if (addressProperties.AcceptDecline == false)
            {
                base.Apply(new DHCPv4DeclineHandledEvent(this.Id, packet, DHCPv4DeclineHandledEvent.DeclineErros.DeclineNotAllowed));
                return DHCPv4Packet.Empty;
            }

            IPv4Address address = packet.GetRequestedAddressFromRequestedOption();
            if (address == IPv4Address.Empty)
            {
                base.Apply(new DHCPv4DeclineHandledEvent(this.Id, packet, DHCPv4DeclineHandledEvent.DeclineErros.IPAddressNotFound));
                return DHCPv4Packet.Empty;
            }
            DHCPv4Lease lease = Leases.GetLeaseByIPAddress(address);
            if (lease == DHCPv4Lease.Empty)
            {
                base.Apply(new DHCPv4DeclineHandledEvent(this.Id, packet, DHCPv4DeclineHandledEvent.DeclineErros.LeaseNotFound));
                return DHCPv4Packet.Empty;
            }

            if (lease.IsPending() == false && lease.IsActive() == false)
            {
                base.Apply(new DHCPv4DeclineHandledEvent(this.Id, packet, DHCPv4DeclineHandledEvent.DeclineErros.LeaseInInvalidState));
                return DHCPv4Packet.Empty;
            }

            if (Leases.IsAddressSuspended(lease.Address) == true)
            {
                base.Apply(new DHCPv4DeclineHandledEvent(this.Id, packet, DHCPv4DeclineHandledEvent.DeclineErros.AddressAlreadySuspended));
                return DHCPv4Packet.Empty;
            }

            lease.Suspend(null);
            base.Apply(new DHCPv4DeclineHandledEvent(this.Id, packet));
            return DHCPv4Packet.Empty;
        }

        public Boolean HasParentScope()
        {
            return ParentScope != DHCPv4Scope.NotFound;
        }

        internal DHCPv4Packet HandleRelease(DHCPv4Packet packet)
        {
            IPv4Address address = packet.IPHeader.Source;

            DHCPv4Lease lease = Leases.GetLeaseByIPAddress(address);
            if (lease == DHCPv4Lease.Empty)
            {
                base.Apply(new DHCPv4ReleaseHandledEvent(this.Id, packet, DHCPv4ReleaseHandledEvent.ReleaseError.NoLeaseFound));
                return DHCPv4Packet.Empty;
            }

            if (lease.IsActive() == false)
            {
                base.Apply(new DHCPv4ReleaseHandledEvent(this.Id, packet, DHCPv4ReleaseHandledEvent.ReleaseError.LeaseNotActive));
                return DHCPv4Packet.Empty;
            }

            lease.Release();
            base.Apply(new DHCPv4ReleaseHandledEvent(this.Id, packet));

            return DHCPv4Packet.Empty;
        }

        internal DHCPv4Packet HandleInform(DHCPv4Packet packet)
        {
            DHCPv4ScopeAddressProperties addressProperties = GetAddressProperties();

            if (addressProperties.InformsAreAllowd == false)
            {
                base.Apply(new DHCPv4InformHandledEvent(this.Id, packet, DHCPv4InformHandledEvent.InformErros.InformsNotAllowed));
                return DHCPv4Packet.Empty;
            }

            DHCPv4ScopeProperties scopeProperties = GetScopeProperties();

            DHCPv4Packet response = DHCPv4Packet.AsInformResponse(
                 packet,
                 scopeProperties.Properties
              );

            base.Apply(new DHCPv4InformHandledEvent(this.Id, packet, response));
            return response;
        }

        #endregion

        #region applies and when

        internal void SetResolver(IDHCPv4ScopeResolver resolver)
        {
            Resolver = resolver;
        }

        internal void DetachFromParent(Boolean includedChildren)
        {
            ParentScope._subscopes.Remove(this);
            this.ParentScope = DHCPv4Scope.NotFound;

            if (includedChildren == true)
            {
                foreach (DHCPv4Scope item in _subscopes)
                {
                    item.DetachFromParent(true);
                }
            }
        }

        internal void SetParent(DHCPv4Scope parent)
        {
            parent.AddSubscope(this);
            this.ParentScope = parent;
        }

        internal void SetSuspendedState(Boolean includeChildren)
        {
            DHCPv4ScopeAddressProperties properties = GetAddressProperties();

            Boolean addressPropertiesAreInValid;
            if (HasParentScope() == false)
            {
                addressPropertiesAreInValid = properties.ValueAreValidForRoot() == false;
            }
            else
            {
                addressPropertiesAreInValid =
                properties.AreTimeValueValid() == false ||
                ParentScope.AddressRelatedProperties.IsAddressRangeBetween(this.AddressRelatedProperties) == false;
            }

            if (addressPropertiesAreInValid == true && IsSuspendend == false)
            {
                base.Apply(new DHCPv4ScopeSuspendedEvent(this.Id));
            }
            else if (addressPropertiesAreInValid == false && IsSuspendend == true)
            {
                base.Apply(new DHCPv4ScopeReactivedEvent(this.Id));
            }

            if (includeChildren == true)
            {
                foreach (var child in _subscopes)
                {
                    child.SetSuspendedState(true);
                }
            }
        }

        private void AddSubscope(DHCPv4Scope child)
        {
            this._subscopes.Add(child);
            //this._returnScopes = this._subscopes.AsReadOnly();
        }

        protected override void When(DomainEvent domainEvent)
        {
            Boolean handled = false;
            IInternalEventHandler entityToApply = Leases;

            switch (domainEvent)
            {

                case DHCPv4ScopePropertiesUpdatedEvent e:
                    Properties = e.Properties;
                    break;
                case DHCPv4ScopeAddressPropertiesUpdatedEvent e:
                    AddressRelatedProperties = e.AddressProperties;
                    SetSuspendedState(true);
                    break;
                case DHCPv4ScopeDescriptionUpdatedEvent e:
                    Description = new ScopeDescription(e.Description);
                    break;
                case DHCPv4ScopeNameUpdatedEvent e:
                    Name = new ScopeName(e.Name);
                    break;
                default:
                    break;
            }

            if (handled == false)
            {
                ApplyToEnity(entityToApply, domainEvent);
            }
        }

        #endregion
    }
}
