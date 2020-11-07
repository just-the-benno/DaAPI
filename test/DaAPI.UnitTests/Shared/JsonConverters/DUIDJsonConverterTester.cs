using DaAPI.Core.Common;
using DaAPI.Shared.JsonConverters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using DaAPI.TestHelper;

namespace DaAPI.UnitTests.Shared.JsonConverters
{
    public class DUIDJsonConverterTester
    {
        [Fact]
        public void SerializeAndDeserialize()
        {
            Random random = new Random();

            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Converters.Add(new DUIDJsonConverter());

            List<DUID> duids = new List<DUID>
            {
                new UUIDDUID(random.NextGuid()),
                new VendorBasedDUID(random.NextUInt32(),random.NextBytes(20)),
                 LinkLayerAddressAndTimeDUID.FromEthernet(random.NextBytes(6),DateTime.Now),
                new LinkLayerAddressDUID(LinkLayerAddressDUID.DUIDLinkLayerTypes.Ethernet,random.NextBytes(6))
            };

            String serialized = JsonConvert.SerializeObject(duids, settings);
            var actual = JsonConvert.DeserializeObject<List<DUID>>(serialized, settings);

            for (int i = 0; i < duids.Count; i++)
            {
                Assert.Equal(duids[i], actual[i]);
            }
        }
    }
}
