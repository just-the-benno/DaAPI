using DaAPI.Core.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Infrastructure.Services.JsonConverters
{
    public class IPv4AddressAsStringJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(IPv4Address);

        public override object ReadJson(JsonReader reader, Type objectType,  object existingValue, JsonSerializer serializer)
        {
            String value = reader.Value as String;
            return IPv4Address.FromString(value);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => writer.WriteValue((value as IPv4Address).ToString());
    }
}
