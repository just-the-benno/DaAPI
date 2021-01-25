using DaAPI.Core.Common;
using DaAPI.Core.Packets.DHCPv4;
using DaAPI.Core.Scopes.DHCPv4;
using DaAPI.Core.Services;
using DaAPI.TestHelper;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static DaAPI.Core.Scopes.DHCPv4.DHCPv4ScopeEvents;
using static DaAPI.Core.Scopes.DHCPv4.DHCPv4PacketHandledEvents;
using static DaAPI.Core.Scopes.DHCPv4.DHCPv4PacketHandledEvents.DHCPv4InformHandledEvent;
using static DaAPI.Core.Packets.DHCPv4.DHCPv4Packet;

namespace DaAPI.UnitTests.Core.Scopes.DHCPv4
{
    public class DHCPv4ScopeInformTester : DHCPv4ScopeTesterBase
    {
        [Fact]
        public void TestInform_ScopeNotFound()
        {
            Random random = new Random();

            IPv4Address sourceAddress = random.GetIPv4Address();
            DHCPv4Packet input = GetInformPacket(random, sourceAddress);

            DHCPv4RootScope rootScope = GetRootScope();
            DHCPv4Packet result = rootScope.HandleInform(input);

            DHCPv4InformHandledEvent domainEvent = TestResult<DHCPv4InformHandledEvent>(
                input, result, true, false, new Guid?(), rootScope);

            Assert.Equal(InformErros.ScopeNotFound, domainEvent.Error);
        }

        [Fact]
        public void TestInform_ScopeNotAllowInforms()
        {
            Random random = new Random();

            IPv4Address start = random.GetIPv4Address();
            IPv4Address end = random.GetIPv4AddressGreaterThan(start);
            IPv4Address sourceAddress = random.GetIPv4AddressBetween(start, end);

            DHCPv4Packet input = GetInformPacket(random, sourceAddress);

            DHCPv4RootScope rootScope = GetRootScope();

            Guid scopeId = Guid.NewGuid();

            rootScope.Load(new List<DomainEvent> { new DHCPv4ScopeAddedEvent(new DHCPv4ScopeCreateInstruction
            {
                AddressProperties = new DHCPv4ScopeAddressProperties(start,end,new List<IPv4Address>(),informsAreAllowd:false),
                Id = scopeId,
            }) });

            DHCPv4Packet result = rootScope.HandleInform(input);

            DHCPv4InformHandledEvent domainEvent = TestResult<DHCPv4InformHandledEvent>(
                input, result, true, false, scopeId, rootScope);

            Assert.Equal(InformErros.InformsNotAllowed, domainEvent.Error);
        }

        [Fact]
        public void TestInform_RootScopeAcceptInforms()
        {
            Random random = new Random();

            IPv4Address start = random.GetIPv4Address();
            IPv4Address end = random.GetIPv4AddressGreaterThan(start);
            IPv4Address sourceAddress = random.GetIPv4AddressBetween(start, end);

            DHCPv4Packet input = GetInformPacket(random, sourceAddress);

            DHCPv4RootScope rootScope = GetRootScope();

            Guid scopeId = Guid.NewGuid();

            rootScope.Load(new List<DomainEvent> { new DHCPv4ScopeAddedEvent(new DHCPv4ScopeCreateInstruction
            {
                AddressProperties = new DHCPv4ScopeAddressProperties(start,end,new List<IPv4Address>(),informsAreAllowd:true),
                Id = scopeId,
            }) });

            DHCPv4Packet result = rootScope.HandleInform(input);

            DHCPv4InformHandledEvent domainEvent = TestResult<DHCPv4InformHandledEvent>(
                input, result, false, true, scopeId, rootScope);

            Assert.Equal(InformErros.NoError, domainEvent.Error);
        }


        private void GenerateScopeTree(
           Double randomValue, Random random, Guid parentId,
           ICollection<Tuple<IPv4Address, IPv4Address>> existingRanges,
           ICollection<DomainEvent> events
           )
        {

            if (randomValue > 0)
            {
                return;
            }

            Int32 scopeAmount = random.Next(3, 10);
            for (int i = 0; i < scopeAmount; i++)
            {
                Guid scopeId = Guid.NewGuid();
                Tuple<IPv4Address, IPv4Address> addressRange = random.GenerateUniqueAddressRange(existingRanges);

                events.Add(new DHCPv4ScopeAddedEvent(new DHCPv4ScopeCreateInstruction
                {
                    Id = scopeId,
                    ParentId = parentId,
                    AddressProperties = new DHCPv4ScopeAddressProperties
                            (addressRange.Item1, addressRange.Item2, Array.Empty<IPv4Address>(), informsAreAllowd: true)
                }));

                GenerateScopeTree(
                    randomValue + random.NextDouble(), random,
                    scopeId, existingRanges, events);
            }
        }


