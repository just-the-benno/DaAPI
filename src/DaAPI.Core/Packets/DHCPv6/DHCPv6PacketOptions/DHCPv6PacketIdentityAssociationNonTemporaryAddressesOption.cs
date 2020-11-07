using DaAPI.Core.Common;
using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Helper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DaAPI.Core.Packets.DHCPv6
{
    public class DHCPv6PacketIdentityAssociationNonTemporaryAddressesOption :
        DHCPv6PacketIdentityAsociationOption, IEquatable<DHCPv6PacketIdentityAssociationNonTemporaryAddressesOption>
    {
        #region Properties

        public TimeSpan T1 { get; private set; }
        public TimeSpan T2 { get; private set; }

        #endregion

        #region Constructor

        public DHCPv6PacketIdentityAssociationNonTemporaryAddressesOption(UInt32 id) : this(
               id, TimeSpan.Zero, TimeSpan.Zero, Array.Empty<DHCPv6PacketSuboption>())
        {
        }

        public DHCPv6PacketIdentityAssociationNonTemporaryAddressesOption(UInt32 id,
            TimeSpan t1, TimeSpan t2, IEnumerable<DHCPv6PacketSuboption> suboptions) : base(
                (UInt16)DHCPv6PacketOptionTypes.IdentityAssociation_NonTemporary, id,
                ByteHelper.ConcatBytes(ByteHelper.GetBytes((UInt32)t1.TotalSeconds), ByteHelper.GetBytes((UInt32)t2.TotalSeconds)),
                suboptions)
        {
            T1 = t1;
            T2 = t2;
        }

        public static DHCPv6PacketIdentityAssociationNonTemporaryAddressesOption FromByteArray(Byte[] data, Int32 offset)
        {
            UInt16 lenght = ByteHelper.ConvertToUInt16FromByte(data, offset + 2);
            UInt32 identifier = ByteHelper.ConvertToUInt32FromByte(data, offset + 4);
            UInt32 T1 = ByteHelper.ConvertToUInt32FromByte(data, offset + 8);
            UInt32 T2 = ByteHelper.ConvertToUInt32FromByte(data, offset + 12);

            List<DHCPv6PacketSuboption> suboptions = new List<DHCPv6PacketSuboption>();
            if (lenght > 12)
            {
                Byte[] subOptionsData = ByteHelper.CopyData(data, offset + 16);
                suboptions.AddRange(DHCPv6PacketSuboptionFactory.GetOptions(subOptionsData));
            }

            return new DHCPv6PacketIdentityAssociationNonTemporaryAddressesOption(
                identifier, TimeSpan.FromSeconds(T1), TimeSpan.FromSeconds(T2),
                suboptions);
        }

        #endregion

        #region Methods

        public IPv6Address GetAddress() => GetAddressSuboption()?.Address ?? IPv6Address.Empty;

        public DHCPv6PacketIdentityAssociationAddressSuboption GetAddressSuboption() => Suboptions.OfType<DHCPv6PacketIdentityAssociationAddressSuboption>().FirstOrDefault();

        public override string ToString()
        {
            return $"type: {Code} | id : {Id} | T1 : {T1} | T1 : {T2}";
        }

        public bool Equals(DHCPv6PacketIdentityAssociationNonTemporaryAddressesOption other)
        {
            return base.Equals(other);
        }

        public static DHCPv6PacketIdentityAssociationNonTemporaryAddressesOption Error(DHCPv6PacketIdentityAssociationNonTemporaryAddressesOption item, DHCPv6StatusCodes statusCode) =>
           new DHCPv6PacketIdentityAssociationNonTemporaryAddressesOption(item.Id, TimeSpan.Zero, TimeSpan.Zero, new DHCPv6PacketSuboption[] { new
                DHCPv6PacketIdentityAssociationAddressSuboption(item.GetAddress(),TimeSpan.Zero,TimeSpan.Zero,new DHCPv6PacketSuboption[]{
                new DHCPv6PacketStatusCodeSuboption(statusCode)
                }
                )});

        public static DHCPv6PacketIdentityAssociationNonTemporaryAddressesOption AsSuccess(UInt32 id, TimeSpan T1, TimeSpan T2, IPv6Address address, TimeSpan preferredLifetime, TimeSpan validLifetime) =>
            new DHCPv6PacketIdentityAssociationNonTemporaryAddressesOption(id, T1, T2, new List<DHCPv6PacketSuboption>
                    {
                        new DHCPv6PacketIdentityAssociationAddressSuboption(address, preferredLifetime,validLifetime, new List<DHCPv6PacketSuboption>
                        {
                            new DHCPv6PacketStatusCodeSuboption(DHCPv6StatusCodes.Success,"have a good lease"),
                        })
                    });

        #endregion
    }
}
