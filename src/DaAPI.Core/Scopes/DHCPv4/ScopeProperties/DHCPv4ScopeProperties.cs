using DaAPI.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DaAPI.Core.Scopes.DHCPv4
{
    public class DHCPv4ScopeProperties : ScopeProperties<DHCPv4ScopeProperty, Byte, DHCPv4ScopePropertyType>
    {
        public DHCPv4ScopeProperties(IEnumerable<DHCPv4ScopeProperty> properties) : base(properties)
        {
        }

        public DHCPv4ScopeProperties() : base(Array.Empty<DHCPv4ScopeProperty>())
        {

        }

        public DHCPv4ScopeProperties(params DHCPv4ScopeProperty[] properties) : this(properties.ToList())
        {
        }

        public static DHCPv4ScopeProperties Empty => new DHCPv4ScopeProperties();
    }
}
