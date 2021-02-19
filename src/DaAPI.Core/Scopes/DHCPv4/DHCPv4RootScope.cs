using DaAPI.Core.Common;
using DaAPI.Core.Exceptions;
using DaAPI.Core.Notifications;
using DaAPI.Core.Packets.DHCPv4;
using DaAPI.Core.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using static DaAPI.Core.Scopes.DHCPv4.DHCPv4LeaseEvents;
using static DaAPI.Core.Scopes.DHCPv4.DHCPv4PacketHandledEvents;
using static DaAPI.Core.Scopes.DHCPv4.DHCPv4ScopeEvents;

namespace DaAPI.Core.Scopes.DHCPv4
{
    public class DHCPv4RootScope : RootScope<DHCPv4Scope, DHCPv4Packet, IPv4Address, DHCPv4Leases, DHCPv4Lease, DHCPv4ScopeAddressProperties, DHCPv4ScopeProperties, DHCPv4ScopeProperty, Byte, DHCPv4ScopePropertyType>
    {
        private readonly ILoggerFactory _loggerFactory;

        public DHCPv4RootScope(
            Guid id,
            IScopeResolverManager<DHCPv4Packet, IPv4Address> resolverManager,
            ILoggerFactory factory
            )
            : base(id, resolverManager, factory.CreateLogger<DHCPv4RootScope>())
        {
            this._loggerFactory = factory;
        }

        #region packet handling

        private void CheckPacket(DHCPv4Packet packet, DHCPv4MessagesTypes expectedType)
        {
            if (packet == null || packet.IsValid == false || packet.MessageType != expectedType)
            {
                throw new ScopeException(DHCPv4ScopeExceptionReasons.InvalidPacket);
            }
        }

        public DHCPv4Packet HandleDiscover(DHCPv4Packet packet)
        {
            CheckPacket(packet, DHCPv4MessagesTypes.Discover);

            DHCPv4Packet result = HandlePacketByResolver(
                packet,
                (scope) => scope.HandleDiscover(packet),
                () => new DHCPv4DiscoverHandledEvent(packet)
                );
            return result;
        }

        public DHCPv4Packet HandleRequest(DHCPv4Packet packet)
        {
            CheckPacket(packet, DHCPv4MessagesTypes.Request);

            DHCPv4Packet result = DHCPv4Packet.Empty;

            if (packet.GetRequestType() == DHCPv4Packet.DHCPv4PacketRequestType.Renewing)
            {
                result = HandlePacketBySourceAddress(
                    packet,
                    (scope) => scope.HandleRequest(packet),
                    () => new DHCPv4RequestHandledEvent(packet)
                    );
            }
            else
            {
                result = HandlePacketByResolver(
                    packet,
                    (scope) => scope.HandleRequest(packet),
                    () => new DHCPv4RequestHandledEvent(packet)
                    );
            }

            return result;
        }

        public DHCPv4Packet HandleDecline(DHCPv4Packet packet)
        {
            CheckPacket(packet, DHCPv4MessagesTypes.Decline);

            IPv4Address address = packet.GetRequestedAddressFromRequestedOption();

            DHCPv4Packet result = HandlePacketByAddress(
                packet,
                address,
                (scope) => scope.HandleDecline(packet),
                () => new DHCPv4DeclineHandledEvent(packet)
                );
            return result;
        }

        public DHCPv4Packet HandleRelease(DHCPv4Packet packet)
        {
            CheckPacket(packet, DHCPv4MessagesTypes.Release);

            DHCPv4Packet result = HandlePacketBySourceAddress(
                packet,
                (scope) => scope.HandleRelease(packet),
                () => new DHCPv4ReleaseHandledEvent(packet)
                ); ;

            return result;
        }

        public DHCPv4Packet HandleInform(DHCPv4Packet packet)
        {
            CheckPacket(packet, DHCPv4MessagesTypes.Inform);

            DHCPv4Packet result = HandlePacketBySourceAddress(
                packet,
                (scope) => scope.HandleInform(packet),
                () => new DHCPv4InformHandledEvent(packet, DHCPv4InformHandledEvent.InformErros.ScopeNotFound)
                );
            return result;
        }

