using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Scopes.DHCPv4
{
    public interface IDHCPv4ScopeResolverContainingOtherResolvers : IDHCPv4ScopeResolver
    {
        Boolean AddResolver(IDHCPv4ScopeResolver resolver);
        IEnumerable<DHCPv4CreateScopeResolverInformation> ExtractResolverCreateModels(DHCPv4CreateScopeResolverInformation item);
    }
}
