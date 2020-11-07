using DaAPI.Core.Common;
using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Packets.DHCPv6;
using DaAPI.Core.Scopes.DHCPv6;
using DaAPI.Infrastructure.StorageEngine.Converters;
using DaAPI.TestHelper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using static DaAPI.Core.Scopes.DHCPv6.DHCPv6LeaseEvents;
using static DaAPI.Core.Scopes.DHCPv6.DHCPv6PacketHandledEvents;
using static DaAPI.Core.Scopes.DHCPv6.DHCPv6ScopeEvents;

namespace DaAPI.UnitTests.Infrastructure.StorageEngine.Converters
{
    public class JsonEventSerializerTester
    {
        private JsonSerializerSettings GetSettings()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Converters.Add(new DUIDJsonConverter());
            settings.Converters.Add(new IPv6AddressJsonConverter());
            settings.Converters.Add(new DHCPv6PacketJsonConverter());
            settings.Converters.Add(new DHCPv6ScopeAddressPropertiesConverter());
            settings.Converters.Add(new IPv6HeaderInformationJsonConverter());

            return settings;
        }

        private T SerializeAndDeserialze<T>(T input)
        {
            var settings = GetSettings();
            String serilizedValue = JsonConvert.SerializeObject(input, settings);

            T output = JsonConvert.DeserializeObject<T>(serilizedValue, settings);
            return output;
        }

        [Fact]
        public void DHCPv6AddressSuspendedEvent()
        {
            Random random = new Random();

            DHCPv6AddressSuspendedEvent expected = new DHCPv6AddressSuspendedEvent
            {
                Address = random.GetIPv6Address(),
                EntityId = random.NextGuid(),
                ScopeId = random.NextGuid(),
                SuspendedTill = DateTime.UtcNow.AddDays(0.5),
                Timestamp = DateTime.UtcNow,
            };

            var actual = SerializeAndDeserialze(expected);

            Assert.Equal(expected.Address, actual.Address);
            Assert.Equal(expected.EntityId, actual.EntityId);
            Assert.Equal(expected.ScopeId, actual.ScopeId);
            Assert.Equal(expected.SuspendedTill, actual.SuspendedTill);
            Assert.Equal(expected.Timestamp, actual.Timestamp);
        }

        [Fact]
        public void DHCPv6LeaseActivatedEvent()
        {
            Random random = new Random();

            DHCPv6LeaseActivatedEvent expected = new DHCPv6LeaseActivatedEvent
            {
                EntityId = random.NextGuid(),
                ScopeId = random.NextGuid(),
                Timestamp = DateTime.UtcNow,
            };

            var actual = SerializeAndDeserialze(expected);

            Assert.Equal(expected.EntityId, actual.EntityId);
            Assert.Equal(expected.ScopeId, actual.ScopeId);
            Assert.Equal(expected.Timestamp, actual.Timestamp);
        }

        [Fact]
        public void DHCPv6LeaseCanceledEvent()
        {
            Random random = new Random();

            DHCPv6LeaseCanceledEvent expected = new DHCPv6LeaseCanceledEvent
            {
                EntityId = random.NextGuid(),
                ScopeId = random.NextGuid(),
                Timestamp = DateTime.UtcNow,
                Reason = DaAPI.Core.Scopes.LeaseCancelReasons.ResolverChanged,
            };

            var actual = SerializeAndDeserialze(expected);

            Assert.Equal(expected.EntityId, actual.EntityId);
            Assert.Equal(expected.ScopeId, actual.ScopeId);
            Assert.Equal(expected.Timestamp, actual.Timestamp);
            Assert.Equal(expected.Reason, actual.Reason);
        }

        [Fact]
        public void DHCPv6LeaseExpiredEvent()
        {
            Random random = new Random();

            DHCPv6LeaseExpiredEvent expected = new DHCPv6LeaseExpiredEvent
            {
                EntityId = random.NextGuid(),
                ScopeId = random.NextGuid(),
                Timestamp = DateTime.UtcNow,
            };

            var actual = SerializeAndDeserialze(expected);

            Assert.Equal(expected.EntityId, actual.EntityId);
            Assert.Equal(expected.ScopeId, actual.ScopeId);
            Assert.Equal(expected.Timestamp, actual.Timestamp);
        }

