using DaAPI.Core.Common;
using DaAPI.Core.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DaAPI.Core.Packets.DHCPv6
{
    public class DHCPv6PacketVendorClassOption : DHCPv6PacketOption, IEquatable<DHCPv6PacketVendorClassOption>
    {
        #region Properties

        public IEnumerable<Byte[]> VendorClassData { get; private set; }
        public UInt32 EnterpriseNumber { get; private set; }

        #endregion

        #region Constructor

        public DHCPv6PacketVendorClassOption(UInt32 enterpriseNumber, IEnumerable<Byte[]> vendorClasses) :
            base((UInt16)DHCPv6PacketOptionTypes.VendorClass,
                ByteHelper.ConcatBytes(ByteHelper.GetBytes(enterpriseNumber),
                    ByteHelper.ConcatBytes(vendorClasses.Select(x => ByteHelper.ConcatBytes(ByteHelper.GetBytes((UInt16)x.Length), x)))))
        {
            EnterpriseNumber = enterpriseNumber;
            VendorClassData = vendorClasses;
        }

        public static DHCPv6PacketVendorClassOption FromByteArray(Byte[] data, Int32 offset)
        {
            UInt16 length = ByteHelper.ConvertToUInt16FromByte(data, offset + 2);
            UInt32 enterpriseNumber = ByteHelper.ConvertToUInt32FromByte(data, offset + 4);

            Int32 pointer = 4;
            List<Byte[]> vendorClasses = new List<byte[]>();
            while (pointer < length)
            {
                UInt16 classLength = ByteHelper.ConvertToUInt16FromByte(data, offset + 4 + pointer);
                Byte[] classData = ByteHelper.CopyData(data, offset + 4 + pointer + 2, classLength);
                vendorClasses.Add(classData);

                pointer += 2 + classLength;
            }

            return new DHCPv6PacketVendorClassOption(enterpriseNumber, vendorClasses);
        }

        #endregion

        #region Methods

        public override string ToString()
        {
            String options = String.Empty;
            foreach (var item in VendorClassData)
            {
                options += $"{ByteHelper.ToString(item)},";
            }

            return $"vendor classes: {options}";
        }

        public bool Equals(DHCPv6PacketVendorClassOption other)
        {
            return base.Equals(other);
        }

        public override bool Equals(object other) =>
          other is DHCPv6PacketVendorClassOption option ? Equals(option) : base.Equals(other);

        public override int GetHashCode() => Data != null ? Data.Length : 0;

        #endregion
    }
}
