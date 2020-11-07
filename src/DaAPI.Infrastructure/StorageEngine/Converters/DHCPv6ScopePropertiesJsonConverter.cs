using DaAPI.Core.Common;
using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Packets.DHCPv6;
using DaAPI.Core.Scopes;
using DaAPI.Core.Scopes.DHCPv6;
using DaAPI.Core.Scopes.DHCPv6.ScopeProperties;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DaAPI.Infrastructure.StorageEngine.Converters
{
    public class DHCPv6ScopePropertiesJsonConverter : JsonConverter
    {
        private class EeasySerialibleVersionOfDHCPv6ScopeProperties
        {
            public IEnumerable<DHCPv6ScopeProperty> Properties { get; set; }
            public IEnumerable<UInt16> ExcludedFromInheritance { get; set; }
        }

        public override bool CanConvert(Type objectType) => objectType == typeof(DHCPv6ScopeProperties);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var info = serializer.Deserialize<EeasySerialibleVersionOfDHCPv6ScopeProperties>(reader);

            DHCPv6ScopeProperties result = new DHCPv6ScopeProperties(info.Properties);
            foreach (var item in info.ExcludedFromInheritance ?? Array.Empty<UInt16>())
            {
                result.RemoveFromInheritance(item);
            }

            return result;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            DHCPv6ScopeProperties item = (DHCPv6ScopeProperties)value;

            serializer.Serialize(writer, new EeasySerialibleVersionOfDHCPv6ScopeProperties
            {
                Properties = item.Properties.Where(x => x != null),
                ExcludedFromInheritance = item.GetMarkedFromInheritanceOptionCodes(),
            }) ;
        }
    }
}