        [Fact]
        public void DHCPv6LeaseCreatedEvent()
        {
            Random random = new Random();

            DHCPv6LeaseCreatedEvent expected = new DHCPv6LeaseCreatedEvent
            {
                EntityId = random.NextGuid(),
                ScopeId = random.NextGuid(),
                Timestamp = DateTime.UtcNow,
                Address = random.GetIPv6Address(),
                ClientIdentifier = new UUIDDUID(random.NextGuid()),
                IdentityAssocationId = random.NextUInt32(),
                StartedAt = DateTime.UtcNow,
                UniqueIdentiifer = random.NextBytes(20),
                ValidUntil = DateTime.UtcNow.AddDays(3)
            };

            var actual = SerializeAndDeserialze(expected);

            Assert.Equal(expected.EntityId, actual.EntityId);
            Assert.Equal(expected.ScopeId, actual.ScopeId);
            Assert.Equal(expected.Timestamp, actual.Timestamp);
            Assert.Equal(expected.Address, actual.Address);
            Assert.Equal(expected.ClientIdentifier, actual.ClientIdentifier);
            Assert.Equal(expected.IdentityAssocationId, actual.IdentityAssocationId);
            Assert.Equal(expected.StartedAt, actual.StartedAt);
            Assert.Equal(expected.UniqueIdentiifer, actual.UniqueIdentiifer);
            Assert.Equal(expected.ValidUntil, actual.ValidUntil);
        }

        [Fact]
        public void DHCPv6LeaseReleasedEvent()
        {
            Random random = new Random();

            DHCPv6LeaseReleasedEvent expected = new DHCPv6LeaseReleasedEvent
            {
                EntityId = random.NextGuid(),
                ScopeId = random.NextGuid(),
                Timestamp = DateTime.UtcNow,
            };

            var actual = SerializeAndDeserialze(expected);

            Assert.Equal(expected.EntityId, actual.EntityId);
            Assert.Equal(expected.ScopeId, actual.ScopeId);
            Assert.Equal(expected.Timestamp, actual.Timestamp);
        }

        [Fact]
        public void DHCPv6LeaseRenewedEvent()
        {
            Random random = new Random();

            DHCPv6LeaseRenewedEvent expected = new DHCPv6LeaseRenewedEvent
            {
                EntityId = random.NextGuid(),
                ScopeId = random.NextGuid(),
                Timestamp = DateTime.UtcNow,
                End = DateTime.UtcNow,
                Reset = random.NextBoolean()
            };

            var actual = SerializeAndDeserialze(expected);

            Assert.Equal(expected.EntityId, actual.EntityId);
            Assert.Equal(expected.ScopeId, actual.ScopeId);
            Assert.Equal(expected.Timestamp, actual.Timestamp);
            Assert.Equal(expected.End, actual.End);
            Assert.Equal(expected.Reset, actual.Reset);

        }

        [Fact]
        public void DHCPv6LeaseRevokedEvent()
        {
            Random random = new Random();

            DHCPv6LeaseRevokedEvent expected = new DHCPv6LeaseRevokedEvent
            {
                EntityId = random.NextGuid(),
                ScopeId = random.NextGuid(),
                Timestamp = DateTime.UtcNow,
            };

            var actual = SerializeAndDeserialze(expected);

            Assert.Equal(expected.EntityId, actual.EntityId);
            Assert.Equal(expected.ScopeId, actual.ScopeId);
            Assert.Equal(expected.Timestamp, actual.Timestamp);
        }

