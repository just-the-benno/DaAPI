using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Shared.Responses
{
    public static class ServerControllerResponses
    {
        public static class V1
        {
            public class ServerInitilizedResponse
            {
                public Boolean IsInitialized { get; set; }
                public Boolean OpenIdIsLocal { get; set; }

                public ServerInitilizedResponse()
                {

                }

                public ServerInitilizedResponse(Boolean isInitialized, Boolean openIdIsLocal)
                {
                    IsInitialized = isInitialized;
                    OpenIdIsLocal = openIdIsLocal;
                }

                public static ServerInitilizedResponse NotInitilized => new ServerInitilizedResponse { IsInitialized = false };
            }
        }
    }
}
