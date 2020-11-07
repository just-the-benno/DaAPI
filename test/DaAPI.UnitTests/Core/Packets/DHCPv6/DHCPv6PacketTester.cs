using DaAPI.Core.Common;
using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Packets.DHCPv6;
using DaAPI.Core.Scopes;
using DaAPI.Core.Scopes.DHCPv6;
using DaAPI.Core.Scopes.DHCPv6.ScopeProperties;
using DaAPI.TestHelper;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Engine;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using Xunit;
using Xunit.Sdk;
using static DaAPI.Core.Scopes.DHCPv6.DHCPv6LeaseEvents;

namespace DaAPI.UnitTests.Core.Packets.DHCPv6
{
    public class DHCPv6PacketTester
    {
        [Theory]
        [InlineData(DHCPv6PacketTypes.ADVERTISE, false)]
        [InlineData(DHCPv6PacketTypes.CONFIRM, false)]
        [InlineData(DHCPv6PacketTypes.DECLINE, true)]
        [InlineData(DHCPv6PacketTypes.INFORMATION_REQUEST, false)]
        [InlineData(DHCPv6PacketTypes.Invalid, false)]
        [InlineData(DHCPv6PacketTypes.REBIND, false)]
        [InlineData(DHCPv6PacketTypes.RECONFIGURE, false)]
        [InlineData(DHCPv6PacketTypes.RELAY_FORW, false)]
        [InlineData(DHCPv6PacketTypes.RELAY_REPL, false)]
        [InlineData(DHCPv6PacketTypes.RELEASE, true)]
        [InlineData(DHCPv6PacketTypes.RENEW, true)]
        [InlineData(DHCPv6PacketTypes.REPLY, false)]
        [InlineData(DHCPv6PacketTypes.REQUEST, true)]
        [InlineData(DHCPv6PacketTypes.Solicit, false)]
        [InlineData(DHCPv6PacketTypes.Unkown, false)]
        public void ShouldHaveDuid(DHCPv6PacketTypes type, Boolean expectedResult)
        {
            DHCPv6Packet packet = DHCPv6Packet.AsInner(1, type, Array.Empty<DHCPv6PacketOption>());

            Boolean actual = packet.ShouldHaveDuid();
            Assert.Equal(expectedResult, actual);
        }

        [Theory]
        [InlineData(DHCPv6PacketTypes.ADVERTISE, false)]
        [InlineData(DHCPv6PacketTypes.CONFIRM, false)]
        [InlineData(DHCPv6PacketTypes.DECLINE, false)]
        [InlineData(DHCPv6PacketTypes.INFORMATION_REQUEST, true)]
        [InlineData(DHCPv6PacketTypes.Invalid, false)]
        [InlineData(DHCPv6PacketTypes.REBIND, false)]
        [InlineData(DHCPv6PacketTypes.RECONFIGURE, false)]
        [InlineData(DHCPv6PacketTypes.RELAY_FORW, false)]
        [InlineData(DHCPv6PacketTypes.RELAY_REPL, false)]
        [InlineData(DHCPv6PacketTypes.RELEASE, false)]
        [InlineData(DHCPv6PacketTypes.RENEW, false)]
        [InlineData(DHCPv6PacketTypes.REPLY, false)]
        [InlineData(DHCPv6PacketTypes.REQUEST, false)]
        [InlineData(DHCPv6PacketTypes.Solicit, false)]
        [InlineData(DHCPv6PacketTypes.Unkown, false)]
        public void CouldHaveDuid(DHCPv6PacketTypes type, Boolean expectedResult)
        {
            DHCPv6Packet packet = DHCPv6Packet.AsInner(1, type, Array.Empty<DHCPv6PacketOption>());

            Boolean actual = packet.CouldHaveDuid();
            Assert.Equal(expectedResult, actual);
        }

        [Theory]
        [InlineData(DHCPv6PacketTypes.ADVERTISE, false)]
        [InlineData(DHCPv6PacketTypes.CONFIRM, true)]
        [InlineData(DHCPv6PacketTypes.DECLINE, true)]
        [InlineData(DHCPv6PacketTypes.INFORMATION_REQUEST, true)]
        [InlineData(DHCPv6PacketTypes.Invalid, false)]
        [InlineData(DHCPv6PacketTypes.REBIND, true)]
        [InlineData(DHCPv6PacketTypes.RECONFIGURE, false)]
        [InlineData(DHCPv6PacketTypes.RELAY_FORW, true)]
        [InlineData(DHCPv6PacketTypes.RELAY_REPL, false)]
        [InlineData(DHCPv6PacketTypes.RELEASE, true)]
        [InlineData(DHCPv6PacketTypes.RENEW, true)]
        [InlineData(DHCPv6PacketTypes.REPLY, false)]
        [InlineData(DHCPv6PacketTypes.REQUEST, true)]
        [InlineData(DHCPv6PacketTypes.Solicit, true)]
        [InlineData(DHCPv6PacketTypes.Unkown, false)]
        public void IsClientRequest(DHCPv6PacketTypes type, Boolean expectedResult)
        {
            DHCPv6Packet packet = DHCPv6Packet.AsInner(1, type, Array.Empty<DHCPv6PacketOption>());

            Boolean actual = packet.IsClientRequest();
            Assert.Equal(expectedResult, actual);
        }

        [Fact]
        public void GetInnerPacket()
        {
            DHCPv6Packet packet = DHCPv6Packet.AsInner(1, DHCPv6PacketTypes.ADVERTISE, Array.Empty<DHCPv6PacketOption>());
            DHCPv6Packet innerPacket = packet.GetInnerPacket();

            Assert.Equal(packet, innerPacket);

        }

        [Fact]
        public void GetIdentifier()
        {
            Random random = new Random();

            UUIDDUID duid = new UUIDDUID(random.NextGuid());

            DHCPv6Packet packet = DHCPv6Packet.AsInner(
                1, DHCPv6PacketTypes.Solicit, new List<DHCPv6PacketOption>
                {
                    new DHCPv6PacketIdentifierOption(DHCPv6PacketOptionTypes.ClientIdentifier,duid),
                    new DHCPv6PacketIdentifierOption(DHCPv6PacketOptionTypes.ServerIdentifer,new UUIDDUID(random.NextGuid()))

                });

            DUID actual = packet.GetIdentifier(DHCPv6PacketOptionTypes.ClientIdentifier);
            Assert.Equal(duid, actual);
        }

        [Fact]
        public void GetClientIdentifer()
        {
            Random random = new Random();

            UUIDDUID duid = new UUIDDUID(random.NextGuid());

            DHCPv6Packet packet = DHCPv6Packet.AsInner(
                1, DHCPv6PacketTypes.Solicit, new List<DHCPv6PacketOption>
                {
                    new DHCPv6PacketIdentifierOption(DHCPv6PacketOptionTypes.ClientIdentifier,duid),
                    new DHCPv6PacketIdentifierOption(DHCPv6PacketOptionTypes.ServerIdentifer,new UUIDDUID(random.NextGuid()))

                });

            DUID actual = packet.GetClientIdentifer();
            Assert.Equal(duid, actual);
        }

