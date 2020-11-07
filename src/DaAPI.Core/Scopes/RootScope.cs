using DaAPI.Core.Common;
using DaAPI.Core.Exceptions;
using DaAPI.Core.Notifications;
using DaAPI.Core.Packets;
using DaAPI.Core.Scopes.DHCPv6.Resolvers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace DaAPI.Core.Scopes
{
    public abstract class RootScope<TScope, TPacket, TAddress, TLeases, TLease, TAddressProperties, TScopeProperties, TScopeProperty, TOption, TValueType> :
        AggregateRootWithEvents, INotificationTriggerSource
        where TScope : Scope<TScope, TPacket, TAddress, TLeases, TLease, TAddressProperties, TScopeProperties, TScopeProperty, TOption, TValueType>
        where TPacket : DHCPPacket<TPacket, TAddress>
        where TAddress : IPAddress<TAddress>
        where TLeases : Leases<TLeases, TLease, TAddress>
        where TLease : Lease<TLease, TAddress>
        where TAddressProperties : ScopeAddressProperties<TAddressProperties, TAddress>
        where TScopeProperties : ScopeProperties<TScopeProperty, TOption, TValueType>, new()
        where TScopeProperty : ScopeProperty<TOption, TValueType>

    {
        #region Fields

        private readonly List<TScope> _firstLevelScopes = new List<TScope>();
        private readonly Dictionary<Guid, TScope> _scopes = new Dictionary<Guid, TScope>();
        private readonly IScopeResolverManager<TPacket, TAddress> _resolverManager;
        protected readonly ILogger _logger;
        private readonly TScope _notFoundScope = Scope<TScope, TPacket, TAddress, TLeases, TLease, TAddressProperties, TScopeProperties, TScopeProperty, TOption, TValueType>.NotFound;

        private readonly List<NotifcationTrigger> _triggers = new List<NotifcationTrigger>();
        public IEnumerable<NotifcationTrigger> GetTriggers() => _triggers.Where(x => x.IsEmpty() == false).ToList().AsEnumerable();
        public void ClearTriggers() => _triggers.Clear();
        protected void AddTrigger(NotifcationTrigger trigger) => _triggers.Add(trigger);

        #endregion

        protected RootScope(Guid id, IScopeResolverManager<TPacket, TAddress> resolverManager, ILogger logger)
            : base(id)
        {
            _resolverManager = resolverManager;
            _logger = logger;
        }

        #region GetScopes

        public Int32 GetTotalAmountOfScopes() => _scopes.Count;

        public int CleanUpLeases()
        {
            Int32 affectedLeaseCounter = 0;
            foreach (var scope in _scopes.Values)
            {
                var expiredLeases = scope.Leases.CleanupExpiredLeases();
                scope.ProcessExpiredLeases(expiredLeases);

                affectedLeaseCounter += scope.Leases.CleanupPendingLeases();
                affectedLeaseCounter += expiredLeases.Count();
            }

            return affectedLeaseCounter;
        }

        public void DropUnusedLeasesOlderThan(DateTime leaseThreshold)
        {
            foreach (var scope in _scopes.Values)
            {
                scope.Leases.DropUnusedLeasesOlderThan(leaseThreshold);
            }
        }

        protected TScope GetScopeByMacthingCondition(TPacket packet, TScope scope, Func<TPacket, TScope, Boolean> matchingCondtions, Func<TScope, Boolean> increaseCondition, ref Int32 depth)
        {
            Boolean scopeResolverResult = matchingCondtions.Invoke(packet, scope);

            if (scopeResolverResult == true && scope.IsSuspendend == false)
            {
                _logger.LogDebug("scope {name} is a match", scope.Name);
                if (increaseCondition(scope) == true)
                {
                    depth += 1;
                }

                if (scope.GetChildScopes().Any() == true)
                {
                    _logger.LogDebug("scope {name} has {count} child scopes", scope.Name, scope.GetChildScopes().Count());
                    TScope innerScopeResult = null;
                    Int32 maxDepth = 0;
                    foreach (TScope subScope in scope.GetChildScopes())
                    {
                        Int32 subDepth = depth;
                        TScope innerScopeBranchResult = GetScopeByMacthingCondition(packet, subScope, matchingCondtions, increaseCondition, ref subDepth);
                        if (innerScopeBranchResult != null)
                        {
                            if (subDepth > maxDepth)
                            {
                                innerScopeResult = innerScopeBranchResult;
                                maxDepth = subDepth;

                                _logger.LogDebug("child scope {name} is match. Depth is now {depth}", innerScopeBranchResult.Name, maxDepth);
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
                _logger.LogDebug("scope {name} is a not match", scope.Name);

                return _notFoundScope;
            }
        }

        protected TScope GetMachtingScope(TPacket packet, Func<TPacket, TScope, Boolean> matchingCondtions, Func<TScope, Boolean> increaseCondition)
        {
            TScope resultScope = _notFoundScope;
            Int32 maxDepth = 0;

            foreach (TScope scope in _firstLevelScopes)
            {
                _logger.LogDebug("{amount} scopes found in root space", _firstLevelScopes.Count);

                Int32 branchDepth = 0;
                TScope result = GetScopeByMacthingCondition(
                    packet,
                    scope,
                    matchingCondtions,
                    increaseCondition,
                    ref branchDepth);

                if (result != _notFoundScope)
                {
                    _logger.LogDebug("found scope {name} as matching scope", result.Name);
                    if (branchDepth > maxDepth)
                    {
                        resultScope = result;
                        maxDepth = branchDepth;
                        _logger.LogDebug("new max depth found. current max depth is {maxDepth}", maxDepth);
                    }
                }
            }

            return resultScope;
        }

        #endregion

        #region handle packets

        protected TPacket HandlePacketByResolver(
            TPacket packet,
            Func<TScope, TPacket> handler,
            Func<DomainEvent> notFoundApplier)
        {
            _logger.LogDebug("handling packet by finding matching resolvers");

            return HandlePacketByMatchingCondtion(packet,
                (inputPacket, inputScope) =>
                inputScope.Resolver.PacketMeetsCondition(inputPacket),
                (scope) => scope.Resolver is IPseudoResolver == false,
                (scope) =>
                {
                    if (scope.Resolver is IPseudoResolver)
                    {
                        _logger.LogDebug("found scope {name} but this scope has a pseudo resolver. Hence, it is treated as no scope found", scope.Name);

                        base.Apply(notFoundApplier());
                        return DHCPPacket<TPacket, TAddress>.Empty;
                    }
                    else
                    {
                        return handler(scope);
                    }
                },
                notFoundApplier
                );
        }

        protected TPacket HandlePacketBySourceAddress(
            TPacket packet,
            Func<TScope, TPacket> handler,
            Func<DomainEvent> notFoundApplier
            )
        {
            return HandlePacketByAddress(
                packet,
                packet.Header.Source,
                handler,
                notFoundApplier);
        }

        protected TPacket HandlePacketByAddress(
            TPacket packet,
            TAddress address,
            Func<TScope, TPacket> handler,
            Func<DomainEvent> notFoundApplier
        )
        {
            return HandlePacketByMatchingCondtion(
                packet,
                (inputPacket, inputScope) => inputScope.AddressRelatedProperties.IsAddressInRange(address),
                (scope) => true,
                handler,
                notFoundApplier
            );
        }

        protected TPacket HandlePacketByMatchingCondtion(
            TPacket packet,
            Func<TPacket, TScope, Boolean> matchingCondtions,
            Func<TScope, Boolean> increaseCondition,
            Func<TScope, TPacket> handler,

            Func<DomainEvent> notFoundApplier
            )
        {
            TScope matchingScope = GetMachtingScope(packet, matchingCondtions, increaseCondition);
            if (matchingScope == _notFoundScope)
            {
                _logger.LogDebug("no patching scope found");
                base.Apply(notFoundApplier());
                return DHCPPacket<TPacket, TAddress>.Empty;
            }

            TPacket result = handler(matchingScope);
            return result;
        }

        #endregion

        public abstract Boolean UpdateScopeName(Guid id, ScopeName name);
        public abstract Boolean UpdateScopeDescription(Guid id, ScopeDescription name);
        public abstract Boolean UpdateScopeResolver(Guid scopeId, CreateScopeResolverInformation resolverInformation);
        public abstract Boolean UpdateAddressProperties(Guid id, TAddressProperties addressProperties);
        public abstract Boolean UpdateScopeProperties(Guid id, TScopeProperties properties);

        public abstract Boolean DeleteScope(Guid scopeId, Boolean includeChildren);
        public abstract Boolean UpdateParent(Guid scopeId, Guid? parentId);

        private void CheckAddressProperties(TScope parent, TAddressProperties addressProperties)
        {
            if (parent == _notFoundScope)
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

                if (resultingAddressProperties.IsValid() == false)
                {
                    throw new ScopeException(DHCPv4ScopeExceptionReasons.InvalidTimeRanges);
                }
            }
        }

        public void CheckScopeCreationInstruction(ScopeCreateInstruction<TAddressProperties, TAddress, TScopeProperties, TScopeProperty, TOption, TValueType> instructions)
        {
            if (instructions == null || instructions.IsValid() == false || instructions.ScopeProperties == null)
            {
                throw new ScopeException(DHCPv4ScopeExceptionReasons.NoInput);
            }

            if (_scopes.ContainsKey(instructions.Id) == true)
            {
                throw new ScopeException(DHCPv4ScopeExceptionReasons.IdExists);
            }

            TScope parent = _notFoundScope;
            if (instructions.ParentId.HasValue == true)
            {
                if (_scopes.ContainsKey(instructions.ParentId.Value) == false)
                {
                    throw new ScopeException(DHCPv4ScopeExceptionReasons.ScopeParentNotFound);
                }

                parent = _scopes[instructions.ParentId.Value];
            }

            CheckAddressProperties(parent, instructions.AddressProperties);

            if (_resolverManager.IsResolverInformationValid(instructions.ResolverInformation) == false)
            {
                throw new ScopeException(DHCPv4ScopeExceptionReasons.InvalidResolver);
            }
        }

        protected void CheckIfScopeExistsById(Guid id)
        {
            if (_scopes.ContainsKey(id) == false)
            {
                throw new ScopeException(DHCPv4ScopeExceptionReasons.ScopeNotFound);
            }
        }

        public void CheckIfScopeResolverIsValid(
            Guid scopeId,
            CreateScopeResolverInformation resolverInformation
            )
        {
            CheckIfScopeExistsById(scopeId);

            if (resolverInformation == null)
            {
                throw new ScopeException(DHCPv4ScopeExceptionReasons.NoInput);
            }

            if (_resolverManager.IsResolverInformationValid(resolverInformation) == false)
            {
                throw new ScopeException(DHCPv4ScopeExceptionReasons.InvalidResolver);
            }

            return;
        }

        protected void CancelAllLeasesBecauseOfChangeOfResolver(Guid scopeId)
        {
            var scope = GetScopeById(scopeId);
            scope.Leases.CancelAllLeases(LeaseCancelReasons.ResolverChanged);
        }

        public void CheckUpdateAddressProperties(Guid id, TAddressProperties addressProperties)
        {
            CheckIfScopeExistsById(id);
            TScope scope = GetScopeById(id);

            CheckAddressProperties(scope.ParentScope, addressProperties);
        }

        protected void CheckUpdateScopeProperties(Guid id, TScopeProperties properties)
        {
            if (properties == null)
            {
                throw new ScopeException(DHCPv4ScopeExceptionReasons.NoInput);
            }

            CheckIfScopeExistsById(id);
        }

        public Boolean ParentNeedsToBeUpdated(Guid scopeId, Guid? parentId)
        {
            CheckIfScopeExistsById(scopeId);
            if (parentId.HasValue == true)
            {
                if (_scopes.ContainsKey(parentId.Value) == false)
                {
                    throw new ScopeException(DHCPv4ScopeExceptionReasons.ScopeParentNotFound);
                }

                TScope parentScope = _scopes[parentId.Value];
                IEnumerable<Guid> parentIds = parentScope.GetParentIds();
                if (parentIds.Contains(scopeId) == true)
                {
                    throw new ScopeException(DHCPv4ScopeExceptionReasons.ParentCanBeAddedAsChild);
                }
            }

            TScope scope = GetScopeById(scopeId);
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

            return true;
        }

        protected TScope GetScopeByLeaseId(Guid leaseId)
        {
            TScope scope = _scopes.Values.FirstOrDefault(x => x.Leases.Contains(leaseId) == true);
            return scope;
        }

        protected void HandleResolverChanged(Guid id, CreateScopeResolverInformation resolverInfo)
        {
            TScope scope = GetScopeById(id);
            var resolver = _resolverManager.InitializeResolver(resolverInfo);
            scope.SetResolver(resolver);
        }

        protected void HandleScopeAdded(TScope scope, ScopeCreateInstruction<TAddressProperties, TAddress, TScopeProperties, TScopeProperty, TOption, TValueType> instruction)
        {
            if (instruction.ParentId.HasValue == true)
            {
                TScope parent = _scopes[instruction.ParentId.Value];
                scope.SetParent(parent);
            }
            else
            {
                _firstLevelScopes.Add(scope);
            }

            IScopeResolver<TPacket, TAddress> resolver = _resolverManager.InitializeResolver(instruction.ResolverInformation);
            scope.SetResolver(resolver);

            _scopes.Add(scope.Id, scope);
        }

        protected void HandleParentChanged(Guid scopeId, Guid? parentId)
        {
            TScope scope = GetScopeById(scopeId);

            if (scope.HasParentScope() == true)
            {
                scope.DetachFromParent(false);
            }

            if (parentId.HasValue == false)
            {
                _firstLevelScopes.Add(scope);
            }
            else
            {
                TScope parentScope = GetScopeById(parentId.Value);
                scope.SetParent(parentScope);
            }

            scope.SetSuspendedState(true, false);
        }

        protected void HandleScopeDeleted(Guid scopeId, Boolean includeChildren)
        {
            TScope scope = GetScopeById(scopeId);
            if (scope == null) { return; }

            if (includeChildren == false)
            {
                Guid? parentId = null;
                if (scope.HasParentScope() == true)
                {
                    parentId = scope.ParentScope.Id;
                }

                foreach (TScope child in scope.GetChildScopes().ToList())
                {
                    HandleParentChanged(child.Id, parentId);
                }

                _scopes.Remove(scope.Id);

                scope.DetachFromParent(false);
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

            if (_firstLevelScopes.Contains(scope) == true)
            {
                _firstLevelScopes.Remove(scope);
            }
        }

        #region queries

        public IEnumerable<TScope> GetRootScopes() => _firstLevelScopes.AsEnumerable();

        public TScope GetScopeById(Guid childId)
        {
            if (_scopes.ContainsKey(childId) == false)
            {
                return _notFoundScope;
            }

            return _scopes[childId];
        }



        #endregion

    }
}


