using DaAPI.Core.Common;
using DaAPI.Core.Helper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DaAPI.Core.Packets.DHCPv6
{
    public class DHCPv6PacketVendorSpecificInformationOption : DHCPv6PacketOption, IEquatable<DHCPv6PacketVendorSpecificInformationOption>
    {
        #region Properties

        public UInt32 EnterpriseNumber { get; private set; }
        public IEnumerable<DHCPv6VendorOptionData> Options { get; private set; }

        #endregion

        #region Constructor

        public DHCPv6PacketVendorSpecificInformationOption(
            UInt32 enterpriseNumber, IEnumerable<DHCPv6VendorOptionData> options) : base((UInt16)DHCPv6PacketOptionTypes.VendorOptions,
                ByteHelper.ConcatBytes(ByteHelper.GetBytes(enterpriseNumber), ByteHelper.ConcatBytes(options.Select(x => x.GetByteStream()))))
        {
            EnterpriseNumber = enterpriseNumber;
            Options = new List<DHCPv6VendorOptionData>(options);
        }

        public static DHCPv6PacketVendorSpecificInformationOption FromByteArray(Byte[] data, Int32 offset)
        {
            UInt16 length = ByteHelper.ConvertToUInt16FromByte(data, offset + 2);
            UInt32 enterpriseNumber = ByteHelper.ConvertToUInt32FromByte(data, offset + 4);

            Int32 pointer = 4;
            List<DHCPv6VendorOptionData> vendorInformations = new List<DHCPv6VendorOptionData>();
            while (pointer < length)
            {
                UInt16 optionCode = ByteHelper.ConvertToUInt16FromByte(data, offset + 4 + pointer);
                UInt16 optionLength = ByteHelper.ConvertToUInt16FromByte(data, offset + 4 + pointer + 2);

                Byte[] optionData = ByteHelper.CopyData(data, offset + 4 + pointer + 4, optionLength);
                vendorInformations.Add(new DHCPv6VendorOptionData(optionCode, optionData));

                pointer += 4 + optionLength;
            }

            return new DHCPv6PacketVendorSpecificInformationOption(enterpriseNumber, vendorInformations);
        }

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"type: {Code} | enterprise : {EnterpriseNumber} | options: {Options.Count()}";
        }

        public bool Equals(DHCPv6PacketVendorSpecificInformationOption other)
        {
            return base.Equals(other);
        }

        #endregion
    }
}
