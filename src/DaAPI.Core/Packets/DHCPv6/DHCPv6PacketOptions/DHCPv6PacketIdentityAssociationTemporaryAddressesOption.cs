using DaAPI.Core.Common;
using DaAPI.Core.Helper;
using System;
using System.Collections.Generic;

namespace DaAPI.Core.Packets.DHCPv6
{
    public class DHCPv6PacketIdentityAssociationTemporaryAddressesOption :
        DHCPv6PacketIdentityAsociationOption, IEquatable<DHCPv6PacketIdentityAssociationPrefixDelegationOption>
    {
        #region Properties

        #endregion

        #region Constructor

        public DHCPv6PacketIdentityAssociationTemporaryAddressesOption(UInt32 id,
            IEnumerable<DHCPv6PacketSuboption> suboptions) : base(
                (UInt16)DHCPv6PacketOptionTypes.IdentityAssociation_Temporary, id, Array.Empty<Byte>(), suboptions)
        {
        }

        public static DHCPv6PacketIdentityAssociationTemporaryAddressesOption FromByteArray(Byte[] data, Int32 offset)
        {
            UInt16 lenght = ByteHelper.ConvertToUInt16FromByte(data, offset + 2);
            UInt32 identifier = ByteHelper.ConvertToUInt32FromByte(data, offset + 4);

            List<DHCPv6PacketSuboption> suboptions = new List<DHCPv6PacketSuboption>();
            if (lenght > 4)
            {
                Byte[] subOptionsData = ByteHelper.CopyData(data, offset + 8);
                suboptions.AddRange(DHCPv6PacketSuboptionFactory.GetOptions(subOptionsData));
            }

            return new DHCPv6PacketIdentityAssociationTemporaryAddressesOption(identifier, suboptions);
        }

        #endregion

        #region Methods


        public override string ToString()
        {
            return $"type: {Code} | id : {Id}";
        }

        public bool Equals(DHCPv6PacketIdentityAssociationPrefixDelegationOption other)
        {
            return base.Equals(other);
        }

        #endregion
    }
}