        #endregion

        #region Applies and when

        public Boolean AddScope(
          DHCPv4ScopeCreateInstruction instructions)
        {
            CheckScopeCreationInstruction(instructions);
            base.Apply(new DHCPv4ScopeAddedEvent(instructions));

            return true;
        }

        public override Boolean UpdateScopeName(Guid id, ScopeName name)
        {
            CheckIfScopeExistsById(id);
            base.Apply(new DHCPv4ScopeNameUpdatedEvent(id, name));

            return true;
        }

        public override Boolean UpdateScopeDescription(Guid id, ScopeDescription name)
        {
            CheckIfScopeExistsById(id);
            base.Apply(new DHCPv4ScopeDescriptionUpdatedEvent(id, name));
            return true;
        }

        public override Boolean UpdateScopeResolver(Guid scopeId, CreateScopeResolverInformation resolverInformation)
        {
            CheckIfScopeResolverIsValid(scopeId, resolverInformation);

            base.Apply(new DHCPv4ScopeResolverUpdatedEvent(scopeId, resolverInformation));

            CancelAllLeasesBecauseOfChangeOfResolver(scopeId);
            return true;
        }

        public override Boolean UpdateAddressProperties(Guid id, DHCPv4ScopeAddressProperties addressProperties)
        {
            CheckUpdateAddressProperties(id, addressProperties);
            var leases = GetScopeById(id).Leases.GetAllLeasesNotInRange(addressProperties.Start, addressProperties.End);

            base.Apply(new DHCPv4ScopeAddressPropertiesUpdatedEvent(id, addressProperties));
            foreach (var item in leases)
            {
                base.Apply(new DHCPv4LeaseCanceledEvent(item.Id, id, LeaseCancelReasons.AddressRangeChanged));
            }

            return true;
        }

        public override Boolean UpdateScopeProperties(Guid id, DHCPv4ScopeProperties properties)
        {
            CheckUpdateScopeProperties(id, properties);

            base.Apply(new DHCPv4ScopePropertiesUpdatedEvent(id, properties));

            return true;
        }

        public override Boolean UpdateParent(Guid scopeId, Guid? parentId)
        {
            Boolean needToChange = ParentNeedsToBeUpdated(scopeId, parentId);
            if (needToChange == false) { return false; }

            base.Apply(new DHCPv4ScopeParentUpdatedEvent(scopeId, parentId));
            return true;

        }

        public override Boolean DeleteScope(Guid scopeId, Boolean includeChildren)
        {
            CheckIfScopeExistsById(scopeId);

            base.Apply(new DHCPv4ScopeDeletedEvent(scopeId, includeChildren));
            return true;
        }

        protected override void When(DomainEvent domainEvent)
        {
            DHCPv4Scope scope;
            switch (domainEvent)
            {
                case DHCPv4ScopeAddedEvent e:
                    DHCPv4Scope scopeToAdd = DHCPv4Scope.FromInstructions(e.Instructions, Apply, AddTrigger, _loggerFactory.CreateLogger<DHCPv4Scope>());
                    HandleScopeAdded(scopeToAdd, e.Instructions);
                    break;
                case DHCPv4ScopeParentUpdatedEvent e:
                    HandleParentChanged(e.EntityId, e.ParentId);
                    break;
                case DHCPv4ScopeDeletedEvent e:
                    HandleScopeDeleted(e.EntityId, e.IncludeChildren);
                    break;
                case DHCPv4LeaseCreatedEvent e:
                    scope = GetScopeById(e.ScopeId);
                    ApplyToEnity(scope, e);
                    break;
                case DHCPv4ScopeResolverUpdatedEvent e:
                    scope = GetScopeById(e.EntityId);
                    HandleResolverChanged(e.EntityId, e.ResolverInformationen);
                    break;
                case DHCPv4ScopeRelatedEvent e:
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

        #endregion
    }
}
