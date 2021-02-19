using DaAPI.Core.Scopes.DHCPv4;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Infrastructure.StorageEngine.Converters
{
    public class DHCPv4ScopePropertyJsonConverter : JsonConverter
    {
        static readonly JsonSerializerSettings SpecifiedSubclassConversion = new JsonSerializerSettings()
        {
            Converters = new List<JsonConverter> { new IPv6AddressJsonConverter() },
            ContractResolver = new BaseSpecifiedConcreteClassConverter<DHCPv4ScopeProperty>(),
        };

        public override bool CanConvert(Type objectType) => objectType == typeof(DHCPv4ScopeProperty);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            Int32 rawValue = jo[nameof(DHCPv4ScopeProperty.ValueType)].Value<Int32>();

            switch ((DHCPv4ScopePropertyType)rawValue)
            {
                case DHCPv4ScopePropertyType.Address:
                    return JsonConvert.DeserializeObject<DHCPv4AddressScopeProperty>(jo.ToString(), SpecifiedSubclassConversion);
                case DHCPv4ScopePropertyType.AddressAndMask:
                    throw new NotImplementedException();
                case DHCPv4ScopePropertyType.AddressList:
                    return JsonConvert.DeserializeObject<DHCPv4AddressListScopeProperty>(jo.ToString(), SpecifiedSubclassConversion);
                case DHCPv4ScopePropertyType.Boolean:
                    return JsonConvert.DeserializeObject<DHCPv4BooleanScopeProperty>(jo.ToString(), SpecifiedSubclassConversion);
                case DHCPv4ScopePropertyType.Byte:
                case DHCPv4ScopePropertyType.UInt16:
                case DHCPv4ScopePropertyType.UInt32:
                    return JsonConvert.DeserializeObject<DHCPv4NumericValueScopeProperty>(jo.ToString(), SpecifiedSubclassConversion);
                case DHCPv4ScopePropertyType.ByteArray:
                    throw new NotImplementedException();
                case DHCPv4ScopePropertyType.Subnet:
                    return JsonConvert.DeserializeObject<DHCPv4AddressScopeProperty>(jo.ToString(), SpecifiedSubclassConversion);
                case DHCPv4ScopePropertyType.Text:
                    return JsonConvert.DeserializeObject<DHCPv4TextScopeProperty>(jo.ToString(), SpecifiedSubclassConversion);
                case DHCPv4ScopePropertyType.Time:
                case DHCPv4ScopePropertyType.TimeOffset:
                    return JsonConvert.DeserializeObject<DHCPv4TimeScopeProperty>(jo.ToString(), SpecifiedSubclassConversion);
                default:
                    throw new Exception();
            }
        }

        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => throw new NotImplementedException();
    }
}

