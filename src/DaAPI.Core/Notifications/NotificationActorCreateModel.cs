using DaAPI.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Notifications
{
    public class NotificationActorCreateModel : IDataTransferObject
    {
        public String Typename { get; set; }
        public IDictionary<String,String> PropertiesAndValues { get; set; }

        public NotificationActorCreateModel()
        {
            PropertiesAndValues = new Dictionary<String, String>();
        }

    }
}
