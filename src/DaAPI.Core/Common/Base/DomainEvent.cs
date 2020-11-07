using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Common
{
    public abstract class DomainEvent : IDataTransferObject
    {
        #region Fields

        private Boolean _isHandled;

        #endregion

        #region Properties

        public DateTime Timestamp { get;  set; }

        #endregion

        #region Constructor

        protected DomainEvent()
        {
            Timestamp = DateTime.UtcNow;
        }

        #endregion

        #region Method

        public void SetHandled() => _isHandled = true;
        public Boolean IsHandled() => _isHandled;

        #endregion
    }
}
