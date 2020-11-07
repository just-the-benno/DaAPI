using DaAPI.Core.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Shared.JsonConverters
{
    public class DUIDJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => typeof(DUID).IsAssignableFrom(objectType);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            String rawValue = (String)reader.Value;
            Byte[] value = Convert.FromBase64String(rawValue);

            DUID address = DUIDFactory.GetDUID(value);
            return address;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            DUID address = (DUID)value;
            Byte[] bytes = address.GetAsByteStream();
            writer.WriteValue(Convert.ToBase64String(bytes));
        }
    }
}
