using DaAPI.Core.Common.DHCPv6;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Infrastructure.Services.JsonConverters
{
    public class IPv6SubnetMaskAsStringJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(IPv6SubnetMask);

        public override object ReadJson(JsonReader reader, Type objectType,  object existingValue, JsonSerializer serializer)
        {
            String value = reader.Value as String;
            return new IPv6SubnetMask(new IPv6SubnetMaskIdentifier(Convert.ToByte(value)));
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue((value as IPv6SubnetMask).Identifier.Value.ToString());
        }
    }
}
