using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DaAPI.Shared.Responses.StatisticsControllerResponses.V1;

namespace DaAPI.Infrastructure.StorageEngine
{
    public interface ILeaseEntry
    {
        public Guid LeaseId { get; set; }
        public String Address { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public Guid ScopeId { get; set; }
        public ReasonToEndLease EndReason { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