        private DHCPv6Packet GetRandomPacket(Random random)
        {

            IPv6HeaderInformation header = new IPv6HeaderInformation(
                random.GetIPv6Address(), random.GetIPv6Address());

            DHCPv6Packet input = new DHCPv6Packet(header, random.NextUInt16(), DHCPv6PacketTypes.Solicit, new List<DHCPv6PacketOption>
            {
                new DHCPv6PacketIdentifierOption(DHCPv6PacketOptionTypes.ClientIdentifier,new  UUIDDUID(Guid.NewGuid())),
                new DHCPv6PacketIdentifierOption(DHCPv6PacketOptionTypes.ServerIdentifer,new  UUIDDUID(Guid.NewGuid())),
            }
            );

            return input;
        }

        [Fact]
        public void DHCPv6DeclineHandledEvent()
        {
            Random random = new Random();

            DHCPv6DeclineHandledEvent expected = new DHCPv6DeclineHandledEvent
            {
                Error = DHCPv6PacketHandledEvents.DHCPv6DeclineHandledEvent.DeclineErros.DeclineNotAllowed,
                Request = GetRandomPacket(random),
                Response = GetRandomPacket(random),
                ScopeId = random.NextGuid(),
                Timestamp = DateTime.UtcNow,
                WasSuccessfullHandled = random.NextBoolean(),
            };

            var actual = SerializeAndDeserialze(expected);

            Assert.Equal(expected.Error, actual.Error);
            Assert.Equal(expected.Request, actual.Request);
            Assert.Equal(expected.Response, actual.Response);
            Assert.Equal(expected.Timestamp, actual.Timestamp);
            Assert.Equal(expected.WasSuccessfullHandled, actual.WasSuccessfullHandled);
        }

        [Fact]
        public void DHCPv6SolicitHandledEvent()
        {
            Random random = new Random();

            DHCPv6SolicitHandledEvent expected = new DHCPv6SolicitHandledEvent
            {
                Error = DHCPv6PacketHandledEvents.DHCPv6SolicitHandledEvent.SolicitErros.ScopeNotFound,
                Request = GetRandomPacket(random),
                Response = GetRandomPacket(random),
                ScopeId = random.NextGuid(),
                Timestamp = DateTime.UtcNow,
                WasSuccessfullHandled = random.NextBoolean(),
                IsRapitCommit = random.NextBoolean(),
            };

            var actual = SerializeAndDeserialze(expected);

            Assert.Equal(expected.Error, actual.Error);
            Assert.Equal(expected.Request, actual.Request);
            Assert.Equal(expected.Response, actual.Response);
            Assert.Equal(expected.Timestamp, actual.Timestamp);
            Assert.Equal(expected.WasSuccessfullHandled, actual.WasSuccessfullHandled);
            Assert.Equal(expected.IsRapitCommit, actual.IsRapitCommit);
        }

        [Fact]
        public void DHCPv6InformRequestHandledEvent()
        {
            Random random = new Random();

            DHCPv6InformRequestHandledEvent expected = new DHCPv6InformRequestHandledEvent
            {
                Error = DHCPv6PacketHandledEvents.DHCPv6InformRequestHandledEvent.InformRequestErros.ScopeNotFound,
                Request = GetRandomPacket(random),
                Response = GetRandomPacket(random),
                ScopeId = random.NextGuid(),
                Timestamp = DateTime.UtcNow,
                WasSuccessfullHandled = random.NextBoolean(),
            };

            var actual = SerializeAndDeserialze(expected);

            Assert.Equal(expected.Error, actual.Error);
            Assert.Equal(expected.Request, actual.Request);
            Assert.Equal(expected.Response, actual.Response);
            Assert.Equal(expected.Timestamp, actual.Timestamp);
            Assert.Equal(expected.WasSuccessfullHandled, actual.WasSuccessfullHandled);
        }

        [Fact]
        public void DHCPv6ReleaseHandledEvent()
        {
            Random random = new Random();

            DHCPv6ReleaseHandledEvent expected = new DHCPv6ReleaseHandledEvent
            {
                Error = DHCPv6PacketHandledEvents.DHCPv6ReleaseHandledEvent.ReleaseError.LeaseNotActive,
                Request = GetRandomPacket(random),
                Response = GetRandomPacket(random),
                ScopeId = random.NextGuid(),
                Timestamp = DateTime.UtcNow,
                WasSuccessfullHandled = random.NextBoolean(),
            };

            var actual = SerializeAndDeserialze(expected);

            Assert.Equal(expected.Error, actual.Error);
            Assert.Equal(expected.Request, actual.Request);
            Assert.Equal(expected.Response, actual.Response);
            Assert.Equal(expected.Timestamp, actual.Timestamp);
            Assert.Equal(expected.WasSuccessfullHandled, actual.WasSuccessfullHandled);
        }

