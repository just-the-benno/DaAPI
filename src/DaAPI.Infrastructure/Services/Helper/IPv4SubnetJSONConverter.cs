using DaAPI.Core.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Infrastructure.Services.Helper
{
    public class IPv4SubnetJSONConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(IPv4SubnetMask);
        }

        public override bool CanRead => true;

        public override bool CanWrite => true;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken value = JValue.Load(reader);

            String rawValue = value.ToString();
            IPv4Address pseudoAddress = IPv4Address.FromString(rawValue);
            Byte[] bytes = pseudoAddress.GetBytes();
            return IPv4SubnetMask.FromByteArray(bytes);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }
    }
}
