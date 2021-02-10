using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DaAPI.App.Pages.DHCPv4Scopes
{
    public enum DHCPv4ScopePropertyExplanationValues
    {
        HasParent,
        Start,
        End,
        AddressRelated,
        ExcludedAddresses,
        PrefixDelegation,
        Prefix,
        PrefixLength,
        AssignedPrefixLength,
        RenewalTime,
        PreferredLifetime,
        Leastime,
        SupportDirectUnicast,
        AccpetDeclines,
        AccpetInforms,
        ReuseAddress,
        AddressAllocationStrategy,
        ScopeOptions,
        Resolver,
    }
}
