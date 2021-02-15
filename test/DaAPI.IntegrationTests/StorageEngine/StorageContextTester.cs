using DaAPI.Core.Common;
using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Packets.DHCPv4;
using DaAPI.Core.Packets.DHCPv6;
using DaAPI.Infrastructure.StorageEngine;
using DaAPI.Infrastructure.StorageEngine.DHCPv4;
using DaAPI.Infrastructure.StorageEngine.DHCPv6;
using DaAPI.TestHelper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Xunit;
using static DaAPI.Core.Scopes.DHCPv4.DHCPv4LeaseEvents;
using static DaAPI.Core.Scopes.DHCPv4.DHCPv4PacketHandledEvents;
using static DaAPI.Core.Scopes.DHCPv6.DHCPv6LeaseEvents;
using static DaAPI.Core.Scopes.DHCPv6.DHCPv6PacketHandledEvents;
using static DaAPI.Infrastructure.StorageEngine.StorageContext;
using static DaAPI.Shared.Requests.StatisticsControllerRequests.V1;
using static DaAPI.Shared.Responses.StatisticsControllerResponses.V1;

namespace DaAPI.IntegrationTests.StorageEngine
{
    public class StorageContextTester
    {
        private class DHCPv6PacketHandledEntryDataModelEqualityComparer : IEqualityComparer<DHCPv6PacketHandledEntryDataModel>
        {
            public bool Equals([AllowNull] DHCPv6PacketHandledEntryDataModel x, [AllowNull] DHCPv6PacketHandledEntryDataModel y) =>
                x.ErrorCode == y.ErrorCode && x.FilteredBy == y.FilteredBy && x.HandledSuccessfully == y.HandledSuccessfully &&
                x.InvalidRequest == y.InvalidRequest && x.RequestSize == y.RequestSize && x.RequestType == y.RequestType &&
                x.ResponseSize == y.ResponseSize && x.ResponseType == y.ResponseType && x.ScopeId == y.ScopeId &&
                Math.Abs((x.Timestamp - y.Timestamp).TotalSeconds) < 20;

            public int GetHashCode([DisallowNull] DHCPv6PacketHandledEntryDataModel obj) => 2;
        }

        private class DHCPv4PacketHandledEntryDataModelEqualityComparer : IEqualityComparer<DHCPv4PacketHandledEntryDataModel>
        {
            public bool Equals([AllowNull] DHCPv4PacketHandledEntryDataModel x, [AllowNull] DHCPv4PacketHandledEntryDataModel y) =>
                x.ErrorCode == y.ErrorCode && x.FilteredBy == y.FilteredBy && x.HandledSuccessfully == y.HandledSuccessfully &&
                x.InvalidRequest == y.InvalidRequest && x.RequestSize == y.RequestSize && x.RequestType == y.RequestType &&
                x.ResponseSize == y.ResponseSize && x.ResponseType == y.ResponseType && x.ScopeId == y.ScopeId &&
                Math.Abs((x.Timestamp - y.Timestamp).TotalSeconds) < 20;

            public int GetHashCode([DisallowNull] DHCPv4PacketHandledEntryDataModel obj) => 2;
        }

        private static (StorageContext, String) GetContext(Random random)
        {
            String filename = $"{random.Next()}.db";
            var stream = File.Create(filename);
            stream.Dispose();

            DbContextOptionsBuilder<StorageContext> dbContextOptionsBuilder = new DbContextOptionsBuilder<StorageContext>();
            dbContextOptionsBuilder.UseSqlite($"Filename={filename}", options =>
            {
                options.MigrationsAssembly(typeof(StorageContext).Assembly.FullName);
            });

            DbContextOptions<StorageContext> contextOptions = dbContextOptionsBuilder.Options;

            StorageContext context = new StorageContext(contextOptions);
            context.Database.Migrate();
            return (context, filename);
        }

        [Fact]
        public async Task Project_DHCPv6PaketHandledEvents()
        {
            Random random = new Random();

            var preContext = GetContext(random);
            StorageContext context = preContext.Item1;
            try
            {
                DHCPv6Packet requestPacket = DHCPv6Packet.AsOuter(
              new IPv6HeaderInformation(IPv6Address.FromString("fe80::1"), IPv6Address.FromString("fe80::2")), 15482, DHCPv6PacketTypes.RENEW, Array.Empty<DHCPv6PacketOption>());

                DHCPv6Packet responsePacket = DHCPv6Packet.AsOuter(
                    new IPv6HeaderInformation(IPv6Address.FromString("fe80::2"), IPv6Address.FromString("fe80::1")), 15482, DHCPv6PacketTypes.REPLY, new DHCPv6PacketOption[] { new DHCPv6PacketTrueOption(DHCPv6PacketOptionTypes.RapitCommit) });

                Guid scopeId = Guid.NewGuid();

                List<DHCPv6PacketHandledEvent> events = new List<DHCPv6PacketHandledEvent>
            {
                new DHCPv6SolicitHandledEvent(scopeId,requestPacket,responsePacket, true),
                new DHCPv6ReleaseHandledEvent(requestPacket),
            };

                List<DHCPv6PacketHandledEntryDataModel> expectedResults = new List<DHCPv6PacketHandledEntryDataModel>
            {
                new DHCPv6PacketHandledEntryDataModel {
                    ErrorCode = 0,
                    HandledSuccessfully = true,
                    InvalidRequest = false,
                    RequestSize = requestPacket.GetSize(),
                    RequestType = DHCPv6PacketTypes.RENEW,
                    ScopeId = scopeId,
                    ResponseSize = responsePacket.GetSize(),
                    ResponseType = DHCPv6PacketTypes.REPLY,
                    Timestamp = DateTime.UtcNow,
                },
                new DHCPv6PacketHandledEntryDataModel {
                    ErrorCode = (Int32)DHCPv6ReleaseHandledEvent.ReleaseError.ScopeNotFound,
                    HandledSuccessfully = false,
                    InvalidRequest = false,
                    RequestSize = requestPacket.GetSize(),
                    RequestType = DHCPv6PacketTypes.RENEW,
                    ScopeId = null,
                    ResponseSize = null,
                    ResponseType = null,
                    Timestamp = DateTime.UtcNow,
                }
            };

                Boolean actual = await context.Project(events);
                Assert.True(actual);

                List<DHCPv6PacketHandledEntryDataModel> actualEntries = await context.DHCPv6PacketEntries.ToListAsync();

                Assert.Equal(expectedResults, actualEntries, new DHCPv6PacketHandledEntryDataModelEqualityComparer());
            }
            finally
            {
                File.Delete(preContext.Item2);
            }
        }

