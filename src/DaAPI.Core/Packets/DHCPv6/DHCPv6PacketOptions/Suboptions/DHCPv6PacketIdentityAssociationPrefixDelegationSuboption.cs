using DaAPI.Core.Common;
using DaAPI.Core.Common.DHCPv6;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DaAPI.Core.Packets.DHCPv6
{
    public class DHCPv6PacketIdentityAssociationPrefixDelegationSuboption : DHCPv6PacketSuboption, IEquatable<DHCPv6PacketIdentityAssociationPrefixDelegationSuboption>
    {
        #region Properties

        public TimeSpan PreferredLifetime { get; private set; }
        public TimeSpan ValidLifetime { get; private set; }
        public Byte PrefixLength { get; private set; }
        public IPv6Address Address { get; private set; }
        public IEnumerable<DHCPv6PacketSuboption> Suboptions { get; private set; }

        #endregion

        #region Constructor

        public DHCPv6PacketIdentityAssociationPrefixDelegationSuboption(
             TimeSpan preferedLifetime, TimeSpan validLifetime, Byte prefixLength, IPv6Address address,
            IEnumerable<DHCPv6PacketSuboption> suboptions)
            : base((UInt16)DHCPv6PacketSuboptionsType.IdentityAssociationPrefixDelegation,
                  ByteHelper.ConcatBytes(
                        ByteHelper.GetBytes((UInt32)preferedLifetime.TotalSeconds),
                        ByteHelper.GetBytes((UInt32)validLifetime.TotalSeconds),
                        new Byte[] { prefixLength },
                        address.GetBytes(),
                        ByteHelper.ConcatBytes(suboptions.Select(x => x.GetByteStream()))
                ))
        {
            Address = address;
            PreferredLifetime = preferedLifetime;
            ValidLifetime = validLifetime;
            PrefixLength = prefixLength;
            Suboptions = new List<DHCPv6PacketSuboption>(suboptions);
        }

        public static DHCPv6PacketIdentityAssociationPrefixDelegationSuboption FromByteArray(Byte[] data, Int32 offset)
        {
            UInt16 lenght = ByteHelper.ConvertToUInt16FromByte(data, offset + 2);

            UInt32 preferredLifetime = ByteHelper.ConvertToUInt32FromByte(data, offset + 4);
            UInt32 validLifetime = ByteHelper.ConvertToUInt32FromByte(data, offset + 4 + 4);
            Byte prefixLengh = data[offset + 4 + 4 + 4];
            IPv6Address address = IPv6Address.FromByteArray(data, offset + 4 + 4 + 4 + 1);

            List<DHCPv6PacketSuboption> suboptions = new List<DHCPv6PacketSuboption>();
            if (lenght > 4 + 4 + 1 + 16)
            {
                Byte[] subOptionsData = ByteHelper.CopyData(data, offset + 4 + 4 + 4 + 1 + 16);
                suboptions.AddRange(DHCPv6PacketSuboptionFactory.GetOptions(subOptionsData));
            }

            return new DHCPv6PacketIdentityAssociationPrefixDelegationSuboption(
                TimeSpan.FromSeconds(preferredLifetime), TimeSpan.FromSeconds(validLifetime), prefixLengh, address,
                suboptions);
        }

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"type: {Code} | prefix : {Address}/{PrefixLength} | pref: {PreferredLifetime} | valid: {ValidLifetime}";
        }

        public bool Equals(DHCPv6PacketIdentityAssociationPrefixDelegationSuboption other)
        {
            return base.Equals(other);
        }

        #endregion

    }
}
