using DaAPI.Infrastructure.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaAPI.Infrastructure.StorageEngine
{
    public interface IPacketHandledEntry<TPacketType> where TPacketType : struct
    {
         TPacketType RequestType { get; set; }
         TPacketType? ResponseType { get; set; }
         UInt16 RequestSize { get; set; }
         UInt16? ResponseSize { get; set; }

         Boolean HandledSuccessfully { get; set; }
         Int32 ErrorCode { get; set; }
         String FilteredBy { get; set; }
         Boolean InvalidRequest { get; set; }

         Guid? ScopeId { get; set; }
        //public Guid? LeaseId { get; set; }

         DateTime Timestamp { get; set; }
         DateTime TimestampDay { get; set; }
         DateTime TimestampWeek { get; set; }
         DateTime TimestampMonth { get; set; }
    }
}
