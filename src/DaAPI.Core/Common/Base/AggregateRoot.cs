using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DaAPI.Core.Common
{
    public abstract class AggregateRoot : IInternalEventHandler
    {
        protected enum EventHandlingStratgies
        {
            OnlyOnce,
            Multiple,
        }

        private readonly Action<DomainEvent> _additionalApplier;

        #region Properties

        protected EventHandlingStratgies EventHandlingStrategy { get; set; } = EventHandlingStratgies.OnlyOnce;
        public Guid Id { get; protected set; }

        #endregion

        #region Constructor

        public AggregateRoot(Guid id)
        {
            Id = id;
        }

        public AggregateRoot(Guid id, Action<DomainEvent> additionalApplier) : this(id)
        {
            _additionalApplier = additionalApplier ?? throw new ArgumentNullException(nameof(additionalApplier));
        }
        #endregion

        #region Methods

        protected abstract void When(DomainEvent domainEvent);
        void IInternalEventHandler.Handle(DomainEvent domainEvent) => When(domainEvent);

        protected virtual void Apply(DomainEvent domainEvent)
        {
            if (domainEvent.IsHandled() == false ||
                (EventHandlingStrategy == EventHandlingStratgies.Multiple))
            {

                When(domainEvent);
                domainEvent.SetHandled();
            }
            //EnsureValidState();

            _additionalApplier?.Invoke(domainEvent);
        }

        protected void ApplyToEnity(IInternalEventHandler entity, DomainEvent domainEvent, Boolean onlyIfNotAlreadyHandeld = true)
        {
            if (entity == null || domainEvent == null) { return; }

            if (domainEvent.IsHandled() == false || onlyIfNotAlreadyHandeld == false)
            {
                entity.Handle(domainEvent);
            }
        }

        #endregion
    }
}
