using DaAPI.Core.Common;
using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Packets.DHCPv6;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Infrastructure.StorageEngine.Converters
{
    public class DHCPv6PacketJsonConverter : JsonConverter
    {
        private class DHCPv6PacketSerializerInfo
        {
            public Byte[] Stream { get; set; }
            public IPv6HeaderInformation Header { get; set; }
        }

        public override bool CanConvert(Type objectType) => typeof(DHCPv6Packet).IsAssignableFrom(objectType);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var info = serializer.Deserialize<DHCPv6PacketSerializerInfo>(reader);
            if(info == null)
            {
                return DHCPv6Packet.Empty;
            }

            DHCPv6Packet packet = DHCPv6Packet.FromByteArray(info.Stream, info.Header);
            return packet;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            DHCPv6Packet packet = (DHCPv6Packet)value;
            Byte[] bytes = packet.GetAsStream();

            serializer.Serialize(writer, new DHCPv6PacketSerializerInfo
            {
                Header = (IPv6HeaderInformation)packet.Header,
                Stream = bytes,
            });
        }
    }
}
