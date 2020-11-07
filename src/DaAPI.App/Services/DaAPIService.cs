using DaAPI.App.Helper;
using DaAPI.Core.Packets.DHCPv6;
using DaAPI.Shared.Commands;
using DaAPI.Shared.JsonConverters;
using DaAPI.Shared.Requests;
using DaAPI.Shared.Responses;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using static DaAPI.Shared.Requests.DHCPv6InterfaceRequests.V1;
using static DaAPI.Shared.Requests.DHCPv6ScopeRequests.V1;
using static DaAPI.Shared.Requests.LocalUserRequests.V1;
using static DaAPI.Shared.Requests.NotificationPipelineRequests.V1;
using static DaAPI.Shared.Requests.StatisticsControllerRequests.V1;
using static DaAPI.Shared.Responses.DHCPv6InterfaceResponses.V1;
using static DaAPI.Shared.Responses.DHCPv6LeasesResponses.V1;
using static DaAPI.Shared.Responses.DHCPv6ScopeResponses.V1;
using static DaAPI.Shared.Responses.LocalUsersResponses.V1;
using static DaAPI.Shared.Responses.NotificationPipelineResponses.V1;
using static DaAPI.Shared.Responses.ServerControllerResponses;
using static DaAPI.Shared.Responses.ServerControllerResponses.V1;
using static DaAPI.Shared.Responses.StatisticsControllerResponses.V1;

namespace DaAPI.App.Services
{
    public class DaAPIService
    {
        private readonly HttpClient _client;
        private readonly ILogger<DaAPIService> _logger;

        public DaAPIService(HttpClient client, ILogger<DaAPIService> logger)
        {
            this._client = client ?? throw new ArgumentNullException(nameof(client));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private StringContent GetStringContentAsJson<T>(T input)
        {
            String serialziedObject = Newtonsoft.Json.JsonConvert.SerializeObject(input, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
            });
            return new StringContent(
                serialziedObject, Encoding.UTF8, "application/json");
        }

        private JsonSerializerSettings GetDefaultSettings()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
            };

            settings.Converters.Add(new DUIDJsonConverter());

