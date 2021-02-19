using DaAPI.Core.Scopes.DHCPv4;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static DaAPI.Shared.Responses.DHCPv4ScopeResponses.V1;

namespace DaAPI.App.Helper
{
    public class DHCPv4ScopePropertyResponseJsonConverter : JsonConverter
    {
        static readonly JsonSerializerSettings SpecifiedSubclassConversion = new JsonSerializerSettings()
        {
            ContractResolver = new BaseSpecifiedConcreteClassConverter<DHCPv4ScopePropertyResponse>()
        };

        public override bool CanConvert(Type objectType) => objectType == typeof(DHCPv4ScopePropertyResponse);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            var token = jo["Type"] ?? jo["type"];
            Int32 rawValue = token.Value<Int32>();

            switch ((DHCPv4ScopePropertyType)rawValue)
            {
                case DHCPv4ScopePropertyType.AddressList:
                    return JsonConvert.DeserializeObject<DHCPv4AddressListScopePropertyResponse>(jo.ToString(), SpecifiedSubclassConversion);
                case DHCPv4ScopePropertyType.Byte:
                case DHCPv4ScopePropertyType.UInt16:
                case DHCPv4ScopePropertyType.UInt32:
                    return JsonConvert.DeserializeObject<DHCPv4NumericScopePropertyResponse>(jo.ToString(), SpecifiedSubclassConversion);
                case DHCPv4ScopePropertyType.Text:
                    return JsonConvert.DeserializeObject<DHCPv4TextScopePropertyResponse>(jo.ToString(), SpecifiedSubclassConversion);
                case DHCPv4ScopePropertyType.Boolean:
                    return JsonConvert.DeserializeObject<DHCPv4BooleanScopePropertyResponse>(jo.ToString(), SpecifiedSubclassConversion);
                case DHCPv4ScopePropertyType.Time:
                    return JsonConvert.DeserializeObject<DHCPv4TimeScopePropertyResponse>(jo.ToString(), SpecifiedSubclassConversion);
                case DHCPv4ScopePropertyType.Address:
                    return JsonConvert.DeserializeObject<DHCPv4AddressScopePropertyResponse>(jo.ToString(), SpecifiedSubclassConversion);
                default:
                    throw new NotImplementedException();
            }
        }

        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => throw new NotImplementedException();
    }
}