        [Fact]
        public async Task Project_DHCPv4PaketHandledEvents()
        {
            Random random = new Random();

            var preContext = GetContext(random);
            StorageContext context = preContext.Item1;
            try
            {
                IPv4HeaderInformation headerInformation =
                                 new IPv4HeaderInformation(IPv4Address.FromString("192.178.10.10"), IPv4Address.Broadcast);

                Byte[] opt82Value = random.NextBytes(6);

                DHCPv4Packet requestPacket = new DHCPv4Packet(
               headerInformation, random.NextBytes(6), (UInt32)random.Next(),
               IPv4Address.Empty, IPv4Address.Empty, IPv4Address.Empty,
               new DHCPv4PacketMessageTypeOption(DHCPv4MessagesTypes.Discover),
               new DHCPv4PacketRawByteOption(82, opt82Value));

                DHCPv4Packet releasePacket = new DHCPv4Packet(
                headerInformation, random.NextBytes(6), (UInt32)random.Next(),
                IPv4Address.Empty, IPv4Address.Empty, IPv4Address.Empty,
                new DHCPv4PacketMessageTypeOption(DHCPv4MessagesTypes.Release),
                new DHCPv4PacketRawByteOption(82, opt82Value));

                DHCPv4Packet responsePacket = new DHCPv4Packet(
               new IPv4HeaderInformation(headerInformation.Destionation, headerInformation.Source), random.NextBytes(6), (UInt32)random.Next(),
               IPv4Address.Empty, IPv4Address.Empty, IPv4Address.Empty,
               new DHCPv4PacketMessageTypeOption(DHCPv4MessagesTypes.Offer),
               new DHCPv4PacketRawByteOption(82, opt82Value));

                Guid scopeId = Guid.NewGuid();

                List<DHCPv4PacketHandledEvent> events = new List<DHCPv4PacketHandledEvent>
            {
                new DHCPv4DiscoverHandledEvent(scopeId,requestPacket,responsePacket),
                new DHCPv4ReleaseHandledEvent(releasePacket),
            };

                List<DHCPv4PacketHandledEntryDataModel> expectedResults = new List<DHCPv4PacketHandledEntryDataModel>
            {
                new DHCPv4PacketHandledEntryDataModel {
                    ErrorCode = 0,
                    HandledSuccessfully = true,
                    InvalidRequest = false,
                    RequestSize = requestPacket.GetSize(),
                    RequestType = DHCPv4MessagesTypes.Discover,
                    ScopeId = scopeId,
                    ResponseSize = responsePacket.GetSize(),
                    ResponseType = DHCPv4MessagesTypes.Offer,
                    Timestamp = DateTime.UtcNow,
                },
                new DHCPv4PacketHandledEntryDataModel {
                    ErrorCode = (Int32)DHCPv4ReleaseHandledEvent.ReleaseError.NoLeaseFound,
                    HandledSuccessfully = false,
                    InvalidRequest = false,
                    RequestSize = releasePacket.GetSize(),
                    RequestType = DHCPv4MessagesTypes.Release,
                    ScopeId = null,
                    ResponseSize = null,
                    ResponseType = null,
                    Timestamp = DateTime.UtcNow,
                }
            };

                Boolean actual = await context.Project(events);
                Assert.True(actual);

                List<DHCPv4PacketHandledEntryDataModel> actualEntries = await context.DHCPv4PacketEntries.ToListAsync();

                Assert.Equal(expectedResults, actualEntries, new DHCPv4PacketHandledEntryDataModelEqualityComparer());
            }
            finally
            {
                File.Delete(preContext.Item2);
            }
        }

