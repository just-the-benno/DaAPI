using DaAPI.Core.Common;
using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Packets;
using DaAPI.Core.Packets.DHCPv6;
using DaAPI.TestHelper;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace DaAPI.UnitTests.Core.Packets.DHCPv6
{
    public class DHCPv6PacketTester_FromByteArray
    {
        private void CheckByteRepresentation(DHCPv6Packet input, IPv6HeaderInformation header)
        {
            Byte[] rawStream = new Byte[1800];
            Int32 writtenBytes = input.GetAsStream(rawStream);
            Byte[] stream = ByteHelper.CopyData(rawStream, 0, writtenBytes);

            DHCPv6Packet secondPacket = DHCPv6Packet.FromByteArray(stream, header);
            Assert.Equal(input, secondPacket);
        }

        [Fact]
        public void FromByteArray_TrueOption()
        {
            Random random = new Random();
            UInt32 transactionId = (UInt32)random.Next(0, 256 * 256 * 256);
            IPv6HeaderInformation header = new IPv6HeaderInformation(IPv6Address.FromString("fe80::1"), IPv6Address.FromString("fe80::2"));

            DHCPv6Packet packet = new DHCPv6Packet(header, transactionId, DHCPv6PacketTypes.Solicit, new List<DHCPv6PacketOption>
            {
                new DHCPv6PacketTrueOption(DHCPv6PacketOptionTypes.RapitCommit),
                new DHCPv6PacketTrueOption(DHCPv6PacketOptionTypes.ReconfigureAccepte),
            }
            );

            CheckByteRepresentation(packet, header);
        }


        [Fact]
        public void FromByteArray_BooleanOption()
        {
            Random random = new Random();
            UInt32 transactionId = (UInt32)random.Next(0, 256 * 256 * 256);
            IPv6HeaderInformation header = new IPv6HeaderInformation(IPv6Address.FromString("fe80::1"), IPv6Address.FromString("fe80::2"));

            UInt16 firstIdentifier = 4000;
            UInt16 secondIdentifier = 4002;

            DHCPv6Packet packet = new DHCPv6Packet(header, transactionId, DHCPv6PacketTypes.Solicit, new List<DHCPv6PacketOption>
            {
                new DHCPv6PacketBooleanOption(firstIdentifier,true),
                new DHCPv6PacketBooleanOption(secondIdentifier,false),
            }
            );

            try
            {
                DHCPv6PacketOptionFactory.AddOptionType(firstIdentifier, (data) => DHCPv6PacketBooleanOption.FromByteArray(data, 0), true);
                DHCPv6PacketOptionFactory.AddOptionType(secondIdentifier, (data) => DHCPv6PacketBooleanOption.FromByteArray(data, 0), true);

                CheckByteRepresentation(packet, header);
            }
            finally
            {
                DHCPv6PacketOptionFactory.RemoveOptionType(firstIdentifier);
                DHCPv6PacketOptionFactory.RemoveOptionType(secondIdentifier);
            }
        }

        [Fact]
        public void FromByteArray_IdentifierOption()
        {
            Random random = new Random();
            UInt32 transactionId = (UInt32)random.Next(0, 256 * 256 * 256);
            IPv6HeaderInformation header = new IPv6HeaderInformation(IPv6Address.FromString("fe80::1"), IPv6Address.FromString("fe80::2"));

            DHCPv6Packet packet = new DHCPv6Packet(header, transactionId, DHCPv6PacketTypes.Solicit, new List<DHCPv6PacketOption>
            {
                new DHCPv6PacketIdentifierOption(DHCPv6PacketOptionTypes.ClientIdentifier,new  UUIDDUID(Guid.NewGuid())),
                new DHCPv6PacketIdentifierOption(DHCPv6PacketOptionTypes.ServerIdentifer,new  UUIDDUID(Guid.NewGuid())),
            }
            );

            CheckByteRepresentation(packet, header);
        }

        [Fact]
        public void FromByteArray_ByteArrayOption()
        {
            Random random = new Random();
            UInt32 transactionId = (UInt32)random.Next(0, 256 * 256 * 256);
            IPv6HeaderInformation header = new IPv6HeaderInformation(IPv6Address.FromString("fe80::1"), IPv6Address.FromString("fe80::2"));

            DHCPv6Packet packet = new DHCPv6Packet(header, transactionId, DHCPv6PacketTypes.Solicit, new List<DHCPv6PacketOption>
            {
                new DHCPv6PacketByteArrayOption(DHCPv6PacketOptionTypes.InterfaceId,random.NextBytes(20)),
                new DHCPv6PacketByteArrayOption(157,random.NextBytes(20)),
            }
            );

            CheckByteRepresentation(packet, header);
        }

        [Fact]
        public void FromByteArray_ByteOption()
        {
            Random random = new Random();
            UInt32 transactionId = (UInt32)random.Next(0, 256 * 256 * 256);
            IPv6HeaderInformation header = new IPv6HeaderInformation(IPv6Address.FromString("fe80::1"), IPv6Address.FromString("fe80::2"));

            UInt16 identifier = 5000;

            DHCPv6Packet packet = new DHCPv6Packet(header, transactionId, DHCPv6PacketTypes.Solicit, new List<DHCPv6PacketOption>
            {
                new DHCPv6PacketByteOption(DHCPv6PacketOptionTypes.Preference,(Byte)random.Next(0,255)),
                new DHCPv6PacketByteOption(identifier,(Byte)random.Next(0,255)),
            }
            );

            try
            {
                DHCPv6PacketOptionFactory.AddOptionType(identifier, (data) => DHCPv6PacketByteOption.FromByteArray(data, 0), true);

                CheckByteRepresentation(packet, header);
            }
            finally
            {
                DHCPv6PacketOptionFactory.RemoveOptionType(identifier);
            }
        }

        [Fact]
        public void FromByteArray_IPAddressOption()
        {
            Random random = new Random();
            UInt32 transactionId = (UInt32)random.Next(0, 256 * 256 * 256);
            IPv6HeaderInformation header = new IPv6HeaderInformation(IPv6Address.FromString("fe80::1"), IPv6Address.FromString("fe80::2"));

            UInt16 identifier = 6000;

            DHCPv6Packet packet = new DHCPv6Packet(header, transactionId, DHCPv6PacketTypes.Solicit, new List<DHCPv6PacketOption>
            {
                new DHCPv6PacketIPAddressOption(DHCPv6PacketOptionTypes.ServerUnicast,IPv6Address.FromString("fe80::1")),
                new DHCPv6PacketIPAddressOption(identifier,IPv6Address.FromString("fe80::2")),
            }
            );

            try
            {
                DHCPv6PacketOptionFactory.AddOptionType(identifier, (data) => DHCPv6PacketIPAddressOption.FromByteArray(data, 0), true);

                CheckByteRepresentation(packet, header);
            }
            finally
            {
                DHCPv6PacketOptionFactory.RemoveOptionType(identifier);
            }
        }

        [Fact]
        public void FromByteArray_OptionRequest()
        {
            Random random = new Random();
            UInt32 transactionId = (UInt32)random.Next(0, 256 * 256 * 256);
            IPv6HeaderInformation header = new IPv6HeaderInformation(IPv6Address.FromString("fe80::1"), IPv6Address.FromString("fe80::2"));


            DHCPv6Packet packet = new DHCPv6Packet(header, transactionId, DHCPv6PacketTypes.Solicit, new List<DHCPv6PacketOption>
            {
                new DHCPv6PacketOptionRequestOption(random.GetUInt16Values(20)),
                new DHCPv6PacketIPAddressOption(DHCPv6PacketOptionTypes.ServerUnicast,IPv6Address.FromString("fe80::1")),
            }
            );

            CheckByteRepresentation(packet, header);
        }

        [Fact]
        public void FromByteArray_ReconfigureOption()
        {
            Random random = new Random();
            UInt32 transactionId = (UInt32)random.Next(0, 256 * 256 * 256);
            IPv6HeaderInformation header = new IPv6HeaderInformation(IPv6Address.FromString("fe80::1"), IPv6Address.FromString("fe80::2"));

            DHCPv6Packet packet = new DHCPv6Packet(header, transactionId, DHCPv6PacketTypes.Solicit, new List<DHCPv6PacketOption>
            {
                new DHCPv6PacketReconfigureOption(DHCPv6PacketTypes.REBIND),
                new DHCPv6PacketIPAddressOption(DHCPv6PacketOptionTypes.ServerUnicast,IPv6Address.FromString("fe80::1")),
            }
            );

            CheckByteRepresentation(packet, header);
        }

        [Fact]
        public void FromByteArray_RemoteIdentifierOption()
        {
            Random random = new Random();
            UInt32 transactionId = (UInt32)random.Next(0, 256 * 256 * 256);
            IPv6HeaderInformation header = new IPv6HeaderInformation(IPv6Address.FromString("fe80::1"), IPv6Address.FromString("fe80::2"));

            DHCPv6Packet packet = new DHCPv6Packet(header, transactionId, DHCPv6PacketTypes.Solicit, new List<DHCPv6PacketOption>
            {
                new DHCPv6PacketRemoteIdentifierOption(random.NextUInt32(),random.NextBytes(25)),
                new DHCPv6PacketIPAddressOption(DHCPv6PacketOptionTypes.ServerUnicast,IPv6Address.FromString("fe80::1")),
            }
            );

            CheckByteRepresentation(packet, header);
        }

        [Fact]
        public void FromByteArray_TimeOption()
        {
            Random random = new Random();
            UInt32 transactionId = (UInt32)random.Next(0, 256 * 256 * 256);
            IPv6HeaderInformation header = new IPv6HeaderInformation(IPv6Address.FromString("fe80::1"), IPv6Address.FromString("fe80::2"));

            DHCPv6Packet packet = new DHCPv6Packet(header, transactionId, DHCPv6PacketTypes.Solicit, new List<DHCPv6PacketOption>
            {
                new DHCPv6PacketTimeOption(DHCPv6PacketOptionTypes.ElapsedTime,random.NextUInt32(),DHCPv6PacketTimeOption.DHCPv6PacketTimeOptionUnits.HundredsOfSeconds),
                new DHCPv6PacketTimeOption(DHCPv6PacketOptionTypes.InformationRefreshTime,random.NextUInt32(),DHCPv6PacketTimeOption.DHCPv6PacketTimeOptionUnits.Seconds),
            }
            );

            CheckByteRepresentation(packet, header);
        }

        [Fact]
        public void FromByteArray_UInt32Option()
        {
            Random random = new Random();
            UInt32 transactionId = (UInt32)random.Next(0, 256 * 256 * 256);
            IPv6HeaderInformation header = new IPv6HeaderInformation(IPv6Address.FromString("fe80::1"), IPv6Address.FromString("fe80::2"));

            DHCPv6Packet packet = new DHCPv6Packet(header, transactionId, DHCPv6PacketTypes.Solicit, new List<DHCPv6PacketOption>
            {
                new DHCPv6PacketUInt32Option(DHCPv6PacketOptionTypes.SOL_MAX_RT,random.NextUInt32()),
                new DHCPv6PacketUInt32Option(DHCPv6PacketOptionTypes.INF_MAX_RT,random.NextUInt32()),
            }
            );

            CheckByteRepresentation(packet, header);
        }

        [Fact]
        public void FromByteArray_UInt16Option()
        {
            Random random = new Random();
            UInt32 transactionId = (UInt32)random.Next(0, 256 * 256 * 256);
            IPv6HeaderInformation header = new IPv6HeaderInformation(IPv6Address.FromString("fe80::1"), IPv6Address.FromString("fe80::2"));

            UInt16 firstIdentifier = random.NextUInt16();
            UInt16 secondIdentifier = random.NextUInt16();

            DHCPv6PacketOptionFactory.AddOptionType(firstIdentifier, (data) => DHCPv6PacketUInt16Option.FromByteArray(data, 0), false);
            DHCPv6PacketOptionFactory.AddOptionType(secondIdentifier, (data) => DHCPv6PacketUInt16Option.FromByteArray(data, 0), false);

            DHCPv6Packet packet = new DHCPv6Packet(header, transactionId, DHCPv6PacketTypes.Solicit, new List<DHCPv6PacketOption>
            {
                new DHCPv6PacketUInt16Option(firstIdentifier,random.NextUInt16()),
                new DHCPv6PacketUInt16Option(secondIdentifier,random.NextUInt16()),
            }
            );

            CheckByteRepresentation(packet, header);
        }

        [Fact]
        public void FromByteArray_AddressListOption()
        {
            Random random = new Random();
            UInt32 transactionId = (UInt32)random.Next(0, 256 * 256 * 256);
            IPv6HeaderInformation header = new IPv6HeaderInformation(IPv6Address.FromString("fe80::1"), IPv6Address.FromString("fe80::2"));

            UInt16 firstIdentifier = random.NextUInt16();
            UInt16 secondIdentifier = random.NextUInt16();

            DHCPv6PacketOptionFactory.AddOptionType(firstIdentifier, (data) => DHCPv6PacketIPAddressListOption.FromByteArray(data, 0), false);
            DHCPv6PacketOptionFactory.AddOptionType(secondIdentifier, (data) => DHCPv6PacketIPAddressListOption.FromByteArray(data, 0), false);

            DHCPv6Packet packet = new DHCPv6Packet(header, transactionId, DHCPv6PacketTypes.Solicit, new List<DHCPv6PacketOption>
            {
                new DHCPv6PacketIPAddressListOption(firstIdentifier,random.GetIPv6Addresses(2,5)),
                new DHCPv6PacketIPAddressListOption(secondIdentifier,random.GetIPv6Addresses(2,5)),
            }
            );

            CheckByteRepresentation(packet, header);
        }

        [Fact]
        public void FromByteArray_UserClassOption()
        {
            Random random = new Random(12);
            UInt32 transactionId = (UInt32)random.Next(0, 256 * 256 * 256);
            IPv6HeaderInformation header = new IPv6HeaderInformation(IPv6Address.FromString("fe80::1"), IPv6Address.FromString("fe80::2"));

            DHCPv6Packet packet = new DHCPv6Packet(header, transactionId, DHCPv6PacketTypes.Solicit, new List<DHCPv6PacketOption>
            {
                new DHCPv6PacketUserClassOption(random.NextByteArrays(10,15,30)),
                new DHCPv6PacketIPAddressOption(DHCPv6PacketOptionTypes.ServerUnicast,IPv6Address.FromString("fe80::1")),
            }
            );

            CheckByteRepresentation(packet, header);
        }

        [Fact]
        public void FromByteArray_VendorClassOption()
        {
            Random random = new Random();
            UInt32 transactionId = (UInt32)random.Next(0, 256 * 256 * 256);
            IPv6HeaderInformation header = new IPv6HeaderInformation(IPv6Address.FromString("fe80::1"), IPv6Address.FromString("fe80::2"));

            DHCPv6Packet packet = new DHCPv6Packet(header, transactionId, DHCPv6PacketTypes.Solicit, new List<DHCPv6PacketOption>
            {
                new DHCPv6PacketVendorClassOption(random.NextUInt32(), random.NextByteArrays(10,15,30)),
                new DHCPv6PacketIPAddressOption(DHCPv6PacketOptionTypes.ServerUnicast,IPv6Address.FromString("fe80::1")),
            }
            );

            CheckByteRepresentation(packet, header);
        }

        private List<DHCPv6VendorOptionData> GetDHCPv6VendorSpecificOptions(Random random,
            Int32 amount)
        {
            List<DHCPv6VendorOptionData> result = new List<DHCPv6VendorOptionData>(amount);
            for (int i = 0; i < amount; i++)
            {
                DHCPv6VendorOptionData data = new DHCPv6VendorOptionData(random.NextUInt16(), random.NextBytes(random.Next(10, 30)));
                result.Add(data);
            }

            return result;
        }

        [Fact]
        public void FromByteArray_VendorSpecificInformationOption()
        {
            Random random = new Random(12345);
            UInt32 transactionId = (UInt32)random.Next(0, 256 * 256 * 256);
            IPv6HeaderInformation header = new IPv6HeaderInformation(IPv6Address.FromString("fe80::1"), IPv6Address.FromString("fe80::2"));

            DHCPv6Packet packet = new DHCPv6Packet(header, transactionId, DHCPv6PacketTypes.Solicit, new List<DHCPv6PacketOption>
            {
                new DHCPv6PacketVendorSpecificInformationOption(random.NextUInt32(),
                GetDHCPv6VendorSpecificOptions(random,10)),
                new DHCPv6PacketIPAddressOption(DHCPv6PacketOptionTypes.ServerUnicast,IPv6Address.FromString("fe80::1")),
            }
            );

            CheckByteRepresentation(packet, header);
        }

        [Fact]
        public void FromByteArray_IdentityAssociationNonTemporaryAddressesOption()
        {
            Random random = new Random();
            UInt32 transactionId = (UInt32)random.Next(0, 256 * 256 * 256);
            IPv6HeaderInformation header = new IPv6HeaderInformation(IPv6Address.FromString("fe80::1"), IPv6Address.FromString("fe80::2"));

            DHCPv6Packet packet = new DHCPv6Packet(header, transactionId, DHCPv6PacketTypes.Solicit, new List<DHCPv6PacketOption>
            {
                new DHCPv6PacketIdentityAssociationNonTemporaryAddressesOption(
                    random.NextUInt16(),
                    TimeSpan.FromSeconds(random.Next(0,36000)),
                    TimeSpan.FromSeconds(random.Next(0,36000)),
                    new List<DHCPv6PacketSuboption>
                    {
                        new DHCPv6PacketIdentityAssociationAddressSuboption(
                            IPv6Address.FromString("fe80::5"),
                            TimeSpan.FromSeconds(random.Next(0,36000)),
                            TimeSpan.FromSeconds(random.Next(0,36000)),
                            new List<DHCPv6PacketSuboption>()
                            {
                                new DHCPv6PacketStatusCodeSuboption(DHCPv6StatusCodes.Success,"Thanks for using DaAPI")
                            }
                            )
                    }
                    ),
                new DHCPv6PacketIPAddressOption(DHCPv6PacketOptionTypes.ServerUnicast,IPv6Address.FromString("fe80::1")),
            }
            );

            CheckByteRepresentation(packet, header);
        }

        [Fact]
        public void FromByteArray_IdentityAssociationTemporaryAddressesOption()
        {
            Random random = new Random(12345);
            UInt32 transactionId = (UInt32)random.Next(0, 256 * 256 * 256);
            IPv6HeaderInformation header = new IPv6HeaderInformation(IPv6Address.FromString("fe80::1"), IPv6Address.FromString("fe80::2"));

            DHCPv6Packet packet = new DHCPv6Packet(header, transactionId, DHCPv6PacketTypes.Solicit, new List<DHCPv6PacketOption>
            {
                new DHCPv6PacketIdentityAssociationTemporaryAddressesOption(
                    random.NextUInt16(),
                    new List<DHCPv6PacketSuboption>
                    {
                        new DHCPv6PacketIdentityAssociationAddressSuboption(
                            IPv6Address.FromString("fe80::5"),
                            TimeSpan.FromSeconds(random.Next(0,36000)),
                            TimeSpan.FromSeconds(random.Next(0,36000)),
                            new List<DHCPv6PacketSuboption>()
                            {
                                new DHCPv6PacketStatusCodeSuboption(DHCPv6StatusCodes.Success,"Thanks for using DaAPI")
                            }
                            )
                    }
                    ),
                new DHCPv6PacketIPAddressOption(DHCPv6PacketOptionTypes.ServerUnicast,IPv6Address.FromString("fe80::1")),
            }
            );

            CheckByteRepresentation(packet, header);
        }

        [Fact]
        public void FromByteArray_IdentityAssociationForPrefixAddressesOption()
        {
            Random random = new Random();
            UInt32 transactionId = (UInt32)random.Next(0, 256 * 256 * 256);
            IPv6HeaderInformation header = new IPv6HeaderInformation(IPv6Address.FromString("fe80::1"), IPv6Address.FromString("fe80::2"));

            DHCPv6Packet packet = new DHCPv6Packet(header, transactionId, DHCPv6PacketTypes.Solicit, new List<DHCPv6PacketOption>
            {
                new DHCPv6PacketIdentityAssociationPrefixDelegationOption(
                    random.NextUInt16(),
                    TimeSpan.FromSeconds(random.Next(0,36000)),
                    TimeSpan.FromSeconds(random.Next(0,36000)),
                    new List<DHCPv6PacketSuboption>
                    {
                        new DHCPv6PacketIdentityAssociationPrefixDelegationSuboption(
                            TimeSpan.FromSeconds(random.Next(0,36000)),
                            TimeSpan.FromSeconds(random.Next(0,36000)),
                            random.NextByte(),
                            IPv6Address.FromString("fe80::20"),
                            new List<DHCPv6PacketSuboption>()
                            {
                                new DHCPv6PacketStatusCodeSuboption(DHCPv6StatusCodes.Success,"Thanks for using DaAPI")
                            }
                            )
                    }
                    ),
                new DHCPv6PacketIPAddressOption(DHCPv6PacketOptionTypes.ServerUnicast,IPv6Address.FromString("fe80::1")),
            }
            );

            CheckByteRepresentation(packet, header);
        }
    }
}