            return settings;
        }

        public async Task<ServerInitilizedResponse> ServerIsInitilized()
        {
            try
            {
                var result = await _client.GetFromJsonAsync<ServerInitilizedResponse>("api/Server/IsInitialized");
                return result;
            }
            catch (Exception)
            {
                return ServerInitilizedResponse.NotInitilized;
            }
        }

        public async Task<ServerInitilizedResponse> ServerIsInitilized2()
        {
            try
            {
                var result = await _client.GetFromJsonAsync<ServerInitilizedResponse>("api/Server/IsInitialized2");
                return result;
            }
            catch (Exception)
            {
                return ServerInitilizedResponse.NotInitilized;
            }
        }

        public async Task<Boolean> InitilizeServer(InitilizeServeRequest request)
        {
            var response = await _client.PostAsJsonAsync("api/Server/Initialize", request);
            return response.IsSuccessStatusCode;
        }

        public async Task<IEnumerable<LocalUserOverview>> GetUsers()
        {
            try
            {
                var result = await _client.GetFromJsonAsync<IEnumerable<LocalUserOverview>>("api/LocalUsers");
                return result;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<Boolean> ResetLocalUserPassword(String localUserId, ResetPasswordRequest request)
        {
            try
            {
                var response = await _client.PutAsJsonAsync(
                    $"api/LocalUsers/ChangePassword/{localUserId}", request);

                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                _logger.LogError("unable to send service request");
                return false;
            }
        }

        public async Task<Boolean> CreateLocalUser(CreateUserRequest request)
        {
            try
            {
                var response = await _client.PostAsJsonAsync("api/LocalUsers/", request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                _logger.LogError("unable to send service request");
                return false;
            }
        }

        public async Task<Boolean> SendDeleteLocalUserRequest(String localUserId)
        {
            try
            {
                var response = await _client.DeleteAsync($"api/LocalUsers/{localUserId}");

                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                _logger.LogError("unable to send service request");
                return false;
            }
        }

        public async Task<DHCPv6InterfaceOverview> GetDHCPv6Interfaces()
        {
            try
            {
                var response = await _client.GetFromJsonAsync<DHCPv6InterfaceOverview>("api/interfaces/dhcpv6");
                return response;
            }
            catch (Exception)
            {
                _logger.LogError("unable to send service request");
                return null;
            }
        }

        public async Task<Boolean> CreateDHCPv6Interface(CreateDHCPv6Listener request)
        {
            try
            {
                var response = await _client.PostAsJsonAsync("api/interfaces/dhcpv6/", request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                _logger.LogError("unable to send service request");
                return false;
            }
        }

        public async Task<Boolean> SendDeleteDHCPv6InterfaceRequest(Guid interfaceId)
        {
            try
            {
                var response = await _client.DeleteAsync($"api/interfaces/dhcpv6/{interfaceId}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                _logger.LogError("unable to send service request");
                return false;
            }
        }

        public async Task<IEnumerable<ScopeItem>> GetScopesAsList()
        {
            try
            {
                var response = await _client.GetFromJsonAsync<IEnumerable<ScopeItem>>("api/scopes/dhcpv6/list");
                return response;
            }
            catch (Exception)
            {
                _logger.LogError("unable to send service request");
                return null;
            }
        }

        public async Task<IEnumerable<ScopeTreeViewItem>> GetScopesAsTree()
        {
            try
            {
                var response = await _client.GetFromJsonAsync<IEnumerable<ScopeTreeViewItem>>("api/scopes/dhcpv6/tree");
                return response;
            }
            catch (Exception)
            {
                _logger.LogError("unable to send service request");
                return null;
            }
        }


        public async Task<Boolean> CreateDHCPv6Scope(CreateOrUpdateDHCPv6ScopeRequest request)
        {
            try
            {
                var response = await _client.PostAsync("api/scopes/dhcpv6/", GetStringContentAsJson(request));
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "unable to send service request");
                return false;
            }
        }

        public async Task<IEnumerable<ScopeResolverDescription>> GetDHCPv6ScopeResolverDescription()
        {
            var response = await _client.GetFromJsonAsync<IEnumerable<ScopeResolverDescription>>(
                "api/scopes/dhcpv6/resolvers/description");

            return response;
        }

        public async Task<DHCPv6ScopePropertiesResponse> GetDHCPv6ScopeProperties(Guid scopeId, Boolean includeParents = true) => await GetDHCPv6ScopeProperties(scopeId.ToString(), includeParents);

        public async Task<DHCPv6ScopePropertiesResponse> GetDHCPv6ScopeProperties(String scopeId, Boolean includeParents = true)
        {
            var response = await _client.GetAsync(
                $"api/scopes/dhcpv6/{scopeId}/properties?includeParents={includeParents}");

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                return null;
            }

            String content = await response.Content.ReadAsStringAsync();
            JsonSerializerSettings settings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
            settings.Converters.Add(new DHCPv6ScopePropertyResponseJsonConverter());

            var result = JsonConvert.DeserializeObject<DHCPv6ScopePropertiesResponse>(
                content, settings);

            return result;
        }


        public async Task<IEnumerable<NotificationPipelineReadModel>> GetNotifactionPipelines()
        {
            var response = await _client.GetFromJsonAsync<IEnumerable<NotificationPipelineReadModel>>(
                "/api/notifications/pipelines/");

            return response;
        }

        public async Task<NotificationPipelineDescriptions> GetpipelineDescriptions()
        {
            var response = await _client.GetFromJsonAsync<NotificationPipelineDescriptions>(
                "/api/notifications/pipelines/descriptions");

            return response;
        }

        public async Task<Boolean> SendDeleteNotificationPipelineRequest(Guid pipelineId)
        {
            try
            {
                var response = await _client.DeleteAsync($"/api/notifications/pipelines/{pipelineId}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                _logger.LogError("unable to send service request");
                return false;
            }
        }

        public async Task<Boolean> CreateNotificationPipeline(CreateNotifcationPipelineRequest request)
        {
            try
            {
                var response = await _client.PostAsJsonAsync("/api/notifications/pipelines/", request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "unable to send service request");
                return false;
            }
        }

        public async Task<Boolean> SendDeleteDHCPv6ScopeRequest(DHCPv6ScopeDeleteRequest request)
        {
            try
            {
                var response = await _client.DeleteAsync(
                    $"/api/scopes/dhcpv6/{request.Id}/?includeChildren={request.IncludeChildren}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "unable to send service request");
                return false;
            }
        }

        public async Task<Boolean> UpdateDHCPv6Scope(CreateOrUpdateDHCPv6ScopeRequest request, String scopeId)
        {
            try
            {
                var response = await _client.PutAsync($"api/scopes/dhcpv6/{scopeId}", GetStringContentAsJson(request));
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "unable to send service request");
                return false;
            }
        }

        private async Task<T> GetResult<T>(String url, T fallback)
        {
            try
            {
                var response = await _client.GetAsync(url);
                if (response.IsSuccessStatusCode == false)
                {
                    return fallback;
                }

                String rawContent = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<T>(rawContent, GetDefaultSettings());
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "unable to send service request");
                return fallback;
            }
        }

        public async Task<IEnumerable<LeaseOverview>> GetLeasesByScope(String scopeId, Boolean includeChildScopes)
        {
            try
            {
                var result = await GetResult<IEnumerable<LeaseOverview>>($"/api/leases/dhcpv6/scopes/{scopeId}?includeChildren={includeChildScopes}", null);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "unable to send service request");
                return null;
            }
        }

        public async Task<DashboardResponse> GetDashboard() => await GetDashboard<DashboardResponse>();

        public async Task<TDashboard> GetDashboard<TDashboard>() where TDashboard : class
        {
            try
            {
                var result = await GetResult<TDashboard>($"/api/Statistics/Dashboard/", null);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "unable to send service request");
                return null;
            }
        }

        private String AppendTimeRangeToUrl(String url, DateTime? start, DateTime? end)
        {
            if (start.HasValue == true)
            {
                url += $"&start={start.Value:o}";
            }
            if (end.HasValue == true)
            {
                url += $"&end={end.Value:o}";
            }

            return url;
        }

        private String AppendGroupingToUrl(String url, DateTime? start, DateTime? end, GroupStatisticsResultBy group) =>
            AppendTimeRangeToUrl($"{url}?GroupbBy={group}", start, end);

        public async Task<IDictionary<DateTime, Int32>> GetActiveDHCPv6Leases(DateTime? start, DateTime? end, GroupStatisticsResultBy group) =>
            await GetSimpleStatisticsData("/api/Statistics/ActiveDHCPv6Leases", start, end, group);

        public async Task<IDictionary<DateTime, Int32>> GetIncomingDHCPv6PacketAmount(DateTime? start, DateTime? end, GroupStatisticsResultBy group) =>
            await GetSimpleStatisticsData("/api/Statistics/IncomingDHCPv6Packets", start, end, group);

        public async Task<IDictionary<DateTime, Int32>> GetErrorDHCPv6Packets(DateTime? start, DateTime? end, GroupStatisticsResultBy group) =>
             await GetSimpleStatisticsData("/api/Statistics/ErrorDHCPv6Packets", start, end, group);

        public async Task<IDictionary<DateTime, Int32>> GetFileredDHCPv6Packets(DateTime? start, DateTime? end, GroupStatisticsResultBy group) =>
             await GetSimpleStatisticsData("/api/Statistics/FileredDHCPv6Packets", start, end, group);

        public async Task<IDictionary<DateTime, IDictionary<DHCPv6PacketTypes, Int32>>> GetIncomingDHCPv6PacketTypes(DateTime? start, DateTime? end, GroupStatisticsResultBy group) =>
                await GetSimpleStatisticsData<IDictionary<DHCPv6PacketTypes, Int32>>("/api/Statistics/IncomingDHCPv6PacketTypes", start, end, group);

        public async Task<IDictionary<Int32, Int32>> GetErrorCodesPerDHCPV6RequestType(DateTime? start, DateTime? end, DHCPv6PacketTypes packetType) =>
           await GetSimpleStatisticsData<Int32, Int32>(AppendTimeRangeToUrl($"/api/Statistics/ErrorCodesPerDHCPV6RequestType?PacketType={packetType}", start, end));

        private async Task<IDictionary<DateTime, Int32>> GetSimpleStatisticsData(String baseurl, DateTime? start, DateTime? end, GroupStatisticsResultBy group) =>
            await GetSimpleStatisticsData<Int32>(baseurl, start, end, group);

        private async Task<IDictionary<TKey, TValue>> GetSimpleStatisticsData<TKey, TValue>(String url)
        {
            try
            {
                var result = await GetResult(url, new Dictionary<TKey, TValue>());
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "unable to send service request");
                return null;
            }
        }

        private async Task<IDictionary<DateTime, T>> GetSimpleStatisticsData<T>(String url) =>
            await GetSimpleStatisticsData<DateTime, T>(url);

        private async Task<IDictionary<DateTime, T>> GetSimpleStatisticsData<T>(String baseurl, DateTime? start, DateTime? end, GroupStatisticsResultBy group) =>
            await GetSimpleStatisticsData<T>(AppendGroupingToUrl(baseurl, start, end, group));

        public async Task<IEnumerable<DHCPv6PacketHandledEntry>> GetHandledDHCPv6PacketByScopeId(String scopeId, Int32 amount = 100) => await GetHandledDHCPv6PacketByScopeId<DHCPv6PacketHandledEntry>(scopeId, amount);

        public async Task<IEnumerable<THandeled>> GetHandledDHCPv6PacketByScopeId<THandeled>(String scopeId, Int32 amount = 100) where THandeled : DHCPv6PacketHandledEntry
        {
            var result = await GetResult<IEnumerable<THandeled>>($"/api/Statistics/HandledDHCPv6Packet/{scopeId}?amount={amount}", Array.Empty<THandeled>());
            return result;
        }

    }
}
