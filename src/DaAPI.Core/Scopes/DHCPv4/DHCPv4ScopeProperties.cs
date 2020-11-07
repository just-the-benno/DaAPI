using DaAPI.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DaAPI.Core.Scopes.DHCPv4
{
    public class DHCPv4ScopeProperties : Value, IEquatable<DHCPv4ScopeProperties>
    {
        #region Fields

        private Dictionary<Byte, DHCPv4ScopeProperty> _properties = new Dictionary<byte, DHCPv4ScopeProperty>();

        #endregion

        #region Properties

        public IEnumerable<DHCPv4ScopeProperty> Properties
        {
            get
            {
                return _properties.Values.ToList();
            }
             set
            {
                _properties = value.ToDictionary(x => x.OptionIdentifier, x => x);
            }
        }

        #endregion

        #region Constructor

        private DHCPv4ScopeProperties()
        {
            _properties = new Dictionary<byte, DHCPv4ScopeProperty>();
        }

        public DHCPv4ScopeProperties(IEnumerable<DHCPv4ScopeProperty> properties) : this()
        {
            foreach (var item in properties)
            {
                Add(item);
            }
        }

        public DHCPv4ScopeProperties(params DHCPv4ScopeProperty[] properties) : this(properties.ToList())
        {
        }

        public static DHCPv4ScopeProperties Empty => new DHCPv4ScopeProperties();

        #endregion

        #region Methods

        private void Add(DHCPv4ScopeProperty property)
        {
            _properties.Add((Byte)property.OptionIdentifier, property);
        }

        internal void OverrideProperties(DHCPv4ScopeProperties source)
        {
            if (source == null)
            {
                return;
            }

            foreach (var item in source._properties)
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

        #endregion

        #region equals

        public override bool Equals(object other) =>
             other is DHCPv4ScopeProperties properties ? this.Equals(properties) : base.Equals(other);

        public bool Equals(DHCPv4ScopeProperties other)
        {
            if(other._properties.Count != this._properties.Count)
            {
                return false;
            }

            foreach (var item in _properties)
            {
                if(other._properties.ContainsKey(item.Key) == false) { return false; }

                DHCPv4ScopeProperty selfValue = item.Value;
                DHCPv4ScopeProperty otherValue = other._properties[item.Key];

                if(selfValue.Equals(otherValue) == false) { return false; }
            }

            return true;
        }

        public override int GetHashCode() => HashCode.Combine(_properties);

        #endregion


    }
}
