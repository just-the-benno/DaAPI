using DaAPI.Core.Common;
using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Packets.DHCPv6;
using DaAPI.Infrastructure.StorageEngine;
using DaAPI.Infrastructure.StorageEngine.DHCPv6;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Threading.Tasks;
using static DaAPI.Core.Scopes.DHCPv6.DHCPv6PacketHandledEvents;
using static DaAPI.Core.Scopes.DHCPv6.DHCPv6PacketHandledEvents.DHCPv6DeclineHandledEvent;

namespace DaAPI.Host
{
    public class DatabaseSeeder
    {
        private Byte[] GetRandomBytes(Random random, Int32 min = 5, Int32 max = 15)
        {
            Int32 length = random.Next(min, max);
            Byte[] result = new byte[length];
            random.NextBytes(result);

            return result;
        }

        public class PseudoAggregateRoot : AggregateRootWithEvents
        {
            public PseudoAggregateRoot() : base(Guid.NewGuid())
            {
                
            }

            public void AddEvents(IEnumerable<DomainEvent> events)
            {
                foreach (var item in events)
                {
                    base.Apply(item);
                };
            }

            protected override void When(DomainEvent domainEvent)
            {
            }
        }

        public async Task SeedDatabase(Boolean reset, StorageContext storageContext)
        {
            if (reset == true)
            {
                var packets = await storageContext.DHCPv6PacketEntries.ToListAsync();
                var entries = await storageContext.DHCPv6LeaseEntries.ToListAsync();
                storageContext.RemoveRange(packets);
                storageContext.RemoveRange(entries);

                await storageContext.SaveChangesAsync();
            }

            if (storageContext.DHCPv6PacketEntries.Count() == 0)
            {
                DateTime start = DateTime.UtcNow.AddDays(-20);
                DateTime end = DateTime.UtcNow.AddDays(20);

                Int32 diff = (Int32)(end - start).TotalMinutes;

                Random random = new Random();

                List<DHCPv6PacketHandledEntryDataModel> packetEntries = new List<DHCPv6PacketHandledEntryDataModel>();
                var requestPacketTypes = new[] { DHCPv6PacketTypes.Solicit, DHCPv6PacketTypes.CONFIRM, DHCPv6PacketTypes.DECLINE, DHCPv6PacketTypes.REBIND, DHCPv6PacketTypes.RELEASE, DHCPv6PacketTypes.RENEW, DHCPv6PacketTypes.REQUEST };
                for (int i = 0; i < 30_000; i++)
                {
                    var entry = new DHCPv6PacketHandledEntryDataModel
                    {
                        Id = Guid.NewGuid(),
                        Timestamp = start.AddMinutes(random.Next(0, diff)),
                        RequestSize = (UInt16)random.Next(40, 259),
                        ScopeId = Guid.NewGuid(),
                        RequestType = requestPacketTypes[random.Next(0, requestPacketTypes.Length)],
                    };

                    if (random.NextDouble() > 0.5)
                    {
                        entry.ResponseSize = (UInt16)random.Next(40, 180);
                        entry.ResponseType = random.NextDouble() > 0.3 ? DHCPv6PacketTypes.REPLY : DHCPv6PacketTypes.ADVERTISE;
                    }


                    if (random.NextDouble() > 0.8)
                    {
                        entry.FilteredBy = "something";
                    }
                    else
                    {
                        if (random.NextDouble() > 0.8)
                        {
                            entry.InvalidRequest = true;
                        }
                        else
                        {
                            if (random.NextDouble() > 0.5)
                            {
                                entry.HandledSuccessfully = true;
                                entry.ErrorCode = 0;
                            }
                            else
                            {
                                entry.HandledSuccessfully = false;
                                entry.ErrorCode = random.Next(0, 5);
                            }
                        }
                    }

                    entry.SetTimestampDates();

                    packetEntries.Add(entry);
                }

                List<DHCPv6LeaseEntryDataModel> leaseEntries = new List<DHCPv6LeaseEntryDataModel>();
                for (int i = 0; i < 30_000; i++)
                {
                    Byte[] addressBytes = new byte[16];
                    Byte[] prefixBytes = new byte[16];
                    random.NextBytes(addressBytes);
                    random.NextBytes(prefixBytes);

                    DHCPv6LeaseEntryDataModel entryDataModel = new DHCPv6LeaseEntryDataModel
                    {
                        Id = Guid.NewGuid(),
                        Timestamp = start.AddMinutes(random.Next(0, diff)),
                        LeaseId = Guid.NewGuid(),
                        Address = IPv6Address.FromByteArray(addressBytes).ToString(),
                        EndReason = Shared.Responses.StatisticsControllerResponses.V1.ReasonToEndLease.Nothing,
                        ScopeId = Guid.NewGuid(),
                        Start = start.AddMinutes(random.Next(0, diff - 50)),
                    };

                    Int32 leaseDiff = (Int32)(end.AddDays(4) - entryDataModel.Start).TotalMinutes;
                    entryDataModel.End = entryDataModel.Start.AddMinutes(random.Next(10, leaseDiff));

                    if (random.NextDouble() > 0.5)
                    {
                        entryDataModel.Prefix = IPv6Address.FromByteArray(prefixBytes).ToString();
                        entryDataModel.PrefixLength = (Byte)random.Next(48, 76);
                    }

                    leaseEntries.Add(entryDataModel);
                }

                List<DomainEvent> handledEvents = new List<DomainEvent>();

                for (int i = 0; i < 8_000; i++)
                {
                    Int32 expectedErrorCode = random.NextDouble() > 0.5 ? random.Next(0, 5) : 0;

                    Func<Int32, DHCPv6PacketHandledEvent> activator = null;
                    Int32 type = random.Next(0, 8);
                    switch (type)
                    {
                        case 0:
                            activator = (x) => new DHCPv6DeclineHandledEvent { Error = (DHCPv6DeclineHandledEvent.DeclineErros)x };
                            break;

                        case 1:
                            activator = (x) => new DHCPv6SolicitHandledEvent { Error = (DHCPv6SolicitHandledEvent.SolicitErros)x };
                            break;

                        case 2:
                            activator = (x) => new DHCPv6InformRequestHandledEvent { Error = (DHCPv6InformRequestHandledEvent.InformRequestErros)x };
                            break;

                        case 3:
                            activator = (x) => new DHCPv6ReleaseHandledEvent { Error = (DHCPv6ReleaseHandledEvent.ReleaseError)x };
                            break;

                        case 4:
                            activator = (x) => new DHCPv6RequestHandledEvent { Error = (DHCPv6RequestHandledEvent.RequestErrors)x };
                            break;

                        case 5:
                            activator = (x) => new DHCPv6RenewHandledEvent { Error = (DHCPv6RenewHandledEvent.RenewErrors)x };
                            break;

                        case 6:
                            activator = (x) => new DHCPv6RebindHandledEvent { Error = (DHCPv6RebindHandledEvent.RebindErrors)x };
                            break;

                        case 7:
                            activator = (x) => new DHCPv6ConfirmHandledEvent { Error = (DHCPv6ConfirmHandledEvent.ConfirmErrors)x };
                            break;

                        default:
                            break;
                    }

                    if (activator == null) { continue; }

                    var @event = activator(expectedErrorCode);

                    @event.Request = 
                           
                        DHCPv6RelayPacket.AsOuterRelay(new IPv6HeaderInformation(IPv6Address.FromString("fe80::1"), IPv6Address.FromString("fe80::2")),true,2, IPv6Address.FromString("faf::2"), IPv6Address.FromString("fefc::23"),
                        new DHCPv6PacketOption[] { 
                                new DHCPv6PacketRemoteIdentifierOption((UInt32)random.Next(), GetRandomBytes(random)),
                                new DHCPv6PacketByteArrayOption(DHCPv6PacketOptionTypes.InterfaceId,GetRandomBytes(random)),
                        },
                          DHCPv6RelayPacket.AsInnerRelay(true,1,IPv6Address.FromString("fe70::2"),  IPv6Address.FromString("fecc::23"),
                           new DHCPv6PacketOption[] {
                                new DHCPv6PacketByteArrayOption(DHCPv6PacketOptionTypes.InterfaceId,GetRandomBytes(random)),
                            },
                            DHCPv6Packet.AsInner(
                            (UInt16)random.Next(0, UInt16.MaxValue),
                            random.NextDouble() > 0.3 ? DHCPv6PacketTypes.Solicit : DHCPv6PacketTypes.RELEASE,
                            new DHCPv6PacketOption[]
                            {
                                    new DHCPv6PacketIdentifierOption(DHCPv6PacketOptionTypes.ServerIdentifer,new UUIDDUID(Guid.NewGuid())),
                                    new DHCPv6PacketIdentifierOption(DHCPv6PacketOptionTypes.ClientIdentifier,new UUIDDUID(Guid.NewGuid())),
                                    new DHCPv6PacketIdentityAssociationNonTemporaryAddressesOption((UInt32)random.Next()),
                            })));

                    if (random.NextDouble() > 0.5)
                    {
                        @event.Response = DHCPv6Packet.AsOuter(
                            new IPv6HeaderInformation(IPv6Address.FromString("fe80::1"), IPv6Address.FromString("fe80::2")),
                            (UInt16)random.Next(0, UInt16.MaxValue),
                            random.NextDouble() > 0.3 ? DHCPv6PacketTypes.REPLY : DHCPv6PacketTypes.ADVERTISE,
                            new DHCPv6PacketOption[]
                            {
                                    new DHCPv6PacketIdentifierOption(DHCPv6PacketOptionTypes.ServerIdentifer,new UUIDDUID(Guid.NewGuid())),
                                    new DHCPv6PacketIdentifierOption(DHCPv6PacketOptionTypes.ClientIdentifier,new UUIDDUID(Guid.NewGuid())),
                                    DHCPv6PacketIdentityAssociationNonTemporaryAddressesOption.AsSuccess(
                                            (UInt16) random.Next(0, UInt16.MaxValue),TimeSpan.FromMinutes(random.Next(30,100)),TimeSpan.FromMinutes(random.Next(30,100)),IPv6Address.FromString("fe80::100"),
                                            TimeSpan.FromMinutes(random.Next(30,100)),TimeSpan.FromMinutes(random.Next(30,100))),
                                     DHCPv6PacketIdentityAssociationPrefixDelegationOption.AsSuccess((UInt32)random.Next(),TimeSpan.FromMinutes(random.Next(30,100)),TimeSpan.FromMinutes(random.Next(30,100)),
                                     (Byte)random.Next(30,68),IPv6Address.FromString("fc:12::0"),TimeSpan.FromMinutes(random.Next(30,100)),TimeSpan.FromMinutes(random.Next(30,100))),
                                    new DHCPv6PacketBooleanOption(DHCPv6PacketOptionTypes.Auth,random.NextDouble() > 0.5),
                                    new DHCPv6PacketByteOption(DHCPv6PacketOptionTypes.Preference,(Byte)random.Next(0,256)),
                                    new DHCPv6PacketTrueOption(DHCPv6PacketOptionTypes.RapitCommit),
                                    new DHCPv6PacketIPAddressOption(DHCPv6PacketOptionTypes.ServerUnicast,IPv6Address.FromString("fd::1")),
                                    new DHCPv6PacketIPAddressListOption(48,new [] {IPv6Address.FromString("2001::1"), IPv6Address.FromString("2001::1") }),
                            });
                    }

                    handledEvents.Add(@event);
                }

                PseudoAggregateRoot pseudo = new PseudoAggregateRoot();
                pseudo.AddEvents(handledEvents);

                await storageContext.Save(pseudo);

                storageContext.AddRange(packetEntries);
                storageContext.AddRange(leaseEntries);

                storageContext.SaveChanges();
            }
        }
    }
}

