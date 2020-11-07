using DaAPI.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DaAPI.Core.Scopes
{
    public abstract class ScopeAddressProperties<TAddressProperties, TAddress> : Value
        where TAddress : IPAddress<TAddress>
        where TAddressProperties : ScopeAddressProperties<TAddressProperties, TAddress>
    {
        public enum AddressAllocationStrategies
        {
            Random = 1,
            Next = 2,
        }

        #region Fields

        private HashSet<TAddress> _excludedAddresses = new HashSet<TAddress>();

        #endregion

        #region Properties

        public TAddress Start { get; private set; }
        public TAddress End { get; private set; }

        public Boolean? ReuseAddressIfPossible { get; private set; }
        public AddressAllocationStrategies? AddressAllocationStrategy { get; private set; }

        public Boolean? SupportDirectUnicast { get; private set; }
        public Boolean? AcceptDecline { get; private set; }
        public Boolean? InformsAreAllowd { get; private set; }

        public IEnumerable<TAddress> ExcludedAddresses
        {
            get { return _excludedAddresses.ToList(); }
            set
            {
                _excludedAddresses = new HashSet<TAddress>(value);
            }
        }

        #endregion

        #region Constructor

        private ScopeAddressProperties()
        {
            _excludedAddresses = new HashSet<TAddress>();
        }

        protected ScopeAddressProperties(TAddress start, TAddress end, Boolean _) : this()
        {
            Start = start;
            End = end;
        }

        protected ScopeAddressProperties(TAddress start, TAddress end) : this(start, end, Array.Empty<TAddress>())
        {

        }


        protected ScopeAddressProperties(TAddress start, TAddress end, IEnumerable<TAddress> excluded) :
            this(start, end, excluded, null, null, null, null, null)
        {


        }

        protected ScopeAddressProperties(
            TAddress start,
            TAddress end,
            IEnumerable<TAddress> excluded,
            Boolean? reuseAddressIfPossible = null,
            AddressAllocationStrategies? addressAllocationStrategy = null,
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

            HashSet<TAddress> excludedAddresses = new HashSet<TAddress>(excluded);

            foreach (var item in excluded)
            {
                if (item.IsBetween(start, end) == false)
                {
                    throw new ArgumentException($"excluded address {item} not in range of {start} and {end}");
                }

                excludedAddresses.Add(item);
            }

            if (AreAllAdrressesExcluded(start, end, excludedAddresses) == true)
            {
                throw new ArgumentException("all possible addresses are excluded");
            }


            Start = start;
            End = end;
            _excludedAddresses = excludedAddresses;
            ReuseAddressIfPossible = reuseAddressIfPossible;
            AddressAllocationStrategy = addressAllocationStrategy;
            SupportDirectUnicast = supportDirectUnicast;
            AcceptDecline = acceptDecline;
            InformsAreAllowd = informsAreAllowd;
        }


        #endregion

        #region Methods

        protected abstract Boolean AreAllAdrressesExcluded(TAddress start, TAddress end, HashSet<TAddress> excludedElements);


        internal virtual void OverrideProperties(TAddressProperties range)
        {
            if (range == null) { return; }

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
                this._excludedAddresses = new HashSet<TAddress>(
                    this._excludedAddresses.Union(range._excludedAddresses).Where(x => x.IsBetween(this.Start, this.End) == true));
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

        public abstract Boolean IsAddressInRange(TAddress address);

        public abstract Boolean IsAddressRangeBetween(TAddressProperties child);

        protected abstract TAddress GetNextRandomAddress(HashSet<TAddress> used);

        protected abstract TAddress GetNextAddress(IEnumerable<TAddress> used);

        protected abstract TAddress GetEmptyAddress();

        public TAddress GetValidAddresses(IEnumerable<TAddress> usedAddresses, params TAddress[] addtionnalExcludedAddresses)
        {
            HashSet<TAddress> exclucedAddresses = new HashSet<TAddress>(usedAddresses.Union(
                addtionnalExcludedAddresses.Where(x => x != GetEmptyAddress())));

            return AddressAllocationStrategy.Value switch
            {
                AddressAllocationStrategies.Random => GetNextRandomAddress(exclucedAddresses),
                AddressAllocationStrategies.Next => GetNextAddress(exclucedAddresses),
                _ => throw new NotImplementedException(),
            };
        }

        public virtual Boolean ValueAreValidForRoot() =>
                ReuseAddressIfPossible.HasValue &&
                AddressAllocationStrategy.HasValue &&
                SupportDirectUnicast.HasValue &&
                AcceptDecline.HasValue &&
                InformsAreAllowd.HasValue;

        public abstract bool IsValid();



        #endregion
    }
}