        [Fact]
        public async Task Project_DHCPv6LeaseCycle()
        {
            Random random = new Random();

            var preContext = GetContext(random);
            StorageContext context = preContext.Item1;

            try
            {
                DHCPv6Packet requestPacket = DHCPv6Packet.AsOuter(
                    new IPv6HeaderInformation(IPv6Address.FromString("fe80::1"), IPv6Address.FromString("fe80::2")), 15482, DHCPv6PacketTypes.RENEW, Array.Empty<DHCPv6PacketOption>());

                DHCPv6Packet responsePacket = DHCPv6Packet.AsOuter(
                    new IPv6HeaderInformation(IPv6Address.FromString("fe80::2"), IPv6Address.FromString("fe80::1")), 15482, DHCPv6PacketTypes.REPLY, new DHCPv6PacketOption[] { new DHCPv6PacketTrueOption(DHCPv6PacketOptionTypes.RapitCommit) });

                Guid scopeId = Guid.NewGuid();
                Guid leaseId = Guid.NewGuid();

                var createdEvent = new DHCPv6LeaseCreatedEvent
                {
                    Address = IPv6Address.FromString("fe80::2"),
                    StartedAt = DateTime.UtcNow.AddHours(-random.Next(3, 10)),
                    ValidUntil = DateTime.UtcNow.AddHours(random.Next(3, 10)),
                    ScopeId = scopeId,
                    EntityId = leaseId,
                    HasPrefixDelegation = true,
                    DelegatedNetworkAddress = IPv6Address.FromString("2000::0"),
                    PrefixLength = 64,
                };
                {
                    Boolean actual = await context.Project(new[] { createdEvent });
                    Assert.True(actual);

                    DHCPv6LeaseEntryDataModel dataModel = await context.DHCPv6LeaseEntries.FirstOrDefaultAsync(x => x.LeaseId == leaseId);

                    CheckLeaseEntry(scopeId, leaseId, dataModel);

                    Assert.Equal(createdEvent.ValidUntil, dataModel.End);
                    Assert.True((DateTime.UtcNow - dataModel.Timestamp).TotalSeconds < 20);

                }
                {
                    DateTime expectedEndTime = DateTime.UtcNow.AddHours(random.Next(50, 100));

                    Boolean actual = await context.Project(new[] {
                        new DHCPv6LeaseRenewedEvent {
                     EntityId = leaseId,
                     ScopeId = scopeId,
                     End = expectedEndTime,
                    } });

                    Assert.True(actual);

                    DHCPv6LeaseEntryDataModel dataModel = await context.DHCPv6LeaseEntries.FirstOrDefaultAsync(x => x.LeaseId == leaseId);
                    CheckLeaseEntry(scopeId, leaseId, dataModel);
                    Assert.Equal(expectedEndTime, dataModel.End);
                }
                {
                    Boolean actual = await context.Project(new[] {
                        new DHCPv6LeasePrefixAddedEvent {
                     EntityId = leaseId,
                     ScopeId = scopeId,
                     NetworkAddress = IPv6Address.FromString("3000::0"),
                     PrefixLength = 62,
                    } });

                    Assert.True(actual);

                    DHCPv6LeaseEntryDataModel dataModel = await context.DHCPv6LeaseEntries.FirstOrDefaultAsync(x => x.LeaseId == leaseId);
                    Assert.Equal(ReasonToEndLease.Nothing, dataModel.EndReason);
                    Assert.Equal("3000::", dataModel.Prefix);
                    Assert.Equal(62, dataModel.PrefixLength);
                }
                {
                    DateTime timesstamp = DateTime.UtcNow.AddHours(random.NextDouble());

                    Boolean actual = await context.Project(new[] {
                        new DHCPv6LeaseRevokedEvent {
                     EntityId = leaseId,
                     ScopeId = scopeId,
                     Timestamp = timesstamp,
                    } });

                    Assert.True(actual);

                    DHCPv6LeaseEntryDataModel dataModel = await context.DHCPv6LeaseEntries.FirstOrDefaultAsync(x => x.LeaseId == leaseId);
                    Assert.Equal(ReasonToEndLease.Revoked, dataModel.EndReason);
                    Assert.Equal(timesstamp, dataModel.End);
                }
                {
                    DateTime timesstamp = DateTime.UtcNow.AddHours(random.NextDouble());

                    Boolean actual = await context.Project(new[] {
                        new DHCPv6LeaseCanceledEvent {
                     EntityId = leaseId,
                     ScopeId = scopeId,
                     Timestamp = timesstamp,
                    } });

                    Assert.True(actual);

                    DHCPv6LeaseEntryDataModel dataModel = await context.DHCPv6LeaseEntries.FirstOrDefaultAsync(x => x.LeaseId == leaseId);
                    Assert.Equal(ReasonToEndLease.Canceled, dataModel.EndReason);
                    Assert.Equal(timesstamp, dataModel.End);
                }
                {
                    DateTime timesstamp = DateTime.UtcNow.AddHours(random.NextDouble());

                    Boolean actual = await context.Project(new[] {
                        new DHCPv6LeaseReleasedEvent {
                     EntityId = leaseId,
                     ScopeId = scopeId,
                     Timestamp = timesstamp,
                    } });

                    Assert.True(actual);

                    DHCPv6LeaseEntryDataModel dataModel = await context.DHCPv6LeaseEntries.FirstOrDefaultAsync(x => x.LeaseId == leaseId);
                    Assert.Equal(ReasonToEndLease.Released, dataModel.EndReason);
                    Assert.Equal(timesstamp, dataModel.End);
                }
                {
                    DateTime timesstamp = DateTime.UtcNow.AddHours(random.NextDouble());

                    Boolean actual = await context.Project(new[] {
                        new DHCPv6LeaseExpiredEvent {
                     EntityId = leaseId,
                     ScopeId = scopeId,
                     Timestamp = timesstamp,
                    } });

                    Assert.True(actual);

                    DHCPv6LeaseEntryDataModel dataModel = await context.DHCPv6LeaseEntries.FirstOrDefaultAsync(x => x.LeaseId == leaseId);
                    Assert.Equal(ReasonToEndLease.Expired, dataModel.EndReason);
                    Assert.Equal(timesstamp, dataModel.End);
                }
                {
                    Boolean nonFoundLease = await context.Project(new[] {
                        new DHCPv6LeaseCanceledEvent {
                     EntityId = Guid.NewGuid(),
                     ScopeId = scopeId,
                    } });

                    Assert.True(nonFoundLease);

                    DHCPv6LeaseEntryDataModel dataModel = await context.DHCPv6LeaseEntries.FirstOrDefaultAsync(x => x.LeaseId == leaseId);
                    Assert.Equal(ReasonToEndLease.Expired, dataModel.EndReason);
                }
            }
            finally
            {
                File.Delete(preContext.Item2);
            }
        }


