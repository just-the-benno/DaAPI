﻿using DaAPI.Core.Packets.DHCPv4;
using DaAPI.Core.Packets.DHCPv6;
using DaAPI.Infrastructure.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DaAPI.Infrastructure.StorageEngine.DHCPv4
{
    public class DHCPv4PacketHandledEntryDataModel : IPacketHandledEntry<DHCPv4MessagesTypes>
    {
        [Key]
        public Guid Id { get; set; }

        public DHCPv4MessagesTypes RequestType { get; set; }
        public DHCPv4MessagesTypes? ResponseType { get; set; }
        public UInt16 RequestSize { get; set; }
        public UInt16? ResponseSize { get; set; }

        public Boolean HandledSuccessfully { get; set; }
        public Int32 ErrorCode { get; set; }
        public String FilteredBy { get; set; }
        public Boolean InvalidRequest { get; set; }

        public Guid? ScopeId { get; set; }
        //public Guid? LeaseId { get; set; }

        public DateTime Timestamp { get; set; }
        public DateTime TimestampDay { get; set; }
        public DateTime TimestampWeek { get; set; }
        public DateTime TimestampMonth { get; set; }

        public void SetTimestampDates()
        {
            TimestampDay = Timestamp.Date;
            TimestampMonth = new DateTime(Timestamp.Year, Timestamp.Month, 1);
            TimestampWeek = Timestamp.GetFirstWeekDay().AddSeconds(1);
        }
    }
}
