using DaAPI.Core.Common;
using DaAPI.Core.Notifications;
using DaAPI.Core.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DaAPI.Core.Scopes
{
    public abstract class Scope<TScope, TPacket, TAddress, TLeases, TLease, TAddressProperties, TScopeProperties, TScopeProperty, TOption, TValueType> : AggregateRoot
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

        private readonly List<TScope> _subscopes;
        private readonly Action<NotifcationTrigger> _addtionalNotifier;

        protected void AddNotificationTrigger(NotifcationTrigger trigger) => _addtionalNotifier?.Invoke(trigger);

        #endregion

        #region Properties

        public ScopeName Name { get; protected set; }
        public ScopeDescription Description { get; protected set; }
        public Boolean IsSuspendend { get; protected set; }

        public IScopeResolver<TPacket, TAddress> Resolver { get; private set; }
        public TScope ParentScope { get; internal set; }
        public IEnumerable<TScope> GetChildScopes() => _subscopes.AsEnumerable();

        public TAddressProperties AddressRelatedProperties { get; protected set; }
        public TScopeProperties Properties { get; protected set; }

        public TLeases Leases { get; protected set; }

        #endregion

        #region Constructor

        protected Scope(
            Guid id,
            TAddressProperties addressRelatedProperties,
            TScopeProperties scopeProperties,
            Action<DomainEvent> addtionalApplier,
            Action<NotifcationTrigger> addtionalNotifier
            ) : base(id, addtionalApplier)
        {
            AddressRelatedProperties = addressRelatedProperties;
            Properties = scopeProperties;
            this._addtionalNotifier = addtionalNotifier;
            _subscopes = new List<TScope>();

            base.EventHandlingStrategy = EventHandlingStratgies.Multiple;
        }

        public static TScope NotFound => null;

        internal protected virtual void ProcessExpiredLeases(IEnumerable<TLease> expiredLeases)
        {
           
        }

        #endregion

        #region Methods

        private void GetProperties(TScopeProperties result)
        {
            if (ParentScope != null)
            {
                ParentScope.GetProperties(result);
            }

            result.OverrideProperties(Properties);
        }

        public TScopeProperties GetScopeProperties()
        {
            TScopeProperties result = new TScopeProperties();
            GetProperties(result);

            return result;
        }

        public TAddressProperties GetAddressProperties()
        {
            TAddressProperties result = GetEmptytProperties();
            GetAddressProperties(result);

            return result;
        }

        private void GetAddressProperties(TAddressProperties result)
        {
            if (ParentScope != null)
            {
                ParentScope.GetAddressProperties(result);
            }

            result.OverrideProperties(AddressRelatedProperties);
        }

        public bool HasParentScope() => ParentScope != NotFound;

        internal void SetResolver(IScopeResolver<TPacket, TAddress> resolver) => Resolver = resolver;

        internal void SetParent(TScope parent)
        {
            parent.AddSubscope((TScope)this);
            this.ParentScope = parent;
        }

        private void AddSubscope(TScope child) => _subscopes.Add(child);

        protected abstract TAddressProperties GetEmptytProperties();


        #endregion

        internal void DetachFromParent(Boolean includedChildren)
        {
            if (ParentScope == null)
            {
                return;
            }

            ParentScope._subscopes.Remove((TScope)this);
            this.ParentScope = NotFound;

            if (includedChildren == true)
            {
                foreach (TScope item in _subscopes.ToList())
                {
                    item.DetachFromParent(true);
                }
            }
        }

        protected abstract void Suspend();
        protected abstract void Reactived();

        internal void SetSuspendedState(Boolean includeChildren, Boolean addEvents)
        {
            TAddressProperties properties = GetAddressProperties();

            Boolean addressPropertiesAreInValid;
            if (HasParentScope() == false)
            {
                addressPropertiesAreInValid = properties.ValueAreValidForRoot() == false;
            }
            else
            {
                addressPropertiesAreInValid =
                properties.IsValid() == false ||
                ParentScope.AddressRelatedProperties.IsAddressRangeBetween(this.AddressRelatedProperties) == false;
            }

            if (addressPropertiesAreInValid == true && IsSuspendend == false)
            {
                if (addEvents == true)
                {
                    Suspend();
                }
                else
                {

                }
            }
            else if (addressPropertiesAreInValid == false && IsSuspendend == true)
            {
                if (addEvents == true)
                {
                    Reactived();
                }
                else
                {

                }
            }

            if (includeChildren == true)
            {
                foreach (var child in _subscopes)
                {
                    child.SetSuspendedState(true, addEvents);
                }
            }
        }

        public IEnumerable<Guid> GetChildIds(Boolean onlyDirectChildren)
        {
            if (onlyDirectChildren == true)
            {
                return new List<Guid>(_subscopes.Select(x => x.Id));
            }

            List<Guid> result = new List<Guid>();

            foreach (TScope item in _subscopes)
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
                foreach (TScope child in _subscopes)
                {
                    child.GetChildIds(ids, true);
                }
            }
        }

        public ICollection<Guid> GetParentIds()
        {
            List<Guid> result = new List<Guid>();

            GetParentIds(result, (TScope)this);
            return result;
        }

        private void GetParentIds(ICollection<Guid> ids, TScope scope)
        {
            if (scope.ParentScope == null)
            {
                return;
            }

            ids.Add(this.ParentScope.Id);
            GetParentIds(ids, this.ParentScope);
        }

        public override string ToString()
        {
            return $"{Name?.Value ?? "<not set>"} - {Id}";
        }
    }
}
