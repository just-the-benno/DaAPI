using DaAPI.Core.Common;
using DaAPI.Core.Common.DHCPv6;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DaAPI.Core.Packets.DHCPv6
{
    public class DHCPv6PacketIdentityAssociationAddressSuboption : DHCPv6PacketSuboption, IEquatable<DHCPv6PacketIdentityAssociationAddressSuboption>
    {
        #region Properties

        public IPv6Address Address { get; private set; }
        public TimeSpan PreferredLifetime { get; private set; }
        public TimeSpan ValidLifetime { get; private set; }
        public IEnumerable<DHCPv6PacketSuboption> Suboptions { get; private set; }

        #endregion

        #region Constructor

        public DHCPv6PacketIdentityAssociationAddressSuboption(
            IPv6Address address, TimeSpan preferedLifetime, TimeSpan validLifetime,
            IEnumerable<DHCPv6PacketSuboption> suboptions)
            : base((UInt16)DHCPv6PacketSuboptionsType.IdentityAssociationAddress,
                  ByteHelper.ConcatBytes(
                        address.GetBytes(),
                        ByteHelper.GetBytes((UInt32)preferedLifetime.TotalSeconds),
                        ByteHelper.GetBytes((UInt32)validLifetime.TotalSeconds),
                        ByteHelper.ConcatBytes(suboptions.Select(x => x.GetByteStream()))
                ))
        {
            Address = address;
            PreferredLifetime = preferedLifetime;
            ValidLifetime = validLifetime;
            Suboptions = new List<DHCPv6PacketSuboption>(suboptions);
        }

        public static DHCPv6PacketIdentityAssociationAddressSuboption FromByteArray(Byte[] data, Int32 offset)
        {
            UInt16 lenght = ByteHelper.ConvertToUInt16FromByte(data, offset + 2);
            IPv6Address address = IPv6Address.FromByteArray(data, offset + 4);

            UInt32 preferredLifetime = ByteHelper.ConvertToUInt32FromByte(data, offset + 4 + 16);
            UInt32 validLifetime = ByteHelper.ConvertToUInt32FromByte(data, offset + 4 + 16 + 4);

            List<DHCPv6PacketSuboption> suboptions = new List<DHCPv6PacketSuboption>();
            if (lenght > 16 + 4 + 4)
            {
                Byte[] subOptionsData = ByteHelper.CopyData(data, offset + 4 + 16 + 4 + 4);
                suboptions.AddRange(DHCPv6PacketSuboptionFactory.GetOptions(subOptionsData));
            }

            return new DHCPv6PacketIdentityAssociationAddressSuboption(
                address, TimeSpan.FromSeconds(preferredLifetime), TimeSpan.FromSeconds(validLifetime),
                suboptions);
        }


        #endregion

        #region Methods

        public override string ToString()
        {
            return $"type: {Code} | address : {Address} | pref: {PreferredLifetime} | valid: {ValidLifetime}";
        }

        public bool Equals(DHCPv6PacketIdentityAssociationAddressSuboption other)
        {
            return base.Equals(other);
        }

        #endregion
    }
}
