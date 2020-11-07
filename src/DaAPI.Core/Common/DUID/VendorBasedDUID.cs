using DaAPI.Core.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Common
{
    public class VendorBasedDUID : DUID
    {
        #region Properties

        public UInt32 EnterpriseNumber { get; private set; }
        public Byte[] Identifier { get; private set; }

        #endregion

        #region Constructor

        private VendorBasedDUID() : base()
        {

        }

        public VendorBasedDUID(UInt32 enterpiseNumber, Byte[] vendorInformation) : base(DUIDTypes.VendorBased,
            ByteHelper.GetBytes(enterpiseNumber),
            vendorInformation)
        {
            EnterpriseNumber = enterpiseNumber;
            if (vendorInformation == null || vendorInformation.Length == 0)
            {
                throw new ArgumentNullException(nameof(vendorInformation));
            }

            Identifier = ByteHelper.CopyData(vendorInformation);
        }

        public static VendorBasedDUID FromByteArray(Byte[] data, Int32 offset)
        {
            UInt16 code = ByteHelper.ConvertToUInt16FromByte(data, offset);
            if (code != (UInt16)DUIDTypes.VendorBased)
            {
                throw new ArgumentException($"invalid duid type. expected {(UInt16)DUIDTypes.VendorBased} actual {code}");
            }

            UInt32 enterpriseNumber = ByteHelper.ConvertToUInt32FromByte(data, offset + 2);
            Byte[] vendorInformation = ByteHelper.CopyData(data, offset + 6);

            return new VendorBasedDUID(enterpriseNumber, vendorInformation);
        }

        #endregion

        #region Methods

        #endregion
    }
}
