using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Shared.Responses
{
    public static class LocalUsersResponses
    {
        public static class V1
        {
            public class LocalUserOverview
            {
                public String Id { get; set; }
                public String Name { get; set; }
            }
        }
    }
}
