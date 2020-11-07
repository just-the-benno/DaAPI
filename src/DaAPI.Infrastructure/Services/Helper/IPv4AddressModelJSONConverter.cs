using DaAPI.Core.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Infrastructure.Services.Helper
{
    public class IPv4AddressJSONConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(IPv4Address);
        }

        public override bool CanRead => true;

        public override bool CanWrite => true;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken value = JValue.Load(reader);

            String rawValue = value.ToString();
            if(String.IsNullOrEmpty(rawValue) == true)
            {
                return IPv4Address.Empty;
            }
            return  IPv4Address.FromString(rawValue);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }
    }
}
