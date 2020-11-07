using DaAPI.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DaAPI.Core.Packets.DHCPv4
{
    public class DHCPv4PacketParameterRequestListOption : DHCPv4PacketOption, IEquatable<DHCPv4PacketParameterRequestListOption>
    {
        #region Fields


        #endregion

        #region Properties

        public IEnumerable<Byte> RequestOptions { get; private set; }

        #endregion

        #region constructor and factories

        public DHCPv4PacketParameterRequestListOption(IEnumerable<DHCPv4OptionTypes> types) : this(types.Cast<Byte>())
        {

        }

        public DHCPv4PacketParameterRequestListOption(IEnumerable<Byte> types) : base(
            (Byte)DHCPv4OptionTypes.ParamterRequestList,
            ByteHelper.ConcatBytes(types))
        {
            if (types == null || types.Any() == false)
            {
                throw new ArgumentException(nameof(types));
            }

            RequestOptions = new List<Byte>(types);
        }

        public static DHCPv4PacketParameterRequestListOption FromByteArray(Byte[] data, Int32 offset)
        {
            if (data == null || data.Length < offset + 3)
            {
                throw new ArgumentException(nameof(data));
            }

            Byte length = data[offset + 1];
            Int32 index = offset + 2;
            Byte[] types = new Byte[length];
            for (int i = 0; i < length; i++, index++)
            {
                types[i] = data[index];
            }

            return new DHCPv4PacketParameterRequestListOption(types);
        }

        #endregion

        #region Methods

        public bool Equals(DHCPv4PacketParameterRequestListOption other)
        {
            return base.Equals(other);
        }

        public override string ToString()
        {
            String options = String.Empty;
            foreach (var item in RequestOptions)
            {
                options += $"{item},";
            }

            return $"requested paramters: {options}";
        }

        #endregion

    }
}
