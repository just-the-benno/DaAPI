using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Shared.Responses
{
    public static class DHCPv6InterfaceResponses
    {
        public static class V1
        {
            public class DHCPv6InterfaceEntry
            {
                public String IPv6Address { get;  set; }
                public String PhysicalInterfaceId { get;  set; }
                public Byte[] MACAddress { get;  set; }
                public String InterfaceName { get;  set; }
            }

            public class DetachedDHCPv6InterfaceEntry
            {
                public Guid SystemId { get; set; }
                public String Name { get; set; }
                public String IPv6Address { get; set; }
            }

            public class ActiveDHCPv6InterfaceEntry
            {
                public Guid SystemId { get; set; }
                public String Name { get; set; }
                public String IPv6Address { get; set; }
                public String PhysicalInterfaceId { get; set; }
                public Byte[] MACAddress { get; set; }
                public String PhysicalInterfaceName { get; set; }
            }

            public class DHCPv6InterfaceOverview
            {
                public IEnumerable<ActiveDHCPv6InterfaceEntry> ActiveEntries { get; set; }
                public IEnumerable<DHCPv6InterfaceEntry> Entries { get; set; }
                public IEnumerable<DetachedDHCPv6InterfaceEntry> DetachedEntries { get; set; }
            }
        }
    }
}
