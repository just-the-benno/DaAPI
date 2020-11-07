using System;
using System.Collections.Generic;

namespace DaAPI.Core.Common
{
    public class UUIDDUID : DUID
    {
        #region Properties

        public Guid UUID { get; private set; }

        #endregion

        #region constructors and factories

        private UUIDDUID() : base()
        {

        }

        public UUIDDUID(Guid guid) : base(DUIDTypes.Uuid,guid.ToByteArray())
        {
            UUID = guid;
        }

        public static UUIDDUID FromByteArray(Byte[] data, Int32 offset)
        {
            UInt16 code = ByteHelper.ConvertToUInt16FromByte(data, offset);
            if(code != (UInt16)DUIDTypes.Uuid)
            {
                throw new ArgumentException($"invalid duid type. expected {(UInt16)DUIDTypes.Uuid} actual {code}");
            }

            Guid guid = new Guid(ByteHelper.CopyData(data, offset + 2, 16));
            return new UUIDDUID(guid);
        }

        #endregion
    }
}