        [Fact]
        public void GetServerIdentifer()
        {
            Random random = new Random();

            UUIDDUID duid = new UUIDDUID(random.NextGuid());

            DHCPv6Packet packet = DHCPv6Packet.AsInner(
                1, DHCPv6PacketTypes.Solicit, new List<DHCPv6PacketOption>
                {
                    new DHCPv6PacketIdentifierOption(DHCPv6PacketOptionTypes.ClientIdentifier,new UUIDDUID(random.NextGuid())),
                    new DHCPv6PacketIdentifierOption(DHCPv6PacketOptionTypes.ServerIdentifer,duid)

                });

            DUID actual = packet.GetServerIdentifer();
            Assert.Equal(duid, actual);
        }

        [Fact]
        public void GetIdentifier_OptionTypeNotFound()
        {
            Random random = new Random();

            DHCPv6Packet packet = DHCPv6Packet.AsInner(
                1, DHCPv6PacketTypes.Solicit, new List<DHCPv6PacketOption>
                {
                    new DHCPv6PacketIdentifierOption(DHCPv6PacketOptionTypes.ServerIdentifer,new UUIDDUID(random.NextGuid()))

                });

            DUID actual = packet.GetIdentifier(DHCPv6PacketOptionTypes.ClientIdentifier);
            Assert.Equal(DUID.Empty, actual);
        }

        [Fact]
        public void GetIdentifier_OptionNotAppropiateType()
        {
            Random random = new Random();

            DHCPv6Packet packet = DHCPv6Packet.AsInner(
                1, DHCPv6PacketTypes.Solicit, new List<DHCPv6PacketOption>
                {
                    new DHCPv6PacketByteOption(DHCPv6PacketOptionTypes.ServerIdentifer,random.NextByte()),
                });

            DUID actual = packet.GetIdentifier(DHCPv6PacketOptionTypes.ClientIdentifier);
            Assert.Equal(DUID.Empty, actual);
        }

        [Theory]
        [InlineData(DHCPv6PacketTypes.Solicit, true)]
        [InlineData(DHCPv6PacketTypes.CONFIRM, true)]
        [InlineData(DHCPv6PacketTypes.REBIND, true)]
        [InlineData(DHCPv6PacketTypes.RENEW, false)]
        [InlineData(DHCPv6PacketTypes.REQUEST, false)]
        [InlineData(DHCPv6PacketTypes.DECLINE, false)]
        [InlineData(DHCPv6PacketTypes.RELEASE, false)]
        public void IsConsistent_WithClientIdentifierButNoServerIdentifier(DHCPv6PacketTypes type, Boolean expectedResult)
        {
            Random random = new Random();

            DHCPv6Packet packet = DHCPv6Packet.AsInner(
                1, type, new List<DHCPv6PacketOption>
                {
                    new DHCPv6PacketIdentifierOption(DHCPv6PacketOptionTypes.ClientIdentifier,new UUIDDUID(random.NextGuid())),

                });

            Boolean actual = packet.IsConsistent();
            Assert.Equal(expectedResult, actual);
        }

        [Theory]
        [InlineData(DHCPv6PacketTypes.Solicit, false)]
        [InlineData(DHCPv6PacketTypes.CONFIRM, false)]
        [InlineData(DHCPv6PacketTypes.REBIND, false)]
        [InlineData(DHCPv6PacketTypes.RENEW, true)]
        [InlineData(DHCPv6PacketTypes.REQUEST, true)]
        [InlineData(DHCPv6PacketTypes.DECLINE, true)]
        [InlineData(DHCPv6PacketTypes.RELEASE, true)]
        public void IsConsistent_WithClientIdentifierAndServerIdentifier(DHCPv6PacketTypes type, Boolean expectedResult)
        {
            Random random = new Random();

            DHCPv6Packet packet = DHCPv6Packet.AsInner(
                1, type, new List<DHCPv6PacketOption>
                {
                    new DHCPv6PacketIdentifierOption(DHCPv6PacketOptionTypes.ClientIdentifier,new UUIDDUID(random.NextGuid())),
                    new DHCPv6PacketIdentifierOption(DHCPv6PacketOptionTypes.ServerIdentifer,new UUIDDUID(random.NextGuid()))
                });

            Boolean actual = packet.IsConsistent();
            Assert.Equal(expectedResult, actual);
        }

        [Fact]
        public void IsConsistent_InformationRequest_WithClientIdentifier()
        {
            Random random = new Random();

            DHCPv6Packet packet = DHCPv6Packet.AsInner(
                1, DHCPv6PacketTypes.INFORMATION_REQUEST, new List<DHCPv6PacketOption>
                {
                    new DHCPv6PacketIdentifierOption(DHCPv6PacketOptionTypes.ClientIdentifier,new UUIDDUID(random.NextGuid())),
                    new DHCPv6PacketIdentifierOption(DHCPv6PacketOptionTypes.ServerIdentifer,new UUIDDUID(random.NextGuid()))
                });

            Boolean actual = packet.IsConsistent();
            Assert.True(actual);
        }

        [Fact]
        public void IsConsistent_InformationRequest_WithoutClientIdentifier()
        {
            Random random = new Random();

            DHCPv6Packet packet = DHCPv6Packet.AsInner(
                1, DHCPv6PacketTypes.INFORMATION_REQUEST, new List<DHCPv6PacketOption>
                {
                    new DHCPv6PacketIdentifierOption(DHCPv6PacketOptionTypes.ServerIdentifer,new UUIDDUID(random.NextGuid()))
                });

            Boolean actual = packet.IsConsistent();
            Assert.False(actual);
        }

        [Theory]
        [InlineData(DHCPv6PacketTypes.REPLY)]
        [InlineData(DHCPv6PacketTypes.RELAY_REPL)]
        [InlineData(DHCPv6PacketTypes.RECONFIGURE)]
        [InlineData(DHCPv6PacketTypes.ADVERTISE)]
        public void IsConsistent_ServerResponses(DHCPv6PacketTypes type)
        {
            DHCPv6Packet packet = DHCPv6Packet.AsInner(
                1, type, new List<DHCPv6PacketOption>
                {
                });

            Boolean actual = packet.IsConsistent();
            Assert.True(actual);
        }

        [Theory]
        [InlineData(DHCPv6PacketTypes.Unkown)]
        [InlineData(DHCPv6PacketTypes.Invalid)]
        public void IsConsistent_UnexpectedTypes(DHCPv6PacketTypes type)
        {
            DHCPv6Packet packet = DHCPv6Packet.AsInner(
                1, type, new List<DHCPv6PacketOption>
                {
                });

            Boolean actual = packet.IsConsistent();
            Assert.False(actual);
        }

        [Fact]
        public void GetNonTemporaryIdentityAssocationId()
        {
            Random random = new Random();
            UInt32 iaId = random.NextUInt32();

            DHCPv6Packet packet = DHCPv6Packet.AsInner(1, DHCPv6PacketTypes.Solicit, new List<DHCPv6PacketOption>
            {
                new DHCPv6PacketIdentityAssociationNonTemporaryAddressesOption(iaId, TimeSpan.Zero, TimeSpan.Zero, Array.Empty<DHCPv6PacketSuboption>())
            });

            var result = packet.GetNonTemporaryIdentityAssocationId().Value;

            Assert.Equal(iaId, result);
        }

