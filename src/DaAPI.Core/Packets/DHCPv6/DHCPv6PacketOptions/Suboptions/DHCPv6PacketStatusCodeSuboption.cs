using DaAPI.Core.Common;
using DaAPI.Core.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Packets.DHCPv6
{
    public enum DHCPv6StatusCodes : ushort
    {
        Success = 0,
        UnspecFail = 1,
        NoAddrsAvail = 2,
        NoBinding = 3,
        NotOnLink = 4,
        UseMulticast = 5,
        NoPrefixAvail = 6,
    }

    public class DHCPv6PacketStatusCodeSuboption : DHCPv6PacketSuboption, IEquatable<DHCPv6PacketStatusCodeSuboption>
    {
        #region Properties

        public UInt16 StatusCode { get; private set; }
        public String Message { get; private set; }

        #endregion

        #region Constructor

        public DHCPv6PacketStatusCodeSuboption(UInt16 statusCode, String message) :
            base((UInt16)DHCPv6PacketSuboptionsType.StatusCode,
                ByteHelper.ConcatBytes(
                    ByteHelper.GetBytes(statusCode), Encoding.UTF8.GetBytes(message)))
        {
            StatusCode = statusCode;
            Message = message;
        }

        public DHCPv6PacketStatusCodeSuboption(UInt16 statusCode) : this(statusCode, String.Empty)
        {

        }

        public DHCPv6PacketStatusCodeSuboption(DHCPv6StatusCodes statusCode, String message) : this((UInt16)statusCode, message)
        {

        }

        public DHCPv6PacketStatusCodeSuboption(DHCPv6StatusCodes statusCode) : this(statusCode, String.Empty)
        {

        }

        public static DHCPv6PacketStatusCodeSuboption FromByteArray(Byte[] data, Int32 offset)
        {
            UInt16 length = ByteHelper.ConvertToUInt16FromByte(data, offset + 2);
            UInt16 statusCode = ByteHelper.ConvertToUInt16FromByte(data, offset + 4);

            String message = String.Empty;
            if (length > 2)
            {
                message = Encoding.UTF8.GetString(ByteHelper.CopyData(data, offset + 6));
            }

            return new DHCPv6PacketStatusCodeSuboption(statusCode, message);
        }

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"type: {Code} | statuscode: {StatusCode} | message {Message}";
        }

        public bool Equals(DHCPv6PacketStatusCodeSuboption other)
        {
            return base.Equals(other);
        }

        #endregion
    }
}
