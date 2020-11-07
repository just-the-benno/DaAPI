using DaAPI.Core.Common;
using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Packets.DHCPv6;
using DaAPI.Core.Scopes;
using DaAPI.Core.Scopes.DHCPv6;
using DaAPI.Core.Scopes.DHCPv6.ScopeProperties;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Infrastructure.StorageEngine.Converters
{
    public class IPv6HeaderInformationJsonConverter : JsonConverter
    {
        private class EeasySerialibleVersionOfIPv6HeaderInformationJsonConverter
        {
            public IPv6Address Source { get; set; }
            public IPv6Address Destination { get; set; }
            public IPv6Address Listener { get; set; }
        }

        public override bool CanConvert(Type objectType) => objectType == typeof(IPv6HeaderInformation);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var info = serializer.Deserialize<EeasySerialibleVersionOfIPv6HeaderInformationJsonConverter>(reader);
            if (info == null) { return null; }

            IPv6HeaderInformation result = new IPv6HeaderInformation(info.Source, info.Destination, info.Listener);
            return result;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            IPv6HeaderInformation item = (IPv6HeaderInformation)value;

            serializer.Serialize(writer, new EeasySerialibleVersionOfIPv6HeaderInformationJsonConverter
            {
                Destination = item.Destionation,
                Source = item.Source,
                Listener = item.ListenerAddress,
            }); ;
        }
    }
}
