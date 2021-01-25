using DaAPI.Core.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Infrastructure.Services.JsonConverters
{
    public class IPv4SubnetMaskAsStringJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(IPv4SubnetMask);

        public override object ReadJson(JsonReader reader, Type objectType,  object existingValue, JsonSerializer serializer)
        {
            String value = reader.Value as String;
            return new IPv4SubnetMask(new IPv4SubnetMaskIdentifier(Convert.ToByte(value)));
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => writer.WriteValue((value as IPv4SubnetMask).GetSlashNotation().ToString());
    }
}
