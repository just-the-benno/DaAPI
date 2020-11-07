using DaAPI.Shared.Commands;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using static DaAPI.Shared.Responses.ServerControllerResponses;

namespace DaAPI.App.Services
{
    public class AnnomynousDaAPIService : DaAPIService
    {
        public AnnomynousDaAPIService(HttpClient client, ILogger<DaAPIService> logger) : base(client, logger)
        {
        }
    }
}
