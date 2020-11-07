using DaAPI.Core.Common;
using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Helper;
using DaAPI.Core.Packets.DHCPv6;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace DaAPI.Core.Packets.DHCPv6
{
    public class DHCPv6RelayPacket : DHCPv6Packet, IEquatable<DHCPv6RelayPacket>
    {
        #region Properties

        public Byte HopCount { get; private set; }

        public IPv6Address LinkAddress { get; private set; }
        public IPv6Address PeerAddress { get; private set; }
        public DHCPv6Packet InnerPacket { get; private set; }

        #endregion

        #region Constructor

        internal DHCPv6RelayPacket(
            DHCPv6PacketTypes type,
            Byte hopCount,
            IPv6Address linkAddress, IPv6Address peerAddress,
            IEnumerable<DHCPv6PacketOption> options,
            DHCPv6Packet innerPacket) : base(type, options)
        {
            HopCount = hopCount;
            LinkAddress = linkAddress;
            PeerAddress = peerAddress;
            InnerPacket = innerPacket;
        }

        public static DHCPv6RelayPacket AsOuterRelay(
            IPv6HeaderInformation header,
            Boolean isForward,
             Byte hopCount,
            IPv6Address linkAddress, IPv6Address peerAddress,
            IEnumerable<DHCPv6PacketOption> options,
            DHCPv6Packet innerPacket
            ) => new DHCPv6RelayPacket(
                isForward == true ? DHCPv6PacketTypes.RELAY_FORW : DHCPv6PacketTypes.RELAY_REPL,
                hopCount,
                linkAddress,
                peerAddress,
                options,
                innerPacket
                )
            {
                Header = header
            };

        public static DHCPv6RelayPacket AsInnerRelay(
           Boolean isForward,
            Byte hopCount,
           IPv6Address linkAddress, IPv6Address peerAddress,
           IEnumerable<DHCPv6PacketOption> options,
           DHCPv6Packet innerPacket
           ) => new DHCPv6RelayPacket(
               isForward == true ? DHCPv6PacketTypes.RELAY_FORW : DHCPv6PacketTypes.RELAY_REPL,
               hopCount,
               linkAddress,
               peerAddress,
               options,
               innerPacket
               );

        #endregion

        #region Methods

        #region queries

        public override DHCPv6Packet GetInnerPacket() => InnerPacket.GetInnerPacket();

        #endregion

        public override Int32 GetAsStream(Byte[] packetAsByteStream, Int32 offset)
        {
            packetAsByteStream[0 + offset] = (Byte)PacketType;
            packetAsByteStream[1 + offset] = HopCount;

            Int32 pointer = 2 + offset;

            var addresses = new IPv6Address[] { LinkAddress, PeerAddress };

            foreach (IPv6Address item in addresses)
            {
                Byte[] addressBytes = item.GetBytes();
                for (int i = 0; i < 16; i++)
                {
                    packetAsByteStream[pointer++] = addressBytes[i];
                }
            }

            Byte[] innerPacket = new Byte[1500];
            Int32 innerPacketLength = InnerPacket.GetAsStream(innerPacket);

            DHCPv6PacketByteArrayOption relayOption = new DHCPv6PacketByteArrayOption(
                DHCPv6PacketOptionTypes.RelayMessage, innerPacket, innerPacketLength);

            foreach (DHCPv6PacketOption option in Options.Union(new DHCPv6PacketOption[] { relayOption }))
            {
                pointer += option.AppendToStream(packetAsByteStream, pointer);
            }

            return pointer;
        }

        internal void GetRelayPacketChain(ICollection<DHCPv6RelayPacket> items)
        {
            if (InnerPacket is DHCPv6RelayPacket packet)
            {
                packet.GetRelayPacketChain(items);
            }

            items.Add(this);
        }

        public IReadOnlyList<DHCPv6RelayPacket> GetRelayPacketChain()
        {
            List<DHCPv6RelayPacket> packets = new List<DHCPv6RelayPacket>();
            this.GetRelayPacketChain(packets);

            return packets.AsReadOnly();
        }

        internal static DHCPv6RelayPacket FromByteArray(byte[] rawData, int offset)
        {
            DHCPv6PacketTypes type = (DHCPv6PacketTypes)rawData[offset];
            offset += 1;

            Byte hopCount = rawData[offset];
            offset += 1;
            IPv6Address linkAddress = IPv6Address.FromByteArray(rawData, offset);
            offset += 16;
            IPv6Address peerAddress = IPv6Address.FromByteArray(rawData, offset);
            offset += 16;

            List<DHCPv6PacketOption> options = new List<DHCPv6PacketOption>();

            Byte[] innerPacketOptionValue = null;

            Int32 byteIndex = offset;
            while (byteIndex < rawData.Length)
            {
                UInt16 optionCode = ByteHelper.ConvertToUInt16FromByte(rawData, byteIndex);
                UInt16 length = ByteHelper.ConvertToUInt16FromByte(rawData, byteIndex + 2);
                Byte[] optionData = ByteHelper.CopyData(rawData, byteIndex, length + 4);

                if (optionCode == (Byte)DHCPv6PacketOptionTypes.RelayMessage)
                {
                    innerPacketOptionValue = ByteHelper.CopyData(optionData, 4);
                }
                else
                {
                    DHCPv6PacketOption suboption = DHCPv6PacketOptionFactory.GetOption(optionCode, optionData);
                    options.Add(suboption);
                }

                byteIndex += 4 + length;
            }

            return new DHCPv6RelayPacket(
                type,
                hopCount,
                linkAddress, peerAddress,
                options,
                DHCPv6Packet.FromByteArray(innerPacketOptionValue));
        }

        public bool Equals(DHCPv6RelayPacket other)
        {
            Boolean preCheck =
                other.Header == this.Header &&
                other.HopCount == this.HopCount &&
                other.LinkAddress == this.LinkAddress &&
                other.PeerAddress == this.PeerAddress &&
                other.InnerPacket == this.InnerPacket &&
                other.Options.Count == this.Options.Count;

            if (preCheck == false)
            {
                return false;
            }

            for (int i = 0; i < other.Options.Count; i++)
            {
                if (this.Options[i] != other.Options[i])
                {
                    return false;
                }
            }

            return true;
        }

        internal static DHCPv6RelayPacket ConstructRelayPacket(DHCPv6RelayPacket receivedPacket, DHCPv6Packet innerResponse)
        {
            DHCPv6Packet innerPacket = innerResponse;
            Byte hopCount = GetRelayPacketNumber(receivedPacket);
            if (receivedPacket.InnerPacket is DHCPv6RelayPacket packet)
            {
                innerPacket = ConstructRelayPacket(packet, innerResponse);
            }

            DHCPv6RelayPacket responsePacket = new DHCPv6RelayPacket(DHCPv6PacketTypes.RELAY_REPL,
                hopCount, receivedPacket.LinkAddress, receivedPacket.PeerAddress, receivedPacket.Options.ToList(), innerPacket);

            return responsePacket;
        }

        private static byte GetRelayPacketNumber(DHCPv6RelayPacket receivedPacket)
        {
            Byte counter = 0;
            if (receivedPacket.InnerPacket is DHCPv6RelayPacket packet)
            {
                counter += 1;
                counter += GetRelayPacketNumber(packet);
            }

            return counter;
        }

        public override bool Equals(object other) => other is DHCPv6RelayPacket packet ? Equals(packet) : base.Equals(other);

        public override int GetHashCode() =>
          InnerPacket.TransactionId < Int32.MaxValue ? (Int32)InnerPacket.TransactionId : Int32.MaxValue;



        #endregion
    }
}
