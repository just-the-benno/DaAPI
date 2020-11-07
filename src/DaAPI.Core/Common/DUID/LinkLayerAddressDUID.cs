using DaAPI.Core.Helper;
using System;
using System.Collections.Generic;

namespace DaAPI.Core.Common
{
    public class LinkLayerAddressDUID : DUID
    {
        public enum DUIDLinkLayerTypes : ushort
        {
            Ethernet = 1,
        }

        #region Properties

        public DUIDLinkLayerTypes AddressType { get; private set; }
        public Byte[] LinkLayerAddress { get; private set; }

        #endregion

        #region Constructor

        private LinkLayerAddressDUID() : base()
        {

        }

        public LinkLayerAddressDUID(DUIDLinkLayerTypes addressType, Byte[] linkLayerAddress) : base(
            DUIDTypes.LinkLayer, 
            ByteHelper.GetBytes((UInt16)addressType),
            linkLayerAddress)
        {
            if (addressType == DUIDLinkLayerTypes.Ethernet)
            {
                if (linkLayerAddress.Length != 6)
                {
                    throw new ArgumentException("invalid mac address", nameof(linkLayerAddress));
                }                                                                                                       
            }

            AddressType = addressType;
            LinkLayerAddress = ByteHelper.CopyData(linkLayerAddress);
        }
      
        public static LinkLayerAddressDUID FromByteArray(Byte[] data, Int32 offset) 
        {
            UInt16 code = ByteHelper.ConvertToUInt16FromByte(data, offset);
            if (code != (UInt16)DUIDTypes.LinkLayer)
            {
                throw new ArgumentException($"invalid duid type. expected {(UInt16)DUIDTypes.LinkLayer} actual {code}");
            }

            DUIDLinkLayerTypes linkLayerType = (DUIDLinkLayerTypes)ByteHelper.ConvertToUInt16FromByte(data, offset+2);
            Byte[] hwAddress = ByteHelper.CopyData(data, offset + 4);

            return new LinkLayerAddressDUID(linkLayerType, hwAddress);
        }

        #endregion

    }
}
