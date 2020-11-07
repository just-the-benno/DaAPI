using DaAPI.Core.Helper;
using System;
using System.Collections.Generic;
using System.Text;
using static DaAPI.Core.Common.LinkLayerAddressDUID;

namespace DaAPI.Core.Common
{
    public class LinkLayerAddressAndTimeDUID : DUID
    {
        #region const

        private static readonly DateTime _nullReferenceTime = new DateTime(2000, 1, 1);

        #endregion

        #region Properties

        public DateTime Time { get; private set; }
        public DUIDLinkLayerTypes LinkType { get; private set; }
        public Byte[] LinkLayerAddress { get; private set; }

        #endregion

        #region Constructor
        private LinkLayerAddressAndTimeDUID(
            DUIDLinkLayerTypes type,
            Byte[] hwAddress,
            DateTime time, Byte[] raw) : base(DUIDTypes.LinkLayerAndTime, raw)
        {
            LinkType = type;
            LinkLayerAddress = hwAddress;
            Time = time;
        }

        public static LinkLayerAddressAndTimeDUID FromEthernet(
            Byte[] hwAddress, DateTime time)
        {
            if (time < _nullReferenceTime)
            {
                throw new ArgumentException($"the time value must greater than {_nullReferenceTime}", nameof(time));
            }

            if (hwAddress.Length != 6)
            {
                throw new ArgumentException("invalid mac address", nameof(hwAddress));
            }

            Byte[] duidTypeByte = ByteHelper.GetBytes((UInt16)DUIDTypes.LinkLayerAndTime);
            Byte[] hwTypeByte = ByteHelper.GetBytes((UInt16)DUIDLinkLayerTypes.Ethernet);
            Byte[] timeByte = ByteHelper.GetBytes((UInt32)((time - _nullReferenceTime).TotalSeconds));

            Byte[] concat = ByteHelper.ConcatBytes(
                new List<Byte[]> { duidTypeByte, hwTypeByte, timeByte, hwAddress });

            return FromByteArray(concat, 0);
        }

        public static LinkLayerAddressAndTimeDUID FromByteArray(Byte[] data, Int32 offset)
        {
            UInt16 code = ByteHelper.ConvertToUInt16FromByte(data, offset);
            if (code != (UInt16)DUIDTypes.LinkLayerAndTime)
            {
                throw new ArgumentException($"invalid duid type. expected {(UInt16)DUIDTypes.LinkLayerAndTime} actual {code}");
            }

            DUIDLinkLayerTypes linkLayerType = (DUIDLinkLayerTypes)ByteHelper.ConvertToUInt16FromByte(data, offset + 2);
            UInt32 seconds = ByteHelper.ConvertToUInt32FromByte(data, offset + 4);
            Byte[] hwAddress = ByteHelper.CopyData(data, offset + 8);

            DateTime time = _nullReferenceTime + TimeSpan.FromSeconds(seconds);

            return new LinkLayerAddressAndTimeDUID(linkLayerType, hwAddress, time, ByteHelper.CopyData(data, offset + 2));
        }

        #endregion
    }
}
