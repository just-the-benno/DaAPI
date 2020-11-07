﻿using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Packets;
using DaAPI.Core.Packets.DHCPv6;
using System;
using System.Collections.Generic;
using System.Text;
using static DaAPI.Core.Scopes.DHCPv6.DHCPv6PacketHandledEvents;

namespace DaAPI.Shared.Responses
{
    public static class StatisticsControllerResponses
    {
        public static class V1
        {
            public enum ReasonToEndLease
            {
                Nothing = 0,
                Canceled = 3,
                Revoked = 6,
                Released = 10,
                Expired = 11,
            }

            public class DHCPv6LeaseEntry
            {
                public Guid LeaseId { get; set; }
                public String Address { get; set; }
                public String Prefix { get; set; }
                public Byte PrefixLength { get; set; }
                public DateTime Start { get; set; }
                public DateTime End { get; set; }
                public Guid ScopeId { get; set; }
                public ReasonToEndLease EndReason { get; set; }
                public DateTime Timestamp { get; set; }
            }


            public class DHCPOverview<TLeaseOverview, DHCPv6PacketHandledEntry>
            {
                public Int32 ScopeAmount { get; set; }
                public Int32 TotalInterfaces { get; set; }
                public Int32 ActiveInterfaces { get; set; }

                public IList<TLeaseOverview> ActiveLeases { get; set; }
                public IList<DHCPv6PacketHandledEntry> Packets { get; set; }
            }
            
            public class SimplifiedIPv6HeaderInformation
            {
                public String Source { get; set; }
                public String Destination { get; set; }

                public SimplifiedIPv6HeaderInformation()
                {

                }

                public SimplifiedIPv6HeaderInformation(IPHeader<IPv6Address> headerInformation)
                {
                    Destination = headerInformation.Destionation.ToString();
                    Source = headerInformation.Source.ToString();
                }
            }

            public class DHCPv6PacketInformation
            {
                public SimplifiedIPv6HeaderInformation Header { get; set; }
                public Byte[] Content { get; set; }

                public DHCPv6PacketInformation()
                {

                }

                public DHCPv6PacketInformation(DHCPv6Packet packet)
                {
                    Content = packet.GetAsStream();
                    Header = new SimplifiedIPv6HeaderInformation(packet.Header);
                }

                private DHCPv6Packet _packet;

                public DHCPv6Packet GetPacket()
                {
                    if(_packet == null)
                    {
                        _packet = DHCPv6Packet.FromByteArray(Content, 
                            new IPv6HeaderInformation(IPv6Address.FromString(Header.Source), IPv6Address.FromString(Header.Destination)));
                    }

                    return _packet;
                }
            }

            public class DHCPv6PacketHandledEntry
            {
                public DHCPv6PacketHandledEntry()
                {

                }

                private DHCPv6PacketHandledEntry(DHCPv6PacketHandledEvent handledEvent, DHCPv6PacketTypes type) : this()
                {
                    Timestamp = handledEvent.Timestamp;
                    RequestType = type;
                    Request = new DHCPv6PacketInformation(handledEvent.Request);
                    ResultCode = handledEvent.ErrorCode;
                    ScopeId = handledEvent.ScopeId;

                    if(handledEvent.Response != null)
                    {
                        ResponseType = handledEvent.Response.GetInnerPacket().PacketType;
                        Response = new DHCPv6PacketInformation(handledEvent.Response);
                    }
                }

                public DHCPv6PacketHandledEntry(DHCPv6SolicitHandledEvent entry) : this(entry, DHCPv6PacketTypes.Solicit)
                {
                }

                public DHCPv6PacketHandledEntry(DHCPv6DeclineHandledEvent entry) : this(entry, DHCPv6PacketTypes.DECLINE)
                {
                }

                public DHCPv6PacketHandledEntry(DHCPv6InformRequestHandledEvent entry) : this(entry, DHCPv6PacketTypes.INFORMATION_REQUEST)
                {
                }

                public DHCPv6PacketHandledEntry(DHCPv6ReleaseHandledEvent entry) : this(entry, DHCPv6PacketTypes.RELEASE)
                {
                }


                public DHCPv6PacketHandledEntry(DHCPv6RequestHandledEvent entry) : this(entry, DHCPv6PacketTypes.REQUEST)
                {
                }

                public DHCPv6PacketHandledEntry(DHCPv6RenewHandledEvent entry) : this(entry, DHCPv6PacketTypes.RENEW)
                {
                }

                public DHCPv6PacketHandledEntry(DHCPv6RebindHandledEvent entry) : this(entry, DHCPv6PacketTypes.REBIND)
                {
                }

                public DHCPv6PacketHandledEntry(DHCPv6ConfirmHandledEvent entry) : this(entry, DHCPv6PacketTypes.CONFIRM)
                {
                }

                public DateTime Timestamp { get; set; }
                public DHCPv6PacketTypes RequestType { get; set; }
                public Int32 ResultCode { get; set; }
                public Guid? ScopeId { get; set; }
                public Guid? LeaseId { get; set; }
                public DHCPv6PacketTypes? ResponseType { get; set; }
                public String FilteredBy { get; set; }
                public Boolean InvalidRequest { get; set; }
                public DHCPv6PacketInformation Request { get; set; }
                public DHCPv6PacketInformation Response { get; set; }
            }

            public class DashboardResponse
            {
                public DHCPOverview<DHCPv6LeaseEntry, DHCPv6PacketHandledEntry> DHCPv6 { get; set; }
                public Int32 AmountOfPipelines { get; set; }
            }
        }
    }
}