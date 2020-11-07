using DaAPI.Core.Common;
using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Exceptions;
using DaAPI.Core.Notifications.Triggers;
using DaAPI.Core.Packets.DHCPv6;
using DaAPI.Core.Scopes.DHCPv6.ScopeProperties;
using DaAPI.Core.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using static DaAPI.Core.Scopes.DHCPv6.DHCPv6LeaseEvents;
using static DaAPI.Core.Scopes.DHCPv6.DHCPv6PacketHandledEvents;
using static DaAPI.Core.Scopes.DHCPv6.DHCPv6ScopeEvents;

namespace DaAPI.Core.Scopes.DHCPv6
{
    public class DHCPv6RootScope : RootScope<DHCPv6Scope, DHCPv6Packet, IPv6Address, DHCPv6Leases, DHCPv6Lease, DHCPv6ScopeAddressProperties, DHCPv6ScopeProperties, DHCPv6ScopeProperty, UInt16, DHCPv6ScopePropertyType>
    {
        private readonly ILoggerFactory _loggerFactory;

        public DHCPv6RootScope(
            Guid id,
            IScopeResolverManager<DHCPv6Packet, IPv6Address> resolverManager,
            ILoggerFactory factory
            )
            : base(id, resolverManager, factory.CreateLogger<DHCPv6RootScope>())
        {
            this._loggerFactory = factory;
        }

        #region packet handling

        private void CheckPacket(DHCPv6Packet packet, DHCPv6PacketTypes expectedType)
        {
            if (packet == null || packet.IsValid == false || packet.GetInnerPacket().PacketType != expectedType)
            {
                _logger.LogError("unexpected packet type found");
                throw new ScopeException(DHCPv4ScopeExceptionReasons.InvalidPacket);
            }
        }



        private DHCPv6Packet HandlePacketByResolver(
            DHCPv6Packet packet,
            Func<DHCPv6Scope, DHCPv6Packet> handler, Func<DHCPv6Scope, DHCPv6Packet> onlyPrefixHandler,
            Func<DomainEvent> notFoundApplier)
        {
            DHCPv6Packet result = HandlePacketByResolver(
               packet,
               (scope) =>
               {
                   _logger.LogDebug("scope {name} resolved to packet", scope.Name);
                   if (packet.GetInnerPacket().GetNonTemporaryIdentityAssocationId().HasValue == true)
                   {
                       _logger.LogDebug("non temporary identity association found. Processing request");
                       return handler(scope);
                   }
                   else if (packet.GetInnerPacket().GetPrefixDelegationIdentityAssocationId().HasValue == true)
                   {
                       _logger.LogDebug("only prefix delegation association found. Processing prefix delegation");
                       return onlyPrefixHandler(scope);
                   }
                   else
                   {
                       _logger.LogError("neihter non temporary nor prefix delgation identification found");
                       return DHCPv6Packet.Empty;
                   }
               },
               notFoundApplier);

            return result;
        }


        public DHCPv6Packet HandleSolicit(DHCPv6Packet packet, IDHCPv6ServerPropertiesResolver propertyResolver)
        {
            CheckPacket(packet, DHCPv6PacketTypes.Solicit);

            DHCPv6Packet result = HandlePacketByResolver(
                packet,
                (scope) => scope.HandleSolicit(packet, propertyResolver),
                (scope) => scope.HandleSolicitWithPrefixDelegation(packet, propertyResolver),
                () => new DHCPv6SolicitHandledEvent(packet)
                );
            return result;
        }

        public DHCPv6Packet HandleRequest(DHCPv6Packet packet, IDHCPv6ServerPropertiesResolver propertyResolver)
        {
            CheckPacket(packet, DHCPv6PacketTypes.REQUEST);

            DHCPv6Packet result = HandlePacketByResolver(
                  packet,
                  (scope) => scope.HandleRequest(packet, propertyResolver),
                  () => new DHCPv6RequestHandledEvent(packet)
                  );
            return result;
        }

        public DHCPv6Packet HandleRenew(DHCPv6Packet packet, IDHCPv6ServerPropertiesResolver propertyResolver)
        {
            CheckPacket(packet, DHCPv6PacketTypes.RENEW);

            DHCPv6Packet result = null;
            if (packet.IsUnicast() == true)
            {
                _logger.LogDebug("RENEW packet is unicasted. Looking for matching scope based on source address {address}", packet.Header.Source);

                result = HandlePacketBySourceAddress(
                packet,
                (scope) => scope.HandleRenew(packet, propertyResolver),
                () => new DHCPv6RenewHandledEvent(packet)
                );
            }
            else
            {
                _logger.LogDebug("Looking for matching scope based on scope resolvers");

                result = HandlePacketByResolver(
                 packet,
                 (scope) => scope.HandleRenew(packet, propertyResolver),
                 () => new DHCPv6RenewHandledEvent(packet)
                 );
            }

            return result;
        }

        public DHCPv6Packet HandleRebind(DHCPv6Packet packet, IDHCPv6ServerPropertiesResolver propertyResolver)
        {
            CheckPacket(packet, DHCPv6PacketTypes.REBIND);

            DHCPv6Packet result = HandlePacketByResolver(
                 packet,
                 (scope) => scope.HandleRebind(packet, propertyResolver),
                 () => new DHCPv6RebindHandledEvent(packet)
                 );

            return result;
        }

