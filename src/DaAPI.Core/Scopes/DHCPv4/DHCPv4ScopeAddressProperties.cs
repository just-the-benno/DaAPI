using DaAPI.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DaAPI.Core.Scopes.DHCPv4
{
    public class DHCPv4ScopeAddressProperties : ScopeAddressProperties<DHCPv4ScopeAddressProperties, IPv4Address>
    {
        #region const

        private const int _maxTriesForRandom = 10000;

        #endregion

        #region Properties

        public TimeSpan? LeaseTime { get; private set; }
        public TimeSpan? RenewalTime { get; private set; }
        public TimeSpan? PreferredLifetime { get; private set; }

        public IPv4SubnetMask Mask { get; private set; }

        #endregion

        #region Constructor

        private DHCPv4ScopeAddressProperties() : base(IPv4Address.Empty, IPv4Address.Empty, false)
        {
        }

        public DHCPv4ScopeAddressProperties(IPv4Address start, IPv4Address end) : base(start, end)
        {
        }

        public DHCPv4ScopeAddressProperties(IPv4Address start, IPv4Address end, IEnumerable<IPv4Address> excluded) :
            base(start, end, excluded)
        {
        }

        public DHCPv4ScopeAddressProperties(
           IPv4Address start,
           IPv4Address end,
           IEnumerable<IPv4Address> excluded,
           TimeSpan? renewalTime = null,
           TimeSpan? preferredLifetime = null,
           TimeSpan? leaseTime = null,
           Byte? maskLength = null,
           Boolean? reuseAddressIfPossible = null,
           AddressAllocationStrategies? addressAllocationStrategy = null,
           Boolean? supportDirectUnicast = null,
           Boolean? acceptDecline = null,
           Boolean? informsAreAllowd = null
           ) : base(start, end, excluded, reuseAddressIfPossible, addressAllocationStrategy, supportDirectUnicast, acceptDecline, informsAreAllowd)
        {

            Mask = (maskLength == null || maskLength == 0) ? null : new IPv4SubnetMask(new IPv4SubnetMaskIdentifier(maskLength.Value));

            RenewalTime = renewalTime;
            PreferredLifetime = preferredLifetime;
            LeaseTime = leaseTime;
        }

        public static DHCPv4ScopeAddressProperties Empty => new DHCPv4ScopeAddressProperties();

        #endregion

        #region Methods

        protected override bool AreAllAdrressesExcluded(IPv4Address start, IPv4Address end, HashSet<IPv4Address> excludedElements)
                    => (end - start) + 1 == excludedElements.Count;

        internal override void OverrideProperties(DHCPv4ScopeAddressProperties range)
        {
            base.OverrideProperties(range);

            if (range.LeaseTime.HasValue == true)
            {
                this.LeaseTime = range.LeaseTime;
            }

            if (range.PreferredLifetime.HasValue == true)
            {
                this.PreferredLifetime = range.PreferredLifetime.Value;
            }

            if (range.RenewalTime.HasValue == true)
            {
                this.RenewalTime = range.RenewalTime.Value;
            }

            if (range.Mask != null)
            {
                this.Mask = new IPv4SubnetMask(new IPv4SubnetMaskIdentifier(range.Mask.GetSlashNotation()));
            }
        }

        protected override IPv4Address GetNextRandomAddress(HashSet<IPv4Address> used) =>
            GetNextRandomAddressInternal(used, (input) => IPv4Address.FromByteArray(input), () => IPv4Address.Empty);

        #endregion


        #region queries

        public override bool IsAddressInRange(IPv4Address address) => Start <= address && address <= End;

        public override bool IsValid() => true;

        protected override IPv4Address GetEmptyAddress() => IPv4Address.Empty;

        protected override IPv4Address GetNextAddress(IEnumerable<IPv4Address> used)
        {
            IList<IPv4Address> sorted = used.Union(ExcludedAddresses).OrderBy(x => x).ToList();
            if (sorted.Count == 0)
            {
                return IPv4Address.FromAddress(Start);
            }

            Double maxDelta = End - Start + 1;
            if (sorted.Count == maxDelta)
            {
                return IPv4Address.Empty;
            }

            IPv4Address current = Start - 1;
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

        public override Boolean IsAddressRangeBetween(DHCPv4ScopeAddressProperties child) =>
            Start <= child.Start && End >= child.Start && End >= child.End;

        public override bool ValueAreValidForRoot()
        {
            Boolean preResult = base.ValueAreValidForRoot();
            if (preResult == false)
            {
                return false;
            }

            return Mask != null &&
              RenewalTime.HasValue &&
              PreferredLifetime.HasValue &&
              LeaseTime.HasValue;
        }

        public Boolean AreTimeValueValid()
        {
            if ((RenewalTime.HasValue && PreferredLifetime.HasValue && LeaseTime.HasValue) == false)
            {
                return false;
            }


            if (PreferredLifetime.Value < RenewalTime.Value)
            {
                return false;
            }

            if (LeaseTime.Value < PreferredLifetime.Value)
            {
                return false;
            }

            return true;
        }

        #endregion
    }
}
