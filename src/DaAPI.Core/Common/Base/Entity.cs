using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Common
{
    public abstract class Entity : IInternalEventHandler
    {
        #region Fields

        readonly Action<DomainEvent> _addtionalApplier;

        #endregion

        #region Properties

        public Guid Id { get; private set; }


        #endregion

        #region Constructor

        public Entity(Guid id, Action<DomainEvent> applier)
        {
            Id = id;
            _addtionalApplier = applier ?? throw new ArgumentNullException(nameof(applier));
        }

        #endregion

        #region Methods

        protected abstract void When(DomainEvent domainEvent);

        void IInternalEventHandler.Handle(DomainEvent domainEvent) => When(domainEvent);

        protected void Apply(DomainEvent domainEvent)
        {
            if (domainEvent is EntityBasedDomainEvent @event)
            {
                if (@event.EntityId != Id)
                {
                    When(domainEvent);
                    domainEvent.SetHandled();
                }
            }
            else
            {
                When(domainEvent);
                domainEvent.SetHandled();
            }

            _addtionalApplier(domainEvent);
        }

        #endregion
    }
}
