using DaAPI.Core.Common;
using DaAPI.Core.Scopes;
using DaAPI.Core.Scopes.DHCPv4;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DaAPI.Infrastructure.StorageEngine.Converters
{
    public class DHCPv4ScopePropertiesJsonConverter : JsonConverter
    {
        private class EeasySerialibleVersionOfDHCPv4ScopeProperties
        {
            public IEnumerable<DHCPv4ScopeProperty> Properties { get; set; }
            public IEnumerable<Byte> ExcludedFromInheritance { get; set; }
        }

        public override bool CanConvert(Type objectType) => objectType == typeof(DHCPv4ScopeProperties);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var info = serializer.Deserialize<EeasySerialibleVersionOfDHCPv4ScopeProperties>(reader);

            DHCPv4ScopeProperties result = new DHCPv4ScopeProperties(info.Properties);
            foreach (var item in info.ExcludedFromInheritance ?? Array.Empty<Byte>())
            {
                result.RemoveFromInheritance(item);
            }

            return result;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            DHCPv4ScopeProperties item = (DHCPv4ScopeProperties)value;

            serializer.Serialize(writer, new EeasySerialibleVersionOfDHCPv4ScopeProperties
            {
                Properties = item.Properties.Where(x => x != null),
                ExcludedFromInheritance = item.GetMarkedFromInheritanceOptionCodes(),
            }) ;
        }
    }
}
