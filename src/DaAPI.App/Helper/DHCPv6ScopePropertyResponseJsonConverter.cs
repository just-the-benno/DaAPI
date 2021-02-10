using DaAPI.Core.Scopes.DHCPv6.ScopeProperties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static DaAPI.Shared.Responses.DHCPv6ScopeResponses.V1;

namespace DaAPI.App.Helper
{
    public class DHCPv6ScopePropertyResponseJsonConverter : JsonConverter
    {
        static readonly JsonSerializerSettings SpecifiedSubclassConversion = new JsonSerializerSettings()
        {
            ContractResolver = new BaseSpecifiedConcreteClassConverter<DHCPv6ScopePropertyResponse>()
        };

        public override bool CanConvert(Type objectType) => objectType == typeof(DHCPv6ScopePropertyResponse);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            var token = jo["Type"] ?? jo["type"];
            Int32 rawValue = token.Value<Int32>();

            switch ((DHCPv6ScopePropertyType)rawValue)
            {
                case DHCPv6ScopePropertyType.AddressList:
                    return JsonConvert.DeserializeObject<DHCPv6AddressListScopePropertyResponse>(jo.ToString(), SpecifiedSubclassConversion);
                case DHCPv6ScopePropertyType.Byte:
                case DHCPv6ScopePropertyType.UInt16:
                case DHCPv6ScopePropertyType.UInt32:
                    return JsonConvert.DeserializeObject<DHCPv6NumericScopePropertyResponse>(jo.ToString(), SpecifiedSubclassConversion);
                case DHCPv6ScopePropertyType.Text:
                    throw new NotImplementedException();

                default:
                    throw new NotImplementedException();
            }
        }

        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => throw new NotImplementedException();
    }
}

