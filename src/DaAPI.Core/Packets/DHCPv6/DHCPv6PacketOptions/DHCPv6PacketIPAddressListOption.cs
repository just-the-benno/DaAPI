using DaAPI.Core.Common;
using DaAPI.Core.Common.DHCPv6;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DaAPI.Core.Packets.DHCPv6
{
    public class DHCPv6PacketIPAddressListOption : DHCPv6PacketOption, IEquatable<DHCPv6PacketIPAddressListOption>
    {
        #region Properties

        public IEnumerable<IPv6Address> Addresses { get; private set; }

        #endregion

        #region Cosntructor

        public DHCPv6PacketIPAddressListOption(UInt16 code, IEnumerable<IPv6Address> addresses) :
            base(code, ByteHelper.ConcatBytes(addresses.Select(x => x.GetBytes())))
        {
            Addresses = addresses;
        }

        public DHCPv6PacketIPAddressListOption(DHCPv6PacketOptionTypes code, IEnumerable<IPv6Address> addresses) :
            this((UInt16)code, addresses)
        {

        }

        public static DHCPv6PacketIPAddressListOption FromByteArray(Byte[] data, Int32 offset)
        {
            if (data == null || data.Length < offset + 4)
            {
                throw new ArgumentException(nameof(data));
            }

            UInt16 code = ByteHelper.ConvertToUInt16FromByte(data, offset);
            UInt16 length = ByteHelper.ConvertToUInt16FromByte(data, offset + 2);

            Int32 addressAmount = length / 16;
            var addresses = new IPv6Address[addressAmount];
            for (int i = 0; i < addressAmount; i++)
            {
                IPv6Address address = IPv6Address.FromByteArray(data, offset + 4 + (i *16));
                addresses[i] = address;
            }

            return new DHCPv6PacketIPAddressListOption(code, addresses);
        }

        #endregion

        #region Methods

        public override string ToString() => $"type: {Code} | value : {Addresses}";

        public bool Equals(DHCPv6PacketIPAddressListOption other) => base.Equals(other);

        #endregion
    }
}
