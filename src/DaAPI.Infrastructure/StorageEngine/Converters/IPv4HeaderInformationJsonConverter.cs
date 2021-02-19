using DaAPI.Core.Common;
using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Packets.DHCPv4;
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
    public class IPv4HeaderInformationJsonConverter : JsonConverter
    {
        private class EeasySerialibleVersionOfIPv4HeaderInformationJsonConverter
        {
            public IPv4Address Source { get; set; }
            public IPv4Address Destination { get; set; }
            public IPv4Address Listener { get; set; }
        }

        public override bool CanConvert(Type objectType) => objectType == typeof(IPv4HeaderInformation);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var info = serializer.Deserialize<EeasySerialibleVersionOfIPv4HeaderInformationJsonConverter>(reader);
            if (info == null) { return null; }

            IPv4HeaderInformation result = new IPv4HeaderInformation(info.Source, info.Destination, info.Listener);
            return result;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            IPv4HeaderInformation item = (IPv4HeaderInformation)value;

            serializer.Serialize(writer, new EeasySerialibleVersionOfIPv4HeaderInformationJsonConverter
            {
                Destination = item.Destionation,
                Source = item.Source,
                Listener = item.ListenerAddress,
            }); ;
        }
    }
}
