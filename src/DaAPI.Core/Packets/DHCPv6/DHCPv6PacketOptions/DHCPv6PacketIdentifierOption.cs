using DaAPI.Core.Common;
using DaAPI.Core.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Packets.DHCPv6
{
    public class DHCPv6PacketIdentifierOption : DHCPv6PacketOption, IEquatable<DHCPv6PacketIdentifierOption>
    {
        #region Properties

        public DUID DUID { get; private set; }

        #endregion

        #region Constructor

        public DHCPv6PacketIdentifierOption(DHCPv6PacketOptionTypes code, DUID duid)
            : this((UInt16)code, duid)
        {

        }

        public DHCPv6PacketIdentifierOption(UInt16 code, DUID duid)
            : base(code, duid.GetAsByteStream())
        {
            DUID = duid;
        }

        public static DHCPv6PacketIdentifierOption FromByteArray(Byte[] data, Int32 offset)
        {
            UInt16 code = ByteHelper.ConvertToUInt16FromByte(data, offset);

            DUID duid = DUIDFactory.GetDUID(data, offset + 4);
            if (duid == DUID.Empty)
            {
                throw new ArgumentException("duid");
            }

            return new DHCPv6PacketIdentifierOption(code,duid);
        }

        #endregion

        #region methods

        public override string ToString()
        {
            return $"type: {Code} | value : {DUID}";
        }

        public bool Equals(DHCPv6PacketIdentifierOption other)
        {
            return base.Equals(other);
        }

        #endregion

    }
}