        [Fact]
        public void DHCPv6RequestHandledEvent()
        {
            Random random = new Random();

            DHCPv6RequestHandledEvent expected = new DHCPv6RequestHandledEvent
            {
                Error = DHCPv6PacketHandledEvents.DHCPv6RequestHandledEvent.RequestErrors.ScopeNotFound,
                Request = GetRandomPacket(random),
                Response = GetRandomPacket(random),
                ScopeId = random.NextGuid(),
                Timestamp = DateTime.UtcNow,
                WasSuccessfullHandled = random.NextBoolean(),
            };

            var actual = SerializeAndDeserialze(expected);

            Assert.Equal(expected.Error, actual.Error);
            Assert.Equal(expected.Request, actual.Request);
            Assert.Equal(expected.Response, actual.Response);
            Assert.Equal(expected.Timestamp, actual.Timestamp);
            Assert.Equal(expected.WasSuccessfullHandled, actual.WasSuccessfullHandled);
        }

        [Fact]
        public void DHCPv6ScopeAddedEvent()
        {
            Random random = new Random();

            DHCPv6ScopeAddedEvent expected = new DHCPv6ScopeAddedEvent
            {
                Timestamp = DateTime.UtcNow,
                Instructions = new DHCPv6ScopeCreateInstruction
                {
                    Description = random.GetAlphanumericString(20),
                    Name = random.GetAlphanumericString(4),
                    ParentId = random.NextGuid(),
                    Id = random.NextGuid(),
                    ResolverInformation = new DaAPI.Core.Scopes.CreateScopeResolverInformation
                    {
                        Typename = random.GetAlphanumericString(20),
                        PropertiesAndValues = new Dictionary<String, String>
                        {
                            { random.GetAlphanumericString(20), random.GetAlphanumericString(50) }
                        }
                    },
                    AddressProperties = new DHCPv6ScopeAddressProperties(
                       IPv6Address.FromString("fe80::1"), IPv6Address.FromString("fe80::A"),
                       new List<IPv6Address> { IPv6Address.FromString("fe80::4"), IPv6Address.FromString("fe80::5") },
                       DHCPv6TimeScale.FromDouble(0.5), DHCPv6TimeScale.FromDouble(0.85),
                       TimeSpan.FromMinutes(random.Next(10, 20)), TimeSpan.FromMinutes(random.Next(40, 60)),
                       random.NextBoolean(), DHCPv6ScopeAddressProperties.AddressAllocationStrategies.Next, random.NextBoolean(),
                       random.NextBoolean(), random.NextBoolean(), random.NextBoolean())
                }
            };

            var actual = SerializeAndDeserialze(expected);

            Assert.Equal(expected.Timestamp, actual.Timestamp);

            Assert.Equal(expected.Instructions.Description, actual.Instructions.Description);
            Assert.Equal(expected.Instructions.Name, actual.Instructions.Name);
            Assert.Equal(expected.Instructions.ParentId, actual.Instructions.ParentId);
            Assert.Equal(expected.Instructions.Id, actual.Instructions.Id);
            Assert.Equal(expected.Instructions.ResolverInformation.Typename, actual.Instructions.ResolverInformation.Typename);
            Assert.Equal(expected.Instructions.ResolverInformation.PropertiesAndValues.ElementAt(0), actual.Instructions.ResolverInformation.PropertiesAndValues.ElementAt(0));

            Assert.Equal(expected.Instructions.AddressProperties.AcceptDecline, actual.Instructions.AddressProperties.AcceptDecline);
            Assert.Equal(expected.Instructions.AddressProperties.AddressAllocationStrategy, actual.Instructions.AddressProperties.AddressAllocationStrategy);
            Assert.Equal(expected.Instructions.AddressProperties.End, actual.Instructions.AddressProperties.End);
            Assert.Equal(expected.Instructions.AddressProperties.ExcludedAddresses, actual.Instructions.AddressProperties.ExcludedAddresses, new IPv6AddressEquatableComparer());
            Assert.Equal(expected.Instructions.AddressProperties.InformsAreAllowd, actual.Instructions.AddressProperties.InformsAreAllowd);
            Assert.Equal(expected.Instructions.AddressProperties.RapitCommitEnabled, actual.Instructions.AddressProperties.RapitCommitEnabled);
            Assert.Equal(expected.Instructions.AddressProperties.ReuseAddressIfPossible, actual.Instructions.AddressProperties.ReuseAddressIfPossible);
            Assert.Equal(expected.Instructions.AddressProperties.Start, actual.Instructions.AddressProperties.Start);
            Assert.Equal(expected.Instructions.AddressProperties.SupportDirectUnicast, actual.Instructions.AddressProperties.SupportDirectUnicast);
            Assert.Equal(expected.Instructions.AddressProperties.PreferredLeaseTime, actual.Instructions.AddressProperties.PreferredLeaseTime);
            Assert.Equal(expected.Instructions.AddressProperties.ValidLeaseTime, actual.Instructions.AddressProperties.ValidLeaseTime);
        }

