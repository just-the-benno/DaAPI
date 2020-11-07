using DaAPI.Core.Helper;
using System;
using System.Collections.Generic;
using System.Text;
using static DaAPI.Core.Common.DUID;

namespace DaAPI.Core.Common
{
    public static class DUIDFactory
    {
        #region Fields

        private static readonly Dictionary<UInt16, Func<Byte[], DUID>> _constructorDict;

        #endregion

        #region Constructor

        static DUIDFactory()
        {
            _constructorDict = new Dictionary<UInt16, Func<byte[], DUID>>
            {
                { (Byte)DUIDTypes.LinkLayer, (data) =>  LinkLayerAddressDUID.FromByteArray(data,0) },
                { (Byte)DUIDTypes.LinkLayerAndTime, (data) =>  LinkLayerAddressAndTimeDUID.FromByteArray(data,0) },
                { (Byte)DUIDTypes.Unknown, (data) =>  UnknownDUID.FromByteArray(data,0) },
                { (Byte)DUIDTypes.Uuid, (data) =>  UUIDDUID.FromByteArray(data,0) },
                { (Byte)DUIDTypes.VendorBased, (data) =>  VendorBasedDUID.FromByteArray(data,0) },
            };
        }

        #endregion

        #region Methods

        public static void AddDUIDType(UInt16 code, Func<Byte[], DUID> func, Boolean replace)
        {
            if (_constructorDict.ContainsKey(code) == true)
            {
                if (replace == false)
                {
                    throw new InvalidOperationException("duid code exists, try replace instead");
                }

                _constructorDict[code] = func;
            }
            else
            {
                _constructorDict.Add(code, func);
            }
        }

        public static DUID GetDUID(Byte[] data, Int32 offset)
        {
            UInt16 code = ByteHelper.ConvertToUInt16FromByte(data, offset);
            return GetDUID(code, ByteHelper.CopyData(data, offset));
        }

        public static DUID GetDUID(Byte[] data)
        {
            return GetDUID(data, 0);
        }

        public static DUID GetDUID(DUIDTypes type, Byte[] data)
        {
            return GetDUID((UInt16)type, data);
        }

        public static DUID GetDUID(UInt16 code, Byte[] data)
        {
            if (_constructorDict.ContainsKey(code) == true)
            {
                return _constructorDict[code].Invoke(data);
            }
            else
            {
                return UnknownDUID.FromByteArray(data, 0);
            }
        }

        #endregion
    }
}
