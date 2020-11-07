using DaAPI.Core.Scopes;
using DaAPI.Core.Scopes.DHCPv4;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Services
{
    public interface IDHCPv4ScopeResolverManager
    {
        Boolean ValidateDHCPv4Resolver(DHCPv4CreateScopeResolverInformation resolverCreateModel);
        IDHCPv4ScopeResolver InitializeResolver(DHCPv4CreateScopeResolverInformation resolverCreateModel);

        IEnumerable<ScopeResolverDescription> GetRegisterResolverDescription();
        void AddOrUpdateDHCPv4ScopeResolver(String name, Func<IDHCPv4ScopeResolver> activator);
    }
}
