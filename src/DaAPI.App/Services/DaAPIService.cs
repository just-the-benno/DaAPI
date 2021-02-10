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
using static DaAPI.Shared.Requests.DHCPv4InterfaceRequests.V1;
using static DaAPI.Shared.Requests.DHCPv4ScopeRequests.V1;
using static DaAPI.Shared.Requests.DHCPv6InterfaceRequests.V1;
using static DaAPI.Shared.Requests.DHCPv6ScopeRequests.V1;
using static DaAPI.Shared.Requests.LocalUserRequests.V1;
using static DaAPI.Shared.Requests.NotificationPipelineRequests.V1;
using static DaAPI.Shared.Requests.StatisticsControllerRequests.V1;
using static DaAPI.Shared.Responses.DHCPv4InterfaceResponses.V1;
using static DaAPI.Shared.Responses.DHCPv4ScopeResponses.V1;
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

        public async Task<ServerInitilizedResponse> ServerIsInitilized() => await GetResponse<ServerInitilizedResponse>("api/Server/IsInitialized");
        public async Task<ServerInitilizedResponse> ServerIsInitilized2() => await GetResponse<ServerInitilizedResponse>("api/Server/IsInitialized2");

        public async Task<Boolean> InitilizeServer(InitilizeServeRequest request) =>
            await ExecuteCommand(() =>  _client.PostAsJsonAsync("api/Server/Initialize", request));

        public async Task<IEnumerable<LocalUserOverview>> GetUsers() => await GetResponse<IEnumerable<LocalUserOverview>>("api/LocalUsers");

        private async Task<Boolean> ExecuteCommand(Func<Task<HttpResponseMessage>> serviceCaller)
        {
            try
            {
                var result = await serviceCaller();
                return result.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                _logger.LogError("unable to send service request");
                return false;
            }
        }

        public async Task<Boolean> ResetLocalUserPassword(String localUserId, ResetPasswordRequest request) =>
            await ExecuteCommand(() => _client.PutAsJsonAsync(
                   $"api/LocalUsers/ChangePassword/{localUserId}", request));

        public async Task<Boolean> CreateLocalUser(CreateUserRequest request) =>
            await ExecuteCommand(() => _client.PostAsJsonAsync("api/LocalUsers/", request));

        public async Task<Boolean> SendDeleteLocalUserRequest(String localUserId) =>
            await ExecuteCommand(() => _client.DeleteAsync($"api/LocalUsers/{localUserId}"));

        public async Task<Boolean> CreateDHCPv6Interface(CreateDHCPv6Listener request) =>
            await ExecuteCommand(() => _client.PostAsJsonAsync("api/interfaces/dhcpv6/", request));

        public async Task<Boolean> SendDeleteDHCPv6InterfaceRequest(Guid interfaceId) =>
            await ExecuteCommand(() => _client.DeleteAsync($"api/interfaces/dhcpv6/{interfaceId}"));

        public async Task<Boolean> CreateDHCPv6Scope(CreateOrUpdateDHCPv6ScopeRequest request) =>
            await ExecuteCommand(() => _client.PostAsync("api/scopes/dhcpv6/", GetStringContentAsJson(request)));

        public async Task<Boolean> SendDeleteNotificationPipelineRequest(Guid pipelineId) =>
            await ExecuteCommand(() => _client.DeleteAsync($"/api/notifications/pipelines/{pipelineId}"));

        public async Task<Boolean> CreateNotificationPipeline(CreateNotifcationPipelineRequest request) =>
            await ExecuteCommand(() => _client.PostAsJsonAsync("/api/notifications/pipelines/", request));

        public async Task<Boolean> SendDeleteDHCPv6ScopeRequest(DHCPv6ScopeDeleteRequest request) =>
            await ExecuteCommand(() => _client.DeleteAsync($"/api/scopes/dhcpv6/{request.Id}/?includeChildren={request.IncludeChildren}"));

        public async Task<Boolean> UpdateDHCPv6Scope(CreateOrUpdateDHCPv6ScopeRequest request, String scopeId) =>
            await ExecuteCommand(() => _client.PutAsync($"api/scopes/dhcpv6/{scopeId}", GetStringContentAsJson(request)));

        private async Task<TResult> GetResponse<TResult>(String url) where TResult : class
        {
            try
            {
                var response = await _client.GetFromJsonAsync<TResult>(url);
                return response;
            }
            catch (Exception)
            {
                _logger.LogError("unable to send service request");
                return null;
            }
        }

        public async Task<DHCPv6InterfaceOverview> GetDHCPv6Interfaces() => await GetResponse<DHCPv6InterfaceOverview>("api/interfaces/dhcpv6");
        public async Task<IEnumerable<DHCPv6ScopeItem>> GetDHCPv6ScopesAsList() => await GetResponse<IEnumerable<DHCPv6ScopeItem>>("api/scopes/dhcpv6/list");
        public async Task<IEnumerable<DHCPv6ScopeTreeViewItem>> GetDHCPv6ScopesAsTree() => await GetResponse<IEnumerable<DHCPv6ScopeTreeViewItem>>("api/scopes/dhcpv6/tree");
        public async Task<IEnumerable<DHCPv6ScopeResolverDescription>> GetDHCPv6ScopeResolverDescription() => await GetResponse<IEnumerable<DHCPv6ScopeResolverDescription>>("api/scopes/dhcpv6/resolvers/description");
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


        public async Task<IEnumerable<NotificationPipelineReadModel>> GetNotifactionPipelines() => await GetResponse<IEnumerable<NotificationPipelineReadModel>>("/api/notifications/pipelines/");
        public async Task<NotificationPipelineDescriptions> GetpipelineDescriptions() => await GetResponse<NotificationPipelineDescriptions>("/api/notifications/pipelines/descriptions");

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

        #region DHCPv4

        public async Task<DHCPv4InterfaceOverview> GetDHCPv4Interfaces() => await GetResponse<DHCPv4InterfaceOverview>("api/interfaces/dhcpv4");

        public async Task<Boolean> CreateDHCPv4Interface(CreateDHCPv4Listener request) =>
                    await ExecuteCommand(() => _client.PostAsJsonAsync("api/interfaces/dhcpv4/", request));

        public async Task<Boolean> SendDeleteDHCPv4InterfaceRequest(Guid interfaceId) =>
            await ExecuteCommand(() => _client.DeleteAsync($"api/interfaces/dhcpv4/{interfaceId}"));

        public async Task<IEnumerable<DHCPv4ScopeItem>> GetDHCPv4ScopesAsList() => await GetResponse<IEnumerable<DHCPv4ScopeItem>>("api/scopes/dhcpv4/list");
        public async Task<IEnumerable<DHCPv4ScopeTreeViewItem>> GetDHCPv4ScopesAsTree() => await GetResponse<IEnumerable<DHCPv4ScopeTreeViewItem>>("api/scopes/dhcpv4/tree");

        public async Task<IEnumerable<DHCPv4ScopeResolverDescription>> GetDHCPv4ScopeResolverDescription() => await GetResponse<IEnumerable<DHCPv4ScopeResolverDescription>>("api/scopes/dhcpv4/resolvers/description");
        public async Task<DHCPv4ScopePropertiesResponse> GetDHCPv4ScopeProperties(Guid scopeId, Boolean includeParents = true) => await GetDHCPv4ScopeProperties(scopeId.ToString(), includeParents);

        public async Task<DHCPv4ScopePropertiesResponse> GetDHCPv4ScopeProperties(String scopeId, Boolean includeParents = true)
        {
            var response = await _client.GetAsync(
                $"api/scopes/dhcpv4/{scopeId}/properties?includeParents={includeParents}");

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                return null;
            }

            String content = await response.Content.ReadAsStringAsync();
            JsonSerializerSettings settings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
            settings.Converters.Add(new DHCPv4ScopePropertyResponseJsonConverter());

            var result = JsonConvert.DeserializeObject<DHCPv4ScopePropertiesResponse>(
                content, settings);

            return result;
        }

        public async Task<Boolean> UpdateDHCPv4Scope(CreateOrUpdateDHCPv4ScopeRequest request, String scopeId) =>
            await ExecuteCommand(() => _client.PutAsync($"api/scopes/dhcpv4/{scopeId}", GetStringContentAsJson(request)));

        public async Task<Boolean> CreateDHCPv4Scope(CreateOrUpdateDHCPv4ScopeRequest request) =>
        await ExecuteCommand(() => _client.PostAsync("api/scopes/dhcpv4/", GetStringContentAsJson(request)));
        
        #endregion

    }
}
