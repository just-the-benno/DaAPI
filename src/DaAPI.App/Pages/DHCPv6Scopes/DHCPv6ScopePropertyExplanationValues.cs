using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DaAPI.App.Pages.DHCPv6Scopes
{
    public enum DHCPv6ScopePropertyExplanationValues
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
        PreferredLifetime,
        ValidLifetime,
        T1,
        T2,
        SupportDirectUnicast,
        AccpetDeclines,
        AccpetInforms,
        RapidCommit,
        ReuseAddress,
        AddressAllocationStrategy,
        ScopeOptions,
        Resolver,
        RenewalTime
    }
}
