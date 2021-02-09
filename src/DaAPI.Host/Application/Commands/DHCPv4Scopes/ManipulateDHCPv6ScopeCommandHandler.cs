using DaAPI.Core.Common;
using DaAPI.Core.Scopes;
using DaAPI.Core.Scopes.DHCPv4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static DaAPI.Shared.Requests.DHCPv4ScopeRequests.V1;

namespace DaAPI.Host.Application.Commands.DHCPv4Scopes
{
    public abstract class ManipulateDHCPv4ScopeCommandHandler
    {
        protected static DHCPv4ScopeAddressProperties GetScopeAddressProperties(IScopeChangeCommand request) =>
           new DHCPv4ScopeAddressProperties
               (
                   IPv4Address.FromString(request.AddressProperties.Start),
                   IPv4Address.FromString(request.AddressProperties.End),
                   request.AddressProperties.ExcludedAddresses.Select(x => IPv4Address.FromString(x)),
                   leaseTime: request.AddressProperties.LeaseTime,
                   renewalTime: request.AddressProperties.RenewalTime,
                   preferredLifetime: request.AddressProperties.PreferredLifetime,
                   reuseAddressIfPossible: request.AddressProperties.ReuseAddressIfPossible,
                   addressAllocationStrategy: (Core.Scopes.ScopeAddressProperties<DHCPv4ScopeAddressProperties, IPv4Address>.AddressAllocationStrategies?)request.AddressProperties.AddressAllocationStrategy,
                   supportDirectUnicast: request.AddressProperties.SupportDirectUnicast,
                   acceptDecline: request.AddressProperties.AcceptDecline,
                   informsAreAllowd: request.AddressProperties.InformsAreAllowd,
                   maskLength: request.AddressProperties.MaskLength
               );

        protected static CreateScopeResolverInformation GetResolverInformation(IScopeChangeCommand request) =>
            new Core.Scopes.CreateScopeResolverInformation
            {
                PropertiesAndValues = request.Resolver.PropertiesAndValues,
                Typename = request.Resolver.Typename,
            };

        protected static DHCPv4ScopeProperties GetScopeProperties(IScopeChangeCommand request)
        {
            List<DHCPv4ScopeProperty> properties = new List<DHCPv4ScopeProperty>();
            List<Byte> optionsToRemove = new();

            foreach (var item in request.Properties ?? Array.Empty<DHCPv4ScopePropertyRequest>())
            {
                if (item.MarkAsRemovedInInheritance == true)
                {
                    optionsToRemove.Add(item.OptionCode);
                }
                else
                {
                    switch (item)
                    {
                        case DHCPv4AddressListScopePropertyRequest property:
                            properties.Add(new DHCPv4AddressListScopeProperty(item.OptionCode, property.Addresses.Select(x => IPv4Address.FromString(x)).ToList()));
                            break;
                        case DHCPv4AddressScopePropertyRequest property:
                            properties.Add(new DHCPv4AddressScopeProperty(item.OptionCode, IPv4Address.FromString(property.Address)));
                            break;
                        case DHCPv4BooleanScopePropertyRequest property:
                            properties.Add(new DHCPv4BooleanScopeProperty(item.OptionCode, property.Value));
                            break;
                        case DHCPv4NumericScopePropertyRequest property:
                            properties.Add(new DHCPv4NumericValueScopeProperty(item.OptionCode, property.Value, property.NumericType, item.Type));
                            break;
                        case DHCPv4TextScopePropertyRequest property:
                            properties.Add(new DHCPv4TextScopeProperty(item.OptionCode, property.Value));
                            break;
                        case DHCPv4TimeScopePropertyRequest property:
                            properties.Add(new DHCPv4TimeScopeProperty(item.OptionCode, false, property.Value));
                            break;
                        default:
                            break;
                    }
                }
            }

            var result = new DHCPv4ScopeProperties(properties);

            foreach (var item in optionsToRemove)
            {
                result.RemoveFromInheritance(item);
            }

            return result;
        }
    }
}
