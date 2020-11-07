using DaAPI.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Notifications
{
    public abstract class NotifcationTrigger : Value
    {
        public abstract String GetTypeIdentifier();

        public virtual bool IsEmpty() => false;
    }
}
