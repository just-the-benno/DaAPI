using DaAPI.Core.Common;
using DaAPI.Core.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Packets.DHCPv6
{
    public static class DHCPv6PacketSuboptionFactory
    {
        #region Fields

        private static readonly Dictionary<UInt16, Func<Byte[], DHCPv6PacketSuboption>> _constructorDict;

        #endregion

        #region Constructor

        static DHCPv6PacketSuboptionFactory()
        {
            _constructorDict = new Dictionary<ushort, Func<byte[], DHCPv6PacketSuboption>>
            {
                { (UInt16)DHCPv6PacketSuboptionsType.IdentityAssociationAddress, (data) => DHCPv6PacketIdentityAssociationAddressSuboption.FromByteArray(data,0) },
                { (UInt16)DHCPv6PacketSuboptionsType.StatusCode, (data) =>  DHCPv6PacketStatusCodeSuboption.FromByteArray(data,0) },
                { (UInt16)DHCPv6PacketSuboptionsType.IdentityAssociationPrefixDelegation, (data) => DHCPv6PacketIdentityAssociationPrefixDelegationSuboption.FromByteArray(data,0) }
            };
        }

        #endregion

        #region Methods

        public static void AddSuboptionType(UInt16 code, Func<Byte[], DHCPv6PacketSuboption> func, Boolean replace)
        {
            if (_constructorDict.ContainsKey(code) == true)
            {
                if (replace == false)
                {
                    throw new InvalidOperationException("option code exists, try replace instead");
                }

                _constructorDict[code] = func;
            }
            else
            {
                _constructorDict.Add(code, func);
            }
        }

        public static DHCPv6PacketSuboption GetOption(Byte[] data)
        {
            UInt16 code = ByteHelper.ConvertToUInt16FromByte(data, 0);
            return GetOption(code, data);
        }

        public static DHCPv6PacketSuboption GetOption(UInt16 code, Byte[] data)
        {
            if (_constructorDict.ContainsKey(code) == true)
            {
                return _constructorDict[code].Invoke(data);
            }
            else
            {
                return new DHCPv6PacketByteValueSuboption(data);
            }
        }

        public static IEnumerable<DHCPv6PacketSuboption> GetOptions(Byte[] subOptionData)
        {
            List<DHCPv6PacketSuboption> suboptions = new List<DHCPv6PacketSuboption>();
            Int32 byteIndex = 0;
            while (byteIndex < subOptionData.Length)
            {
                UInt16 optionCode = ByteHelper.ConvertToUInt16FromByte(subOptionData, byteIndex);
                UInt16 length = ByteHelper.ConvertToUInt16FromByte(subOptionData, byteIndex + 2);
                Byte[] entryData = ByteHelper.CopyData(subOptionData, byteIndex, length + 4);

                DHCPv6PacketSuboption suboption = GetOption(optionCode, entryData);
                suboptions.Add(suboption);

                byteIndex += 4 + length;
            }

            return suboptions;
        }

        #endregion
    }
}
