using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DaAPI.Infrastructure.Services
{
    public class HttpBasedNxOsDeviceConfigurationService : INxOsDeviceConfigurationService
    {
        private class NxOsDeviceErrorData
        {
            [JsonProperty(PropertyName = "msg")]
            public String Message { get; set; }
        }

        private class NxOsDeviceError
        {
            [JsonProperty(PropertyName = "code")]
            public String Code { get; set; }

            [JsonProperty(PropertyName = "message")]
            public String Message { get; set; }

            [JsonProperty(PropertyName = "data")]
            public NxOsDeviceErrorData Data { get; set; }
        }

        private class NxOsDeviceResponse
        {
            [JsonProperty(PropertyName = "result")]
            public String Result { get; set; }

            [JsonProperty(PropertyName = "error")]
            public NxOsDeviceError Errror { get; set; }
        }

        private HttpClient _client;
        private readonly ILogger<HttpBasedNxOsDeviceConfigurationService> _logger;

        public HttpBasedNxOsDeviceConfigurationService(
            ILogger<HttpBasedNxOsDeviceConfigurationService> logger)
        {
            this._logger = logger;
        }

        public Task<Boolean> Connect(String endpoint, String username, String password)
        {
            HttpClientHandler handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };
            String authHeaderValue = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes($"{username}:{password}"));

            _client = new HttpClient(handler)
            {
                BaseAddress = new Uri(endpoint)
            };
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authHeaderValue);

            return Task.FromResult(true);
        }

        private StringContent ExecuteCLICommandContent(String cmd)
        {
            String input =
             "[" +
              "{" +
                "\"jsonrpc\": \"2.0\"," +
                "\"method\": \"cli\"," +
                "\"params\": {" +
                            $"\"cmd\": \"{cmd}\"," +
                  "\"version\": 1" +
                "}," +
                "\"id\": 1," +
                "\"rollback\": \"stop-on-error\"" +
              "}" +
            "]";

            var content = new StringContent(input, UTF8Encoding.UTF8, "application/json-rpc");
            return content;
        }

        private async Task<Boolean> ExecuteCLICommand(String command)
        {
            var result = await _client.PostAsync("/ins", ExecuteCLICommandContent(command));

            _logger.LogDebug("nxos response has code {statusCode}", result.StatusCode);


            if (result.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                _logger.LogError("unable to connect to nx os device. user is unauthorized");
                return false;
            }

            String rawContent = await result.Content.ReadAsStringAsync();
            NxOsDeviceResponse response = JsonConvert.DeserializeObject<NxOsDeviceResponse>(rawContent);

            if (result.IsSuccessStatusCode == false)
            {
                _logger.LogDebug("unable to execute command {errMsg}", response?.Errror?.Message + " " + response?.Errror?.Data?.Message);
                return false;
            }

            return response.Errror == null;
        }

        public async Task<Boolean> AddIPv6StaticRoute(IPv6Address prefix, IPv6SubnetMaskIdentifier length, IPv6Address host)
        {
            String command = $"ipv6 route {prefix}/{length} {host} 60";
            _logger.LogDebug("adding a static ipv6 route with {command}", command);

            Boolean result = await ExecuteCLICommand(command);
            return result;
        }

        public async Task<Boolean> RemoveIPv6StaticRoute(IPv6Address prefix, IPv6SubnetMaskIdentifier length, IPv6Address host)
        {
            String command = $"no ipv6 route {prefix}/{length} {host} 60";
            _logger.LogDebug("removing a static ipv6 route with {command}", command);

            Boolean result = await ExecuteCLICommand(command);
            return result;
        }
    }
}
