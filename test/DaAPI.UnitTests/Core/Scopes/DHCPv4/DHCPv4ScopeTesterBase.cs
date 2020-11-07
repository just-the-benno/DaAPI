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
using Xunit;
using static DaAPI.Core.Scopes.DHCPv4.DHCPv4PacketHandledEvents;

namespace DaAPI.UnitTests.Core.Scopes.DHCPv4
{
    public abstract class DHCPv4ScopeTesterBase
    {
        protected static DHCPv4RootScope GetRootScope() =>
            new DHCPv4RootScope(Guid.NewGuid(), Mock.Of<IDHCPv4ScopeResolverManager>());

        protected static DHCPv4RootScope GetRootScope(Mock<IDHCPv4ScopeResolverManager> mock) =>
            new DHCPv4RootScope(Guid.NewGuid(), mock.Object);


        public static DHCPv4Packet GetInformPacket(Random random, IPv4Address clientAddress, IPv4Address serverAddress = null) =>
                   new DHCPv4Packet(
                new IPv4HeaderInformation(clientAddress, serverAddress ?? random.GetIPv4Address()),
                random.NextBytes(6),
                (UInt32)random.Next(),
                IPv4Address.Empty,
                IPv4Address.Empty,
                clientAddress,
                new DHCPv4PacketMessageTypeOption(DHCPv4Packet.DHCPv4MessagesTypes.DHCPINFORM)
                );


        public static T TestResult<T>(
        DHCPv4Packet input,
        DHCPv4Packet result,
        Boolean emptyResponseExpected,
        Boolean expectedSuccessfullHandling,
        Guid? scopeId,
        DHCPv4RootScope rootScope) where T : DHCPv4PacketHandledEvent
        {
            var changes = rootScope.GetChanges();
            Assert.Single(changes);

            DomainEvent domainEvent = changes.First();
            Assert.IsAssignableFrom<T>(domainEvent);

            T castedDomainEvents = (T)domainEvent;

            if (scopeId.HasValue == false)
            {
                Assert.False(castedDomainEvents.ScopeId.HasValue);
            }
            else
            {
                Assert.True(castedDomainEvents.ScopeId.HasValue);
                Assert.Equal(scopeId.Value, castedDomainEvents.ScopeId.Value);
            }
            Assert.Equal(input, castedDomainEvents.Request);
            if (emptyResponseExpected == true)
            {
                Assert.Equal(DHCPv4Packet.Empty, result);
                Assert.Equal(DHCPv4Packet.Empty, castedDomainEvents.Response);
            }
            else
            {
                Assert.NotEqual(DHCPv4Packet.Empty, result);
                Assert.NotEqual(DHCPv4Packet.Empty, castedDomainEvents.Response);
                Assert.Equal(result, castedDomainEvents.Response);
            }
            if (expectedSuccessfullHandling == false)
            {
                Assert.False(castedDomainEvents.WasSuccessfullHandled);
            }
            else
            {
                Assert.True(castedDomainEvents.WasSuccessfullHandled);
            }

            return castedDomainEvents;
        }

        public static bool IsOptionPresentend(DHCPv4Packet result, Byte optionType)
        {
            foreach (var item in result.Options)
            {
                if (item.OptionType == optionType)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsOptionPresentend(DHCPv4Packet result, DHCPv4OptionTypes optionType)
        {
            return IsOptionPresentend(result, (Byte)optionType);
        }

        public static bool HasOptionThisIPv4Adress(DHCPv4Packet result, DHCPv4OptionTypes optionType, IPv4Address address)
        {
            foreach (var item in result.Options)
            {
                if (item.OptionType == (Byte)optionType)
                {
                    Assert.IsAssignableFrom<DHCPv4PacketAddressOption>(item);

                    DHCPv4PacketAddressOption castedItem = (DHCPv4PacketAddressOption)item;
                    return castedItem.Address == address;
                }
            }

            return false;
        }

        public static void ChecKIfPropertyCorrelatesToOption(DHCPv4ScopeProperty property, DHCPv4PacketOption option)
        {
            Assert.NotNull(property);
            Assert.NotNull(option);

            Assert.Equal(property.OptionIdentifier, option.OptionType);

            if (property is DHCPv4AddressListScopeProperty castedProperty7)
            {
                Assert.IsAssignableFrom<DHCPv4PacketAddressListOption>(option);
                DHCPv4PacketAddressListOption castedOption = (DHCPv4PacketAddressListOption)option;

                Assert.Equal(castedProperty7.Addresses.OrderBy(x => x), castedOption.Addresses.OrderBy(x => x));
            }
            else if (property is DHCPv4AddressScopeProperty castedProperty6)
            {
                Assert.IsAssignableFrom<DHCPv4PacketAddressOption>(option);
                DHCPv4PacketAddressOption castedOption = (DHCPv4PacketAddressOption)option;

                Assert.Equal(castedProperty6.Address, castedOption.Address);
            }
            else if (property is DHCPv4BooleanScopeProperty castedProperty3)
            {
                Assert.IsAssignableFrom<DHCPv4PacketBooleanOption>(option);
                DHCPv4PacketBooleanOption castedOption = (DHCPv4PacketBooleanOption)option;

                Assert.Equal(castedProperty3.Value, castedOption.Value);
            }
            else if (property is DHCPv4BooleanScopeProperty castedProperty2)
            {
                Assert.IsAssignableFrom<DHCPv4PacketBooleanOption>(option);
                DHCPv4PacketBooleanOption castedOption = (DHCPv4PacketBooleanOption)option;

                Assert.Equal(castedProperty2.Value, castedOption.Value);
            }

            else if (property is DHCPv4NumericValueScopeProperty castedProperty)
            {
                Int64 value = 0;
                switch (castedProperty.NumericType)
                {
                    case DHCPv4NumericValueTypes.Byte:
                        Assert.IsAssignableFrom<DHCPv4PacketByteOption>(option);
                        value = ((DHCPv4PacketByteOption)option).Value;
                        break;
                    case DHCPv4NumericValueTypes.UInt16:
                        Assert.IsAssignableFrom<DHCPv4PacketUInt16Option>(option);
                        value = ((DHCPv4PacketUInt16Option)option).Value;
                        break;
                    case DHCPv4NumericValueTypes.UInt32:
                        Assert.IsAssignableFrom<DHCPv4PacketUInt32Option>(option);
                        value = ((DHCPv4PacketUInt32Option)option).Value;
                        break;
                    default:
                        break;
                }
            }
            else if (property is DHCPv4TextScopeProperty castedProperty4)
            {
                Assert.IsAssignableFrom<DHCPv4PacketTextOption>(option);
                DHCPv4PacketTextOption castedOption = (DHCPv4PacketTextOption)option;

                Assert.Equal(castedProperty4.Value, castedOption.Value);
            }
            else if (property is DHCPv4TimeScopeProperty castedProperty5)
            {
                Assert.IsAssignableFrom<DHCPv4PacketTimeSpanOption>(option);
                DHCPv4PacketTimeSpanOption castedOption = (DHCPv4PacketTimeSpanOption)option;

                Assert.Equal(castedProperty5.Value, castedOption.Value);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
