using DaAPI.Core.Common;
using DaAPI.Core.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Packets.DHCPv6
{
    public class DHCPv6VendorOptionData : Value<DHCPv6VendorOptionData>
    {
        #region Properties

        public UInt16 Code { get; set; }
        public Byte[] Data { get; set; }

        #endregion

        #region Constructor

        public DHCPv6VendorOptionData(UInt16 code, Byte[] data)
        {
            Code = code;
            Data = data;
        }

        public Byte[] GetByteStream()
        {
            Byte[] data = new Byte[Data.Length + 4];
            AppendToStream(data, 0);
            return data;
        }

        public Int32 AppendToStream(Byte[] stream, Int32 offset)
        {
            Byte[] optionCode = ByteHelper.GetBytes(Code);
            Byte[] optionLength = ByteHelper.GetBytes((UInt16)Data.Length);

            stream[offset] = optionCode[0];
            stream[offset + 1] = optionCode[1];

            stream[offset + 2] = optionLength[0];
            stream[offset + 3] = optionLength[1];

            Data.CopyTo(stream, offset + 4);

            return 4 + Data.Length;
        }

        #endregion
    }
}
