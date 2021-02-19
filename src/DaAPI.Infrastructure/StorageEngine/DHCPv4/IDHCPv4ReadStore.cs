using DaAPI.Core.Listeners;
using DaAPI.Core.Packets.DHCPv4;
using DaAPI.Shared.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DaAPI.Shared.Requests.StatisticsControllerRequests.V1;

namespace DaAPI.Infrastructure.StorageEngine.DHCPv4
{
    public interface IDHCPv4ReadStore : IReadStore
    {
        Task<IEnumerable<DHCPv4Listener>> GetDHCPv4Listener();

        Task<StatisticsControllerResponses.V1.DashboardResponse> GetDashboardOverview();
        Task<IDictionary<DateTime, Int32>> GetIncomingDHCPv4PacketAmount(DateTime? start, DateTime? end, GroupStatisticsResultBy groupedBy);
        Task<IDictionary<DateTime, IDictionary<DHCPv4MessagesTypes, Int32>>> GetIncomingDHCPv4PacketTypes(DateTime? start, DateTime? end, GroupStatisticsResultBy groupedBy);
        Task<IDictionary<DateTime, Int32>> GetFileredDHCPv4Packets(DateTime? start, DateTime? end, GroupStatisticsResultBy groupedBy);
        Task<IDictionary<DateTime, Int32>> GetErrorDHCPv4Packets(DateTime? start, DateTime? end, GroupStatisticsResultBy groupedBy);
        Task<IDictionary<DateTime, Int32>> GetActiveDHCPv4Leases(DateTime? start, DateTime? end, GroupStatisticsResultBy groupedBy);

        Task<IDictionary<Int32, Int32>> GetErrorCodesPerDHCPv4DHCPv4MessagesTypes(DateTime? start, DateTime? end, DHCPv4MessagesTypes type);
        Task<IEnumerable<StatisticsControllerResponses.V1.DHCPv4PacketHandledEntry>> GetHandledDHCPv4PacketByScopeId(Guid scopeId, Int32 amount);
    }
}