        [Fact]
        public void DHCPv6ScopeNameUpdatedEvent()
        {
            Random random = new Random();

            DHCPv6ScopeNameUpdatedEvent expected = new DHCPv6ScopeNameUpdatedEvent
            {
                EntityId = random.NextGuid(),
                Timestamp = DateTime.UtcNow,
                Name = random.GetAlphanumericString(20),
            };

            var actual = SerializeAndDeserialze(expected);

            Assert.Equal(expected.EntityId, actual.EntityId);
            Assert.Equal(expected.Timestamp, actual.Timestamp);
            Assert.Equal(expected.Name, actual.Name);
        }

        [Fact]
        public void DHCPv6ScopeDescriptionUpdatedEvent()
        {
            Random random = new Random();

            DHCPv6ScopeDescriptionUpdatedEvent expected = new DHCPv6ScopeDescriptionUpdatedEvent
            {
                EntityId = random.NextGuid(),
                Timestamp = DateTime.UtcNow,
                Description = random.GetAlphanumericString(20),
            };

            var actual = SerializeAndDeserialze(expected);

            Assert.Equal(expected.EntityId, actual.EntityId);
            Assert.Equal(expected.Timestamp, actual.Timestamp);
            Assert.Equal(expected.Description, actual.Description);
        }

        [Fact]
        public void DHCPv6ScopeResolverUpdatedEvent()
        {
            Random random = new Random();

            DHCPv6ScopeResolverUpdatedEvent expected = new DHCPv6ScopeResolverUpdatedEvent
            {
                EntityId = random.NextGuid(),
                Timestamp = DateTime.UtcNow,
                ResolverInformationen = new DaAPI.Core.Scopes.CreateScopeResolverInformation
                {
                    Typename = random.GetAlphanumericString(20),
                    PropertiesAndValues = new Dictionary<String, String>
                        {
                            { random.GetAlphanumericString(20), random.GetAlphanumericString(50) }
                        }
                },
            };

            var actual = SerializeAndDeserialze(expected);

            Assert.Equal(expected.EntityId, actual.EntityId);
            Assert.Equal(expected.Timestamp, actual.Timestamp);

            Assert.Equal(expected.ResolverInformationen.Typename, actual.ResolverInformationen.Typename);
            Assert.Equal(expected.ResolverInformationen.PropertiesAndValues.ElementAt(0), actual.ResolverInformationen.PropertiesAndValues.ElementAt(0));

        }

