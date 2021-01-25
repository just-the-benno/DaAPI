using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Shared.Responses
{
    public static class DHCPv4InterfaceResponses
    {
        public static class V1
        {
            public class DHCPv4InterfaceEntry
            {
                public String IPv4Address { get;  set; }
                public String PhysicalInterfaceId { get;  set; }
                public Byte[] MACAddress { get;  set; }
                public String InterfaceName { get;  set; }
            }

            public class DetachedDHCPv4InterfaceEntry
            {
                public Guid SystemId { get; set; }
                public String Name { get; set; }
                public String IPv4Address { get; set; }
            }

            public class ActiveDHCPv4InterfaceEntry
            {
                public Guid SystemId { get; set; }
                public String Name { get; set; }
                public String IPv4Address { get; set; }
                public String PhysicalInterfaceId { get; set; }
                public Byte[] MACAddress { get; set; }
                public String PhysicalInterfaceName { get; set; }
            }

            public class DHCPv4InterfaceOverview
            {
                public IEnumerable<ActiveDHCPv4InterfaceEntry> ActiveEntries { get; set; }
                public IEnumerable<DHCPv4InterfaceEntry> Entries { get; set; }
                public IEnumerable<DetachedDHCPv4InterfaceEntry> DetachedEntries { get; set; }
            }
        }
    }
}
