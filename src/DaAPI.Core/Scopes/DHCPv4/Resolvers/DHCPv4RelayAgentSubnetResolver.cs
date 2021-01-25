using DaAPI.Core.Common;
using DaAPI.Core.Packets.DHCPv4;
using DaAPI.Core.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using static DaAPI.Core.Scopes.ScopeResolverPropertyDescription;

namespace DaAPI.Core.Scopes.DHCPv4
{
    public class DHCPv4RelayAgentSubnetResolver : IScopeResolver<DHCPv4Packet, IPv4Address>
    {
        #region Properties

        public IPv4Address NetworkAddress { get; private set; } = IPv4Address.Empty;
        public IPv4SubnetMask Mask { get; private set; } = IPv4SubnetMask.AllZero;

        #endregion

        #region Methods

        public Boolean HasUniqueIdentifier => false;
        public byte[] GetUniqueIdentifier(DHCPv4Packet packet) => throw new NotImplementedException();

        public Boolean PacketMeetsCondition(DHCPv4Packet packet)
        {
            Byte[] target = ByteHelper.AndArray(Mask.GetBytes(), NetworkAddress.GetBytes());
            Byte[] actual = ByteHelper.AndArray(Mask.GetBytes(), packet.GatewayIPAdress.GetBytes());

            Boolean result = ByteHelper.AreEqual(target, actual);

            return result;
        }

        public Boolean ArePropertiesAndValuesValid(IDictionary<String, String> valueMapper, ISerializer serializer)
        {
            try
            {
                if (valueMapper.ContainsKeys(new[] { nameof(NetworkAddress), nameof(Mask) }) == false)
                {
                    return false;
                }

                var address = IPv4Address.FromString(serializer.Deserialze<String>(valueMapper[nameof(NetworkAddress)]));
                var mask = new IPv4SubnetMask(new IPv4SubnetMaskIdentifier(Convert.ToInt32(serializer.Deserialze<String>(valueMapper[nameof(Mask)]))));

                return mask.IsIPAdressANetworkAddress(address);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void ApplyValues(IDictionary<String, String> valueMapper, ISerializer serializer)
        {
            this.NetworkAddress = IPv4Address.FromString(serializer.Deserialze<String>(valueMapper[nameof(NetworkAddress)]));
            this.Mask = new IPv4SubnetMask(new IPv4SubnetMaskIdentifier(Convert.ToInt32(serializer.Deserialze<String>(valueMapper[nameof(Mask)]))));
        }

        public ScopeResolverDescription GetDescription()
        {
            return new ScopeResolverDescription(
                 nameof(DHCPv4RelayAgentSubnetResolver),
                 new List<ScopeResolverPropertyDescription>
                {
                   new ScopeResolverPropertyDescription (nameof(NetworkAddress),  ScopeResolverPropertyValueTypes.IPv4Address ),
                   new ScopeResolverPropertyDescription ( nameof(Mask),  ScopeResolverPropertyValueTypes.IPv4Subnetmask ),
                }
                );
        }

        public IDictionary<String, String> GetValues() => new Dictionary<String, String>
        {
            { nameof(NetworkAddress), NetworkAddress.ToString() },
            { nameof(Mask), Mask.ToString() },
        };

        #endregion
    }
}