        [Fact]
        public async Task Project_DHCPv4LeaseCycle()
        {
            Random random = new Random();

            var preContext = GetContext(random);
            StorageContext context = preContext.Item1;

            try
            {
                IPv4HeaderInformation headerInformation =
                   new IPv4HeaderInformation(IPv4Address.FromString("192.178.10.10"), IPv4Address.Broadcast);

                Byte[] opt82Value = random.NextBytes(6);

                DHCPv4Packet requestPacket = new DHCPv4Packet(
               headerInformation, random.NextBytes(6), (UInt32)random.Next(),
               IPv4Address.Empty, IPv4Address.Empty, IPv4Address.Empty,
               new DHCPv4PacketMessageTypeOption(DHCPv4MessagesTypes.Discover),
               new DHCPv4PacketRawByteOption(82, opt82Value));

                DHCPv4Packet responsePacket = new DHCPv4Packet(
               new IPv4HeaderInformation(headerInformation.Destionation, headerInformation.Source), random.NextBytes(6), (UInt32)random.Next(),
               IPv4Address.Empty, IPv4Address.Empty, IPv4Address.Empty,
               new DHCPv4PacketMessageTypeOption(DHCPv4MessagesTypes.Discover),
               new DHCPv4PacketRawByteOption(82, opt82Value));

                Guid scopeId = Guid.NewGuid();
                Guid leaseId = Guid.NewGuid();

                IPv4Address leaseAddress = IPv4Address.FromString("192.178.10.30");
                var createdEvent = new DHCPv4LeaseCreatedEvent
                {
                    Address = leaseAddress,
                    StartedAt = DateTime.UtcNow.AddHours(-random.Next(3, 10)),
                    ValidUntil = DateTime.UtcNow.AddHours(random.Next(3, 10)),
                    ScopeId = scopeId,
                    EntityId = leaseId,
                };
                {
                    Boolean actual = await context.Project(new[] { createdEvent });
                    Assert.True(actual);

                    DHCPv4LeaseEntryDataModel dataModel = await context.DHCPv4LeaseEntries.FirstOrDefaultAsync(x => x.LeaseId == leaseId);

                    CheckLeaseEntry(scopeId, leaseId, dataModel, leaseAddress);

                    Assert.Equal(createdEvent.ValidUntil, dataModel.End);
                    Assert.True((DateTime.UtcNow - dataModel.Timestamp).TotalSeconds < 20);

                }
                {
                    DateTime expectedEndTime = DateTime.UtcNow.AddHours(random.Next(50, 100));

                    Boolean actual = await context.Project(new[] {
                        new DHCPv4LeaseRenewedEvent {
                     EntityId = leaseId,
                     ScopeId = scopeId,
                     End = expectedEndTime,
                    } });

                    Assert.True(actual);

                    DHCPv4LeaseEntryDataModel dataModel = await context.DHCPv4LeaseEntries.FirstOrDefaultAsync(x => x.LeaseId == leaseId);
                    CheckLeaseEntry(scopeId, leaseId, dataModel, leaseAddress);
                    Assert.Equal(expectedEndTime, dataModel.End);
                }
                {
                    DateTime timesstamp = DateTime.UtcNow.AddHours(random.NextDouble());

                    Boolean actual = await context.Project(new[] {
                        new DHCPv4LeaseRevokedEvent {
                     EntityId = leaseId,
                     ScopeId = scopeId,
                     Timestamp = timesstamp,
                    } });

                    Assert.True(actual);

                    DHCPv4LeaseEntryDataModel dataModel = await context.DHCPv4LeaseEntries.FirstOrDefaultAsync(x => x.LeaseId == leaseId);
                    Assert.Equal(ReasonToEndLease.Revoked, dataModel.EndReason);
                    Assert.Equal(timesstamp, dataModel.End);
                }
                {
                    DateTime timesstamp = DateTime.UtcNow.AddHours(random.NextDouble());

                    Boolean actual = await context.Project(new[] {
                        new DHCPv4LeaseCanceledEvent {
                     EntityId = leaseId,
                     ScopeId = scopeId,
                     Timestamp = timesstamp,
                    } });

                    Assert.True(actual);

                    DHCPv4LeaseEntryDataModel dataModel = await context.DHCPv4LeaseEntries.FirstOrDefaultAsync(x => x.LeaseId == leaseId);
                    Assert.Equal(ReasonToEndLease.Canceled, dataModel.EndReason);
                    Assert.Equal(timesstamp, dataModel.End);
                }
                {
                    DateTime timesstamp = DateTime.UtcNow.AddHours(random.NextDouble());

                    Boolean actual = await context.Project(new[] {
                        new DHCPv4LeaseReleasedEvent {
                     EntityId = leaseId,
                     ScopeId = scopeId,
                     Timestamp = timesstamp,
                    } });

                    Assert.True(actual);

                    DHCPv4LeaseEntryDataModel dataModel = await context.DHCPv4LeaseEntries.FirstOrDefaultAsync(x => x.LeaseId == leaseId);
                    Assert.Equal(ReasonToEndLease.Released, dataModel.EndReason);
                    Assert.Equal(timesstamp, dataModel.End);
                }
                {
                    DateTime timesstamp = DateTime.UtcNow.AddHours(random.NextDouble());

                    Boolean actual = await context.Project(new[] {
                        new DHCPv4LeaseExpiredEvent {
                     EntityId = leaseId,
                     ScopeId = scopeId,
                     Timestamp = timesstamp,
                    } });

                    Assert.True(actual);

                    DHCPv4LeaseEntryDataModel dataModel = await context.DHCPv4LeaseEntries.FirstOrDefaultAsync(x => x.LeaseId == leaseId);
                    Assert.Equal(ReasonToEndLease.Expired, dataModel.EndReason);
                    Assert.Equal(timesstamp, dataModel.End);
                }
                {
                    Boolean nonFoundLease = await context.Project(new[] {
                        new DHCPv4LeaseCanceledEvent {
                     EntityId = Guid.NewGuid(),
                     ScopeId = scopeId,
                    } });

                    Assert.True(nonFoundLease);

                    DHCPv4LeaseEntryDataModel dataModel = await context.DHCPv4LeaseEntries.FirstOrDefaultAsync(x => x.LeaseId == leaseId);
                    Assert.Equal(ReasonToEndLease.Expired, dataModel.EndReason);
                }
            }
            finally
            {
                File.Delete(preContext.Item2);
            }
        }

        private static void CheckLeaseEntry(Guid scopeId, Guid leaseId, DHCPv6LeaseEntryDataModel dataModel)
        {
            Assert.NotNull(dataModel);

            Assert.NotEqual(Guid.Empty, dataModel.Id);
            Assert.Equal(leaseId, dataModel.LeaseId);
            Assert.Equal(scopeId, dataModel.ScopeId);
            Assert.Equal("fe80::2", dataModel.Address);
            Assert.Equal(ReasonToEndLease.Nothing, dataModel.EndReason);
            Assert.Equal("2000::", dataModel.Prefix);
            Assert.Equal(64, dataModel.PrefixLength);
        }

        private static void CheckLeaseEntry(Guid scopeId, Guid leaseId, DHCPv4LeaseEntryDataModel dataModel, IPv4Address address)
        {
            Assert.NotNull(dataModel);

            Assert.NotEqual(Guid.Empty, dataModel.Id);
            Assert.Equal(leaseId, dataModel.LeaseId);
            Assert.Equal(scopeId, dataModel.ScopeId);
            Assert.Equal(address.ToString(), dataModel.Address);
            Assert.Equal(ReasonToEndLease.Nothing, dataModel.EndReason);
        }

        [Fact]
        public async Task GetIncomingPacketTypes()
        {
            Random random = new Random();

            var preContext = GetContext(random);
            StorageContext context = preContext.Item1;
            try
            {
                Int32 elementAmount = random.Next(100, 1000);
                DateTime start = DateTime.UtcNow.AddDays(-100);
                DateTime end = DateTime.UtcNow.AddDays(-10);

                Dictionary<DateTime, IDictionary<DHCPv6PacketTypes, Int32>> expectedDailyDicitionart = new Dictionary<DateTime, IDictionary<DHCPv6PacketTypes, int>>();

                TimeSpan diff = end - start;

                List<DHCPv6PacketHandledEntryDataModel> seed = new List<DHCPv6PacketHandledEntryDataModel>();
                for (int i = 0; i < elementAmount; i++)
                {
                    var entry = new DHCPv6PacketHandledEntryDataModel
                    {
                        ErrorCode = 0,
                        HandledSuccessfully = true,
                        InvalidRequest = false,
                        RequestSize = (UInt16)random.Next(0, 1024),
                        RequestType = random.GetEnumValue<DHCPv6PacketTypes>(),
                        ScopeId = Guid.NewGuid(),
                        ResponseSize = (UInt16)random.Next(0, 1024),
                        ResponseType = random.GetEnumValue<DHCPv6PacketTypes>(),
                        Timestamp = DateTime.UtcNow,
                    };

                    if (random.NextDouble() > 0.5)
                    {
                        entry.Timestamp = start.AddSeconds(random.Next(10, (Int32)diff.TotalSeconds));

                        expectedDailyDicitionart.AddIfNotExisting(entry.Timestamp.Date, new Dictionary<DHCPv6PacketTypes, int>());

                        expectedDailyDicitionart[entry.Timestamp.Date].AddIfNotExisting(entry.RequestType, 0);

                        expectedDailyDicitionart[entry.Timestamp.Date][entry.RequestType] += 1;
                    }

                    entry.SetTimestampDates();
                    seed.Add(entry);
                }

                context.AddRange(seed);
                await context.SaveChangesAsync();

                var actual = await context.GetIncomingDHCPv6PacketTypes(start, end, GroupStatisticsResultBy.Day);

                Assert.NotNull(actual);
                Assert.Equal(expectedDailyDicitionart, actual, new NonStrictNestedDictionaryComparer<DateTime, DHCPv6PacketTypes, Int32>());
            }
            finally
            {
                File.Delete(preContext.Item2);
            }
        }