        public DHCPv6Packet HandleRelease(DHCPv6Packet packet, IDHCPv6ServerPropertiesResolver propertyResolver)
        {
            CheckPacket(packet, DHCPv6PacketTypes.RELEASE);

            DHCPv6Packet result = null;
            if (packet.IsUnicast() == true)
            {
                _logger.LogDebug("RELEASE packet is unicasted. Looking for matching scope based on source address {address}", packet.Header.Source);

                result = HandlePacketBySourceAddress(
                packet,
                (scope) => scope.HandleRelease(packet, propertyResolver),
                () => new DHCPv6ReleaseHandledEvent(packet)
                );
            }
            else
            {
                _logger.LogDebug("Looking for matching scope based on scope resolvers");

                result = HandlePacketByResolver(
                 packet,
                 (scope) => scope.HandleRelease(packet, propertyResolver),
                 () => new DHCPv6ReleaseHandledEvent(packet)
                 );
            }

            return result;
        }

        public DHCPv6Packet HandleConfirm(DHCPv6Packet packet, IDHCPv6ServerPropertiesResolver propertyResolver)
        {
            CheckPacket(packet, DHCPv6PacketTypes.CONFIRM);

            DHCPv6Packet result = HandlePacketByResolver(
                 packet,
                 (scope) => scope.HandleConfirm(packet, propertyResolver),
                 () => new DHCPv6RebindHandledEvent(packet)
                 );

            return result;
        }

        #endregion

        #region Helper

        #endregion

        #region Applies and when

        public Boolean AddScope(
          DHCPv6ScopeCreateInstruction instructions)
        {
            CheckScopeCreationInstruction(instructions);
            base.Apply(new DHCPv6ScopeAddedEvent(instructions));

            return true;
        }

        public override Boolean UpdateScopeName(Guid id, ScopeName name)
        {
            CheckIfScopeExistsById(id);
            base.Apply(new DHCPv6ScopeNameUpdatedEvent(id, name));

            return true;
        }

        public override Boolean UpdateScopeDescription(Guid id, ScopeDescription name)
        {
            CheckIfScopeExistsById(id);
            base.Apply(new DHCPv6ScopeDescriptionUpdatedEvent(id, name));
            return true;
        }

        public override Boolean UpdateScopeResolver(Guid scopeId, CreateScopeResolverInformation resolverInformation)
        {
            CheckIfScopeResolverIsValid(scopeId, resolverInformation);

            base.Apply(new DHCPv6ScopeResolverUpdatedEvent(scopeId, resolverInformation));

            CancelAllLeasesBecauseOfChangeOfResolver(scopeId);
            return true;
        }

        public override Boolean UpdateAddressProperties(Guid id, DHCPv6ScopeAddressProperties addressProperties)
        {
            CheckUpdateAddressProperties(id, addressProperties);
            var leases = GetScopeById(id).Leases.GetAllLeasesNotInRange(addressProperties.Start, addressProperties.End);

            base.Apply(new DHCPv6ScopeAddressPropertiesUpdatedEvent(id, addressProperties));
            foreach (var item in leases)
            {
                if(item.PrefixDelegation != DHCPv6PrefixDelegation.None)
                {
                    AddTrigger(PrefixEdgeRouterBindingUpdatedTrigger.WithOldBinding(id, PrefixBinding.FromLease(item)));
                }

                base.Apply(new DHCPv6LeaseCanceledEvent(item.Id, id, LeaseCancelReasons.AddressRangeChanged));
            }

            return true;
        }

        public override Boolean UpdateScopeProperties(Guid id, DHCPv6ScopeProperties properties)
        {
            CheckUpdateScopeProperties(id, properties);

            base.Apply(new DHCPv6ScopePropertiesUpdatedEvent(id, properties));

            return true;
        }

        public override Boolean UpdateParent(Guid scopeId, Guid? parentId)
        {
            Boolean needToChange = ParentNeedsToBeUpdated(scopeId, parentId);
            if (needToChange == false) { return false; }

            base.Apply(new DHCPv6ScopeParentUpdatedEvent(scopeId, parentId));
            return true;

        }

        public override Boolean DeleteScope(Guid scopeId, Boolean includeChildren)
        {
            CheckIfScopeExistsById(scopeId);

            base.Apply(new DHCPv6ScopeDeletedEvent(scopeId, includeChildren));
            return true;
        }

        #endregion

        protected override void When(DomainEvent domainEvent)
        {
            DHCPv6Scope scope;
            switch (domainEvent)
            {
                case DHCPv6ScopeAddedEvent e:
                    DHCPv6Scope scopeToAdd = DHCPv6Scope.FromInstructions(e.Instructions, Apply, AddTrigger, _loggerFactory.CreateLogger<DHCPv6Scope>());
                    HandleScopeAdded(scopeToAdd, e.Instructions);
                    break;
                case DHCPv6ScopeParentUpdatedEvent e:
                    HandleParentChanged(e.EntityId, e.ParentId);
                    break;
                case DHCPv6ScopeDeletedEvent e:
                    HandleScopeDeleted(e.EntityId, e.IncludeChildren);
                    break;
                case DHCPv6LeaseCreatedEvent e:
                    scope = GetScopeById(e.ScopeId);
                    ApplyToEnity(scope, e);
                    break;
                case DHCPv6ScopeResolverUpdatedEvent e:
                    scope = GetScopeById(e.EntityId);
                    HandleResolverChanged(e.EntityId, e.ResolverInformationen);
                    break;
                case DHCPv6ScopeRelatedEvent e:
                    scope = GetScopeByLeaseId(e.EntityId);
                    ApplyToEnity(scope, domainEvent);
                    break;
                case EntityBasedDomainEvent e:
                    scope = GetScopeById(e.EntityId);
                    ApplyToEnity(scope, domainEvent);
                    break;
                default:
                    break;
            }
        }
    }
}
