using DaAPI.Core.Common;
using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Packets.DHCPv6;
using DaAPI.Core.Scopes;
using DaAPI.Core.Scopes.DHCPv6;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Infrastructure.StorageEngine.Converters
{
    public class DHCPv6PrefixDelgationInfoJsonConverter : JsonConverter
    {
        public class EeasySerialibleVersionOfDHCPv6PrefixDelgationInfo
        {
            public IPv6Address Prefix { get; set; }
            public Byte PrefixLength { get; set; }
            public Byte AssingedPrefixLength { get; set; }

            public EeasySerialibleVersionOfDHCPv6PrefixDelgationInfo()
            {

            }

            public EeasySerialibleVersionOfDHCPv6PrefixDelgationInfo(DHCPv6PrefixDelgationInfo info)
            {
                Prefix = info.Prefix;
                PrefixLength = info.PrefixLength;
                AssingedPrefixLength = info.AssignedPrefixLength;
            }
        }

        public override bool CanConvert(Type objectType) => objectType == typeof(DHCPv6PrefixDelgationInfo);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var info = serializer.Deserialize<EeasySerialibleVersionOfDHCPv6PrefixDelgationInfo>(reader);

            if (info == null) {
                return null;
            }

            DHCPv6PrefixDelgationInfo result = DHCPv6PrefixDelgationInfo.FromValues(info.Prefix,
                new IPv6SubnetMaskIdentifier(info.PrefixLength), new IPv6SubnetMaskIdentifier(info.AssingedPrefixLength));   

            return result;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            DHCPv6PrefixDelgationInfo item = (DHCPv6PrefixDelgationInfo)value;

            serializer.Serialize(writer, new EeasySerialibleVersionOfDHCPv6PrefixDelgationInfo(item));
        }
    }
}