        [Fact]
        public async Task GetErrorPackets()
        {
            Random random = new Random();

            var preContext = GetContext(random);
            StorageContext context = preContext.Item1;
            try
            {
                Int32 elementAmount = random.Next(100, 1000);
                DateTime start = DateTime.UtcNow.AddDays(-100);
                DateTime end = DateTime.UtcNow.AddDays(-10);

                Dictionary<DateTime, Int32> expectedDailyDicitionart = new Dictionary<DateTime, int>();

                TimeSpan diff = end - start;

                List<DHCPv6PacketHandledEntryDataModel> seed = new List<DHCPv6PacketHandledEntryDataModel>();
                for (int i = 0; i < elementAmount; i++)
                {
                    var entry = new DHCPv6PacketHandledEntryDataModel
                    {
                        HandledSuccessfully = true,
                        InvalidRequest = false,
                        RequestSize = (UInt16)random.Next(0, 1024),
                        RequestType = random.GetEnumValue<DHCPv6PacketTypes>(),
                        ScopeId = Guid.NewGuid(),
                        ResponseSize = (UInt16)random.Next(0, 1024),
                        ResponseType = random.GetEnumValue<DHCPv6PacketTypes>(),
                        Timestamp = DateTime.UtcNow,
                    };

                    if (random.NextDouble() > 0.5)
                    {
                        entry.Timestamp = start.AddSeconds(random.Next(10, (Int32)diff.TotalSeconds));

                        if (random.NextBoolean() == true)
                        {
                            entry.InvalidRequest = true;

                            expectedDailyDicitionart.AddIfNotExisting(entry.Timestamp.Date, 0);
                            expectedDailyDicitionart[entry.Timestamp.Date] += 1;
                        }
                    }

                    entry.SetTimestampDates();
                    seed.Add(entry);
                }

                context.AddRange(seed);
                await context.SaveChangesAsync();

                var actual = await context.GetErrorDHCPv6Packets(start, end, GroupStatisticsResultBy.Day);

                Assert.NotNull(actual);
                Assert.Equal(expectedDailyDicitionart, actual, new NonStrictDictionaryComparer<DateTime, Int32>());
            }
            finally
            {
                File.Delete(preContext.Item2);
            }
        }

        [Fact]
        public async Task GetFileredPackets()
        {
            Random random = new Random();

            var preContext = GetContext(random);
            StorageContext context = preContext.Item1;
            try
            {
                Int32 elementAmount = random.Next(100, 1000);
                DateTime start = DateTime.UtcNow.AddDays(-100);
                DateTime end = DateTime.UtcNow.AddDays(-10);

                Dictionary<DateTime, Int32> expectedDailyDicitionart = new Dictionary<DateTime, int>();

                TimeSpan diff = end - start;

                List<DHCPv6PacketHandledEntryDataModel> seed = new List<DHCPv6PacketHandledEntryDataModel>();
                for (int i = 0; i < elementAmount; i++)
                {
                    var entry = new DHCPv6PacketHandledEntryDataModel
                    {
                        HandledSuccessfully = true,
                        InvalidRequest = false,
                        RequestSize = (UInt16)random.Next(0, 1024),
                        RequestType = random.GetEnumValue<DHCPv6PacketTypes>(),
                        ScopeId = Guid.NewGuid(),
                        ResponseSize = (UInt16)random.Next(0, 1024),
                        ResponseType = random.GetEnumValue<DHCPv6PacketTypes>(),
                        Timestamp = DateTime.UtcNow,
                    };

                    if (random.NextDouble() > 0.5)
                    {
                        entry.Timestamp = start.AddSeconds(random.Next(10, (Int32)diff.TotalSeconds));

                        if (random.NextBoolean() == true)
                        {
                            entry.FilteredBy = random.GetAlphanumericString();

                            expectedDailyDicitionart.AddIfNotExisting(entry.Timestamp.Date, 0);
                            expectedDailyDicitionart[entry.Timestamp.Date] += 1;
                        }
                    }

                    entry.SetTimestampDates();
                    seed.Add(entry);
                }

                context.AddRange(seed);
                await context.SaveChangesAsync();

                var actual = await context.GetFileredDHCPv6Packets(start, end, GroupStatisticsResultBy.Day);

                Assert.NotNull(actual);
                Assert.Equal(expectedDailyDicitionart, actual, new NonStrictDictionaryComparer<DateTime, Int32>());
            }
            finally
            {
                File.Delete(preContext.Item2);
            }
        }

        [Fact]
        public async Task GetErrorCodesPerRequest()
        {
            Random random = new Random();

            var preContext = GetContext(random);
            StorageContext context = preContext.Item1;
            try
            {
                Int32 elementAmount = random.Next(100, 1000);
                DateTime start = DateTime.UtcNow.AddDays(-100);
                DateTime end = DateTime.UtcNow.AddDays(-10);

                Dictionary<DHCPv6PacketTypes, Dictionary<Int32, Int32>> expectedDailyDicitionart = new Dictionary<DHCPv6PacketTypes, Dictionary<Int32, int>>();

                TimeSpan diff = end - start;

                List<DHCPv6PacketHandledEntryDataModel> seed = new List<DHCPv6PacketHandledEntryDataModel>();
                for (int i = 0; i < elementAmount; i++)
                {
                    var entry = new DHCPv6PacketHandledEntryDataModel
                    {
                        HandledSuccessfully = true,
                        InvalidRequest = false,
                        RequestSize = (UInt16)random.Next(0, 1024),
                        RequestType = random.GetEnumValue<DHCPv6PacketTypes>(),
                        ScopeId = Guid.NewGuid(),
                        ResponseSize = (UInt16)random.Next(0, 1024),
                        ResponseType = random.GetEnumValue<DHCPv6PacketTypes>(),
                        Timestamp = DateTime.UtcNow,
                    };

                    if (random.NextDouble() > 0.25)
                    {
                        entry.Timestamp = start.AddSeconds(random.Next(10, (Int32)diff.TotalSeconds));

                        entry.ErrorCode = random.Next(0, 5);


                        expectedDailyDicitionart.AddIfNotExisting(entry.RequestType, new Dictionary<Int32, int>());
                        var element = expectedDailyDicitionart[entry.RequestType];
                        element.AddIfNotExisting(entry.ErrorCode, 0);

                        element[entry.ErrorCode] += 1;
                    }

                    seed.Add(entry);
                }

                context.AddRange(seed);
                await context.SaveChangesAsync();

                foreach (var item in expectedDailyDicitionart)
                {
                    var actual = await context.GetErrorCodesPerDHCPV6RequestType(start, end, item.Key);

                    Assert.Equal(item.Value, actual, new NonStrictDictionaryComparer<Int32, Int32>());
                }
            }
            finally
            {
                File.Delete(preContext.Item2);
            }
        }

