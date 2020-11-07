using DaAPI.Core.Common;
using DaAPI.Core.Common.DHCPv6;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Infrastructure.StorageEngine.Converters
{
    public class IPv6AddressJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(IPv6Address);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            String rawValue = (String)reader.Value;

            if(String.IsNullOrEmpty(rawValue) == true)
            {
                return null;
            }

            Byte[] value = Convert.FromBase64String(rawValue);

            IPv6Address address = IPv6Address.FromByteArray(value);
            return address;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            IPv6Address address = (IPv6Address)value;
            Byte[] bytes = address.GetBytes();
            writer.WriteValue(Convert.ToBase64String(bytes));
        }
    }
}
