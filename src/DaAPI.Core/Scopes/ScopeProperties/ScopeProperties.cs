using DaAPI.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DaAPI.Core.Scopes
{
    public abstract class ScopeProperties<TScopeProperty, TOption, TValueType> : Value, IEquatable<ScopeProperties<TScopeProperty, TOption, TValueType>>
        where TScopeProperty : ScopeProperty<TOption, TValueType>
    {
        #region Fields

        private Dictionary<TOption, TScopeProperty> _properties = new Dictionary<TOption, TScopeProperty>();

        #endregion

        #region Properties

        public IEnumerable<TScopeProperty> Properties
        {
            get => _properties.Values.ToList();
            private set => _properties = value.ToDictionary(x => x.OptionIdentifier, x => x);
        }

        #endregion

        #region Constructor

        protected ScopeProperties(IEnumerable<TScopeProperty> properties)
        {
            foreach (var item in properties)
            {
                Add(item);
            }
        }

        #endregion

        #region Methods

        private void Add(TScopeProperty property) => _properties.Add(property.OptionIdentifier, property);

        public void RemoveFromInheritance(TOption optionCode) => _properties.Add(optionCode, null);
        public Boolean IsMarkedAsRemovedFromInheritance(TOption optionCode) => _properties.ContainsKey(optionCode) == true && _properties[optionCode] == null;
        public IEnumerable<TOption> GetMarkedFromInheritanceOptionCodes() => _properties.Where(x => x.Value == null).Select(x => x.Key).ToArray();

        internal void OverrideProperties(ScopeProperties<TScopeProperty, TOption, TValueType> source)
        {
            if (source == null)
            {
                return;
            }

            foreach (var item in source._properties)
            {
                if (item.Value == null)
                {
                    this._properties.Remove(item.Key);
                }
                else
                {
                    if (this._properties.ContainsKey(item.Key) == false)
                    {
                        this._properties.Add(item.Key, item.Value);
                    }
                    else
                    {
                        this._properties[item.Key] = item.Value;
                    }
                }
            }
        }

        #endregion

        #region equals

        public override bool Equals(object other) =>
          other is ScopeProperties<TScopeProperty, TOption, TValueType> properties
                ? this.Equals(properties)
                : base.Equals(other);


        public bool Equals(ScopeProperties<TScopeProperty, TOption, TValueType> other)
        {
            if (other._properties.Count != this._properties.Count)
            {
                return false;
            }

            foreach (var item in _properties)
            {
                if (other._properties.ContainsKey(item.Key) == false) { return false; }

                TScopeProperty selfValue = item.Value;
                TScopeProperty otherValue = other._properties[item.Key];

                if(ReferenceEquals(selfValue,otherValue) == true) { return true; }

                if (selfValue.Equals(otherValue) == false) { return false; }
            }

            return true;
        }

        public override int GetHashCode() => HashCode.Combine(_properties);

        #endregion
    }
}
