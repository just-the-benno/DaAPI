using DaAPI.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Packets.DHCPv4
{
    public class DHCPv4PacketAddressOption : DHCPv4PacketOption, IEquatable<DHCPv4PacketAddressOption>
    {
        #region Fields

        private const Int32 _expectedDataLength = 4;

        #endregion

        #region Properties

        public IPv4Address Address { get; private set; }

        #endregion

        #region constructor and factories

        public DHCPv4PacketAddressOption(DHCPv4OptionTypes type, IPv4Address address) : this((Byte)type, address)
        {

        }

        public DHCPv4PacketAddressOption(Byte type, IPv4Address address) : base(type,  address.GetBytes())
        {
            Address = address;
        }

        public static DHCPv4PacketAddressOption FromByteArray(Byte[] data, Int32 offset)
        {
            if (data == null || data.Length < offset + 2 + _expectedDataLength)
            {
                throw new ArgumentException(nameof(data));
            }

            if (data[offset + 1] != _expectedDataLength)
            {
                throw new ArgumentException(nameof(data));
            }

            IPv4Address address = IPv4Address.FromByteArray(data, offset + 2);
            return new DHCPv4PacketAddressOption(data[offset], address);
        }

        #endregion

        #region Methods

        public bool Equals(DHCPv4PacketAddressOption other)
        {
            return base.Equals(other);
        }

        public override string ToString()
        {
            return $"type: {OptionType} | value : {Address}";
        }

        #endregion

    }
}
