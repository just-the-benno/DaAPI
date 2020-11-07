using DaAPI.Core.Common;
using DaAPI.Core.Common.DHCPv6;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Scopes.DHCPv6
{
    public class DHCPv6PrefixDelegation : Value<DHCPv6PrefixDelegation>
    {
        #region Properties

        public UInt32 IdentityAssociation { get; private set; }
        public IPv6Address NetworkAddress { get; private set; }
        public IPv6SubnetMask Mask { get; private set; }
        public DateTime Started { get; private set; }

        #endregion

        #region Constructor and factories

        private DHCPv6PrefixDelegation()
        {

        }

        internal DHCPv6PrefixDelegation(IPv6Address address, Byte length, UInt32 identityAssociation, DateTime started)
        {
            NetworkAddress = address;
            Mask = new IPv6SubnetMask(new IPv6SubnetMaskIdentifier(length));
            IdentityAssociation = identityAssociation;
            Started = started;
        }

        public static DHCPv6PrefixDelegation None => new DHCPv6PrefixDelegation
        {
            NetworkAddress = IPv6Address.Empty,
            Mask = IPv6SubnetMask.Empty,
            IdentityAssociation = 0,
            Started = DateTime.MinValue,
        };


        public static DHCPv6PrefixDelegation FromValues(
            IPv6Address address, IPv6SubnetMask mask, UInt32 identityAssociation)
        {
            if (mask.IsIPv6AdressANetworkAddress(address) == false)
            {
                throw new ArgumentException(nameof(address));
            }

            return new DHCPv6PrefixDelegation
            {
                IdentityAssociation = identityAssociation,
                Mask = mask,
                NetworkAddress = address,
            };
        }

        internal bool AreValuesEqual(DHCPv6PrefixDelegation other) =>
            this.IdentityAssociation == other.IdentityAssociation &&
            this.Mask == other.Mask &&
            this.NetworkAddress == other.NetworkAddress;

        #endregion


    }
}