        [Fact]
        public async Task GetActiveLeases_NoLeasesSaved()
        {
            Random random = new Random();

            var preContext = GetContext(random);
            StorageContext context = preContext.Item1;
            try
            {
                var actual = await context.GetActiveDHCPv6Leases(null, null, GroupStatisticsResultBy.Day);
                Assert.Empty(actual);
            }
            finally
            {
                File.Delete(preContext.Item2);
            }
        }

        [Fact]
        public async Task GetActiveLeases_NoLeasesInTimespan()
        {
            Random random = new Random();

            var preContext = GetContext(random);
            StorageContext context = preContext.Item1;
            try
            {
                Int32 elementAmount = random.Next(100, 1000);
                DateTime start = DateTime.UtcNow.AddDays(-100);
                DateTime end = DateTime.UtcNow.AddDays(-10);

                TimeSpan diff = end - start;

                List<DHCPv6LeaseEntryDataModel> seed = new List<DHCPv6LeaseEntryDataModel>
                {
                    new DHCPv6LeaseEntryDataModel
                    {
                        Address = random.GetIPv6Address().ToString(),
                        Id = random.NextGuid(),
                        EndReason = random.GetEnumValue<ReasonToEndLease>(),
                        LeaseId = random.NextGuid(),
                        Prefix = random.GetIPv6Addresses().ToString(),
                        PrefixLength = random.NextByte(),
                        Timestamp = DateTime.UtcNow,
                        Start = start.AddDays(-10),
                        End = start.AddDays(-4),
                    },
                    new DHCPv6LeaseEntryDataModel
                    {
                        Address = random.GetIPv6Address().ToString(),
                        Id = random.NextGuid(),
                        EndReason = random.GetEnumValue<ReasonToEndLease>(),
                        LeaseId = random.NextGuid(),
                        Prefix = random.GetIPv6Addresses().ToString(),
                        PrefixLength = random.NextByte(),
                        Timestamp = DateTime.UtcNow,
                        Start = end.AddDays(4),
                        End = end.AddDays(10),
                    }
                };

                context.DHCPv6LeaseEntries.AddRange(seed);
                await context.SaveChangesAsync();

                var actual = await context.GetActiveDHCPv6Leases(start, end, GroupStatisticsResultBy.Day);
                Assert.Empty(actual);
            }
            finally
            {
                File.Delete(preContext.Item2);
            }
        }


        [Fact]
        public async Task GetActiveLeases_GroupedByDay()
        {
            Random random = new Random();

            var preContext = GetContext(random);
            StorageContext context = preContext.Item1;
            try
            {
                DateTime start = new DateTime(2020, 08, 20).AddSeconds(random.Next(30, 1000));
                DateTime end = start.AddDays(4);

                Dictionary<DateTime, Int32> expectedResults = new Dictionary<DateTime, int>
                {
                    { start,2 },
                    { start.Date.AddDays(1),1  },
                    { start.Date.AddDays(2),2  },
                    { start.Date.AddDays(3),2  },
                    { start.Date.AddDays(4),2  },
                };

                List<DHCPv6LeaseEntryDataModel> seed = new List<DHCPv6LeaseEntryDataModel>
                {
                    new DHCPv6LeaseEntryDataModel
                    {
                        Address = random.GetIPv6Address().ToString(),
                        Id = random.NextGuid(),
                        EndReason = random.GetEnumValue<ReasonToEndLease>(),
                        LeaseId = random.NextGuid(),
                        Prefix = random.GetIPv6Addresses().ToString(),
                        PrefixLength = random.NextByte(),
                        Timestamp = DateTime.UtcNow,
                        Start = start.AddDays(-1),
                        End = start.AddSeconds(10),
                    },
                    new DHCPv6LeaseEntryDataModel
                    {
                        Address = random.GetIPv6Address().ToString(),
                        Id = random.NextGuid(),
                        EndReason = random.GetEnumValue<ReasonToEndLease>(),
                        LeaseId = random.NextGuid(),
                        Prefix = random.GetIPv6Addresses().ToString(),
                        PrefixLength = random.NextByte(),
                        Timestamp = DateTime.UtcNow,
                        Start = start.AddDays(-1),
                        End = end.AddSeconds(10),
                    },
                    new DHCPv6LeaseEntryDataModel
                    {
                        Address = random.GetIPv6Address().ToString(),
                        Id = random.NextGuid(),
                        EndReason = random.GetEnumValue<ReasonToEndLease>(),
                        LeaseId = random.NextGuid(),
                        Prefix = random.GetIPv6Addresses().ToString(),
                        PrefixLength = random.NextByte(),
                        Timestamp = DateTime.UtcNow,
                        Start = start.Date.AddDays(2).AddSeconds(random.Next(30, 3600)),
                        End = start.Date.AddDays(2).AddHours(random.Next(2, 22)),
                    },
                    new DHCPv6LeaseEntryDataModel
                    {
                        Address = random.GetIPv6Address().ToString(),
                        Id = random.NextGuid(),
                        EndReason = random.GetEnumValue<ReasonToEndLease>(),
                        LeaseId = random.NextGuid(),
                        Prefix = random.GetIPv6Addresses().ToString(),
                        PrefixLength = random.NextByte(),
                        Timestamp = DateTime.UtcNow,
                        Start = start.Date.AddDays(3).AddSeconds(random.Next(30, 3600)),
                        End = start.Date.AddDays(3).AddDays(random.Next(2, 22)),
                    }
                };

                context.DHCPv6LeaseEntries.AddRange(seed);
                await context.SaveChangesAsync();

                var actual = await context.GetActiveDHCPv6Leases(start, end, GroupStatisticsResultBy.Day);
                Assert.Equal(expectedResults, actual, new NonStrictDictionaryComparer<DateTime, Int32>());
            }
            finally
            {
                File.Delete(preContext.Item2);
            }
        }

