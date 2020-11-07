using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Common
{
    public class UnknownDUID : DUID
    {
        #region constructor and factories

        private UnknownDUID() : base()
        {

        }

        public UnknownDUID(Byte[] data) : base(DUIDTypes.Unknown, data)
        {

        }

        public static UnknownDUID FromByteArray(Byte[] data, Int32 offset)
        {
            //UInt16 code = ByteHelper.ConvertToUInt16FromByte(data, offset);
            return new UnknownDUID(ByteHelper.CopyData(data, offset + 2));
        }

        #endregion


    }
}
