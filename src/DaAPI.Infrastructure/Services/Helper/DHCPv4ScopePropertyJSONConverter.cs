using DaAPI.Core.Scopes.DHCPv4;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using static DaAPI.Core.Scopes.DHCPv4.DHCPv4ScopeProperty;

namespace DaAPI.Infrastructure.Services.Helper
{
    public class BaseSpecifiedConcreteClassConverter : DefaultContractResolver
    {
        protected override JsonConverter ResolveContractConverter(Type objectType)
        {
            if (typeof(DHCPv4ScopeProperty).IsAssignableFrom(objectType) && !objectType.IsAbstract)
                return null; // pretend TableSortRuleConvert is not specified (thus avoiding a stack overflow)
            return base.ResolveContractConverter(objectType);
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var prop = base.CreateProperty(member, memberSerialization);

            if (!prop.Writable)
            {
                var property = member as PropertyInfo;
                if (property != null)
                {
                    var hasPrivateSetter = property.GetSetMethod(true) != null;
                    prop.Writable = hasPrivateSetter;
                }
            }

            return prop;
        }
    }

    // see https://stackoverflow.com/questions/20995865/deserializing-json-to-abstract-class

    public class DHCPv4ScopePropertyJSONConverter : JsonConverter
    {
        static readonly JsonSerializerSettings SpecifiedSubclassConversion = new JsonSerializerSettings()
        {
            Converters = new List<JsonConverter> { new IPv4AddressJSONConverter() },
            ContractResolver = new BaseSpecifiedConcreteClassConverter(),
        };

        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(DHCPv4ScopeProperty));
        }

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

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            // won't be called because CanWrite returns false
            throw new NotImplementedException();
        }
    }
}