        [Fact]
        public async Task GetActiveLeases_GroupedByWeek()
        {
            Random random = new Random();

            var preContext = GetContext(random);
            StorageContext context = preContext.Item1;
            try
            {
                DateTime start = new DateTime(2020, 08, 20).AddSeconds(random.Next(30, 1000));
                DateTime end = start.AddDays(7 * 4);

                Dictionary<DateTime, Int32> expectedResults = new Dictionary<DateTime, int>
                {
                    { start,2 }, // entirly + before
                    { new DateTime(2020, 08, 24),1  }, // entirly 
                    { new DateTime(2020, 08, 24).AddDays(7),2  }, // entirely + second element
                    { new DateTime(2020, 08, 24).AddDays(2*7),2  }, // entirely + start in third window
                    { new DateTime(2020, 08, 24).AddDays(3*7),2  },
                };

                List<DHCPv6LeaseEntryDataModel> seed = new List<DHCPv6LeaseEntryDataModel>
                {
                    //starting before start
                    new DHCPv6LeaseEntryDataModel
                    {
                        Address = random.GetIPv6Address().ToString(),
                        Id = random.NextGuid(),
                        EndReason = random.GetEnumValue<ReasonToEndLease>(),
                        LeaseId = random.NextGuid(),
                        Prefix = random.GetIPv6Addresses().ToString(),
                        PrefixLength = random.NextByte(),
                        Timestamp = DateTime.UtcNow,
                        Start = start.AddDays(-1),
                        End = start.AddSeconds(10),
                    },
                    // entireliy in the range 
                    new DHCPv6LeaseEntryDataModel
                    {
                        Address = random.GetIPv6Address().ToString(),
                        Id = random.NextGuid(),
                        EndReason = random.GetEnumValue<ReasonToEndLease>(),
                        LeaseId = random.NextGuid(),
                        Prefix = random.GetIPv6Addresses().ToString(),
                        PrefixLength = random.NextByte(),
                        Timestamp = DateTime.UtcNow,
                        Start = start.AddDays(-1),
                        End = end.AddSeconds(10),
                    },
                    // only in the second element
                    new DHCPv6LeaseEntryDataModel
                    {
                        Address = random.GetIPv6Address().ToString(),
                        Id = random.NextGuid(),
                        EndReason = random.GetEnumValue<ReasonToEndLease>(),
                        LeaseId = random.NextGuid(),
                        Prefix = random.GetIPv6Addresses().ToString(),
                        PrefixLength = random.NextByte(),
                        Timestamp = DateTime.UtcNow,
                        Start = start.Date.AddDays(7 + 6).AddSeconds(random.Next(30, 3600)),
                        End = start.Date.AddDays(7 + 6 + 4).AddHours(random.Next(2, 22)),
                    },
                    //starting at the third and ending after end
                    new DHCPv6LeaseEntryDataModel
                    {
                        Address = random.GetIPv6Address().ToString(),
                        Id = random.NextGuid(),
                        EndReason = random.GetEnumValue<ReasonToEndLease>(),
                        LeaseId = random.NextGuid(),
                        Prefix = random.GetIPv6Addresses().ToString(),
                        PrefixLength = random.NextByte(),
                        Timestamp = DateTime.UtcNow,
                        Start = start.Date.AddDays(3 * 7).AddSeconds(random.Next(30, 3600)),
                        End = start.Date.AddDays(3 * 7 + 6).AddDays(random.Next(2, 22)),
                    }
                };

                context.DHCPv6LeaseEntries.AddRange(seed);
                await context.SaveChangesAsync();

                var actual = await context.GetActiveDHCPv6Leases(start, end, GroupStatisticsResultBy.Week);
                Assert.Equal(expectedResults, actual, new NonStrictDictionaryComparer<DateTime, Int32>());
            }
            finally
            {
                File.Delete(preContext.Item2);
            }
        }

        [Fact]
        public async Task GetActiveLeases_GroupedByMonth()
        {
            Random random = new Random();

            var preContext = GetContext(random);
            StorageContext context = preContext.Item1;
            try
            {
                DateTime start = new DateTime(2020, 08, 20).AddSeconds(random.Next(30, 1000));
                DateTime end = start.AddMonths(4);

                Dictionary<DateTime, Int32> expectedResults = new Dictionary<DateTime, int>
                {
                    { start,2 },
                    { new DateTime(2020, 08, 1).AddMonths(1),1  },
                    { new DateTime(2020, 08, 1).AddMonths(2),2  },
                    { new DateTime(2020, 08, 1).AddMonths(3),2  },
                    { new DateTime(2020, 08, 1).AddMonths(4),2  },
                };

                List<DHCPv6LeaseEntryDataModel> seed = new List<DHCPv6LeaseEntryDataModel>
                {
                    new DHCPv6LeaseEntryDataModel
                    {
                        Address = random.GetIPv6Address().ToString(),
                        Id = random.NextGuid(),
                        EndReason = random.GetEnumValue<ReasonToEndLease>(),
                        LeaseId = random.NextGuid(),
                        Prefix = random.GetIPv6Addresses().ToString(),
                        PrefixLength = random.NextByte(),
                        Timestamp = DateTime.UtcNow,
                        Start = start.AddMonths(-1),
                        End = start.AddSeconds(10),
                    },
                    new DHCPv6LeaseEntryDataModel
                    {
                        Address = random.GetIPv6Address().ToString(),
                        Id = random.NextGuid(),
                        EndReason = random.GetEnumValue<ReasonToEndLease>(),
                        LeaseId = random.NextGuid(),
                        Prefix = random.GetIPv6Addresses().ToString(),
                        PrefixLength = random.NextByte(),
                        Timestamp = DateTime.UtcNow,
                        Start = start.AddMonths(-1),
                        End = end.AddSeconds(10),
                    },
                    new DHCPv6LeaseEntryDataModel
                    {
                        Address = random.GetIPv6Address().ToString(),
                        Id = random.NextGuid(),
                        EndReason = random.GetEnumValue<ReasonToEndLease>(),
                        LeaseId = random.NextGuid(),
                        Prefix = random.GetIPv6Addresses().ToString(),
                        PrefixLength = random.NextByte(),
                        Timestamp = DateTime.UtcNow,
                        Start = start.Date.AddMonths(2).AddSeconds(random.Next(30, 3600)),
                        End = start.Date.AddMonths(2).AddHours(random.Next(2, 22)),
                    },
                    new DHCPv6LeaseEntryDataModel
                    {
                        Address = random.GetIPv6Address().ToString(),
                        Id = random.NextGuid(),
                        EndReason = random.GetEnumValue<ReasonToEndLease>(),
                        LeaseId = random.NextGuid(),
                        Prefix = random.GetIPv6Addresses().ToString(),
                        PrefixLength = random.NextByte(),
                        Timestamp = DateTime.UtcNow,
                        Start = start.Date.AddMonths(3).AddSeconds(random.Next(30, 3600)),
                        End = start.Date.AddMonths(3).AddMonths(random.Next(2, 22)),
                    }
                };

                context.DHCPv6LeaseEntries.AddRange(seed);
                await context.SaveChangesAsync();

                var actual = await context.GetActiveDHCPv6Leases(start, end, GroupStatisticsResultBy.Month);
                Assert.Equal(expectedResults, actual, new NonStrictDictionaryComparer<DateTime, Int32>());
            }
            finally
            {
                File.Delete(preContext.Item2);
            }
        }

