using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Common
{
    public abstract class EntityBasedDomainEvent : DomainEvent
    {
        #region Properties

        public Guid EntityId { get; set; }

        #endregion

        #region Constructor

        protected EntityBasedDomainEvent(Guid id)
        {
            EntityId = id;
        }

        public EntityBasedDomainEvent() : base()
        {

        }

        #endregion
    }
}
