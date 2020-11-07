using DaAPI.Core.Common;
using DaAPI.Core.Helper;
using System;
using System.Collections.Generic;

namespace DaAPI.Core.Packets.DHCPv6
{
    public static class DHCPv6PacketOptionFactory
    {
        #region Fields

        private static readonly Dictionary<UInt16, Func<Byte[], DHCPv6PacketOption>> _constructorDict;

        #endregion

        #region Constructor

        static DHCPv6PacketOptionFactory()
        {
            _constructorDict = new Dictionary<ushort, Func<byte[], DHCPv6PacketOption>>
            {
                { (UInt16)DHCPv6PacketOptionTypes.ClientIdentifier, (data) => DHCPv6PacketIdentifierOption.FromByteArray(data,0) },
                { (UInt16)DHCPv6PacketOptionTypes.ServerIdentifer, (data) =>  DHCPv6PacketIdentifierOption.FromByteArray(data,0) },
                { (UInt16)DHCPv6PacketOptionTypes.IdentityAssociation_NonTemporary, (data) =>  DHCPv6PacketIdentityAssociationNonTemporaryAddressesOption.FromByteArray(data,0) },
                { (UInt16)DHCPv6PacketOptionTypes.IdentityAssociation_Temporary, (data) =>  DHCPv6PacketIdentityAssociationTemporaryAddressesOption.FromByteArray(data,0) },
                { (UInt16)DHCPv6PacketOptionTypes.OptionRequest, (data) =>  DHCPv6PacketOptionRequestOption.FromByteArray(data,0) },
                { (UInt16)DHCPv6PacketOptionTypes.Preference, (data) =>  DHCPv6PacketByteOption.FromByteArray(data,0) },
                { (UInt16)DHCPv6PacketOptionTypes.ElapsedTime, (data) =>  DHCPv6PacketTimeOption.FromByteArray(data,0, DHCPv6PacketTimeOption.DHCPv6PacketTimeOptionUnits.HundredsOfSeconds) },
                { (UInt16)DHCPv6PacketOptionTypes.ServerUnicast, (data) =>  DHCPv6PacketIPAddressOption.FromByteArray(data,0) },
                { (UInt16)DHCPv6PacketOptionTypes.RapitCommit, (data) =>  DHCPv6PacketTrueOption.FromByteArray(data,0) },
                { (UInt16)DHCPv6PacketOptionTypes.UserClass, (data) =>  DHCPv6PacketUserClassOption.FromByteArray(data,0) },
                { (UInt16)DHCPv6PacketOptionTypes.VendorClass, (data) =>  DHCPv6PacketVendorClassOption.FromByteArray(data,0) },
                { (UInt16)DHCPv6PacketOptionTypes.VendorOptions, (data) =>  DHCPv6PacketVendorSpecificInformationOption.FromByteArray(data,0) },
                { (UInt16)DHCPv6PacketOptionTypes.InterfaceId, (data) =>  DHCPv6PacketByteArrayOption.FromByteArray(data,0) },
                { (UInt16)DHCPv6PacketOptionTypes.Reconfigure, (data) =>  DHCPv6PacketReconfigureOption.FromByteArray(data,0) },
                { (UInt16)DHCPv6PacketOptionTypes.ReconfigureAccepte, (data) =>  DHCPv6PacketTrueOption.FromByteArray(data,0) },
                { (UInt16)DHCPv6PacketOptionTypes.IdentityAssociation_PrefixDelegation, (data) =>  DHCPv6PacketIdentityAssociationPrefixDelegationOption.FromByteArray(data,0) },
                { (UInt16)DHCPv6PacketOptionTypes.InformationRefreshTime, (data) =>  DHCPv6PacketTimeOption.FromByteArray(data,0, DHCPv6PacketTimeOption.DHCPv6PacketTimeOptionUnits.Seconds) },
                { (UInt16)DHCPv6PacketOptionTypes.RemoteIdentifier, (data) =>  DHCPv6PacketRemoteIdentifierOption.FromByteArray(data,0) },
                { (UInt16)DHCPv6PacketOptionTypes.SOL_MAX_RT, (data) =>  DHCPv6PacketUInt32Option.FromByteArray(data,0) },
                { (UInt16)DHCPv6PacketOptionTypes.INF_MAX_RT, (data) =>  DHCPv6PacketUInt32Option.FromByteArray(data,0) },
                { (UInt16)DHCPv6PacketOptionTypes.DNSServer, (data) =>  DHCPv6PacketIPAddressListOption.FromByteArray(data,0) },
                { (UInt16)DHCPv6PacketOptionTypes.NTPServer, (data) =>  DHCPv6PacketIPAddressListOption.FromByteArray(data,0) },
                { (UInt16)DHCPv6PacketOptionTypes.SNTPServer, (data) =>  DHCPv6PacketIPAddressListOption.FromByteArray(data,0) },
            };
        }

        #endregion

        #region Methods

        public static void AddOptionType(UInt16 code, Func<Byte[], DHCPv6PacketOption> func, Boolean replace)
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

        public static void RemoveOptionType(UInt16 code)
        {
            if (_constructorDict.ContainsKey(code) == false)
            {
                return;
            }

            _constructorDict.Remove(code);
        }

        public static DHCPv6PacketOption GetOption(Byte[] data)
        {
            UInt16 code = ByteHelper.ConvertToUInt16FromByte(data, 0);
            return GetOption(code, data);
        }

        public static DHCPv6PacketOption GetOption(UInt16 code, Byte[] data)
        {
            if (_constructorDict.ContainsKey(code) == true)
            {
                return _constructorDict[code].Invoke(data);
            }
            else
            {
                return new DHCPv6PacketByteArrayOption(code, ByteHelper.CopyData(data, 4));
            }
        }

        public static IEnumerable<DHCPv6PacketOption> GetOptions(Byte[] subOptionData, Int32 offset)
        {
            List<DHCPv6PacketOption> options = new List<DHCPv6PacketOption>();
            Int32 byteIndex = offset;
            while (byteIndex < subOptionData.Length)
            {
                UInt16 optionCode = ByteHelper.ConvertToUInt16FromByte(subOptionData, byteIndex);
                UInt16 length = ByteHelper.ConvertToUInt16FromByte(subOptionData, byteIndex + 2);
                Byte[] optionData = ByteHelper.CopyData(subOptionData, byteIndex, length + 4);

                DHCPv6PacketOption suboption = GetOption(optionCode, optionData);
                options.Add(suboption);

                byteIndex += 4 + length;
            }

            return options;
        }

        #endregion



    }
}
