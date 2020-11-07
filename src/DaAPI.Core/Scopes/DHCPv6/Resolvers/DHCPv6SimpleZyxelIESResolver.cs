using DaAPI.Core.Common;
using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Packets.DHCPv6;
using DaAPI.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Schema;
using static DaAPI.Core.Scopes.ScopeResolverPropertyDescription;

namespace DaAPI.Core.Scopes.DHCPv6.Resolvers
{
    public class DHCPv6SimpleZyxelIESResolver : IScopeResolver<DHCPv6Packet, IPv6Address>
    {
        #region Fields

        private const int _macAddressLength = 6;
        private static readonly Encoding _encoding = ASCIIEncoding.ASCII;
        private Byte[] _interfaceIdValueAsByte;
        private Byte[] _remoteIdentifierValueAsByte;

        #endregion

        #region Properties

        public UInt16 Index { get; private set; }
        public UInt16 SlotId { get; private set; }
        public UInt16 PortId { get; private set; }
        public Byte[] DeviceMacAddress { get; private set; }

        #endregion

        private (DHCPv6PacketRemoteIdentifierOption RemoteOption, DHCPv6PacketByteArrayOption InterfaceOption) GetOptions(DHCPv6Packet packet)
        {
            var chain = ((DHCPv6RelayPacket)packet).GetRelayPacketChain();
            DHCPv6RelayPacket relayedPacket = chain[Index];

            var remoteIdentiiferOption = relayedPacket.GetOption<DHCPv6PacketRemoteIdentifierOption>(DHCPv6PacketOptionTypes.RemoteIdentifier);
            var interfaceOption = relayedPacket.GetOption<DHCPv6PacketByteArrayOption>(DHCPv6PacketOptionTypes.InterfaceId);

            return (remoteIdentiiferOption, interfaceOption);
        }

        public Boolean HasUniqueIdentifier => true;

        public Byte[] GetUniqueIdentifier(DHCPv6Packet packet)
        {
            var (RemoteOption, InterfaceOption) = GetOptions(packet);
            Byte[] result = ByteHelper.ConcatBytes(RemoteOption.Data, InterfaceOption.Data);

            return result;
        }

        public Boolean ArePropertiesAndValuesValid(IDictionary<String, String> valueMapper, ISerializer serializer)
        {
            if (valueMapper.ContainsKeys(new[] { nameof(Index), nameof(SlotId), nameof(PortId), nameof(DeviceMacAddress) }) == false)
            {
                return false;
            }

            var numberValues = new[] { nameof(Index), nameof(SlotId), nameof(PortId) };
            foreach (var item in numberValues)
            {
                String index = serializer.Deserialze<String>(valueMapper[item]);
                if (UInt16.TryParse(index, out UInt16 numberValue) == true)
                {
                    if (item != nameof(Index) && numberValue < 1)
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            String value = serializer.Deserialze<String>(valueMapper[nameof(DeviceMacAddress)]);
            if (String.IsNullOrEmpty(value) == true) { return false; }

            try
            {
                var macAddress = ByteHelper.GetBytesFromHexString(value);
                if (macAddress.Length != _macAddressLength)
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public void ApplyValues(IDictionary<String, String> valueMapper, ISerializer serializer)
        {
            Index = serializer.Deserialze<UInt16>(valueMapper[nameof(Index)]);
            SlotId = serializer.Deserialze<UInt16>(valueMapper[nameof(SlotId)]);
            PortId = serializer.Deserialze<UInt16>(valueMapper[nameof(PortId)]);

            DeviceMacAddress = ByteHelper.GetBytesFromHexString(serializer.Deserialze<String>(valueMapper[nameof(DeviceMacAddress)]));

            _interfaceIdValueAsByte = _encoding.GetBytes($"{SlotId}/{PortId}");
            _remoteIdentifierValueAsByte = ByteHelper.ConcatBytes(new Byte[4], DeviceMacAddress);
        }

        public ScopeResolverDescription GetDescription() => new ScopeResolverDescription(
          nameof(DHCPv6SimpleZyxelIESResolver),
          new List<ScopeResolverPropertyDescription>
         {
                   new ScopeResolverPropertyDescription(nameof(Index), ScopeResolverPropertyValueTypes.UInt32 ),
                   new ScopeResolverPropertyDescription(nameof(SlotId), ScopeResolverPropertyValueTypes.UInt32 ),
                   new ScopeResolverPropertyDescription(nameof(PortId), ScopeResolverPropertyValueTypes.UInt32 ),
                   new ScopeResolverPropertyDescription(nameof(DeviceMacAddress),ScopeResolverPropertyValueTypes.ByteArray)
          });

        public Boolean PacketMeetsCondition(DHCPv6Packet packet)
        {
            if (packet is DHCPv6RelayPacket == false) { return false; }

            var chain = ((DHCPv6RelayPacket)packet).GetRelayPacketChain();
            if (chain.Count <= Index) { return false; }

            var (RemoteOption, InterfaceOption) = GetOptions(packet);

            if (InterfaceOption == null || RemoteOption == null)
            {
                return false;
            }

            Boolean interfaceResult = ByteHelper.AreEqual(_interfaceIdValueAsByte, InterfaceOption.Data);
            Boolean remodeIdentifierResult = ByteHelper.AreEqual(_remoteIdentifierValueAsByte, RemoteOption.Data, 4);

            return interfaceResult && remodeIdentifierResult;
        }

        public IDictionary<String, String> GetValues() => new Dictionary<String, String>
        {
            { nameof(Index), Index.ToString() },
            { nameof(SlotId), SlotId.ToString() },
            { nameof(PortId), PortId.ToString() },
            { nameof(DeviceMacAddress), ByteHelper.ToString(DeviceMacAddress,false)  },
        };
    }
}
