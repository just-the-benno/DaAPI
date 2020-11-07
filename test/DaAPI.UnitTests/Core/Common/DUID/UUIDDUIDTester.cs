using DaAPI.Core.Common;
using DaAPI.TestHelper;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using static DaAPI.Core.Common.DUID;

namespace DaAPI.UnitTests.Core.Common.DUID
{
    public class UUIDDUIDTester
    {
        [Fact]
        public void Construtor()
        {
            Random random = new Random();
            Guid id = random.NextGuid();

            UUIDDUID duid = new UUIDDUID(id);

            Assert.Equal(DUIDTypes.Uuid, duid.Type);
            Assert.Equal(id, duid.UUID);
        }

        [Fact]
        public void FromByteArray()
        {
            Random random = new Random();
            Guid id = random.NextGuid();

            Byte[] input = new Byte[18];
            input[0] = 0;
            input[1] = (Byte)DUIDTypes.Uuid;
            id.ToByteArray().CopyTo(input, 2);

            UUIDDUID duid = UUIDDUID.FromByteArray(input, 0);

            Assert.Equal(DUIDTypes.Uuid, duid.Type);
            Assert.Equal(id, duid.UUID);

            Byte[] asStream = duid.GetAsByteStream();
            Assert.Equal(input, asStream);
        }


    }
}