        [Fact]
        public void TestInform_RandomScopeAcceptInforms()
        {
            Random random = new Random();

            IPv4Address start = random.GetIPv4Address();
            IPv4Address end = random.GetIPv4AddressGreaterThan(start);
            IPv4Address sourceAddress = random.GetIPv4AddressBetween(start, end);

            List<Tuple<IPv4Address, IPv4Address>> existingRanges = new List<Tuple<IPv4Address, IPv4Address>>
            {
                new Tuple<IPv4Address, IPv4Address>(start,end),
            };

            DHCPv4Packet input = GetInformPacket(random, sourceAddress);

            Int32 rootScopeAmount = random.Next(3, 10);
            List<DomainEvent> events = new List<DomainEvent>();

            for (int i = 0; i < rootScopeAmount; i++)
            {
                Guid rootScopeId = Guid.NewGuid();
                Tuple<IPv4Address, IPv4Address> addressRange = random.GenerateUniqueAddressRange(existingRanges);

                events.Add(new DHCPv4ScopeAddedEvent(new DHCPv4ScopeCreateInstruction
                {
                    Id = rootScopeId,
                    AddressProperties = new DHCPv4ScopeAddressProperties
                            (addressRange.Item1, addressRange.Item2, Array.Empty<IPv4Address>(), informsAreAllowd: true)
                }));

                GenerateScopeTree(
                    random.NextDouble(), random,
                    rootScopeId, existingRanges, events);
            }

            Int32 randomScopeIndex = random.Next(0, events.Count);
            DHCPv4ScopeAddedEvent corelatedScopeEvent = (DHCPv4ScopeAddedEvent)events[randomScopeIndex];

            corelatedScopeEvent.Instructions.AddressProperties = new DHCPv4ScopeAddressProperties
            (start, end, Array.Empty<IPv4Address>(), informsAreAllowd: true);

            Guid scopeId = corelatedScopeEvent.Instructions.Id;

            DHCPv4RootScope rootScope = GetRootScope();

            rootScope.Load(events);

            DHCPv4Packet result = rootScope.HandleInform(input);

            DHCPv4InformHandledEvent domainEvent = TestResult<DHCPv4InformHandledEvent>(
                input, result, false, true, scopeId, rootScope);

            Assert.Equal(InformErros.NoError, domainEvent.Error);
        }

        [Fact]
        public void TestInform_CheckReponsePacket()
        {
            Random random = new Random();

            IPv4Address start = random.GetIPv4Address();
            IPv4Address end = random.GetIPv4AddressGreaterThan(start);
            IPv4Address sourceAddress = random.GetIPv4AddressBetween(start, end);
            IPv4Address destinationAddress = random.GetIPv4AddressBetween(start, end);

            DHCPv4Packet input = GetInformPacket(random, sourceAddress, destinationAddress);

            List<DHCPv4ScopeProperty> scopeProperties = random.GenerateProperties(new List<DHCPv4OptionTypes>
            {
                DHCPv4OptionTypes.RebindingTimeValue,
                DHCPv4OptionTypes.RenewalTimeValue,
                DHCPv4OptionTypes.IPAddressLeaseTime,
                DHCPv4OptionTypes.ServerIdentifier,
                DHCPv4OptionTypes.MessageType,
            });

            DHCPv4RootScope rootScope = GetRootScope();

            Guid scopeId = Guid.NewGuid();

            rootScope.Load(new List<DomainEvent> { new DHCPv4ScopeAddedEvent(new DHCPv4ScopeCreateInstruction
            {
                AddressProperties = new DHCPv4ScopeAddressProperties(start,end,new List<IPv4Address>(),
                informsAreAllowd: true,
                renewalTime: TimeSpan.FromMinutes(random.Next(3,10)),
                preferredLifetime: TimeSpan.FromMinutes(random.Next(20,30)),
                leaseTime: TimeSpan.FromMinutes(random.Next(40,60))
                ),
                Id = scopeId,
                ScopeProperties = new DHCPv4ScopeProperties(scopeProperties)
            })
            });

            DHCPv4Packet result = rootScope.HandleInform(input);

            Assert.NotEqual(DHCPv4Packet.Empty, result);

            Assert.Equal(sourceAddress, result.Header.Destionation);
            Assert.Equal(destinationAddress, result.Header.Source);

            Assert.Equal(DHCPv4MessagesTypes.Acknowledge, result.MessageType);
            Assert.Equal(input.ClientHardwareAddress, result.ClientHardwareAddress);
            Assert.Equal(input.HardwareAddressLength, result.HardwareAddressLength);
            Assert.Equal(input.HardwareType, result.HardwareType);

            Assert.Equal(0, result.Hops);
            Assert.Equal(0, result.SecondsElapsed);

            Assert.Equal(input.TransactionId, result.TransactionId);
            Assert.Equal(String.Empty, result.FileName);
            Assert.Equal(String.Empty, result.ServerHostname);

            Assert.Equal(DHCPv4PacketFlags.Unicast, result.Flags);
            Assert.Equal(DHCPv4PacketOperationCodes.BootReply, result.OpCode);

            Assert.Equal(IPv4Address.Empty, result.GatewayIPAdress);
            Assert.Equal(IPv4Address.Empty, result.YourIPAdress);
            Assert.Equal(IPv4Address.Empty, result.ServerIPAdress);
            Assert.Equal(input.ClientIPAdress, result.ClientIPAdress);

            Assert.Equal(input.ClientIPAdress, result.Header.Destionation);

            // no time values
            Assert.False(IsOptionPresentend(result, DHCPv4OptionTypes.RebindingTimeValue));
            Assert.False(IsOptionPresentend(result, DHCPv4OptionTypes.RenewalTimeValue));
            Assert.False(IsOptionPresentend(result, DHCPv4OptionTypes.IPAddressLeaseTime));

            // server identifier 
            Assert.True(IsOptionPresentend(result, DHCPv4OptionTypes.ServerIdentifier));
            Assert.True(HasOptionThisIPv4Adress(result, DHCPv4OptionTypes.ServerIdentifier, destinationAddress));

            foreach (var item in scopeProperties)
            {
                Assert.True(IsOptionPresentend(result, item.OptionIdentifier));
                DHCPv4PacketOption existingOption = result.GetOptionByIdentifier(item.OptionIdentifier);
                Assert.NotEqual(DHCPv4PacketOption.NotPresented, existingOption);

                ChecKIfPropertyCorrelatesToOption(item, existingOption);
            }
        }
    }
}
