using DaAPI.Core.Common;
using DaAPI.Core.Common.DHCPv6;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace DaAPI.Core.Scopes.DHCPv6
{
    public class DHCPv6PrefixDelgationInfo : Value<DHCPv6PrefixDelgationInfo>
    {
        public IPv6Address Prefix { get; private set; }
        public IPv6SubnetMaskIdentifier PrefixLength { get; private set; }
        public IPv6SubnetMaskIdentifier AssignedPrefixLength { get; private set; }

        internal DHCPv6PrefixDelgationInfo(
            IPv6Address prefix, Byte prefixLength, Byte assignedPrefixLength)
        {
            Prefix = prefix;
            PrefixLength = new IPv6SubnetMaskIdentifier(prefixLength);
            AssignedPrefixLength = new IPv6SubnetMaskIdentifier(assignedPrefixLength);
        }

        public static DHCPv6PrefixDelgationInfo FromValues(
            IPv6Address prefix, IPv6SubnetMaskIdentifier prefixLength, IPv6SubnetMaskIdentifier assignedPrefixLength)
        {
            if (prefixLength.Value > assignedPrefixLength.Value)
            {
                throw new ArgumentException();
            }

            IPv6SubnetMask mask = new IPv6SubnetMask(prefixLength);
            if(mask.IsIPv6AdressANetworkAddress(prefix) == false)
            {
                throw new ArgumentException();
            }

            return new DHCPv6PrefixDelgationInfo(prefix, prefixLength, assignedPrefixLength);
        }

        internal DHCPv6PrefixDelgationInfo Copy() => new DHCPv6PrefixDelgationInfo(this.Prefix, this.PrefixLength, this.AssignedPrefixLength);
    }
}
