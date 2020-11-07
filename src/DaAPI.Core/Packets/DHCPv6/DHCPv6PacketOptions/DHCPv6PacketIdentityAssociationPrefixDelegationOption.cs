using DaAPI.Core.Common;
using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Helper;
using DaAPI.Core.Scopes.DHCPv6;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DaAPI.Core.Packets.DHCPv6
{
    public class DHCPv6PacketIdentityAssociationPrefixDelegationOption :
        DHCPv6PacketIdentityAsociationOption, IEquatable<DHCPv6PacketIdentityAssociationPrefixDelegationOption>
    {
        #region Properties

        public TimeSpan T1 { get; private set; }
        public TimeSpan T2 { get; private set; }

        #endregion

        #region Constructor

        public DHCPv6PacketIdentityAssociationPrefixDelegationOption(UInt32 id,
            TimeSpan t1, TimeSpan t2, IEnumerable<DHCPv6PacketSuboption> suboptions) : base(
                (UInt16)DHCPv6PacketOptionTypes.IdentityAssociation_PrefixDelegation, id,
                ByteHelper.ConcatBytes(ByteHelper.GetBytes((UInt32)t1.TotalSeconds), ByteHelper.GetBytes((UInt32)t2.TotalSeconds)),
                suboptions)
        {
            T1 = t1;
            T2 = t2;
        }

        public static DHCPv6PacketIdentityAssociationPrefixDelegationOption FromByteArray(Byte[] data, Int32 offset)
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

            return new DHCPv6PacketIdentityAssociationPrefixDelegationOption(
                identifier, TimeSpan.FromSeconds(T1), TimeSpan.FromSeconds(T2),
                suboptions);
        }

        #endregion

        #region Methods

        public DHCPv6PrefixDelegation GetPrefixDelegation()
        {
            var suboption = GetPrefixSuboption();
            if (suboption == null)
            {
                return DHCPv6PrefixDelegation.None;
            }
            else
            {
                return DHCPv6PrefixDelegation.FromValues(
                    suboption.Address,
                    new IPv6SubnetMask(new IPv6SubnetMaskIdentifier(suboption.PrefixLength)),
                    Id);
            }
        }

        public DHCPv6PacketIdentityAssociationPrefixDelegationSuboption GetPrefixSuboption() => Suboptions.OfType<DHCPv6PacketIdentityAssociationPrefixDelegationSuboption>().FirstOrDefault();

        public override string ToString()
        {
            return $"type: {Code} | id : {Id} | T1 : {T1} | T1 : {T2}";
        }

        public bool Equals(DHCPv6PacketIdentityAssociationPrefixDelegationOption other)
        {
            return base.Equals(other);
        }

        private IPv6Address GetNetworkAddress()
        {
            if (Suboptions.OfType<DHCPv6PacketIdentityAssociationPrefixDelegationSuboption>().Any() == false)
            {
                return IPv6Address.Empty;
            }
            else
            {
                return Suboptions.OfType<DHCPv6PacketIdentityAssociationPrefixDelegationSuboption>().Select(x => x.Address).First();
            }
        }

        private Byte GetPrefixLength()
        {
            if (Suboptions.OfType<DHCPv6PacketIdentityAssociationPrefixDelegationSuboption>().Any() == false)
            {
                return 0;
            }
            else
            {
                return Suboptions.OfType<DHCPv6PacketIdentityAssociationPrefixDelegationSuboption>().Select(x => x.PrefixLength).First();
            }
        }

        public static DHCPv6PacketIdentityAssociationPrefixDelegationOption NotAvailable(DHCPv6PacketIdentityAssociationPrefixDelegationOption item) =>
            Error(item, DHCPv6StatusCodes.NoPrefixAvail);

        public static DHCPv6PacketIdentityAssociationPrefixDelegationOption Error(DHCPv6PacketIdentityAssociationPrefixDelegationOption item, DHCPv6StatusCodes statusCode) =>
          new DHCPv6PacketIdentityAssociationPrefixDelegationOption(item.Id, TimeSpan.Zero, TimeSpan.Zero, new DHCPv6PacketSuboption[] { new
                DHCPv6PacketIdentityAssociationPrefixDelegationSuboption(TimeSpan.Zero,TimeSpan.Zero,item.GetPrefixLength(), item.GetNetworkAddress(), new DHCPv6PacketSuboption[]{
                new DHCPv6PacketStatusCodeSuboption(statusCode)
                }
                )});

        public static DHCPv6PacketIdentityAssociationPrefixDelegationOption AsSuccess(UInt32 id, TimeSpan T1, TimeSpan T2, Byte prefixLength, IPv6Address prefixAddress, TimeSpan preferredLifetime, TimeSpan validLifetime) =>
           new DHCPv6PacketIdentityAssociationPrefixDelegationOption(id, T1, T2, new List<DHCPv6PacketSuboption>
                   {
                        new DHCPv6PacketIdentityAssociationPrefixDelegationSuboption(preferredLifetime,validLifetime, prefixLength,prefixAddress, new List<DHCPv6PacketSuboption>
                        {
                            new DHCPv6PacketStatusCodeSuboption(DHCPv6StatusCodes.Success,"have a good lease"),
                        })
                   });

        #endregion
    }
}
