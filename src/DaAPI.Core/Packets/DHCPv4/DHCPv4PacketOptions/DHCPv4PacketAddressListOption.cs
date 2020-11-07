using DaAPI.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DaAPI.Core.Packets.DHCPv4
{
    public class DHCPv4PacketAddressListOption : DHCPv4PacketOption, IEquatable<DHCPv4PacketAddressListOption>
    {
        #region Fields

        #endregion

        #region Properties

        public IEnumerable<IPv4Address> Addresses { get; private set; }


        #endregion

        #region constructor and factories

        public DHCPv4PacketAddressListOption(DHCPv4OptionTypes type, IEnumerable<IPv4Address> addresses) : this((Byte)type, addresses)
        {

        }

        public DHCPv4PacketAddressListOption(Byte type, IEnumerable<IPv4Address> addresses) : base(
            type,
            ByteHelper.ConcatBytes(addresses.Select(x => x.GetBytes())))
        {
            if (addresses == null || addresses.Any() == false)
            {
                throw new ArgumentException(nameof(addresses));
            }

            Addresses = new List<IPv4Address>(addresses);
        }

        public static DHCPv4PacketAddressListOption FromByteArray(Byte[] data, Int32 offset)
        {
            if (data == null || data.Length < offset + 4)
            {
                throw new ArgumentException(nameof(data));
            }

            Byte length = data[offset + 1];
            Int32 addressAmount = length / 4;

            Int32 index = offset + 2;

            IPv4Address[] addresses = new IPv4Address[addressAmount];

            for (int i = 0; i < addressAmount; i++, index += 4)
            {
                IPv4Address address = IPv4Address.FromByteArray(data, index);
                addresses[i] = address;
            }

            return new DHCPv4PacketAddressListOption(data[offset], addresses);
        }

        #endregion

        #region Methods

        public bool Equals(DHCPv4PacketAddressListOption other)
        {
            return base.Equals(other);
        }

        public override string ToString()
        {
            String values = String.Empty;
            foreach (var item in Addresses)
            {
                values += $"{item},";
            }

            return $"type: {OptionType} | addresses : {values}";
        }

        #endregion

    }
}
