using DaAPI.Core.Common;
using DaAPI.Core.Common.DHCPv6;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DaAPI.Core.Scopes.DHCPv6
{
    public class DHCPv6ScopeAddressProperties : ScopeAddressProperties<DHCPv6ScopeAddressProperties, IPv6Address>
    {
        #region const

        private const int _maxTriesForRandom = 10000;

        #endregion

        #region Properties

        public DHCPv6TimeScale T1 { get; private set; }
        public DHCPv6TimeScale T2 { get; private set; }
        public TimeSpan? PreferredLeaseTime { get; private set; }
        public TimeSpan? ValidLeaseTime { get; private set; }
        public Boolean? RapitCommitEnabled { get; private set; }
        public DHCPv6PrefixDelgationInfo PrefixDelgationInfo { get; private set; }

        #endregion

        #region Constructor

        private DHCPv6ScopeAddressProperties() : base(IPv6Address.Empty, IPv6Address.Empty, false)
        {
        }

        public DHCPv6ScopeAddressProperties(IPv6Address start, IPv6Address end) : base(start, end)
        {
        }

        public DHCPv6ScopeAddressProperties(IPv6Address start, IPv6Address end, IEnumerable<IPv6Address> excluded) :
            base(start, end, excluded)
        {
        }

        public DHCPv6ScopeAddressProperties(
            IPv6Address start,
            IPv6Address end,
            IEnumerable<IPv6Address> excluded,
            DHCPv6TimeScale t1 = null,
            DHCPv6TimeScale t2 = null,
            TimeSpan? preferredLifeTime = null,
            TimeSpan? validLifeTime = null,
            Boolean? reuseAddressIfPossible = null,
            AddressAllocationStrategies? addressAllocationStrategy = null,
            Boolean? supportDirectUnicast = null,
            Boolean? acceptDecline = null,
            Boolean? informsAreAllowd = null,
            Boolean? rapitCommitEnabled = null,
            DHCPv6PrefixDelgationInfo prefixDelgationInfo = null
            ) : base(start, end, excluded, reuseAddressIfPossible, addressAllocationStrategy, supportDirectUnicast, acceptDecline, informsAreAllowd)
        {

            this.PreferredLeaseTime = preferredLifeTime;
            this.ValidLeaseTime = validLifeTime;
            RapitCommitEnabled = rapitCommitEnabled;
            PrefixDelgationInfo = prefixDelgationInfo;

            T1 = t1;
            T2 = t2;
        }

        public static DHCPv6ScopeAddressProperties Empty => new DHCPv6ScopeAddressProperties();

        #endregion

        public bool IsRapitCommitEnabled() => RapitCommitEnabled.HasValue && RapitCommitEnabled.Value;

        protected override bool AreAllAdrressesExcluded(IPv6Address start, IPv6Address end, HashSet<IPv6Address> excludedElements)
            => (end - start) + 1 == excludedElements.Count;

        internal override void OverrideProperties(DHCPv6ScopeAddressProperties range)
        {
            base.OverrideProperties(range);

            if (range.RapitCommitEnabled.HasValue == true)
            {
                this.RapitCommitEnabled = range.RapitCommitEnabled.Value;
            }

            if (range.T1 != null)
            {
                this.T1 = new DHCPv6TimeScale(range.T1);
            }

            if (range.T2 != null)
            {
                this.T2 = new DHCPv6TimeScale(range.T2);
            }

            if (range.PreferredLeaseTime.HasValue == true)
            {
                this.PreferredLeaseTime = range.PreferredLeaseTime.Value;
            }

            if (range.ValidLeaseTime.HasValue == true)
            {
                this.ValidLeaseTime = range.ValidLeaseTime.Value;
            }

            if (range.PrefixDelgationInfo != null)
            {
                this.PrefixDelgationInfo = range.PrefixDelgationInfo.Copy();
            }
        }

        protected override IPv6Address GetNextRandomAddress(HashSet<IPv6Address> used) =>
             GetNextRandomAddressInternal(used, (input) => IPv6Address.FromByteArray(input), () => IPv6Address.Empty);

        internal IPv6Address GetRandomPrefix(HashSet<IPv6Address> hashedUsedPrefixes)
        {
            Random random = new Random();

            Byte[] addressBytes = new byte[16];

            IPv6SubnetMask mask = new IPv6SubnetMask(new IPv6SubnetMaskIdentifier(PrefixDelgationInfo.PrefixLength));
            Byte[] addressMaskBytes = mask.GetMaskBytes();
            Byte[] ipAddressBytes = PrefixDelgationInfo.Prefix.GetBytes();

            IPv6SubnetMask delegatedMask = new IPv6SubnetMask(new IPv6SubnetMaskIdentifier(PrefixDelgationInfo.AssignedPrefixLength));
            Byte[] delegatedMaskBytes = delegatedMask.GetMaskBytes();

            Int32 randomizationIndex = -1;
            for (int i = 0; i < 16; i++)
            {
                if (addressMaskBytes[i] != 255)
                {
                    randomizationIndex = i;
                    break;
                }
                else
                {
                    addressBytes[i] = ipAddressBytes[i];
                }
            }

            IPv6Address nextNetworkAddress;
            Int32 trysLeft = _maxTriesForRandom;
            do
            {
                if (randomizationIndex >= 0)
                {
                    Boolean lengthAndAssigendLengthInSameByte = PrefixDelgationInfo.PrefixLength / 8 == (PrefixDelgationInfo.AssignedPrefixLength - 1) / 8;
                    if (lengthAndAssigendLengthInSameByte == true)
                    {
                        Byte diff = (Byte)(delegatedMaskBytes[randomizationIndex] - addressMaskBytes[randomizationIndex]);
                        ByteHelper.AddRandomBits(random, addressBytes, randomizationIndex, diff, ipAddressBytes[randomizationIndex]);
                    }
                    else
                    {
                        addressBytes[randomizationIndex] = (Byte)(addressMaskBytes[randomizationIndex] + random.Next(0, 255 - addressMaskBytes[randomizationIndex] + 1));
                    }

                    for (int i = randomizationIndex + 1; i < 16; i++)
                    {
                        if (delegatedMaskBytes[i] == 255)
                        {
                            addressBytes[i] = (Byte)random.Next(0, 256);
                        }
                        else
                        {
                            addressBytes[i] = (Byte)(random.Next(0, delegatedMaskBytes[i]));
                            ByteHelper.AddRandomBits(random, addressBytes, i, delegatedMaskBytes[i], 0);

                            break;
                        }
                    }
                }

                nextNetworkAddress = IPv6Address.FromByteArray(addressBytes);
            } while (trysLeft-- > 0 && hashedUsedPrefixes.Contains(nextNetworkAddress) == true);

            return nextNetworkAddress;
        }

        public override bool IsAddressInRange(IPv6Address address) => Start <= address && address <= End;

        public override bool IsValid() => (PreferredLeaseTime <= ValidLeaseTime) && (T1 < T2);

        protected override IPv6Address GetEmptyAddress() => IPv6Address.Empty;

        protected override IPv6Address GetNextAddress(IEnumerable<IPv6Address> used)
        {
            IList<IPv6Address> sorted = used.Union(ExcludedAddresses).OrderBy(x => x).ToList();
            if (sorted.Count == 0)
            {
                return IPv6Address.FromAddress(Start);
            }

            Double maxDelta = End - Start + 1;
            if (sorted.Count == maxDelta)
            {
                return IPv6Address.Empty;
            }

            IPv6Address current = Start - 1;
            foreach (var item in sorted)
            {
                Double delta = (item - current);
                if (delta > 1)
                {
                    break;
                }

                current = item;
            }

            return current + 1;
        }

        internal IPv6Address GetNextPrefix(IEnumerable<IPv6Address> used)
        {
            IList<IPv6Address> sorted = used.OrderBy(x => x).ToList();
            if (sorted.Count == 0)
            {
                return IPv6Address.FromAddress(PrefixDelgationInfo.Prefix);
            }

            Double max = Math.Pow(2, PrefixDelgationInfo.AssignedPrefixLength - PrefixDelgationInfo.PrefixLength);
            if (sorted.Count == max)
            {
                return IPv6Address.Empty;
            }

            Byte[] maskBytes = new IPv6SubnetMask(PrefixDelgationInfo.AssignedPrefixLength).GetMaskBytes();
            Byte[] deltaBytes = new Byte[16];
            for (int i = 0; i < 16; i++)
            {
                deltaBytes[i] = (Byte)(255 - maskBytes[i]);
            }

            IPv6Address delta = IPv6Address.FromByteArray(deltaBytes) + 1;

            IPv6Address current = PrefixDelgationInfo.Prefix - delta.GetBytes();
            foreach (var item in sorted)
            {
                IPv6Address expectedAddresses = current + delta;
                if (item != expectedAddresses)
                {
                    break;
                }

                current = expectedAddresses;
            }

            return current + delta;
        }

        public DHCPv6PrefixDelegation GetValidPrefix(IEnumerable<IPv6Address> usedAddresses, UInt32 prefixIdentityAsscocationId, params IPv6Address[] addtionnalExcludedAddresses)
        {
            if (PrefixDelgationInfo == null) { return DHCPv6PrefixDelegation.None; }

            HashSet<IPv6Address> exclucedAddresses = new HashSet<IPv6Address>(usedAddresses.Union(
                addtionnalExcludedAddresses.Where(x => x != GetEmptyAddress())));

            IPv6Address address = AddressAllocationStrategy.Value switch
            {
                AddressAllocationStrategies.Random => GetRandomPrefix(exclucedAddresses),
                AddressAllocationStrategies.Next => GetNextPrefix(exclucedAddresses),
                _ => throw new NotImplementedException(),
            };
            return new DHCPv6PrefixDelegation(
               address, PrefixDelgationInfo.AssignedPrefixLength, prefixIdentityAsscocationId, DateTime.UtcNow);
        }


        public override Boolean IsAddressRangeBetween(DHCPv6ScopeAddressProperties child) =>
            Start <= child.Start && End >= child.Start && End >= child.End;

        public override bool ValueAreValidForRoot()
        {
            Boolean preResult = base.ValueAreValidForRoot();
            if (preResult == false)
            {
                return false;
            }

            return
                RapitCommitEnabled.HasValue &&
                PreferredLeaseTime.HasValue &&
                ValidLeaseTime.HasValue &&
                PreferredLeaseTime.Value < ValidLeaseTime.Value &&
                T1 != null && T2 != null &&
                T1 < T2;
        }
    }
}
