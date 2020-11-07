using DaAPI.Core.Common;
using DaAPI.Core.Helper;
using System;

namespace DaAPI.Core.Packets.DHCPv6
{
    public class DHCPv6PacketRemoteIdentifierOption : DHCPv6PacketOption
    {
        #region Properties

        public UInt32 EnterpriseNumber { get; private set; }
        public Byte[] Value { get; private set; }

        #endregion

        #region Constructor

        public DHCPv6PacketRemoteIdentifierOption(UInt32 enterpriseNumber, Byte[] value)
            : base((UInt16)DHCPv6PacketOptionTypes.RemoteIdentifier,
                  ByteHelper.ConcatBytes(ByteHelper.GetBytes(enterpriseNumber),value))
        {
            EnterpriseNumber = enterpriseNumber;
            Value = value;
        }

        public static DHCPv6PacketRemoteIdentifierOption FromByteArray(Byte[] data, Int32 offset)
        {
            UInt16 length = ByteHelper.ConvertToUInt16FromByte(data, offset + 2);

            UInt32 enterpriseNumner = ByteHelper.ConvertToUInt32FromByte(data, offset + 4);
            Byte[] value = ByteHelper.CopyData(data, offset + 8, length - 4);

            return new DHCPv6PacketRemoteIdentifierOption(enterpriseNumner, value);
        }

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"type: {Code} | Enterprise : {EnterpriseNumber} | Value: {ByteHelper.ToString(Value)}";
        }

        public bool Equals(DHCPv6PacketUInt32Option other)
        {
            return base.Equals(other);
        }

        #endregion
    }
}
