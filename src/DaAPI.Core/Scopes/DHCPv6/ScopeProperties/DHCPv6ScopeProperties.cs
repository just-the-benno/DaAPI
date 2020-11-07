using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DaAPI.Core.Scopes.DHCPv6.ScopeProperties
{
    public class DHCPv6ScopeProperties : ScopeProperties<DHCPv6ScopeProperty, UInt16, DHCPv6ScopePropertyType>
    {
        public DHCPv6ScopeProperties(IEnumerable<DHCPv6ScopeProperty> properties) : base(properties)
        { 
        }   

        public DHCPv6ScopeProperties() : base(Array.Empty<DHCPv6ScopeProperty>())
        {

        }

        public DHCPv6ScopeProperties(params DHCPv6ScopeProperty[] properties) : this(properties.ToList())
        {
        }

        public static DHCPv6ScopeProperties Empty => new DHCPv6ScopeProperties();
    }
}
