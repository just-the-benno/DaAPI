using DaAPI.Core.Common;
using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Notifications;
using DaAPI.Core.Notifications.Triggers;
using DaAPI.Core.Packets.DHCPv6;
using DaAPI.Core.Scopes.DHCPv6.ScopeProperties;
using DaAPI.Core.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DaAPI.Core.Scopes.DHCPv6.DHCPv6LeaseEvents;
using static DaAPI.Core.Scopes.DHCPv6.DHCPv6PacketHandledEvents;
using static DaAPI.Core.Scopes.DHCPv6.DHCPv6ScopeEvents;

namespace DaAPI.Core.Scopes.DHCPv6
{
    public class DHCPv6Scope : Scope<DHCPv6Scope, DHCPv6Packet, IPv6Address, DHCPv6Leases, DHCPv6Lease, DHCPv6ScopeAddressProperties, DHCPv6ScopeProperties, DHCPv6ScopeProperty, UInt16, DHCPv6ScopePropertyType>
    {
        private static readonly TimeSpan _leaseReboundTime = TimeSpan.FromSeconds(60);
        private ILogger<DHCPv6Scope> _logger;

        #region constructor and factories

        private DHCPv6Scope(Guid id, Action<DomainEvent> addtionalApplier, Action<NotifcationTrigger> addtionalNotifier) : base(
            id,
            DHCPv6ScopeAddressProperties.Empty,
            DHCPv6ScopeProperties.Empty,
            addtionalApplier,
            addtionalNotifier)
        {
            Leases = new DHCPv6Leases(Guid.NewGuid(), (e) =>
            {
                switch (e)
                {
                    case DHCPv6ScopeRelatedEvent ce:
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
        }

        internal static DHCPv6Scope FromInstructions(
            DHCPv6ScopeCreateInstruction instructions,
            Action<DomainEvent> rootApplier,
            Action<NotifcationTrigger> rootNotifier,
            ILogger<DHCPv6Scope> logger
            )
        {
            DHCPv6Scope scope = new DHCPv6Scope(instructions.Id, rootApplier, rootNotifier)
            {
                Name = new ScopeName(instructions.Name),
                Description = new ScopeDescription(instructions.Description),
                Properties = instructions.ScopeProperties,
                AddressRelatedProperties = instructions.AddressProperties,
            };

            scope._logger = logger;

            return scope;
        }

        #endregion

        #region utilitiy

        protected internal override void ProcessExpiredLeases(IEnumerable<DHCPv6Lease> expiredLeases)
        {
            foreach (var item in expiredLeases)
            {
                if (item.PrefixDelegation != DHCPv6PrefixDelegation.None)
                {
                    AddNotificationTrigger(PrefixEdgeRouterBindingUpdatedTrigger.WithOldBinding(
                        Id, PrefixBinding.FromLease(item)));
                }
            }

            base.ProcessExpiredLeases(expiredLeases);
        }

        #endregion

        #region packet handling

        internal DHCPv6Packet HandleSolicitWithPrefixDelegation(DHCPv6Packet packet, IDHCPv6ServerPropertiesResolver properyResolver)
        {
            DHCPv6Packet innerPacket = packet.GetInnerPacket();

            var addressProperties = GetAddressProperties();
            Boolean rapitCommit = addressProperties.IsRapitCommitEnabled() && innerPacket.HasRapitCommitOption();
            DUID clientDuid = innerPacket.GetClientIdentifer();

            DHCPv6Lease currentLease = Leases.GetLeaseByClientDuid(clientDuid);
            DHCPv6Packet response = null;
            Boolean responseNeeded = false;
            Int32 waitingForLease = 15;

            // In some client implementations like Cisco IOS XE, two solicit packets are sent, one for an address and a second for a prefix, which would be handled without this loop. 
            // Unfortunately, the PD is sent first and as long as the router received an answer for the address the PD packet is not sent again. Hence, progressing is delayed for a small amount of time. 
            while ((currentLease == DHCPv6Lease.NotFound || currentLease.IsActive() == false) && --waitingForLease > 0)
            {
                Task.Delay(100).GetAwaiter().GetResult();
                currentLease = Leases.GetLeaseByClientDuid(clientDuid);
            }

            if (currentLease == DHCPv6Lease.NotFound)
            {
                _logger.LogDebug("no lease for client duid {duid} found", clientDuid);
                base.Apply(new DHCPv6SolicitHandledEvent(this.Id, packet, DHCPv6SolicitHandledEvent.SolicitErros.LeaseNotFound));
            }
            else if (currentLease.IsActive() == false)
            {
                _logger.LogDebug("lease with {address} for client duid {duid} found but not active. state is {state}", currentLease.Address, clientDuid, currentLease.State);
                base.Apply(new DHCPv6SolicitHandledEvent(this.Id, packet, DHCPv6SolicitHandledEvent.SolicitErros.LeaseNotActive));
            }
            else if (addressProperties.PrefixDelgationInfo == null)
            {
                _logger.LogDebug("prefix delegation for {name} is not enabled", Name);
                base.Apply(new DHCPv6SolicitHandledEvent(this.Id, packet, DHCPv6SolicitHandledEvent.SolicitErros.PrefixDelegationNotAvailable));
            }
            else
            {
                if (IsFreshPrefix(currentLease) == false)
                {
                    responseNeeded = true;
                }
                else
                {
                    _logger.LogInformation("a fresh leased prefix found. Skipping creating of a new prefix");
                }
            }

            if (responseNeeded == true)
            {
                UInt32 prefixIdentityAsscocationId = packet.GetInnerPacket().GetPrefixDelegationIdentityAssocationId().Value;
                var prefix = addressProperties.GetValidPrefix(Leases.GetUsedPrefixes(), prefixIdentityAsscocationId);
                currentLease.UpdateAddressPrefix(prefix, false);

                _logger.LogDebug("prefix {prefix}/{lenth} for id {id} is generated", currentLease.PrefixDelegation.NetworkAddress, currentLease.PrefixDelegation.Mask.Identifier.Value, currentLease.PrefixDelegation.IdentityAssociation);


                if (rapitCommit == true)
                {
                    _logger.LogDebug("rapit commit is enabled. sending packet as reply");
                    response = DHCPv6Packet.AsPrefixReplyWithRapitCommit(
                        packet, addressProperties, DHCPv6ScopeProperties.Empty, currentLease, properyResolver.GetServerDuid());

                    base.AddNotificationTrigger(
                         PrefixEdgeRouterBindingUpdatedTrigger.WithNewBinding(Id, PrefixBinding.FromLease(currentLease)
                         ));
                }
                else
                {
                    _logger.LogDebug("rapit commit is not enabled. sending packet as advertise");

                    response = DHCPv6Packet.AsPrefixAdvertise(
                        packet, addressProperties, DHCPv6ScopeProperties.Empty, currentLease, properyResolver.GetServerDuid());
                }

                base.Apply(new DHCPv6SolicitHandledEvent(this.Id, packet, response, rapitCommit));
            }

            return response;

        }

        internal Boolean IsFreshLease(DHCPv6Lease lease) => (DateTime.UtcNow - lease.Start) < _leaseReboundTime && lease.AddressIsInUse() == true;
        internal Boolean IsFreshPrefix(DHCPv6Lease lease) => (DateTime.UtcNow - lease.PrefixDelegation.Started) < _leaseReboundTime;

        internal DHCPv6Packet HandleSolicit(DHCPv6Packet packet, IDHCPv6ServerPropertiesResolver properyResolver)
        {
            DHCPv6Packet innerPacket = packet.GetInnerPacket();

            var addressProperties = GetAddressProperties();
            Boolean rapitCommit = addressProperties.IsRapitCommitEnabled() && innerPacket.HasRapitCommitOption();

            DUID clientIdentifier = innerPacket.GetClientIdentifer();
            UInt32 identityAssociationId = innerPacket.GetNonTemporaryIdentityAssocationId().Value;
            UInt32? prefixIdentityAsscocationId = innerPacket.GetPrefixDelegationIdentityAssocationId();

            DHCPv6Lease currentLease = Leases.GetLeaseAssociationIdAndDuid(identityAssociationId, clientIdentifier);
            if (currentLease == null && Resolver.HasUniqueIdentifier)
            {
                currentLease = Leases.GetLeaseByUniqueIdentifier(Resolver.GetUniqueIdentifier(packet));
            }

            IPv6Address excludeFromLease = IPv6Address.Empty;
            IPv6Address excludeFromPrefix = IPv6Address.Empty;

            IPv6Address leaseAddress = IPv6Address.Empty;
            DHCPv6PrefixDelegation leasedPrefix = DHCPv6PrefixDelegation.None;

            Boolean skipLeaseCreation = false;

            if (currentLease != DHCPv6Lease.NotFound)
            {
                _logger.LogDebug("active lease found for client {duid} and {iaid}", clientIdentifier, identityAssociationId);

                if (IsFreshLease(currentLease) == true)
                {
                    _logger.LogInformation("a fresh lease found.  Skipping creating of a new lease");
                    skipLeaseCreation = true;
                }
                else
                {
                    if (addressProperties.ReuseAddressIfPossible == true && currentLease.CanBeExtended() == true)
                    {
                        leaseAddress = currentLease.Address;
                        leasedPrefix = currentLease.PrefixDelegation;
                    }
                    else
                    {
                        if (currentLease.IsActive() == true || currentLease.IsPending() == true)
                        {
                            excludeFromLease = IPv6Address.FromByteArray(currentLease.Address.GetBytes());
                            excludeFromPrefix = currentLease.PrefixDelegation.NetworkAddress;
                        }
                    }
                }
            }

            if (skipLeaseCreation == false)
            {
                if (leaseAddress == IPv6Address.Empty)
                {
                    leaseAddress = addressProperties.GetValidAddresses(Leases.GetUsedAddresses(), excludeFromLease);

                    if (leaseAddress == IPv6Address.Empty)
                    {
                        _logger.LogError("{scope} has noip addresses left", Name);
                        base.Apply(new DHCPv6ScopeAddressesAreExhaustedEvent(Id));
                        base.Apply(new DHCPv6SolicitHandledEvent(this.Id, packet, DHCPv6SolicitHandledEvent.SolicitErros.NoAddressesLeft));
                        return DHCPv6Packet.Empty;
                    }
                }

                if (prefixIdentityAsscocationId.HasValue == true && leasedPrefix == DHCPv6PrefixDelegation.None)
                {
                    leasedPrefix = addressProperties.GetValidPrefix(Leases.GetUsedPrefixes(), prefixIdentityAsscocationId.Value, excludeFromPrefix);
                }

                Leases.AddLease(
                       Guid.NewGuid(),
                       leaseAddress,
                       addressProperties.ValidLeaseTime.Value,
                       identityAssociationId,
                       clientIdentifier,
                       Resolver.HasUniqueIdentifier == true ? Resolver.GetUniqueIdentifier(packet) : null,
                       prefixIdentityAsscocationId.HasValue == false ? DHCPv6PrefixDelegation.None : leasedPrefix,
                       currentLease);
            }

            DHCPv6Packet response;
            if (rapitCommit == true)
            {
                response = HandleRequestInternal(packet, properyResolver, true, out DHCPv6RequestHandledEvent.RequestErrors _, skipLeaseCreation);
            }
            else
            {
                response = DHCPv6Packet.AsAdvertise(
                packet,
                leaseAddress,
                prefixIdentityAsscocationId.HasValue == false ? DHCPv6PrefixDelegation.None : leasedPrefix,
                addressProperties,
                 GetScopeProperties(),
                 properyResolver.GetServerDuid());
            }

            base.Apply(new DHCPv6SolicitHandledEvent(this.Id, packet, response, rapitCommit));
            return response;
        }

        internal DHCPv6Packet HandleRequestInternal(
            DHCPv6Packet packet,
            IDHCPv6ServerPropertiesResolver properyResolver,
            Boolean isRapitCommit,
            out DHCPv6RequestHandledEvent.RequestErrors requestError,
            Boolean onlyGenerateResponse)
        {
            var addressProperties = GetAddressProperties();
            var innerPacket = packet.GetInnerPacket();

            DUID clientIdentifier = innerPacket.GetClientIdentifer();
            UInt32? identityAssociationId = innerPacket.GetNonTemporaryIdentityAssocationId();

            DHCPv6Lease lease;
            if (identityAssociationId.HasValue == true)
            {
                lease = Leases.GetLeaseAssociationIdAndDuid(identityAssociationId.Value, clientIdentifier);
            }
            else
            {
                lease = Leases.GetLeaseByClientDuid(clientIdentifier);
            }

            requestError = DHCPv6RequestHandledEvent.RequestErrors.NoError;
            DHCPv6Packet response;
            if (lease == DHCPv6Lease.Empty)
            {
                response = DHCPv6Packet.AsError(packet, DHCPv6StatusCodes.NoAddrsAvail, properyResolver.GetServerDuid());
                requestError = DHCPv6RequestHandledEvent.RequestErrors.LeaseNotFound;
            }
            else
            {
                if (onlyGenerateResponse == true)
                {
                    return DHCPv6Packet.AsReply(packet, addressProperties, GetScopeProperties(), lease, true, properyResolver.GetServerDuid(), isRapitCommit);
                }

                var ancsestorLease = lease.HasAncestor() == true ? Leases.GetAncestor(lease) : null;

                if (lease.IsPending() == true)
                {
                    if (identityAssociationId.HasValue == true)
                    {
                        lease.RemovePendingState();
                        ancsestorLease?.Revoke();

                        response = DHCPv6Packet.AsReply(packet, addressProperties, GetScopeProperties(), lease, false, properyResolver.GetServerDuid(), isRapitCommit);
                    }
                    else
                    {
                        response = DHCPv6Packet.AsError(packet, DHCPv6StatusCodes.NoAddrsAvail, properyResolver.GetServerDuid());
                        requestError = DHCPv6RequestHandledEvent.RequestErrors.LeasePendingButOnlyPrefixRequested;
                    }
                }
                else if (lease.IsActive() == true)
                {
                    if (identityAssociationId.HasValue == false)
                    {
                        lease.ActivateAddressPrefix();
                    }

                    response = DHCPv6Packet.AsReply(packet, addressProperties, GetScopeProperties(), lease, true, properyResolver.GetServerDuid(), isRapitCommit);
                }
                else
                {
                    response = DHCPv6Packet.AsError(packet, DHCPv6StatusCodes.NoAddrsAvail, properyResolver.GetServerDuid());
                    requestError = DHCPv6RequestHandledEvent.RequestErrors.LeaseNotInCorrectState;
                }

                if (requestError == DHCPv6RequestHandledEvent.RequestErrors.NoError)
                {
                    PrefixBinding oldBinding = null;
                    PrefixBinding newBinding = null;

                    if ((ancsestorLease?.PrefixDelegation ?? DHCPv6PrefixDelegation.None) != DHCPv6PrefixDelegation.None)
                    {
                        oldBinding = PrefixBinding.FromLease(ancsestorLease, false);
                    }

                    if (lease.PrefixDelegation != DHCPv6PrefixDelegation.None)
                    {
                        newBinding = PrefixBinding.FromLease(lease, false);
                    }

                    if ((oldBinding != newBinding) && (oldBinding != null || newBinding != null))
                    {
                        base.AddNotificationTrigger(new PrefixEdgeRouterBindingUpdatedTrigger(oldBinding, newBinding, Id));
                    }
                }
            }

            return response;
        }

        internal DHCPv6Packet HandleRequest(DHCPv6Packet packet, IDHCPv6ServerPropertiesResolver properyResolver)
        {
            DHCPv6Packet response = HandleRequestInternal(packet, properyResolver, false, out DHCPv6RequestHandledEvent.RequestErrors error, false);
            base.Apply(new DHCPv6RequestHandledEvent(this.Id, packet, response, error));
            return response;
        }

        private enum LeaseExtentionsErros
        {
            NoError,
            LeaseNotFound,
            NoAddressAvailaibe,
            OnlyPrefixIsNotAllowed,
        }

        private DHCPv6Packet HandleLeaseExtentions(DHCPv6Packet packet, IDHCPv6ServerPropertiesResolver properyResolver, out LeaseExtentionsErros extentionError)
        {
            var innerPacket = packet.GetInnerPacket();
            var addressProperties = GetAddressProperties();

            DUID clientIdentifier = innerPacket.GetClientIdentifer();
            UInt32? identityAssociationId = innerPacket.GetNonTemporaryIdentityAssocationId();
            UInt32? prefixIdentityAsscocationId = innerPacket.GetPrefixDelegationIdentityAssocationId();
            DHCPv6Lease lease;
            if (identityAssociationId.HasValue == false && prefixIdentityAsscocationId.HasValue == true)
            {
                DHCPv6Lease belongingLease = Leases.GetLeaseByClientDuid(clientIdentifier);
                if (belongingLease == DHCPv6Lease.NotFound)
                {
                    extentionError = LeaseExtentionsErros.OnlyPrefixIsNotAllowed;
                    return DHCPv6Packet.AsError(packet, DHCPv6StatusCodes.NoBinding, properyResolver.GetServerDuid());
                }

                // In some client implementations like Cisco IOS XE, two renew packets are sent, one for an address and a second for a prefix.
                // Unfortunately, if the PD is sent first, the lease is not extented. So... we just wait a second and hopefully than the lease is ready  
                Task.Delay(1000).GetAwaiter().GetResult();
                lease = Leases.GetLeaseByClientDuid(clientIdentifier);
            }
            else
            {
                lease = Leases.GetLeaseAssociationIdAndDuid(identityAssociationId.Value, clientIdentifier);
            }

            DHCPv6Packet response = null;
            extentionError = LeaseExtentionsErros.NoError;
            if (lease == DHCPv6Lease.NotFound)
            {
                response = DHCPv6Packet.AsError(packet, DHCPv6StatusCodes.NoBinding, properyResolver.GetServerDuid());
                extentionError = LeaseExtentionsErros.LeaseNotFound;
            }
            else
            {
                DHCPv6Lease leaseUsedToGenerateResponse = null;
                if (addressProperties.ReuseAddressIfPossible == true && lease.CanBeExtended() == true)
                {
                    var tempDelegation = lease.PrefixDelegation;
                    Boolean resetPrefix = false;
                    if(prefixIdentityAsscocationId.HasValue == false && tempDelegation != DHCPv6PrefixDelegation.None)
                    {
                        resetPrefix = true;
                        AddNotificationTrigger(
                            PrefixEdgeRouterBindingUpdatedTrigger.WithOldBinding(Id,
                            PrefixBinding.FromLease(lease)));
                    }
                    else if (tempDelegation == DHCPv6PrefixDelegation.None && prefixIdentityAsscocationId.HasValue == true)
                    {
                        var prefix = addressProperties.GetValidPrefix(Leases.GetUsedPrefixes(), prefixIdentityAsscocationId.Value, lease.PrefixDelegation.NetworkAddress);
                        lease.UpdateAddressPrefix(prefix, false);
                        AddNotificationTrigger(
                            PrefixEdgeRouterBindingUpdatedTrigger.WithNewBinding(Id,
                            PrefixBinding.FromLease(lease)));
                    }
                  
                    if (identityAssociationId.HasValue == true)
                    {
                        lease.Renew(addressProperties.ValidLeaseTime.Value, false, resetPrefix);
                    }

                    leaseUsedToGenerateResponse = lease;
                }
                else
                {
                    if ((identityAssociationId.HasValue == true && IsFreshLease(lease) == true)
                        || (prefixIdentityAsscocationId.HasValue == true && IsFreshPrefix(lease) == true))
                    {
                        leaseUsedToGenerateResponse = lease;
                    }
                    else
                    {
                        PrefixBinding oldBinding = null;
                        PrefixBinding newBinding = null;

                        if (lease.PrefixDelegation != DHCPv6PrefixDelegation.None)
                        {
                            oldBinding = PrefixBinding.FromLease(lease);
                        }

                        if (lease.AddressIsInUse() == true && identityAssociationId.HasValue == true)
                        {
                            lease.Revoke();
                        }

                        IPv6Address leaseAddress = identityAssociationId.HasValue == true ? addressProperties.GetValidAddresses(Leases.GetUsedAddresses(), lease.Address) : lease.Address;
                        DHCPv6PrefixDelegation leasedPrefix = DHCPv6PrefixDelegation.None;
                        if (prefixIdentityAsscocationId.HasValue == true)
                        {
                            leasedPrefix = addressProperties.GetValidPrefix(Leases.GetUsedPrefixes(), prefixIdentityAsscocationId.Value, lease.PrefixDelegation.NetworkAddress);
                            newBinding = new PrefixBinding(leasedPrefix.NetworkAddress, leasedPrefix.Mask, leaseAddress);
                            if (identityAssociationId.HasValue == false)
                            {
                                lease.UpdateAddressPrefix(leasedPrefix, false);
                            }
                        }

                        if (leaseAddress == IPv6Address.Empty)
                        {
                            base.Apply(new DHCPv6ScopeAddressesAreExhaustedEvent(Id));
                            AddNotificationTrigger(PrefixEdgeRouterBindingUpdatedTrigger.WithOldBinding(Id, oldBinding));
                            response = DHCPv6Packet.AsError(packet, DHCPv6StatusCodes.NoAddrsAvail, properyResolver.GetServerDuid());
                            extentionError = LeaseExtentionsErros.NoAddressAvailaibe;
                        }
                        else
                        {
                            leaseUsedToGenerateResponse = identityAssociationId.HasValue == true ? Leases.AddLease(
                              Guid.NewGuid(),
                              leaseAddress,
                              addressProperties.ValidLeaseTime.Value,
                              identityAssociationId.Value,
                              clientIdentifier,
                              Resolver.HasUniqueIdentifier == true ? Resolver.GetUniqueIdentifier(packet) : null,
                              prefixIdentityAsscocationId.HasValue == false ? DHCPv6PrefixDelegation.None : leasedPrefix,
                              lease
                              ) : lease;

                            if (identityAssociationId.HasValue == true)
                            {
                                leaseUsedToGenerateResponse.RemovePendingState();
                            }

                            if (newBinding != null || oldBinding != null && oldBinding != newBinding)
                            {
                                AddNotificationTrigger(new PrefixEdgeRouterBindingUpdatedTrigger(oldBinding, newBinding, Id));
                            }
                        }
                    }
                }

                if (extentionError == LeaseExtentionsErros.NoError)
                {
                    response = DHCPv6Packet.AsReply(packet, addressProperties, GetScopeProperties(), leaseUsedToGenerateResponse, false, properyResolver.GetServerDuid(), false);
                }
            }

            return response;
        }

        internal DHCPv6Packet HandleRenew(DHCPv6Packet packet, IDHCPv6ServerPropertiesResolver properyResolver)
        {

            DHCPv6Packet response = HandleLeaseExtentions(packet, properyResolver, out LeaseExtentionsErros error);
            var handlingError = DHCPv6RenewHandledEvent.RenewErrors.NoError;
            switch (error)
            {
                case LeaseExtentionsErros.LeaseNotFound:
                    handlingError = DHCPv6RenewHandledEvent.RenewErrors.LeaseNotFound;
                    break;
                case LeaseExtentionsErros.NoAddressAvailaibe:
                    handlingError = DHCPv6RenewHandledEvent.RenewErrors.NoAddressesAvaiable;
                    break;
                case LeaseExtentionsErros.OnlyPrefixIsNotAllowed:
                    handlingError = DHCPv6RenewHandledEvent.RenewErrors.OnlyPrefixIsNotAllowed;
                    break;
                default:
                    break;
            }

            base.Apply(new DHCPv6RenewHandledEvent(this.Id, packet, response, handlingError));
            return response;
        }

        internal DHCPv6Packet HandleRebind(DHCPv6Packet packet, IDHCPv6ServerPropertiesResolver properyResolver)
        {
            DHCPv6Packet response = HandleLeaseExtentions(packet, properyResolver, out LeaseExtentionsErros error);
            var handlingError = DHCPv6RebindHandledEvent.RebindErrors.NoError;
            switch (error)
            {
                case LeaseExtentionsErros.LeaseNotFound:
                    handlingError = DHCPv6RebindHandledEvent.RebindErrors.LeaseNotFound;
                    break;
                case LeaseExtentionsErros.NoAddressAvailaibe:
                    handlingError = DHCPv6RebindHandledEvent.RebindErrors.NoAddressesAvaiable;
                    break;
                case LeaseExtentionsErros.OnlyPrefixIsNotAllowed:
                    handlingError = DHCPv6RebindHandledEvent.RebindErrors.OnlyPrefixIsNotAllowed;
                    break;
                default:
                    break;
            }

            base.Apply(new DHCPv6RebindHandledEvent(this.Id, packet, response, handlingError));
            return response;
        }

        internal DHCPv6Packet HandleRelease(DHCPv6Packet packet, IDHCPv6ServerPropertiesResolver properyResolver)
        {
            var innerPacket = packet.GetInnerPacket();

            DUID clientIdentifier = innerPacket.GetClientIdentifer();
            UInt32? identityAssociationId = innerPacket.GetNonTemporaryIdentityAssocationId();
            UInt32? prefixIdentityAssociationId = innerPacket.GetPrefixDelegationIdentityAssocationId();

            DHCPv6Lease lease = DHCPv6Lease.NotFound;
            if (identityAssociationId.HasValue == true)
            {
                lease = Leases.GetLeaseAssociationIdAndDuid(identityAssociationId.Value, clientIdentifier);
            }
            else
            {
                var tempLease = Leases.GetLeaseByClientDuid(clientIdentifier);

                if (tempLease.PrefixDelegation != DHCPv6PrefixDelegation.None &&
                    tempLease.PrefixDelegation.IdentityAssociation == prefixIdentityAssociationId.Value
                    )
                {
                    lease = tempLease;
                }
            }

            DHCPv6Packet response;
            var error = DHCPv6ReleaseHandledEvent.ReleaseError.NoError;
            if (lease == DHCPv6Lease.NotFound)
            {
                response = DHCPv6Packet.AsError(packet, DHCPv6StatusCodes.NoBinding, properyResolver.GetServerDuid());
                error = DHCPv6ReleaseHandledEvent.ReleaseError.NoLeaseFound;
            }
            else
            {
                if (lease.IsActive() == false)
                {
                    response = DHCPv6Packet.AsError(packet, DHCPv6StatusCodes.NoBinding, properyResolver.GetServerDuid());
                }
                else
                {
                    PrefixBinding prefixBinding = lease.PrefixDelegation != DHCPv6PrefixDelegation.None ?
                        PrefixBinding.FromLease(lease) : null;

                    lease.Release(identityAssociationId.HasValue == false);
                    response = DHCPv6Packet.AsReleaseResponse(packet, lease.IdentityAssocicationId, lease.PrefixDelegation.IdentityAssociation, properyResolver.GetServerDuid());

                    if (prefixBinding != null)
                    {
                        AddNotificationTrigger(PrefixEdgeRouterBindingUpdatedTrigger.WithOldBinding(
                            Id, prefixBinding));
                    }
                }
            }

            base.Apply(new DHCPv6ReleaseHandledEvent(this.Id, packet, response, error));
            return response;
        }

        internal DHCPv6Packet HandleConfirm(DHCPv6Packet packet, IDHCPv6ServerPropertiesResolver properyResolver)
        {
            var addressProperties = GetAddressProperties();
            var innerPacket = packet.GetInnerPacket();

            DUID clientIdentifier = innerPacket.GetClientIdentifer();
            UInt32? identityAssociationId = innerPacket.GetNonTemporaryIdentityAssocationId();
            UInt32? prefixIdentityAsscocationId = innerPacket.GetPrefixDelegationIdentityAssocationId();

            DHCPv6Lease lease;
            if (identityAssociationId.HasValue == true)
            {
                lease = Leases.GetLeaseAssociationIdAndDuid(identityAssociationId.Value, clientIdentifier);
            }
            else
            {
                lease = Leases.GetLeaseByClientDuid(clientIdentifier);
            }

            if (lease == DHCPv6Lease.NotFound)
            {
                base.Apply(new DHCPv6ConfirmHandledEvent(this.Id, packet, DHCPv6ConfirmHandledEvent.ConfirmErrors.LeaseNotFound));
                return DHCPv6Packet.Empty;
            }

            DHCPv6Packet response;
            if (lease.IsActive() == false)
            {
                response = DHCPv6Packet.AsError(packet, DHCPv6StatusCodes.NotOnLink, properyResolver.GetServerDuid());
                base.Apply(new DHCPv6ConfirmHandledEvent(this.Id, packet, response, DHCPv6ConfirmHandledEvent.ConfirmErrors.LeaseNotActive));
            }
            else
            {
                Boolean bindingVerified = true;
                if (identityAssociationId.HasValue == true)
                {
                    IPv6Address requestedAddress = innerPacket.GetNonTemporaryIdentiyAssocation(identityAssociationId.Value).GetAddress();
                    if (requestedAddress != lease.Address)
                    {
                        bindingVerified = false;
                    }
                }

                if (prefixIdentityAsscocationId.HasValue == true && bindingVerified == true)
                {
                    if ((prefixIdentityAsscocationId != lease.PrefixDelegation.IdentityAssociation))
                    {
                        bindingVerified = false;
                    }
                    else
                    {
                        DHCPv6PrefixDelegation requestedPrefix = innerPacket.GetPrefixDelegationIdentiyAssocation(prefixIdentityAsscocationId.Value).GetPrefixDelegation();
                        bindingVerified = requestedPrefix.AreValuesEqual(lease.PrefixDelegation);
                    }
                }

                if (bindingVerified == true)
                {
                    response = DHCPv6Packet.AsReply(packet, addressProperties, DHCPv6ScopeProperties.Empty, lease, true, properyResolver.GetServerDuid(), false);
                    base.Apply(new DHCPv6ConfirmHandledEvent(this.Id, packet, response));
                }
                else
                {
                    response = DHCPv6Packet.AsError(packet, DHCPv6StatusCodes.NotOnLink, properyResolver.GetServerDuid());
                    base.Apply(new DHCPv6ConfirmHandledEvent(this.Id, packet, response, DHCPv6ConfirmHandledEvent.ConfirmErrors.AddressMismtach));
                }
            }

            return response;
        }

        #endregion

        #region applies and when

        protected override DHCPv6ScopeAddressProperties GetEmptytProperties() => DHCPv6ScopeAddressProperties.Empty;

        protected override void Reactived()
        {
            base.Apply(new DHCPv6ScopeReactivedEvent(this.Id));
        }

        protected override void Suspend()
        {
            base.Apply(new DHCPv6ScopeSuspendedEvent(this.Id));
        }

        protected override void When(DomainEvent domainEvent)
        {
            Boolean handled = false;
            IInternalEventHandler entityToApply = Leases;

            switch (domainEvent)
            {
                case DHCPv6ScopePropertiesUpdatedEvent e:
                    Properties = e.Properties;
                    break;
                case DHCPv6ScopeAddressPropertiesUpdatedEvent e:
                    AddressRelatedProperties = e.AddressProperties;
                    SetSuspendedState(true, false);
                    break;
                case DHCPv6ScopeDescriptionUpdatedEvent e:
                    Description = new ScopeDescription(e.Description);
                    break;
                case DHCPv6ScopeNameUpdatedEvent e:
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
