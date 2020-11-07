using DaAPI.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Packets.DHCPv4
{
    public class DHCPv4PacketClientIdentifierOption : DHCPv4PacketOption, IEquatable<DHCPv4PacketClientIdentifierOption>
    {
        #region Properties

        public DHCPv4ClientIdentifier Identifier { get; private set; }

        #endregion

        #region constructor and factories

        private DHCPv4PacketClientIdentifierOption(DHCPv4ClientIdentifier identifier) : 
            base((Byte)DHCPv4OptionTypes.ClientIdentifier, identifier.DUID != DUID.Empty ? identifier.DUID.GetAsByteStream() : identifier.HwAddress  )
        {

        }

        public DHCPv4PacketClientIdentifierOption FromByteArray(Byte[] data)
        {
            return FromByteArray(data, 0);
        }

        public static DHCPv4PacketClientIdentifierOption FromByteArray(Byte[] data, Int32 offset)
        {
            if (data == null || data.Length < offset + 2)
            {
                throw new ArgumentException(nameof(data));
            }

            if (data[offset] != (Byte)DHCPv4OptionTypes.ClientIdentifier)
            {
                throw new ArgumentException(nameof(data));
            }

            Int32 lenght = data[offset + 1];

            Byte thirdByte = data[offset + 2];
            if (thirdByte == 255)
            {
                // try to convert in option RFC 4361
                if(lenght > 7)
                {
                    //UInt32 iaid = ByteHelper.ConvertToUInt32FromByte(data, offset + 3);
                    try
                    {
                        DUID duid = DUIDFactory.GetDUID(data, offset + 7);
                        return new DHCPv4PacketClientIdentifierOption(DHCPv4ClientIdentifier.FromDuid(duid));
                    }
                    catch (Exception)
                    {
                    }
                 
                }
            }

            Byte[] hwAddress = ByteHelper.CopyData(data, offset + 2, lenght);
            return new DHCPv4PacketClientIdentifierOption(DHCPv4ClientIdentifier.FromHwAddress(hwAddress));
        }

        #endregion

        #region Methods

        public bool Equals(DHCPv4PacketClientIdentifierOption other)
        {
            return base.Equals(other);
        }

        #endregion

    }
}