        [Fact]
        public void DHCPv6ScopeAddressPropertiesUpdatedEvent()
        {
            Random random = new Random();

            DHCPv6ScopeAddressPropertiesUpdatedEvent expected = new DHCPv6ScopeAddressPropertiesUpdatedEvent
            {
                EntityId = random.NextGuid(),
                Timestamp = DateTime.UtcNow,
                AddressProperties = new DHCPv6ScopeAddressProperties(
                       IPv6Address.FromString("fe80::1"), IPv6Address.FromString("fe80::A"),
                       new List<IPv6Address> { IPv6Address.FromString("fe80::4"), IPv6Address.FromString("fe80::5") },
                       DHCPv6TimeScale.FromDouble(0.5), DHCPv6TimeScale.FromDouble(0.85),
                       TimeSpan.FromMinutes(random.Next(10, 20)), TimeSpan.FromMinutes(random.Next(40, 60)),
                       random.NextBoolean(), DHCPv6ScopeAddressProperties.AddressAllocationStrategies.Next, random.NextBoolean(),
                       random.NextBoolean(), random.NextBoolean(), random.NextBoolean())
            };

            var actual = SerializeAndDeserialze(expected);

            Assert.Equal(expected.EntityId, actual.EntityId);
            Assert.Equal(expected.Timestamp, actual.Timestamp);
            Assert.Equal(expected.AddressProperties, actual.AddressProperties);
        }

        [Fact]
        public void DHCPv6ScopeParentUpdatedEvent()
        {
            Random random = new Random();

            DHCPv6ScopeParentUpdatedEvent expected = new DHCPv6ScopeParentUpdatedEvent
            {
                EntityId = random.NextGuid(),
                Timestamp = DateTime.UtcNow,
                ParentId = random.NextGuid(),
            };

            var actual = SerializeAndDeserialze(expected);

            Assert.Equal(expected.EntityId, actual.EntityId);
            Assert.Equal(expected.Timestamp, actual.Timestamp);
            Assert.Equal(expected.ParentId, actual.ParentId);
        }

        [Fact]
        public void DHCPv6ScopeDeletedEvent()
        {
            Random random = new Random();

            DHCPv6ScopeDeletedEvent expected = new DHCPv6ScopeDeletedEvent
            {
                EntityId = random.NextGuid(),
                Timestamp = DateTime.UtcNow,
                IncludeChildren = random.NextBoolean(),
            };

            var actual = SerializeAndDeserialze(expected);

            Assert.Equal(expected.EntityId, actual.EntityId);
            Assert.Equal(expected.Timestamp, actual.Timestamp);
            Assert.Equal(expected.IncludeChildren, actual.IncludeChildren);
        }

        [Fact]
        public void DHCPv6ScopeAddressesAreExhaustedEvent()
        {
            Random random = new Random();

            DHCPv6ScopeAddressesAreExhaustedEvent expected = new DHCPv6ScopeAddressesAreExhaustedEvent
            {
                EntityId = random.NextGuid(),
                Timestamp = DateTime.UtcNow,
            };

            var actual = SerializeAndDeserialze(expected);

            Assert.Equal(expected.EntityId, actual.EntityId);
            Assert.Equal(expected.Timestamp, actual.Timestamp);
        }

        [Fact]
        public void DHCPv6ScopeSuspendedEvent()
        {
            Random random = new Random();

            DHCPv6ScopeSuspendedEvent expected = new DHCPv6ScopeSuspendedEvent
            {
                EntityId = random.NextGuid(),
                Timestamp = DateTime.UtcNow,
            };

            var actual = SerializeAndDeserialze(expected);

            Assert.Equal(expected.EntityId, actual.EntityId);
            Assert.Equal(expected.Timestamp, actual.Timestamp);
        }

        [Fact]
        public void DHCPv6ScopeReactivedEvent()
        {
            Random random = new Random();

            DHCPv6ScopeReactivedEvent expected = new DHCPv6ScopeReactivedEvent
            {
                EntityId = random.NextGuid(),
                Timestamp = DateTime.UtcNow,
            };

            var actual = SerializeAndDeserialze(expected);

            Assert.Equal(expected.EntityId, actual.EntityId);
            Assert.Equal(expected.Timestamp, actual.Timestamp);
        }
    }
}
