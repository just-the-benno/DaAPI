using DaAPI.Core.Common;
using DaAPI.TestHelper;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using static DaAPI.Core.Common.DUID;

namespace DaAPI.UnitTests.Core.Common.DUID
{
    public class VendorBasedDUIDTester
    {
        [Fact]
        public void Constructor()
        {
            Random random = new Random();
            UInt32 enterpriseNumber = (UInt32)random.Next() + (UInt32)random.Next();
            Byte[] vendorInformation = random.NextBytes(20);

            VendorBasedDUID duid = new VendorBasedDUID(enterpriseNumber,vendorInformation);

            Assert.Equal(enterpriseNumber, duid.EnterpriseNumber);
            Assert.Equal(vendorInformation, duid.Identifier);
            Assert.Equal(DUIDTypes.VendorBased, duid.Type);
        }

        [Fact]
        public void FromByteArray()
        {
            Random random = new Random();
            UInt32 enterpriseNumber = UInt32.MaxValue - 2;
            Byte[] vendorInformation = random.NextBytes(20);

            Byte[] input = new Byte[2 + 4 + vendorInformation.Length];
            input[0] = 0;
            input[1] = (Byte)DUIDTypes.VendorBased;
            
            input[2] = 255;
            input[3] = 255;
            input[4] = 255;
            input[5] = 255 - 2;

            vendorInformation.CopyTo(input, 6);

            VendorBasedDUID duid = VendorBasedDUID.FromByteArray(input, 0);

            Assert.Equal(enterpriseNumber, duid.EnterpriseNumber);
            Assert.Equal(vendorInformation, duid.Identifier);
            Assert.Equal(DUIDTypes.VendorBased, duid.Type);

            Byte[] asByteArray = duid.GetAsByteStream();
            Assert.Equal(input, asByteArray);
        }
    }
}
