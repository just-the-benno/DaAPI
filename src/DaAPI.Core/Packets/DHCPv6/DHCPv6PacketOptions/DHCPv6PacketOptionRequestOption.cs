using DaAPI.Core.Common;
using DaAPI.Core.Helper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DaAPI.Core.Packets.DHCPv6
{
    public class DHCPv6PacketOptionRequestOption : DHCPv6PacketOption, IEquatable<DHCPv6PacketOptionRequestOption>
    {
        #region Properties

        public IEnumerable<UInt16> RequestedOptions { get; private set; }

        #endregion

        #region Constructor

        public DHCPv6PacketOptionRequestOption(IEnumerable<UInt16> requestedOptions)
            : base((UInt16)DHCPv6PacketOptionTypes.OptionRequest, ByteHelper.ConcatBytes(requestedOptions.Select(x => ByteHelper.GetBytes(x))))
        {
            RequestedOptions = new List<UInt16>(requestedOptions);
        }

        public DHCPv6PacketOptionRequestOption(IEnumerable<DHCPv6PacketOptionTypes> requestedOptions)
    : this(requestedOptions.Cast<UInt16>())
        {

        }

        public static DHCPv6PacketOptionRequestOption FromByteArray(Byte[] data, Int32 offset)
        {
            UInt16 length = ByteHelper.ConvertToUInt16FromByte(data, offset + 2);

            Int32 pointer = offset + 4;
            UInt16[] options = new UInt16[length / 2];
            for (int i = 0; i < length / 2; i++)
            {
                UInt16 optionCode = ByteHelper.ConvertToUInt16FromByte(data, pointer);
                options[i] = optionCode;

                pointer += 2;
            }

            return new DHCPv6PacketOptionRequestOption(options);
        }

        #endregion

        #region Methods

        public bool Equals(DHCPv6PacketOptionRequestOption other)
        {
            return base.Equals(other);
        }

        public override string ToString()
        {
            String options = String.Empty;
            foreach (var item in RequestedOptions)
            {
                options += $"{item},";
            }

            return $"requested paramters: {options}";
        }

        #endregion
    }
}
