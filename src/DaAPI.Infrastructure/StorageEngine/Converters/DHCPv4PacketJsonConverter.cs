using DaAPI.Core.Common;
using DaAPI.Core.Packets.DHCPv4;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Infrastructure.StorageEngine.Converters
{
    public class DHCPv4PacketJsonConverter : JsonConverter
    {
        private class DHCPv4PacketSerializerInfo
        {
            public Byte[] Stream { get; set; }
            public IPv4HeaderInformation Header { get; set; }
        }

        public override bool CanConvert(Type objectType) => typeof(DHCPv4Packet).IsAssignableFrom(objectType);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var info = serializer.Deserialize<DHCPv4PacketSerializerInfo>(reader);
            if(info == null)
            {
                return DHCPv4Packet.Empty;
            }

            DHCPv4Packet packet = DHCPv4Packet.FromByteArray(info.Stream, info.Header);
            return packet;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            DHCPv4Packet packet = (DHCPv4Packet)value;
            Byte[] bytes = packet.GetAsStream();

            serializer.Serialize(writer, new DHCPv4PacketSerializerInfo
            {
                Header = (IPv4HeaderInformation)packet.Header,
                Stream = bytes,
            });
        }
    }
}
