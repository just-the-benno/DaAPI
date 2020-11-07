using DaAPI.Core.Scopes.DHCPv6.ScopeProperties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Infrastructure.StorageEngine.Converters
{
    public class DHCPv6ScopePropertyJsonConverter : JsonConverter
    {
        static readonly JsonSerializerSettings SpecifiedSubclassConversion = new JsonSerializerSettings()
        {
            Converters = new List<JsonConverter> { new IPv6AddressJsonConverter() },
            ContractResolver = new BaseSpecifiedConcreteClassConverter<DHCPv6ScopeProperty>(),
        };

        public override bool CanConvert(Type objectType) => objectType == typeof(DHCPv6ScopeProperty);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            Int32 rawValue = jo[nameof(DHCPv6ScopeProperty.ValueType)].Value<Int32>();

            switch ((DHCPv6ScopePropertyType)rawValue)
            {
                case DHCPv6ScopePropertyType.AddressList:
                    return JsonConvert.DeserializeObject<DHCPv6AddressListScopeProperty>(jo.ToString(), SpecifiedSubclassConversion);
                case DHCPv6ScopePropertyType.Byte:
                case DHCPv6ScopePropertyType.UInt16:
                case DHCPv6ScopePropertyType.UInt32:
                    return JsonConvert.DeserializeObject<DHCPv6NumericValueScopeProperty>(jo.ToString(), SpecifiedSubclassConversion);
                case DHCPv6ScopePropertyType.Text:
                    return JsonConvert.DeserializeObject<DHCPv6TextScopeProperty>(jo.ToString(), SpecifiedSubclassConversion);
                default:
                    throw new Exception();
            }
        }

        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => throw new NotImplementedException();
    }
}