        [Fact]
        public void GetNonTemporaryIdentityAssocationId_NotFound()
        {
            DHCPv6Packet packet = DHCPv6Packet.AsInner(1, DHCPv6PacketTypes.Solicit, new List<DHCPv6PacketOption>
            {
            });

            var result = packet.GetNonTemporaryIdentityAssocationId();

            Assert.Null(result);
        }

        [Fact]
        public void GetPrefixDelegationIdentityAssocationId()
        {
            Random random = new Random();
            UInt32 iaId = random.NextUInt32();

            DHCPv6Packet packet = DHCPv6Packet.AsInner(1, DHCPv6PacketTypes.Solicit, new List<DHCPv6PacketOption>
            {
                new DHCPv6PacketIdentityAssociationPrefixDelegationOption(iaId, TimeSpan.Zero, TimeSpan.Zero, Array.Empty<DHCPv6PacketSuboption>())
            });

            var result = packet.GetPrefixDelegationIdentityAssocationId().Value;

            Assert.Equal(iaId, result);
        }

        [Fact]
        public void GetPrefixDelegationIdentityAssocationId_NotFound()
        {
            DHCPv6Packet packet = DHCPv6Packet.AsInner(1, DHCPv6PacketTypes.Solicit, new List<DHCPv6PacketOption>
            {
            });

            var result = packet.GetPrefixDelegationIdentityAssocationId();

            Assert.Null(result);
        }


        [Fact]
        public void GetNonTemporaryIdentiyAssocation()
        {
            Random random = new Random();
            UInt32 iaId = random.NextUInt32();

            var option = new DHCPv6PacketIdentityAssociationNonTemporaryAddressesOption(iaId, TimeSpan.Zero, TimeSpan.Zero, Array.Empty<DHCPv6PacketSuboption>());

            DHCPv6Packet packet = DHCPv6Packet.AsInner(1, DHCPv6PacketTypes.Solicit, new List<DHCPv6PacketOption>
            {
                option
            });

            var result1 = packet.GetNonTemporaryIdentiyAssocation(iaId);
            var result2 = packet.GetNonTemporaryIdentiyAssocation(random.NextUInt32());

            Assert.Equal(option, result1);

            Assert.Null(result2);
        }