        [Fact]
        public async Task GetIncomingDHCPv6PacketAmount()
        {
            Random random = new Random();

            var preContext = GetContext(random);
            StorageContext context = preContext.Item1;
            try
            {
                Int32 elementAmount = random.Next(100, 1000);
                DateTime start = DateTime.UtcNow.AddDays(-100);
                DateTime end = DateTime.UtcNow.AddDays(-10);

                Dictionary<DateTime, Int32> expectedDailyDicitionart = new Dictionary<DateTime, int>();

                TimeSpan diff = end - start;

                List<DHCPv6PacketHandledEntryDataModel> seed = new List<DHCPv6PacketHandledEntryDataModel>();
                for (int i = 0; i < elementAmount; i++)
                {
                    var entry = new DHCPv6PacketHandledEntryDataModel
                    {
                        HandledSuccessfully = true,
                        InvalidRequest = false,
                        RequestSize = (UInt16)random.Next(0, 1024),
                        RequestType = random.GetEnumValue<DHCPv6PacketTypes>(),
                        ScopeId = Guid.NewGuid(),
                        ResponseSize = (UInt16)random.Next(0, 1024),
                        ResponseType = random.GetEnumValue<DHCPv6PacketTypes>(),
                        Timestamp = DateTime.UtcNow,
                    };

                    if (random.NextDouble() > 0.5)
                    {
                        entry.Timestamp = start.AddSeconds(random.Next(10, (Int32)diff.TotalSeconds));

                        if (random.NextBoolean() == true)
                        {
                            entry.InvalidRequest = true;
                        }
                        else
                        {
                            if (random.NextBoolean() == true)
                            {
                                entry.FilteredBy = random.GetAlphanumericString();
                            }
                        }

                        expectedDailyDicitionart.AddIfNotExisting(entry.Timestamp.Date, 0);
                        expectedDailyDicitionart[entry.Timestamp.Date] += 1;
                    }

                    entry.SetTimestampDates();
                    seed.Add(entry);
                }

                context.AddRange(seed);
                await context.SaveChangesAsync();

                var actual = await context.GetIncomingDHCPv6PacketAmount(start, end, GroupStatisticsResultBy.Day);

                Assert.NotNull(actual);
                Assert.Equal(expectedDailyDicitionart, actual, new NonStrictDictionaryComparer<DateTime, Int32>());
            }
            finally
            {
                File.Delete(preContext.Item2);
            }
        }

        [Fact]
        public async Task GetHandledDHCPv6PacketByScopeId()
        {
            Random random = new Random();

            var preContext = GetContext(random);
            StorageContext context = preContext.Item1;

            Guid id = random.NextGuid();
            Int32 amount = 4;

            DHCPv6Packet requestPacket = DHCPv6Packet.AsOuter(
          new IPv6HeaderInformation(IPv6Address.FromString("fe80::1"), IPv6Address.FromString("fe80::2")), 15482, DHCPv6PacketTypes.RENEW, Array.Empty<DHCPv6PacketOption>());

            DHCPv6Packet responsePacket = DHCPv6Packet.AsOuter(
                new IPv6HeaderInformation(IPv6Address.FromString("fe80::2"), IPv6Address.FromString("fe80::1")), 15482, DHCPv6PacketTypes.REPLY, new DHCPv6PacketOption[] { new DHCPv6PacketTrueOption(DHCPv6PacketOptionTypes.RapitCommit) });

            DHCPv6SolicitHandledEvent firstHandledEventBelongingToScope = new DHCPv6SolicitHandledEvent
            {
                IsRapitCommit = random.NextBoolean(),
                WasSuccessfullHandled = false,
                Error = DHCPv6SolicitHandledEvent.SolicitErros.LeaseNotActive,
                ScopeId = id,
                Request = requestPacket,
            };

            DHCPv6SolicitHandledEvent firstHandledEventBelongingNotToScope = new DHCPv6SolicitHandledEvent
            {
                IsRapitCommit = random.NextBoolean(),
                WasSuccessfullHandled = false,
                Error = DHCPv6SolicitHandledEvent.SolicitErros.LeaseNotActive,
                ScopeId = random.NextGuid(),
                Request = requestPacket,
            };

            DHCPv6ReleaseHandledEvent secondHandledEventBelongingToScope = new DHCPv6ReleaseHandledEvent
            {
                WasSuccessfullHandled = true,
                Error = DHCPv6ReleaseHandledEvent.ReleaseError.NoError,
                ScopeId = id,
                Request = requestPacket,
                Response = responsePacket,
            };

            DHCPv6ReleaseHandledEvent secondHandledEventBelongingNotToScope = new DHCPv6ReleaseHandledEvent
            {
                WasSuccessfullHandled = true,
                Error = DHCPv6ReleaseHandledEvent.ReleaseError.NoError,
                ScopeId = random.NextGuid(),
                Request = requestPacket,
                Response = responsePacket,
            };

            try
            {
                var mockedAggregateRoot = new MockedAggregateRoot(new DomainEvent[]{
                    firstHandledEventBelongingToScope,
                    firstHandledEventBelongingNotToScope,
                    secondHandledEventBelongingToScope,
                    secondHandledEventBelongingNotToScope,
                });

                await context.Save(mockedAggregateRoot);

                var actual = await context.GetHandledDHCPv6PacketByScopeId(id, amount);

                Assert.NotNull(actual);
                Assert.Equal(2, actual.Count());

                {
                    var element = actual.ElementAt(1);
                    Assert.Equal(id, element.ScopeId);
                    Assert.Equal(firstHandledEventBelongingToScope.Timestamp, element.Timestamp);
                    Assert.Equal(firstHandledEventBelongingToScope.Request.GetAsStream(), element.Request.Content);
                    Assert.Equal(DHCPv6PacketTypes.Solicit, element.RequestType);
                }
                {
                    var element = actual.ElementAt(0);
                    Assert.Equal(id, element.ScopeId);
                    Assert.Equal(secondHandledEventBelongingToScope.Timestamp, element.Timestamp);
                    Assert.Equal(secondHandledEventBelongingToScope.Request.GetAsStream(), element.Request.Content);
                    Assert.Equal(secondHandledEventBelongingToScope.Response.GetAsStream(), element.Response.Content);
                    Assert.Equal(DHCPv6PacketTypes.RELEASE, element.RequestType);
                }
            }
            finally
            {
                File.Delete(preContext.Item2);
            }
        }
    }
}
