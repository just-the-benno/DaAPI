using DaAPI.Core.Common;
using DaAPI.Core.Exceptions;
using DaAPI.Core.Packets.DHCPv4;
using DaAPI.Core.Services;
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
    public class DHCPv4RootScope : AggregateRootWithEvents
    {
        #region Fields

        private readonly List<DHCPv4Scope> _rootScopes = new List<DHCPv4Scope>();
        private readonly Dictionary<Guid, DHCPv4Scope> _scopes = new Dictionary<Guid, DHCPv4Scope>();
        private readonly IDHCPv4ScopeResolverManager _resolverManager;

        #endregion

        #region Properties

        #endregion

        #region Constructor

        public DHCPv4RootScope(
            Guid id,
            IDHCPv4ScopeResolverManager resolverManager) : base(id)
        {
            this._resolverManager = resolverManager ?? throw new ArgumentNullException(nameof(resolverManager));
        }

        
        #endregion

        #region packet handling

        protected DHCPv4Scope GetScopeByMacthingCondition(DHCPv4Packet packet, DHCPv4Scope scope, Func<DHCPv4Packet, DHCPv4Scope, Boolean> matchingCondtions, ref Int32 depth)
        {
            Boolean scopeResolverResult = matchingCondtions.Invoke(packet, scope);

            if (scopeResolverResult == true && scope.IsSuspendend == false)
            {
                depth += 1;

                if (scope.Subscopes.Count > 0)
                {
                    DHCPv4Scope innerScopeResult = null;
                    Int32 maxDepth = 0;
                    foreach (DHCPv4Scope subScope in scope.Subscopes)
                    {
                        Int32 subDepth = depth;
                        DHCPv4Scope innerScopeBranchResult = GetScopeByMacthingCondition(packet, subScope, matchingCondtions, ref subDepth);
                        if (innerScopeBranchResult != null)
                        {
                            if (subDepth > maxDepth)
                            {
                                innerScopeResult = innerScopeBranchResult;
                                maxDepth = subDepth;
                            }
                        }
                    }

                    if (innerScopeResult == null)
                    {
                        return scope;
                    }
                    else
                    {
                        depth = maxDepth;
                        return innerScopeResult;
                    }
                }
                else
                {
                    return scope;
                }
            }
            else
            {
                return DHCPv4Scope.NotFound;
            }
        }


        private DHCPv4Scope GetMachtingScope(DHCPv4Packet packet, Func<DHCPv4Packet, DHCPv4Scope, Boolean> matchingCondtions)
        {
            DHCPv4Scope resultScope = DHCPv4Scope.NotFound;
            Int32 maxDepth = 0;

            foreach (DHCPv4Scope scope in _rootScopes)
            {
                Int32 branchDepth = 0;
                DHCPv4Scope result = GetScopeByMacthingCondition(
                    packet,
                    scope,
                    matchingCondtions,
                    ref branchDepth);


                if (result != DHCPv4Scope.NotFound)
                {
                    if (branchDepth > maxDepth)
                    {
                        resultScope = result;
                        maxDepth = branchDepth;
                    }
                }
            }

            return resultScope;
        }

        private void CheckPacket(DHCPv4Packet packet, DHCPv4Packet.DHCPv4MessagesTypes expectedType)
        {
            if (packet == null || packet.IsValid == false || packet.MessageType != expectedType)
            {
                throw new ScopeException(DHCPv4ScopeExceptionReasons.InvalidPacket);
            }
        }

        private DHCPv4Packet HandlePacketByResolver(
            DHCPv4Packet packet,
            Func<DHCPv4Scope, DHCPv4Packet> handler,
            Func<DHCPv4PacketHandledEvent> notFoundApplier)
        {
            return HandlePacketByMatchingCondtion(packet,
                (inputPacket, inputScope) => inputScope.Resolver.PacketMeetsCondition(inputPacket),
                handler,
                notFoundApplier
                );
        }

        private DHCPv4Packet HandlePacketBySourceAddress(
            DHCPv4Packet packet,
            Func<DHCPv4Scope, DHCPv4Packet> handler,
            Func<DHCPv4PacketHandledEvent> notFoundApplier
            )
        {
            return HandlePacketByIPAddress(
                packet,
                packet.IPHeader.Source,
                handler,
                notFoundApplier);
        }

        private DHCPv4Packet HandlePacketByIPAddress(
            DHCPv4Packet packet,
            IPv4Address address,
            Func<DHCPv4Scope, DHCPv4Packet> handler,
            Func<DHCPv4PacketHandledEvent> notFoundApplier
        )
        {
            return HandlePacketByMatchingCondtion(
                packet,
                (inputPacket, inputScope) => inputScope.AddressRelatedProperties.IsAddressInRange(address),
                handler,
                notFoundApplier
            );
        }

        private DHCPv4Packet HandlePacketByMatchingCondtion(
            DHCPv4Packet packet,
            Func<DHCPv4Packet, DHCPv4Scope, Boolean> matchingCondtions,
            Func<DHCPv4Scope, DHCPv4Packet> handler,
            Func<DHCPv4PacketHandledEvent> notFoundApplier
            )
        {
            DHCPv4Scope matchingScope = GetMachtingScope(packet, matchingCondtions);
            if (matchingScope == DHCPv4Scope.NotFound)
            {
                base.Apply(notFoundApplier());
                return DHCPv4Packet.Empty;
            }

            DHCPv4Packet result = handler(matchingScope);
            return result;
        }


        public DHCPv4Packet HandleDiscover(DHCPv4Packet packet)
        {
            CheckPacket(packet, DHCPv4Packet.DHCPv4MessagesTypes.DHCPDISCOVER);

            DHCPv4Packet result = HandlePacketByResolver(
                packet,
                (scope) => scope.HandleDiscover(packet),
                () => new DHCPv4DiscoverHandledEvent(packet)
                );
            return result;
        }

        public DHCPv4Packet HandleRequest(DHCPv4Packet packet)
        {
            CheckPacket(packet, DHCPv4Packet.DHCPv4MessagesTypes.Request);

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
            CheckPacket(packet, DHCPv4Packet.DHCPv4MessagesTypes.Decline);

            IPv4Address address = packet.GetRequestedAddressFromRequestedOption();

            DHCPv4Packet result = HandlePacketByIPAddress(
                packet,
                address,
                (scope) => scope.HandleDecline(packet),
                () => new DHCPv4DeclineHandledEvent(packet)
                );
            return result;
        }

        public DHCPv4Packet HandleRelease(DHCPv4Packet packet)
        {
            CheckPacket(packet, DHCPv4Packet.DHCPv4MessagesTypes.DHCPRELEASE);

            DHCPv4Packet result = HandlePacketBySourceAddress(
                packet,
                (scope) => scope.HandleRelease(packet),
                () => new DHCPv4ReleaseHandledEvent(packet)
                ); ;

            return result;
        }

        public DHCPv4Packet HandleInform(DHCPv4Packet packet)
        {
            CheckPacket(packet, DHCPv4Packet.DHCPv4MessagesTypes.DHCPINFORM);

            DHCPv4Packet result = HandlePacketBySourceAddress(
                packet,
                (scope) => scope.HandleInform(packet),
                () => new DHCPv4InformHandledEvent(packet, DHCPv4InformHandledEvent.InformErros.ScopeNotFound)
                );
            return result;
        }

        #endregion

        #region applies and when

        private void CheckAddressProperties(DHCPv4Scope parent, DHCPv4ScopeAddressProperties addressProperties)
        {
            if (parent == DHCPv4Scope.NotFound)
            {
                if (addressProperties.ValueAreValidForRoot() == false)
                {
                    throw new ScopeException(DHCPv4ScopeExceptionReasons.AddressPropertiesInvalidForParents);
                }
            }
            else
            {
                if (parent.AddressRelatedProperties.IsAddressRangeBetween(addressProperties) == false)
                {
                    throw new ScopeException(DHCPv4ScopeExceptionReasons.NotInParentRange);
                }

                var resultingAddressProperties = parent.GetAddressProperties();
                resultingAddressProperties.OverrideProperties(addressProperties);

                if (resultingAddressProperties.AreTimeValueValid() == false)
                {
                    throw new ScopeException(DHCPv4ScopeExceptionReasons.InvalidTimeRanges);
                }
            }
        }

        public Boolean AddScope(
            DHCPv4ScopeCreateInstruction instructions)
        {
            if (instructions == null ||
                instructions.AddressProperties == null ||
                instructions.ResolverInformations == null ||
                instructions.Properties == null)
            {
                throw new ScopeException(DHCPv4ScopeExceptionReasons.NoInput);
            }

            if (_scopes.ContainsKey(instructions.Id) == true)
            {
                throw new ScopeException(DHCPv4ScopeExceptionReasons.IdExists);
            }

            DHCPv4Scope parent = DHCPv4Scope.NotFound;
            if (instructions.ParentId.HasValue == true)
            {
                if (_scopes.ContainsKey(instructions.ParentId.Value) == false)
                {
                    throw new ScopeException(DHCPv4ScopeExceptionReasons.ScopeParentNotFound);
                }

                parent = _scopes[instructions.ParentId.Value];
            }

            CheckAddressProperties(parent, instructions.AddressProperties);

            if (_resolverManager.ValidateDHCPv4Resolver(instructions.ResolverInformations) == false)
            {
                throw new ScopeException(DHCPv4ScopeExceptionReasons.InvalidResolver);
            }

            base.Apply(new DHCPv4ScopeAddedEvent(instructions));

            return true;
        }

        private void CheckIfScopeExistsById(Guid id)
        {
            if (_scopes.ContainsKey(id) == false)
            {
                throw new ScopeException(DHCPv4ScopeExceptionReasons.ScopeNotFound);
            }
        }

        public Boolean UpdateScopeName(Guid id, ScopeName name)
        {
            CheckIfScopeExistsById(id);
            base.Apply(new DHCPv4ScopeNameUpdatedEvent(id, name));

            return true;
        }

        public Boolean UpdateScopeDescription(Guid id, ScopeDescription name)
        {
            CheckIfScopeExistsById(id);
            base.Apply(new DHCPv4ScopeDescriptionUpdatedEvent(id, name));
            return true;
        }

        public Boolean UpdateScopeResolver(Guid id, DHCPv4CreateScopeResolverInformation resolverInformation)
        {
            CheckIfScopeExistsById(id);

            if (resolverInformation == null)
            {
                throw new ScopeException(DHCPv4ScopeExceptionReasons.NoInput);
            }

            if (_resolverManager.ValidateDHCPv4Resolver(resolverInformation) == false)
            {
                throw new ScopeException(DHCPv4ScopeExceptionReasons.InvalidResolver);
            }

            base.Apply(new DHCPv4ScopeResolverUpdatedEvent(id, resolverInformation));

            DHCPv4Scope scope = GetScopeById(id);
            scope.Leases.CancelAllLeases(DHCPv4Lease.DHCPv4LeaseCancelReasons.ResolverChanged);

            return true;
        }

        public Boolean UpdateAddressProperties(Guid id, DHCPv4ScopeAddressProperties addressProperties)
        {
            CheckIfScopeExistsById(id);
            DHCPv4Scope scope = GetScopeById(id);

            CheckAddressProperties(scope.ParentScope, addressProperties);

            base.Apply(new DHCPv4ScopeAddressPropertiesUpdatedEvent(id, addressProperties));
            return true;
        }

        public Boolean UpdateScopeProperties(Guid id, DHCPv4ScopeProperties properties)
        {
            if (properties == null)
            {
                throw new ScopeException(DHCPv4ScopeExceptionReasons.NoInput);
            }

            CheckIfScopeExistsById(id);
            base.Apply(new DHCPv4ScopePropertiesUpdatedEvent(id, properties));

            return true;
        }

        public Boolean UpdateParent(Guid scopeId, Guid? parentId)
        {
            CheckIfScopeExistsById(scopeId);
            if (parentId.HasValue == true)
            {
                CheckIfScopeExistsById(parentId.Value);
            }

            DHCPv4Scope scope = GetScopeById(scopeId);
            if (parentId.HasValue == false && scope.HasParentScope() == false)
            {
                //nothing to change
                return false;
            }
            else if (
                parentId.HasValue == true && scope.HasParentScope() == true &&
                scope.ParentScope.Id == parentId.Value
                )
            {
                //nothing to change
                return false;
            }

            base.Apply(new DHCPv4ScopeParentUpdatedEvent(scopeId, parentId));
            return true;
        }

        public Boolean DeleteScope(Guid scopeId, Boolean includeChildren)
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
                    HandleScopeAdded(e.Instructions);
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
                    IDHCPv4ScopeResolver resolver = _resolverManager.InitializeResolver(e.ResolverInformationen);
                    scope.SetResolver(resolver);
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

        private DHCPv4Scope GetScopeByLeaseId(Guid leaseId)
        {
            DHCPv4Scope scope = _scopes.Values.FirstOrDefault(x => x.Leases.Contains(leaseId) == true);
            return scope;
        }

        private void HandleScopeAdded(DHCPv4ScopeCreateInstruction instruction)
        {
            DHCPv4Scope scope = DHCPv4Scope.FromInstructions(instruction, _resolverManager, Apply);

            if (instruction.ParentId.HasValue == true)
            {
                DHCPv4Scope parent = _scopes[instruction.ParentId.Value];
                scope.SetParent(parent);
            }
            else
            {
                _rootScopes.Add(scope);
            }

            _scopes.Add(scope.Id, scope);
        }

        private void HandleParentChanged(Guid scopeId, Guid? parentId)
        {
            DHCPv4Scope scope = GetScopeById(scopeId);

            if (scope.HasParentScope() == true)
            {
                scope.DetachFromParent(false);
            }

            if (parentId.HasValue == false)
            {
                _rootScopes.Add(scope);
            }
            else
            {
                DHCPv4Scope parentScope = GetScopeById(parentId.Value);
                scope.SetParent(parentScope);
            }

            scope.SetSuspendedState(true);
        }

        private void HandleScopeDeleted(Guid scopeId, Boolean includeChildren)
        {
            DHCPv4Scope scope = GetScopeById(scopeId);
            IEnumerable<DHCPv4Scope> childScopes = scope.GetChildScopes();

            if (includeChildren == false)
            {
                Guid? parentId = null;
                if (scope.HasParentScope() == true)
                {
                    parentId = scope.ParentScope.Id;
                }

                foreach (DHCPv4Scope child in childScopes)
                {
                    UpdateParent(child.Id, parentId);
                }

                _scopes.Remove(scope.Id);
            }
            else
            {
                List<Guid> idsToRemove = new List<Guid>(scope.GetChildIds(false));
                idsToRemove.Insert(0, scope.Id);

                scope.DetachFromParent(true);

                foreach (Guid item in idsToRemove)
                {
                    _scopes.Remove(item);
                }
            }
        }

        #endregion

        #region queries

        public IEnumerable<DHCPv4Scope> GetRootScopes() => _rootScopes.AsEnumerable();

        public DHCPv4Scope GetScopeById(Guid childId)
        {
            if (_scopes.ContainsKey(childId) == false)
            {
                return DHCPv4Scope.NotFound;
            }

            return _scopes[childId];
        }

        #endregion
    }
}
