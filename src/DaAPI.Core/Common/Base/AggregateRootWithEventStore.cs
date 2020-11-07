using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DaAPI.Core.Common
{
    public abstract class AggregateRootWithEvents : AggregateRoot
    {
        #region Fields

        private readonly List<DomainEvent> _changes = new List<DomainEvent>();
        public Int32 Version { get; private set; } = -1;

        #endregion

        #region Constructor

        protected AggregateRootWithEvents(Guid id) : base(id)
        {

        }

        #endregion

        #region Methods

        protected override void Apply(DomainEvent domainEvent)
        {
            Apply(domainEvent, true);
        }

        protected void Apply(DomainEvent domainEvent, Boolean incrementVersion)
        {
            //EnsureValidState();
            _changes.Add(domainEvent);

            base.Apply(domainEvent);

            if (incrementVersion == true)
            {
                Version++;
            }
        }

        public IEnumerable<DomainEvent> GetChanges() => _changes.ToList().AsEnumerable();

        public void Load(IEnumerable<DomainEvent> history)
        {
            foreach (var domainEvent in history)
            {
                When(domainEvent);
                Version++;
            }
        }

        public void ClearChanges() => _changes.Clear();

        #endregion
    }
}
