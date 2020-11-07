using DaAPI.Core.Common;
using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Packets.DHCPv6;
using DaAPI.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;
using static DaAPI.Core.Scopes.ScopeResolverPropertyDescription;

namespace DaAPI.Core.Scopes.DHCPv6.Resolvers
{
    public class DHCPv6MilegateResolver : IScopeResolver<DHCPv6Packet, IPv6Address>
    {
        #region Fields

        private static readonly Encoding _encoding = ASCIIEncoding.ASCII;
        private Byte[] _valueAsByte;

        #endregion

        #region Properties

        public String Value { get; private set; }
        public UInt16 Index { get; private set; }
        public Boolean IsCaseSenstiveMatch { get; private set; }

        #endregion

        public bool HasUniqueIdentifier => true;


        public byte[] GetUniqueIdentifier(DHCPv6Packet packet)
        {
            var chain = ((DHCPv6RelayPacket)packet).GetRelayPacketChain();
            var relayedPacket = chain[Index];

            var option = relayedPacket.GetOption<DHCPv6PacketRemoteIdentifierOption>(DHCPv6PacketOptionTypes.RemoteIdentifier);
            return option.Value;
        }

        public Boolean ArePropertiesAndValuesValid(IDictionary<String, String> valueMapper, ISerializer serializer)
        {
            try
            {
                if (valueMapper.ContainsKeys(new[] { nameof(Index), nameof(Value), nameof(IsCaseSenstiveMatch) }) == false)
                {
                    return false;
                }

                String index = serializer.Deserialze<String>(valueMapper[nameof(Index)]);
                if (UInt16.TryParse(index, out UInt16 _) == false)
                {
                    return false;
                }
                String value = serializer.Deserialze<String>(valueMapper[nameof(Value)]);
                if (String.IsNullOrEmpty(value) == true)
                {
                    return false;
                }

                String insenstiveMatchValue = serializer.Deserialze<String>(valueMapper[nameof(IsCaseSenstiveMatch)]);
                if(Boolean.TryParse(insenstiveMatchValue,out Boolean _) == false)
                {
                    return false;
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void ApplyValues(IDictionary<String, String> valueMapper, ISerializer serializer)
        {
            Index = serializer.Deserialze<UInt16>(valueMapper[nameof(Index)]);
            Value = serializer.Deserialze<String>(valueMapper[nameof(Value)]);
            IsCaseSenstiveMatch = serializer.Deserialze<Boolean>(valueMapper[nameof(IsCaseSenstiveMatch)]);

            _valueAsByte = _encoding.GetBytes(Value);
        }

        public ScopeResolverDescription GetDescription() => new ScopeResolverDescription(
          nameof(DHCPv6MilegateResolver),
          new List<ScopeResolverPropertyDescription>
         {
                   new ScopeResolverPropertyDescription(nameof(Value), ScopeResolverPropertyValueTypes.String ),
                   new ScopeResolverPropertyDescription(nameof(Index), ScopeResolverPropertyValueTypes.UInt32 ),
                   new ScopeResolverPropertyDescription(nameof(IsCaseSenstiveMatch),ScopeResolverPropertyValueTypes.Boolean)
          });

        public Boolean PacketMeetsCondition(DHCPv6Packet packet)
        {
            if (packet is DHCPv6RelayPacket == false) { return false; }

            var chain = ((DHCPv6RelayPacket)packet).GetRelayPacketChain();
            if (chain.Count <= Index) { return false; }

            var relayedPacket = chain[Index];

            var option = relayedPacket.GetOption<DHCPv6PacketRemoteIdentifierOption>(DHCPv6PacketOptionTypes.RemoteIdentifier);
            if (option == null) { return false; }

            Boolean casesenstiveMatch = ByteHelper.AreEqual(_valueAsByte, option.Value);
            if (casesenstiveMatch == true)
            {
                return true;
            }

            if (IsCaseSenstiveMatch == true)
            {
                return false;
            }

            String content = _encoding.GetString(option.Value);

            return String.Compare(Value, content, true) == 0;
        }

        public IDictionary<String, String> GetValues() => new Dictionary<String, String>
        {
            { nameof(Value), Value  },
            { nameof(IsCaseSenstiveMatch), IsCaseSenstiveMatch == true ? "true" : "false"  },
            { nameof(Index), Index.ToString() },
        };
    }
}
