using DaAPI.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DaAPI.Core.Scopes.DHCPv4
{
    public class DHCPv4ScopeAddressProperties : Value
    {
        public enum DHCPv4AddressAllocationStrategies
        {
            Random = 1,
            Next = 2,
        }

        private const int _maxTriesForRandom = 10000;

        #region Fields

        private HashSet<IPv4Address> _excludedAddresses = new HashSet<IPv4Address>();

        #endregion

        #region Properties

        public IPv4Address Start { get; private set; }
        public IPv4Address End { get; private set; }

        public TimeSpan? RenewalTime { get; private set; }
        public TimeSpan? PreferredLifetime { get; private set; }
        public TimeSpan? ValidLifetime { get; private set; }

        public Boolean? ReuseAddressIfPossible { get; private set; }
        public DHCPv4AddressAllocationStrategies? AddressAllocationStrategy { get; private set; }

        public Boolean? SupportDirectUnicast { get; private set; }
        public Boolean? AcceptDecline { get; private set; }
        public Boolean? InformsAreAllowd { get; private set; }

        public IEnumerable<IPv4Address> ExcludedAddresses
        {
            get { return _excludedAddresses.ToList(); }
            set
            {
                _excludedAddresses = new HashSet<IPv4Address>(value);
            }
        }

        #endregion

        #region Constructor

        private DHCPv4ScopeAddressProperties()
        {
            _excludedAddresses = new HashSet<IPv4Address>();
            Start = IPv4Address.Empty;
            End = IPv4Address.Empty;
        }

        public DHCPv4ScopeAddressProperties(IPv4Address start, IPv4Address end) : this(start, end, new IPv4Address[0])
        {

        }


        public DHCPv4ScopeAddressProperties(IPv4Address start, IPv4Address end, IEnumerable<IPv4Address> excluded) :
            this(start, end, excluded, null, null, null, null, null, null, null, null)
        {


        }

        public DHCPv4ScopeAddressProperties(
            IPv4Address start,
            IPv4Address end,
            IEnumerable<IPv4Address> excluded,
            TimeSpan? renewalTime = null,
            TimeSpan? preferredLifetime = null,
            TimeSpan? validLifetime = null,
            Boolean? reuseAddressIfPossible = null,
            DHCPv4AddressAllocationStrategies? addressAllocationStrategy = null,
            Boolean? supportDirectUnicast = null,
            Boolean? acceptDecline = null,
            Boolean? informsAreAllowd = null
            ) : this()
        {
            if (start is null)
            {
                throw new ArgumentNullException(nameof(excluded));
            }

            if (end is null)
            {
                throw new ArgumentNullException(nameof(excluded));
            }

            if (excluded is null)
            {
                throw new ArgumentNullException(nameof(excluded));
            }

            if (start > end)
            {
                throw new ArgumentException("the start address have to be lower than the end address");
            }

            HashSet<IPv4Address> excludedAddresses = new HashSet<IPv4Address>();

            foreach (var item in excluded)
            {
                if (item.IsInBetween(start, end) == false)
                {
                    throw new ArgumentException($"excluded address {item} not in range of {start} and {end}");
                }

                excludedAddresses.Add(item);
            }

            Int64 between = (end - start) + 1;
            if (between == excludedAddresses.Count)
            {
                throw new ArgumentException("all possible addresses are excluded");
            }

            Start = start;
            End = end;
            _excludedAddresses = excludedAddresses;
            RenewalTime = renewalTime;
            PreferredLifetime = preferredLifetime;
            ValidLifetime = validLifetime;
            ReuseAddressIfPossible = reuseAddressIfPossible;
            AddressAllocationStrategy = addressAllocationStrategy;
            SupportDirectUnicast = supportDirectUnicast;
            AcceptDecline = acceptDecline;
            InformsAreAllowd = informsAreAllowd;
        }

        public static DHCPv4ScopeAddressProperties Empty => new DHCPv4ScopeAddressProperties();

        #endregion

        #region Methods

        internal void OverrideProperties(DHCPv4ScopeAddressProperties range)
        {
            if (range == null) { return; }

            if (range.PreferredLifetime.HasValue == true)
            {
                this.PreferredLifetime = range.PreferredLifetime.Value;
            }

            if (range.ValidLifetime.HasValue == true)
            {
                this.ValidLifetime = range.ValidLifetime.Value;
            }

            if (range.RenewalTime.HasValue == true)
            {
                this.RenewalTime = range.RenewalTime.Value;
            }

            if (range.Start != null)
            {
                this.Start = range.Start;
            }

            if (range.End != null)
            {
                this.End = range.End;
            }

            if (range._excludedAddresses.Count > 0)
            {
                this._excludedAddresses = new HashSet<IPv4Address>(
                    this._excludedAddresses.Union(range._excludedAddresses).Where(X => X.IsInBetween(this.Start,this.End)));
            }

            if (range.ReuseAddressIfPossible.HasValue == true)
            {
                this.ReuseAddressIfPossible = range.ReuseAddressIfPossible.Value;
            }

            if (range.AddressAllocationStrategy.HasValue == true)
            {
                this.AddressAllocationStrategy = range.AddressAllocationStrategy.Value;
            }

            if (range.SupportDirectUnicast.HasValue == true)
            {
                this.SupportDirectUnicast = range.SupportDirectUnicast.Value;
            }

            if (range.AcceptDecline.HasValue == true)
            {
                this.AcceptDecline = range.AcceptDecline.Value;
            }

            if (range.InformsAreAllowd.HasValue == true)
            {
                this.InformsAreAllowd = range.InformsAreAllowd.Value;
            }
        }

        #endregion

        #region queries

        public Boolean IsAddressInRange(IPv4Address address)
        {
            return Start <= address && address <= End;
        }

        private IPv4Address GetNextRandomAddress(HashSet<IPv4Address> used)
        {
            Random random = new Random();

            Int64 maxDelta = End - Start + 1;
            Int32 delta = Int32.MaxValue;

            if (maxDelta < Int32.MaxValue)
            {
                delta = (Int32)maxDelta;
            }

            if(delta == used.Count + _excludedAddresses.Count)
            {
                return IPv4Address.Empty;
            }

            IPv4Address nextAddress;
            Int32 trysLeft = _maxTriesForRandom;
            do
            {
                Int32 nextDelta = random.Next(0, delta);
                nextAddress = Start + nextDelta;
                trysLeft--;
            } while (
             (used.Contains(nextAddress) == true || _excludedAddresses.Contains(nextAddress) == true) && trysLeft >= 0);

            if (trysLeft < 0)
            {
                return IPv4Address.Empty;
            }

            return nextAddress;
        }

        private IPv4Address GetNextAddress(IEnumerable<IPv4Address> used)
        {
            IList<IPv4Address> sorted = used.Union(_excludedAddresses).OrderBy(x => x).ToList();
            if (sorted.Count == 0)
            {
                return IPv4Address.FromAddress(Start);
            }

            Int64 maxDelta = End - Start + 1;
            if (sorted.Count == maxDelta)
            {
                return IPv4Address.Empty;
            }

            IPv4Address current = Start - 1;
            foreach (var item in sorted)
            {
                Int64 delta = (item - current);
                if(delta > 1)
                {
                   break;
                }

                current = item;
            }

            return current + 1;
        }

        public IPv4Address GetValidAddresses(IEnumerable<IPv4Address> usedAddresses, params IPv4Address[] addtionnalExcludedAddresses)
        {
            HashSet<IPv4Address> exclucedAddresses = new HashSet<IPv4Address>(usedAddresses.Union(
                addtionnalExcludedAddresses.Where(x => x != IPv4Address.Empty)));

            return AddressAllocationStrategy.Value switch
            {
                DHCPv4AddressAllocationStrategies.Random => GetNextRandomAddress(exclucedAddresses),
                DHCPv4AddressAllocationStrategies.Next => GetNextAddress(exclucedAddresses),
                _ => throw new NotImplementedException(),
            };
        }

        public Boolean ValueAreValidForRoot()
        {
            Boolean result =
                RenewalTime.HasValue &&
                PreferredLifetime.HasValue &&
                ValidLifetime.HasValue &&
                ReuseAddressIfPossible.HasValue &&
                AddressAllocationStrategy.HasValue &&
                SupportDirectUnicast.HasValue &&
                AcceptDecline.HasValue &&
                InformsAreAllowd.HasValue;

            result &= AreTimeValueValid();

            return result;
        }

        public Boolean AreTimeValueValid()
        {
            if ((RenewalTime.HasValue && PreferredLifetime.HasValue && ValidLifetime.HasValue) == false)
            {
                return false;
            }


            if (PreferredLifetime.Value < RenewalTime.Value)
            {
                return false;
            }

            if (ValidLifetime.Value < PreferredLifetime.Value)
            {
                return false;
            }

            return true;
        }

        public Boolean IsAddressRangeBetween(DHCPv4ScopeAddressProperties child)
        {
            Boolean result =
                Start <= child.Start && End >= child.Start && End >= child.End;

            return result;
        }

        #endregion
    }
}