        [Fact]
        public void ConstructPacket()
        {
            IPv6HeaderInformation headerInformation = new IPv6HeaderInformation(
                IPv6Address.FromString("fe80::1"), IPv6Address.FromString("fe80::2"));

            DHCPv6Packet received = DHCPv6Packet.AsOuter(headerInformation, 1, DHCPv6PacketTypes.Solicit, Array.Empty<DHCPv6PacketOption>());
            DHCPv6Packet response = DHCPv6Packet.AsOuter(headerInformation, 1, DHCPv6PacketTypes.ADVERTISE, Array.Empty<DHCPv6PacketOption>());

            DHCPv6Packet responseToSend = DHCPv6Packet.ConstructPacket(received, response);
            Assert.Equal(response, responseToSend);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void AsAdvertise(Boolean withPrefixDelegation)
        {
            Random random = new Random();

            IPv6HeaderInformation headerInformation = new IPv6HeaderInformation(
                IPv6Address.FromString("fe80::1"), IPv6Address.FromString("fe80::2"));

            IPv6Address leaseAddress = IPv6Address.FromString("fe80::20");
            DHCPv6ScopeAddressProperties properties = new DHCPv6ScopeAddressProperties(
                IPv6Address.FromString("fe80::1"), IPv6Address.FromString("fe80::2"), Array.Empty<IPv6Address>(),
                t1: DHCPv6TimeScale.FromDouble(0.25), t2: DHCPv6TimeScale.FromDouble(0.5),
                preferredLifeTime: TimeSpan.FromMinutes(30), validLifeTime: TimeSpan.FromMinutes(60));

            IPv6Address prefixAddress = IPv6Address.FromString("fd80::0");
            UInt32 prefixIaid = 450;
            DHCPv6PrefixDelegation delegation = withPrefixDelegation == true ? DHCPv6PrefixDelegation.FromValues(
                prefixAddress, new IPv6SubnetMask(new IPv6SubnetMaskIdentifier(62)), prefixIaid) :
                DHCPv6PrefixDelegation.None;

            DUID serverDuid = new UUIDDUID(Guid.NewGuid());
            DUID clientDuid = new UUIDDUID(Guid.NewGuid());

            UInt32 transactionId = 424242;
            UInt32 iaid = 666789;

            List<DHCPv6PacketOption> options = new List<DHCPv6PacketOption>
            {
                new DHCPv6PacketIdentifierOption(DHCPv6PacketOptionTypes.ClientIdentifier,clientDuid),
                new DHCPv6PacketIdentityAssociationNonTemporaryAddressesOption(iaid,TimeSpan.Zero,TimeSpan.Zero,Array.Empty<DHCPv6PacketSuboption>())
            };

            if (withPrefixDelegation == true)
            {
                options.Add(new DHCPv6PacketIdentityAssociationPrefixDelegationOption(prefixIaid, TimeSpan.Zero, TimeSpan.Zero, Array.Empty<DHCPv6PacketSuboption>()));
            }

            DHCPv6Packet received = DHCPv6Packet.AsOuter(headerInformation, transactionId, DHCPv6PacketTypes.Solicit, options);

            DHCPv6ScopeProperties scopeProperties = GetScopeProperties(random);

            DHCPv6Packet response = DHCPv6Packet.AsAdvertise(received, leaseAddress, delegation, properties, scopeProperties, serverDuid);

            Assert.NotNull(response);
            Assert.Equal(response.Header.Source, headerInformation.Destionation);
            Assert.Equal(response.Header.Destionation, headerInformation.Source);
            Assert.True(response.IsValid);
            Assert.Equal(DHCPv6PacketTypes.ADVERTISE, response.PacketType);
            Assert.Equal(transactionId, response.TransactionId);

            CheckPacketOptions(leaseAddress, properties, prefixAddress, prefixIaid, serverDuid, clientDuid, iaid, response, true, out Boolean clientIdentifierFound, out Boolean serverIdentifierFound, out Boolean identityFound, out Boolean prefixIdentityFound);

            Assert.True(clientIdentifierFound);
            Assert.True(serverIdentifierFound);
            Assert.True(identityFound);
            if (withPrefixDelegation == true)
            {
                Assert.True(prefixIdentityFound);
            }
            else
            {
                Assert.False(prefixIdentityFound);
            }

            CheckScopeOptions(scopeProperties, response);
        }

        private static DHCPv6ScopeProperties GetScopeProperties(Random random)
        {
            return new DHCPv6ScopeProperties(
                            new DHCPv6AddressListScopeProperty(random.NextUInt16(), random.GetIPv6Addresses()),
                            new DHCPv6NumericValueScopeProperty(random.NextUInt16(), random.NextUInt32(), NumericScopePropertiesValueTypes.UInt32, DHCPv6ScopePropertyType.UInt32),
                            new DHCPv6NumericValueScopeProperty(random.NextUInt16(), random.NextUInt16(), NumericScopePropertiesValueTypes.UInt16, DHCPv6ScopePropertyType.UInt16),
                            new DHCPv6NumericValueScopeProperty(random.NextUInt16(), random.NextByte(), NumericScopePropertiesValueTypes.Byte, DHCPv6ScopePropertyType.Byte)
                            );
        }

        private void CheckScopeOptions(DHCPv6ScopeProperties scopeProperties, DHCPv6Packet response)
        {
            var elementsToCheck = scopeProperties.Properties.ToDictionary(x => x.OptionIdentifier, x => x);
            foreach (var item in response.GetInnerPacket().Options)
            {
                if (elementsToCheck.ContainsKey(item.Code) == false) { continue; }
                var correspondingOption = elementsToCheck[item.Code];

                switch (correspondingOption.ValueType)
                {
                    case DHCPv6ScopePropertyType.AddressList:
                        {
                            Assert.IsAssignableFrom<DHCPv6PacketIPAddressListOption>(item);
                            var castedPacketOption = (DHCPv6PacketIPAddressListOption)item;
                            Assert.Equal(((DHCPv6AddressListScopeProperty)correspondingOption).Addresses, castedPacketOption.Addresses, new IPv6AddressEquatableComparer());
                        }
                        break;
                    case DHCPv6ScopePropertyType.Byte:
                        {
                            Assert.IsAssignableFrom<DHCPv6PacketByteOption>(item);
                            var castedPacketOption = (DHCPv6PacketByteOption)item;
                            Assert.Equal(((DHCPv6NumericValueScopeProperty)correspondingOption).Value, castedPacketOption.Value);
                        }
                        break;
                    case DHCPv6ScopePropertyType.UInt16:
                        {
                            Assert.IsAssignableFrom<DHCPv6PacketUInt16Option>(item);
                            var castedPacketOption = (DHCPv6PacketUInt16Option)item;
                            Assert.Equal(((DHCPv6NumericValueScopeProperty)correspondingOption).Value, castedPacketOption.Value);
                        }
                        break;
                    case DHCPv6ScopePropertyType.UInt32:
                        {
                            Assert.IsAssignableFrom<DHCPv6PacketUInt32Option>(item);
                            var castedPacketOption = (DHCPv6PacketUInt32Option)item;
                            Assert.Equal(((DHCPv6NumericValueScopeProperty)correspondingOption).Value, castedPacketOption.Value);
                        }
                        break;
                    case DHCPv6ScopePropertyType.Text:
                        break;
                    default:
                        break;
                }

                elementsToCheck.Remove(item.Code);
            }

            Assert.Empty(elementsToCheck);
        }

        private static void CheckPacketOptions(IPv6Address leaseAddress, DHCPv6ScopeAddressProperties properties, IPv6Address prefixAddress, uint prefixIaid, DUID serverDuid, DUID clientDuid, uint iaid, DHCPv6Packet response, Boolean checkTimes, out bool clientIdentifierFound, out bool serverIdentifierFound, out bool identityFound, out bool prefixIdentityFound)
        {
            clientIdentifierFound = false;
            serverIdentifierFound = false;
            identityFound = false;
            prefixIdentityFound = false;
            foreach (var item in response.Options)
            {
                if (item.Code == (UInt16)DHCPv6PacketOptionTypes.ServerIdentifer)
                {
                    Assert.IsAssignableFrom<DHCPv6PacketIdentifierOption>(item);

                    DHCPv6PacketIdentifierOption identifierOption = (DHCPv6PacketIdentifierOption)item;
                    Assert.Equal(serverDuid, identifierOption.DUID);

                    serverIdentifierFound = true;
                }
                else if (item.Code == (UInt16)DHCPv6PacketOptionTypes.ClientIdentifier)
                {
                    Assert.IsAssignableFrom<DHCPv6PacketIdentifierOption>(item);

                    DHCPv6PacketIdentifierOption identifierOption = (DHCPv6PacketIdentifierOption)item;
                    Assert.Equal(clientDuid, identifierOption.DUID);

                    clientIdentifierFound = true;
                }
                else if (item.Code == (UInt16)DHCPv6PacketOptionTypes.IdentityAssociation_NonTemporary)
                {
                    Assert.IsAssignableFrom<DHCPv6PacketIdentityAssociationNonTemporaryAddressesOption>(item);

                    var iaOption = (DHCPv6PacketIdentityAssociationNonTemporaryAddressesOption)item;

                    Assert.Equal(iaid, iaOption.Id);

                    Assert.NotEmpty(iaOption.Suboptions);
                    Assert.Single(iaOption.Suboptions);

                    var uncastedSuboption = iaOption.Suboptions.First();
                    Assert.NotNull(uncastedSuboption);
                    Assert.IsAssignableFrom<DHCPv6PacketIdentityAssociationAddressSuboption>(uncastedSuboption);

                    var addressSubOption = (DHCPv6PacketIdentityAssociationAddressSuboption)uncastedSuboption;

                    if (checkTimes == true)
                    {
                        Assert.Equal(properties.T1 * properties.PreferredLeaseTime.Value, iaOption.T1);
                        Assert.Equal(properties.T2 * properties.PreferredLeaseTime.Value, iaOption.T2);

                        Assert.Equal(properties.PreferredLeaseTime.Value, addressSubOption.PreferredLifetime);
                        Assert.Equal(properties.ValidLeaseTime.Value, addressSubOption.ValidLifetime);
                    }
                    else
                    {

                    }

                    Assert.Equal(leaseAddress, addressSubOption.Address);

                    Assert.NotEmpty(addressSubOption.Suboptions);
                    Assert.Single(addressSubOption.Suboptions);

                    var uncastedSubSuboption = addressSubOption.Suboptions.First();
                    Assert.NotNull(uncastedSubSuboption);
                    Assert.IsAssignableFrom<DHCPv6PacketStatusCodeSuboption>(uncastedSubSuboption);

                    var statusSubOption = (DHCPv6PacketStatusCodeSuboption)uncastedSubSuboption;
                    Assert.Equal((UInt16)DHCPv6StatusCodes.Success, statusSubOption.StatusCode);
                    Assert.False(String.IsNullOrEmpty(statusSubOption.Message));

                    identityFound = true;
                }
                else if (item.Code == (UInt16)DHCPv6PacketOptionTypes.IdentityAssociation_PrefixDelegation)
                {
                    Assert.IsAssignableFrom<DHCPv6PacketIdentityAssociationPrefixDelegationOption>(item);

                    var iaOption = (DHCPv6PacketIdentityAssociationPrefixDelegationOption)item;

                    Assert.Equal(prefixIaid, iaOption.Id);

                    Assert.NotEmpty(iaOption.Suboptions);
                    Assert.Single(iaOption.Suboptions);

                    var uncastedSuboption = iaOption.Suboptions.First();
                    Assert.NotNull(uncastedSuboption);
                    Assert.IsAssignableFrom<DHCPv6PacketIdentityAssociationPrefixDelegationSuboption>(uncastedSuboption);

                    var addressSubOption = (DHCPv6PacketIdentityAssociationPrefixDelegationSuboption)uncastedSuboption;

                    if (checkTimes == true)
                    {
                        Assert.Equal(properties.T1 * properties.PreferredLeaseTime.Value, iaOption.T1);
                        Assert.Equal(properties.T2 * properties.PreferredLeaseTime.Value, iaOption.T2);

                        Assert.Equal(properties.PreferredLeaseTime.Value, addressSubOption.PreferredLifetime);
                        Assert.Equal(properties.ValidLeaseTime.Value, addressSubOption.ValidLifetime);
                    }
                    else
                    {

                    }

                    Assert.Equal(prefixAddress, addressSubOption.Address);
                    Assert.Equal(62, addressSubOption.PrefixLength);

                    Assert.NotEmpty(addressSubOption.Suboptions);
                    Assert.Single(addressSubOption.Suboptions);

                    var uncastedSubSuboption = addressSubOption.Suboptions.First();
                    Assert.NotNull(uncastedSubSuboption);
                    Assert.IsAssignableFrom<DHCPv6PacketStatusCodeSuboption>(uncastedSubSuboption);

                    var statusSubOption = (DHCPv6PacketStatusCodeSuboption)uncastedSubSuboption;
                    Assert.Equal((UInt16)DHCPv6StatusCodes.Success, statusSubOption.StatusCode);
                    Assert.False(String.IsNullOrEmpty(statusSubOption.Message));

                    prefixIdentityFound = true;
                }
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void PrefixAdvertiseResponse(Boolean withRappitCommit)
        {
            Random random = new Random();

            IPv6HeaderInformation headerInformation = new IPv6HeaderInformation(
                IPv6Address.FromString("fe80::1"), IPv6Address.FromString("fe80::2"));

            IPv6Address leaseAddress = IPv6Address.FromString("fe80::20");
            DHCPv6ScopeAddressProperties properties = new DHCPv6ScopeAddressProperties(
                IPv6Address.FromString("fe80::1"), IPv6Address.FromString("fe80::2"), Array.Empty<IPv6Address>(),
                t1: DHCPv6TimeScale.FromDouble(0.5), t2: DHCPv6TimeScale.FromDouble(0.75),
                preferredLifeTime: TimeSpan.FromDays(1), validLifeTime: TimeSpan.FromDays(2), supportDirectUnicast: true);

            IPv6Address prefixAddress = IPv6Address.FromString("fd80::0");
            UInt32 prefixIaid = 450;
            DHCPv6PrefixDelegation delegation = DHCPv6PrefixDelegation.FromValues(
                prefixAddress, new IPv6SubnetMask(new IPv6SubnetMaskIdentifier(62)), prefixIaid);

            DUID serverDuid = new UUIDDUID(Guid.NewGuid());
            DUID clientDuid = new UUIDDUID(Guid.NewGuid());

            UInt32 transactionId = 424242;

            DHCPv6Packet received = DHCPv6Packet.AsOuter(headerInformation, transactionId, DHCPv6PacketTypes.Solicit, new List<DHCPv6PacketOption>
            {
                new DHCPv6PacketIdentifierOption(DHCPv6PacketOptionTypes.ClientIdentifier,clientDuid),
                new DHCPv6PacketIdentityAssociationPrefixDelegationOption(prefixIaid,TimeSpan.Zero,TimeSpan.Zero,Array.Empty<DHCPv6PacketSuboption>())
            });

            Guid scopeId = random.NextGuid();

            Mock<ILoggerFactory> factoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);
            factoryMock.Setup(x => x.CreateLogger(It.IsAny<String>())).Returns(Mock.Of<ILogger<DHCPv6RootScope>>());

            DHCPv6RootScope rootScope = new DHCPv6RootScope(random.NextGuid(), Mock.Of<IScopeResolverManager<DHCPv6Packet, IPv6Address>>(), factoryMock.Object);
            rootScope.Load(new List<DomainEvent>{ new DHCPv6ScopeEvents.DHCPv6ScopeAddedEvent(
                new DHCPv6ScopeCreateInstruction
                {
                    Name = "Testscope",
                    Id = scopeId,
                }),
                new DHCPv6LeaseCreatedEvent
                {
                    EntityId = random.NextGuid(),
                    Address = random.GetIPv6Address(),
                    ScopeId = scopeId,
                    StartedAt = DateTime.UtcNow.AddDays(-0.25),
                    ValidUntil = DateTime.UtcNow.AddDays(1.75),
                    HasPrefixDelegation = true,
                    IdentityAssocationIdForPrefix = delegation.IdentityAssociation,
                    PrefixLength = delegation.Mask.Identifier,
                    DelegatedNetworkAddress = delegation.NetworkAddress,
                    ClientIdentifier = new UUIDDUID(random.NextGuid()),
                 },
            });

            var lease = rootScope.GetRootScopes().First().Leases.GetAllLeases().First();

            DHCPv6ScopeProperties scopeProperties = GetScopeProperties(random);
            DHCPv6Packet response;
            if (withRappitCommit == false)
            {
                response = DHCPv6Packet.AsPrefixAdvertise(received, properties, scopeProperties, lease, serverDuid);
            }
            else
            {
                response = DHCPv6Packet.AsPrefixReplyWithRapitCommit(received, properties, scopeProperties, lease, serverDuid);
            }

            Assert.NotNull(response);
            Assert.Equal(response.Header.Source, headerInformation.Destionation);
            Assert.Equal(response.Header.Destionation, headerInformation.Source);
            Assert.True(response.IsValid);
            if (withRappitCommit == false)
            {
                Assert.Equal(DHCPv6PacketTypes.ADVERTISE, response.PacketType);
            }
            else
            {
                Assert.Equal(DHCPv6PacketTypes.REPLY, response.PacketType);
            }

            Assert.Equal(transactionId, response.TransactionId);

            CheckPacketOptions(leaseAddress, properties, prefixAddress, prefixIaid, serverDuid, clientDuid, 0, response, false, out Boolean clientIdentifierFound, out Boolean serverIdentifierFound, out Boolean identityFound, out Boolean prefixIdentityFound);

            Boolean serverUnicastOptionFound = false;
            Boolean rapitCommitOptionFound = false;

            foreach (var item in response.Options)
            {
                if (item.Code == (UInt16)DHCPv6PacketOptionTypes.ServerIdentifer)
                {
                    serverUnicastOptionFound = true;
                }
                if (item.Code == (UInt32)DHCPv6PacketOptionTypes.RapitCommit)
                {
                    rapitCommitOptionFound = true;
                }
                if (item.Code == (UInt32)DHCPv6PacketOptionTypes.IdentityAssociation_PrefixDelegation)
                {
                    var identityOption = (DHCPv6PacketIdentityAssociationPrefixDelegationOption)item;


                    TimeSpan expectedPreferredLifetime = properties.PreferredLeaseTime.Value - (DateTimeOffset.Now - lease.Start);
                    TimeSpan expectedValidLifetime = properties.ValidLeaseTime.Value - (DateTimeOffset.Now - lease.Start);

                    Assert.True(Math.Abs((identityOption.T1 - (expectedPreferredLifetime * properties.T1.Value)).TotalSeconds) < 20);
                    Assert.True(Math.Abs((identityOption.T2 - (expectedPreferredLifetime * properties.T2.Value)).TotalSeconds) < 20);

                    var suboption = identityOption.Suboptions.First() as DHCPv6PacketIdentityAssociationPrefixDelegationSuboption;
                    Assert.True(Math.Abs((suboption.PreferredLifetime - expectedPreferredLifetime).TotalSeconds) < 20);
                    Assert.True(Math.Abs((suboption.ValidLifetime - expectedValidLifetime).TotalSeconds) < 20);
                }
            }

            Assert.True(clientIdentifierFound);
            Assert.True(serverIdentifierFound);
            Assert.False(identityFound);
            Assert.True(prefixIdentityFound);
            Assert.True(serverUnicastOptionFound);
            if (withRappitCommit == true)
            {
                Assert.True(rapitCommitOptionFound);
            }
            else
            {
                Assert.False(rapitCommitOptionFound);
            }

            CheckScopeOptions(scopeProperties, response);
        }


        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void AsError(Boolean withPrefixDelegation)
        {
            Random random = new Random();

            IPv6HeaderInformation headerInformation = new IPv6HeaderInformation(
                IPv6Address.FromString("fe80::1"), IPv6Address.FromString("fe80::2"));

            IPv6Address leaseAddress = IPv6Address.FromString("fe80::20");

            IPv6Address prefixAddress = IPv6Address.FromString("fd80::0");
            UInt32 prefixIaid = 450;
            Byte prefixLength = 62;

            DUID serverDuid = new UUIDDUID(Guid.NewGuid());
            DUID clientDuid = new UUIDDUID(Guid.NewGuid());

            UInt32 transactionId = 424242;
            UInt32 iaid = 666789;

            List<DHCPv6PacketOption> options = new List<DHCPv6PacketOption>
            {
                new DHCPv6PacketIdentifierOption(DHCPv6PacketOptionTypes.ClientIdentifier,clientDuid),
                new DHCPv6PacketIdentityAssociationNonTemporaryAddressesOption(iaid,TimeSpan.FromSeconds(random.Next()),TimeSpan.FromSeconds(random.Next()),new DHCPv6PacketSuboption[]
                {
                    new DHCPv6PacketIdentityAssociationAddressSuboption(leaseAddress,TimeSpan.FromSeconds(random.Next()),TimeSpan.FromSeconds(random.Next()),Array.Empty<DHCPv6PacketSuboption>())
                })
            };

            if (withPrefixDelegation == true)
            {
                options.Add(new DHCPv6PacketIdentityAssociationPrefixDelegationOption(prefixIaid, TimeSpan.Zero, TimeSpan.Zero, new DHCPv6PacketSuboption[]
                {
                    new DHCPv6PacketIdentityAssociationPrefixDelegationSuboption(TimeSpan.FromSeconds(random.Next()),TimeSpan.FromSeconds(random.Next()),prefixLength,prefixAddress, Array.Empty<DHCPv6PacketSuboption>())
                }));
            }

            DHCPv6Packet received = DHCPv6Packet.AsOuter(headerInformation, transactionId, DHCPv6PacketTypes.REQUEST, options);

            DHCPv6Packet response = DHCPv6Packet.AsError(received, DHCPv6StatusCodes.NoAddrsAvail, serverDuid);

            Assert.NotNull(response);
            Assert.Equal(response.Header.Source, headerInformation.Destionation);
            Assert.Equal(response.Header.Destionation, headerInformation.Source);
            Assert.True(response.IsValid);
            Assert.Equal(DHCPv6PacketTypes.REPLY, response.PacketType);
            Assert.Equal(transactionId, response.TransactionId);

            Boolean serverIdentifierFound = false, clientIdentifierFound = false, identityFound = false, prefixIdentityFound = false;

            foreach (var item in response.Options)
            {
                if (item.Code == (UInt16)DHCPv6PacketOptionTypes.ServerIdentifer)
                {
                    Assert.IsAssignableFrom<DHCPv6PacketIdentifierOption>(item);

                    DHCPv6PacketIdentifierOption identifierOption = (DHCPv6PacketIdentifierOption)item;
                    Assert.Equal(serverDuid, identifierOption.DUID);

                    serverIdentifierFound = true;
                }
                else if (item.Code == (UInt16)DHCPv6PacketOptionTypes.ClientIdentifier)
                {
                    Assert.IsAssignableFrom<DHCPv6PacketIdentifierOption>(item);

                    DHCPv6PacketIdentifierOption identifierOption = (DHCPv6PacketIdentifierOption)item;
                    Assert.Equal(clientDuid, identifierOption.DUID);

                    clientIdentifierFound = true;
                }
                else if (item.Code == (UInt16)DHCPv6PacketOptionTypes.IdentityAssociation_NonTemporary)
                {
                    Assert.IsAssignableFrom<DHCPv6PacketIdentityAssociationNonTemporaryAddressesOption>(item);

                    var iaOption = (DHCPv6PacketIdentityAssociationNonTemporaryAddressesOption)item;

                    Assert.Equal(iaid, iaOption.Id);
                    Assert.Equal(TimeSpan.Zero, iaOption.T1);
                    Assert.Equal(TimeSpan.Zero, iaOption.T2);

                    Assert.NotEmpty(iaOption.Suboptions);
                    Assert.Single(iaOption.Suboptions);

                    var uncastedSuboption = iaOption.Suboptions.First();
                    Assert.NotNull(uncastedSuboption);
                    Assert.IsAssignableFrom<DHCPv6PacketIdentityAssociationAddressSuboption>(uncastedSuboption);

                    var addressSubOption = (DHCPv6PacketIdentityAssociationAddressSuboption)uncastedSuboption;

                    Assert.Equal(TimeSpan.Zero, addressSubOption.PreferredLifetime);
                    Assert.Equal(TimeSpan.Zero, addressSubOption.ValidLifetime);
                    Assert.Equal(leaseAddress, addressSubOption.Address);

                    Assert.NotEmpty(addressSubOption.Suboptions);
                    Assert.Single(addressSubOption.Suboptions);

                    var uncastedSubSuboption = addressSubOption.Suboptions.First();
                    Assert.NotNull(uncastedSubSuboption);
                    Assert.IsAssignableFrom<DHCPv6PacketStatusCodeSuboption>(uncastedSubSuboption);

                    var statusSubOption = (DHCPv6PacketStatusCodeSuboption)uncastedSubSuboption;
                    Assert.Equal((UInt16)DHCPv6StatusCodes.NoAddrsAvail, statusSubOption.StatusCode);
                    //Assert.False(String.IsNullOrEmpty(statusSubOption.Message));

                    identityFound = true;
                }
                else if (item.Code == (UInt16)DHCPv6PacketOptionTypes.IdentityAssociation_PrefixDelegation)
                {
                    Assert.IsAssignableFrom<DHCPv6PacketIdentityAssociationPrefixDelegationOption>(item);

                    var iaOption = (DHCPv6PacketIdentityAssociationPrefixDelegationOption)item;

                    Assert.Equal(prefixIaid, iaOption.Id);

                    Assert.Equal(TimeSpan.Zero, iaOption.T1);
                    Assert.Equal(TimeSpan.Zero, iaOption.T2);

                    Assert.NotEmpty(iaOption.Suboptions);
                    Assert.Single(iaOption.Suboptions);

                    var uncastedSuboption = iaOption.Suboptions.First();
                    Assert.NotNull(uncastedSuboption);
                    Assert.IsAssignableFrom<DHCPv6PacketIdentityAssociationPrefixDelegationSuboption>(uncastedSuboption);

                    var addressSubOption = (DHCPv6PacketIdentityAssociationPrefixDelegationSuboption)uncastedSuboption;

                    Assert.Equal(TimeSpan.Zero, addressSubOption.PreferredLifetime);
                    Assert.Equal(TimeSpan.Zero, addressSubOption.ValidLifetime);

                    Assert.Equal(prefixAddress, addressSubOption.Address);
                    Assert.Equal(prefixLength, addressSubOption.PrefixLength);

                    Assert.NotEmpty(addressSubOption.Suboptions);
                    Assert.Single(addressSubOption.Suboptions);

                    var uncastedSubSuboption = addressSubOption.Suboptions.First();
                    Assert.NotNull(uncastedSubSuboption);
                    Assert.IsAssignableFrom<DHCPv6PacketStatusCodeSuboption>(uncastedSubSuboption);

                    var statusSubOption = (DHCPv6PacketStatusCodeSuboption)uncastedSubSuboption;
                    Assert.Equal((UInt16)DHCPv6StatusCodes.NoPrefixAvail, statusSubOption.StatusCode);
                    //Assert.False(String.IsNullOrEmpty(statusSubOption.Message));

                    prefixIdentityFound = true;
                }
            }

            Assert.True(clientIdentifierFound);
            Assert.True(serverIdentifierFound);
            Assert.True(identityFound);

            if (withPrefixDelegation == true)
            {
                Assert.True(prefixIdentityFound);
            }
            else
            {
                Assert.False(prefixIdentityFound);
            }
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        [InlineData(true, false)]
        public void AsReply(Boolean adjustTimers, Boolean isRapitCommit)
        {
            Random random = new Random();

            IPv6HeaderInformation headerInformation = new IPv6HeaderInformation(
                IPv6Address.FromString("fe80::1"), IPv6Address.FromString("fe80::2"));

            IPv6Address leaseAddress = IPv6Address.FromString("fe80::20");
            DHCPv6ScopeAddressProperties properties = new DHCPv6ScopeAddressProperties(
                IPv6Address.FromString("fe80::1"), IPv6Address.FromString("fe80::2"), Array.Empty<IPv6Address>(),
                t1: DHCPv6TimeScale.FromDouble(0.5), t2: DHCPv6TimeScale.FromDouble(0.75),
                preferredLifeTime: TimeSpan.FromDays(1), validLifeTime: TimeSpan.FromDays(2), supportDirectUnicast: true);

            UInt32 iaid = random.NextUInt32();

            IPv6Address prefixAddress = IPv6Address.FromString("fd80::0");
            UInt32 prefixIaid = random.NextUInt32();
            Byte prefixLength = (Byte)random.Next(30, 62);
            DHCPv6PrefixDelegation delegation = DHCPv6PrefixDelegation.FromValues(
                prefixAddress, new IPv6SubnetMask(new IPv6SubnetMaskIdentifier(62)), prefixIaid);

            DUID serverDuid = new UUIDDUID(Guid.NewGuid());
            DUID clientDuid = new UUIDDUID(Guid.NewGuid());

            UInt32 transactionId = 424242;

            DHCPv6Packet received = DHCPv6Packet.AsOuter(headerInformation, transactionId, DHCPv6PacketTypes.Solicit, new List<DHCPv6PacketOption>
            {
                new DHCPv6PacketIdentifierOption(DHCPv6PacketOptionTypes.ClientIdentifier,clientDuid),
                    new DHCPv6PacketIdentityAssociationNonTemporaryAddressesOption(iaid,TimeSpan.FromSeconds(random.Next()),TimeSpan.FromSeconds(random.Next()),new DHCPv6PacketSuboption[]
                {
                    new DHCPv6PacketIdentityAssociationAddressSuboption(leaseAddress,TimeSpan.FromSeconds(random.Next()),TimeSpan.FromSeconds(random.Next()),Array.Empty<DHCPv6PacketSuboption>())
                }),
                    new DHCPv6PacketIdentityAssociationPrefixDelegationOption(prefixIaid, TimeSpan.FromSeconds(random.Next()), TimeSpan.FromSeconds(random.Next()), new DHCPv6PacketSuboption[]
                {
                    new DHCPv6PacketIdentityAssociationPrefixDelegationSuboption(TimeSpan.FromSeconds(random.Next()),TimeSpan.FromSeconds(random.Next()),prefixLength,prefixAddress, Array.Empty<DHCPv6PacketSuboption>())
                })

            });

            Guid scopeId = random.NextGuid();

            Mock<ILoggerFactory> factoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);
            factoryMock.Setup(x => x.CreateLogger(It.IsAny<String>())).Returns(Mock.Of<ILogger<DHCPv6RootScope>>());

            DHCPv6RootScope rootScope = new DHCPv6RootScope(random.NextGuid(), Mock.Of<IScopeResolverManager<DHCPv6Packet, IPv6Address>>(), factoryMock.Object);
            rootScope.Load(new List<DomainEvent>{ new DHCPv6ScopeEvents.DHCPv6ScopeAddedEvent(
                new DHCPv6ScopeCreateInstruction
                {
                    Name = "Testscope",
                    Id = scopeId,
                }),
                new DHCPv6LeaseCreatedEvent
                {
                    EntityId = random.NextGuid(),
                    Address = leaseAddress,
                    IdentityAssocationId = iaid,
                    ClientIdentifier = clientDuid,
                    ScopeId = scopeId,
                    StartedAt = DateTime.UtcNow.AddDays(-0.25),
                    ValidUntil = DateTime.UtcNow.AddDays(1.75),
                    HasPrefixDelegation = true,
                    IdentityAssocationIdForPrefix = delegation.IdentityAssociation,
                    PrefixLength = delegation.Mask.Identifier,
                    DelegatedNetworkAddress = delegation.NetworkAddress,
                 },
            });

            var lease = rootScope.GetRootScopes().First().Leases.GetAllLeases().First();
            DHCPv6ScopeProperties scopeProperties = GetScopeProperties(random);

            DHCPv6Packet response = DHCPv6Packet.AsReply(received, properties, scopeProperties, lease, adjustTimers, serverDuid, isRapitCommit);

            Assert.NotNull(response);
            Assert.Equal(response.Header.Source, headerInformation.Destionation);
            Assert.Equal(response.Header.Destionation, headerInformation.Source);
            Assert.True(response.IsValid);
            Assert.Equal(DHCPv6PacketTypes.REPLY, response.PacketType);

            Assert.Equal(transactionId, response.TransactionId);

            CheckPacketOptions(leaseAddress, properties, prefixAddress, prefixIaid, serverDuid, clientDuid, iaid, response, !adjustTimers, out Boolean clientIdentifierFound, out Boolean serverIdentifierFound, out Boolean identityFound, out Boolean prefixIdentityFound);

            Boolean serverUnicastOptionFound = false;
            Boolean rappitCommitOptionFound = false;

            foreach (var item in response.Options)
            {
                if (item.Code == (UInt16)DHCPv6PacketOptionTypes.ServerIdentifer)
                {
                    serverUnicastOptionFound = true;
                }
                if (item.Code == (UInt16)DHCPv6PacketOptionTypes.RapitCommit)
                {
                    rappitCommitOptionFound = true;
                }
                if (item.Code == (UInt32)DHCPv6PacketOptionTypes.IdentityAssociation_PrefixDelegation)
                {
                    if (adjustTimers == true)
                    {
                        var identityOption = (DHCPv6PacketIdentityAssociationPrefixDelegationOption)item;

                        TimeSpan expectedPreferredLifetime = properties.PreferredLeaseTime.Value - (DateTimeOffset.Now - lease.Start);
                        TimeSpan expectedValidLifetime = properties.ValidLeaseTime.Value - (DateTimeOffset.Now - lease.Start);

                        Assert.True(Math.Abs((identityOption.T1 - (expectedPreferredLifetime * properties.T1.Value)).TotalSeconds) < 20);
                        Assert.True(Math.Abs((identityOption.T2 - (expectedPreferredLifetime * properties.T2.Value)).TotalSeconds) < 20);

                        var suboption = identityOption.Suboptions.First() as DHCPv6PacketIdentityAssociationPrefixDelegationSuboption;
                        Assert.True(Math.Abs((suboption.PreferredLifetime - expectedPreferredLifetime).TotalSeconds) < 20);
                        Assert.True(Math.Abs((suboption.ValidLifetime - expectedValidLifetime).TotalSeconds) < 20);

                    }
                }
            }

            Assert.True(clientIdentifierFound);
            Assert.True(serverIdentifierFound);
            Assert.True(identityFound);
            Assert.True(prefixIdentityFound);
            Assert.True(serverUnicastOptionFound);
            Assert.Equal(isRapitCommit, rappitCommitOptionFound);

            CheckScopeOptions(scopeProperties, response);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void HasOption(Boolean optionExpected)
        {
            Random random = new Random();
            UInt16 optionCode = random.NextUInt16();

            List<DHCPv6PacketOption> options = new List<DHCPv6PacketOption>
            {
                new DHCPv6PacketByteArrayOption(random.NextUInt16(), random.NextBytes(20))
            };

            if (optionExpected == true)
            {
                options.Add(new DHCPv6PacketByteArrayOption(optionCode, random.NextBytes(20)));

            }

            DHCPv6Packet packet = DHCPv6Packet.AsInner(1, DHCPv6PacketTypes.RENEW, options);

            Boolean actual = packet.HasOption(optionCode);
            Assert.Equal(optionExpected, actual);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void HasRapitCommitOption(Boolean optionExpected)
        {
            Random random = new Random();

            List<DHCPv6PacketOption> options = new List<DHCPv6PacketOption>
            {
                new DHCPv6PacketByteArrayOption(random.NextUInt16(), random.NextBytes(20))
            };

            if (optionExpected == true)
            {
                options.Add(new DHCPv6PacketTrueOption(DHCPv6PacketOptionTypes.RapitCommit));

            }

            DHCPv6Packet packet = DHCPv6Packet.AsInner(1, DHCPv6PacketTypes.RENEW, options);

            Boolean actual = packet.HasRapitCommitOption();
            Assert.Equal(optionExpected, actual);
        }

        [Fact]
        public void GetOption()
        {
            Random random = new Random();

            DUID duid = new UUIDDUID(random.NextGuid());

            DHCPv6Packet packet = DHCPv6Packet.AsInner(1, DHCPv6PacketTypes.RENEW, new List<DHCPv6PacketOption>
            {
                new DHCPv6PacketIdentifierOption(DHCPv6PacketOptionTypes.ClientIdentifier, duid),
            });

            var actual = packet.GetOption<DHCPv6PacketIdentifierOption>(DHCPv6PacketOptionTypes.ClientIdentifier);
            Assert.NotNull(actual);
            Assert.Equal(duid, actual.DUID);
        }

        [Fact]
        public void GetOption_NotFound()
        {
            Random random = new Random();

            DUID duid = new UUIDDUID(random.NextGuid());

            DHCPv6Packet packet = DHCPv6Packet.AsInner(1, DHCPv6PacketTypes.RENEW, new List<DHCPv6PacketOption>
            {
                new DHCPv6PacketIdentifierOption(DHCPv6PacketOptionTypes.ClientIdentifier, duid),
            });

            var actual = packet.GetOption<DHCPv6PacketIdentifierOption>(DHCPv6PacketOptionTypes.ServerIdentifer);
            Assert.Null(actual);
        }

        [Fact]
        public void GetOption_WrongType()
        {
            Random random = new Random();

            DUID duid = new UUIDDUID(random.NextGuid());

            DHCPv6Packet packet = DHCPv6Packet.AsInner(1, DHCPv6PacketTypes.RENEW, new List<DHCPv6PacketOption>
            {
                new DHCPv6PacketIdentifierOption(DHCPv6PacketOptionTypes.ClientIdentifier, duid),
            });

            var actual = packet.GetOption<DHCPv6PacketTrueOption>(DHCPv6PacketOptionTypes.ClientIdentifier);
            Assert.Null(actual);
        }

        [Theory]
        [InlineData(DHCPv6PacketTypes.REBIND, "fe80::1", true)]
        [InlineData(DHCPv6PacketTypes.RELAY_FORW, "fe80::1", false)]
        [InlineData(DHCPv6PacketTypes.RELAY_REPL, "fe80::1", false)]
        [InlineData(DHCPv6PacketTypes.REBIND, "FF00::3", false)]
        [InlineData(DHCPv6PacketTypes.RELEASE, "2001:e68:5423:3a2b:716b:ef6e:b215:fb96", true)]
        public void IsUnicast(DHCPv6PacketTypes packetType, String sourceAddress, Boolean shouldBeUnicast)
        {
            IPv6HeaderInformation headerInformation = new IPv6HeaderInformation(
              IPv6Address.FromString(sourceAddress), IPv6Address.FromString("fe80::2"));

            DHCPv6Packet packet = DHCPv6Packet.AsOuter(headerInformation, 1, packetType, Array.Empty<DHCPv6PacketOption>());

            Boolean actual = packet.IsUnicast();
            Assert.Equal(shouldBeUnicast, actual);
        }

        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        [Fact]
        public void Blub()
        {
            String input = "0c002a0ccac5020100960000000000000001fe80000000000000025d73fffe66ce8800090034012a5b6900080002ffff0001000a00030001005d7366ce87000e000000060004001700180003000c0024000100000000000000000025000e00000009000308002c4f52ceac850012000409010060";
            Byte[] content = StringToByteArray(input);

            DHCPv6Packet packet = DHCPv6Packet.FromByteArray(content, new IPv6HeaderInformation(IPv6Address.FromString("fe80::1"), IPv6Address.FromString("fe80::2")));
            Assert.True(packet.IsValid);
        }

    }
}
