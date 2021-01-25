using DaAPI.Core.Common;
using DaAPI.Core.Listeners;
using DaAPI.Core.Packets.DHCPv6;
using DaAPI.Shared.Responses;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static DaAPI.Shared.Requests.StatisticsControllerRequests.V1;

namespace DaAPI.Infrastructure.StorageEngine.DHCPv6
{
    public interface IDHCPv6ReadStore : IReadStore
    {
        Task<IEnumerable<DHCPv6Listener>> GetDHCPv6Listener();
        Task<DHCPv6ServerProperties> GetServerProperties();

        Task<StatisticsControllerResponses.V1.DashboardResponse> GetDashboardOverview();
        Task<IDictionary<DateTime, Int32>> GetIncomingDHCPv6PacketAmount(DateTime? start, DateTime? end, GroupStatisticsResultBy groupedBy);
        Task<IDictionary<DateTime, IDictionary<DHCPv6PacketTypes, Int32>>> GetIncomingDHCPv6PacketTypes(DateTime? start, DateTime? end, GroupStatisticsResultBy groupedBy);
        Task<IDictionary<DateTime, Int32>> GetFileredDHCPv6Packets(DateTime? start, DateTime? end, GroupStatisticsResultBy groupedBy);
        Task<IDictionary<DateTime, Int32>> GetErrorDHCPv6Packets(DateTime? start, DateTime? end, GroupStatisticsResultBy groupedBy);
        Task<IDictionary<DateTime, Int32>> GetActiveDHCPv6Leases(DateTime? start, DateTime? end, GroupStatisticsResultBy groupedBy);
        
        Task<IDictionary<Int32, Int32>> GetErrorCodesPerDHCPV6RequestType(DateTime? start, DateTime? end, DHCPv6PacketTypes type);
        Task<IEnumerable<StatisticsControllerResponses.V1.DHCPv6PacketHandledEntry>> GetHandledDHCPv6PacketByScopeId(Guid scopeId, Int32 amount);
    }
}
