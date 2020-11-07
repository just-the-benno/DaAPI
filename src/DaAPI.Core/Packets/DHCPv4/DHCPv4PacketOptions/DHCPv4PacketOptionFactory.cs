using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.Core.Packets.DHCPv4
{
    public static class DHCPv4PacketOptionFactory
    {
        #region Fields

        private static readonly Dictionary<Byte, Func<Byte[], DHCPv4PacketOption>> _constructorDict;

        #endregion

        #region Constructor

        static DHCPv4PacketOptionFactory()
        {
            _constructorDict = new Dictionary<Byte, Func<byte[], DHCPv4PacketOption>>
            {
                { (Byte)DHCPv4OptionTypes.SubnetMask, (data) => DHCPv4PacketAddressOption.FromByteArray(data,0) },
                { (Byte)DHCPv4OptionTypes.TimeOffset, (data) => DHCPv4PacketTimeSpanOption.FromByteArray(data,0,true) },
                { (Byte)DHCPv4OptionTypes.Router, (data) => DHCPv4PacketAddressListOption.FromByteArray(data,0) },
                { (Byte)DHCPv4OptionTypes.TimeServer, (data) => DHCPv4PacketAddressListOption.FromByteArray(data,0)  },
                { (Byte)DHCPv4OptionTypes.NameServer, (data) => DHCPv4PacketAddressListOption.FromByteArray(data,0)  },
                { (Byte)DHCPv4OptionTypes.DNSServers, (data) => DHCPv4PacketAddressListOption.FromByteArray(data,0)  },
                { (Byte)DHCPv4OptionTypes.LogServers, (data) => DHCPv4PacketAddressListOption.FromByteArray(data,0)  },
                { (Byte)DHCPv4OptionTypes.CookieServer, (data) => DHCPv4PacketAddressListOption.FromByteArray(data,0)  },
                { (Byte)DHCPv4OptionTypes.LRPServers, (data) =>DHCPv4PacketAddressListOption.FromByteArray(data,0)  },
                { (Byte)DHCPv4OptionTypes.ImpressSever, (data) => DHCPv4PacketAddressListOption.FromByteArray(data,0)  },
                { (Byte)DHCPv4OptionTypes.ResourceLocationServer, (data) => DHCPv4PacketAddressListOption.FromByteArray(data,0)  },
                { (Byte)DHCPv4OptionTypes.Hostname, (data) => DHCPv4PacketTextOption.FromByteArray(data,0) },
                { (Byte)DHCPv4OptionTypes.BootFileSize, (data) => DHCPv4PacketUInt16Option.FromByteArray(data,0)  },
                { (Byte)DHCPv4OptionTypes.MaritDumpFile, (data) => DHCPv4PacketTextOption.FromByteArray(data,0) },
                { (Byte)DHCPv4OptionTypes.DomainName, (data) => DHCPv4PacketTextOption.FromByteArray(data,0)},
                { (Byte)DHCPv4OptionTypes.SwapServer, (data) => DHCPv4PacketAddressOption.FromByteArray(data,0)  },
                { (Byte)DHCPv4OptionTypes.RootPath, (data) => DHCPv4PacketTextOption.FromByteArray(data,0) },
                { (Byte)DHCPv4OptionTypes.ExtentionsPath, (data) => DHCPv4PacketTextOption.FromByteArray(data,0) },
                { (Byte)DHCPv4OptionTypes.IpForwardingEnabled, (data) => DHCPv4PacketBooleanOption.FromByteArray(data,0)  },
                { (Byte)DHCPv4OptionTypes.NonLocalSourceRouting, (data) => DHCPv4PacketBooleanOption.FromByteArray(data,0) },
                { (Byte)DHCPv4OptionTypes.PolicyFilter, (data) => DHCPv4PacketRouteListOption.FromByteArray(data,0) },
                { (Byte)DHCPv4OptionTypes.MaximumDatagramReassembly, (data) =>  DHCPv4PacketUInt16Option.FromByteArray(data,0)},
                { (Byte)DHCPv4OptionTypes.DefaultTTL, (data) => DHCPv4PacketByteOption.FromByteArray(data,0) },
                { (Byte)DHCPv4OptionTypes.PathMTUAgingTimeout, (data) => DHCPv4PacketUInt32Option.FromByteArray(data,0) },
                { (Byte)DHCPv4OptionTypes.PathMTUPlateauTable, (data) =>  DHCPv4PacketUInt16Option.FromByteArray(data,0)},
                { (Byte)DHCPv4OptionTypes.InterfaceMTU, (data) =>  DHCPv4PacketUInt16Option.FromByteArray(data,0) },
                { (Byte)DHCPv4OptionTypes.AllSubnetsAreLocal, (data) => DHCPv4PacketBooleanOption.FromByteArray(data,0)},
                { (Byte)DHCPv4OptionTypes.BroadcastAddress, (data) => DHCPv4PacketAddressOption.FromByteArray(data,0) },
                { (Byte)DHCPv4OptionTypes.PerformMaskDiscovery, (data) => DHCPv4PacketBooleanOption.FromByteArray(data,0) },
                { (Byte)DHCPv4OptionTypes.MaskSupplier, (data) => DHCPv4PacketBooleanOption.FromByteArray(data,0) },
                { (Byte)DHCPv4OptionTypes.PerformRouterDiscovery, (data) => DHCPv4PacketBooleanOption.FromByteArray(data,0) },
                { (Byte)DHCPv4OptionTypes.RouterSolicitationAddress, (data) => DHCPv4PacketAddressOption.FromByteArray(data,0)  },
                { (Byte)DHCPv4OptionTypes.StaticRoute, (data) => DHCPv4PacketRouteListOption.FromByteArray(data,0) },
                { (Byte)DHCPv4OptionTypes.TrailerEncapsulation, (data) => DHCPv4PacketBooleanOption.FromByteArray(data,0)},
                { (Byte)DHCPv4OptionTypes.ARPCacheTimeout, (data) => DHCPv4PacketTimeSpanOption.FromByteArray(data,0,false) },
                { (Byte)DHCPv4OptionTypes.EthernetEncapsulation, (data) => DHCPv4PacketBooleanOption.FromByteArray(data,0)},
                { (Byte)DHCPv4OptionTypes.TCPDefaultTTL, (data) => DHCPv4PacketByteOption.FromByteArray(data,0) },
                { (Byte)DHCPv4OptionTypes.TCPKeepaliveGarbage, (data) => DHCPv4PacketBooleanOption.FromByteArray(data,0)},
                { (Byte)DHCPv4OptionTypes.NetworkInformationServiceDomain, (data) =>  DHCPv4PacketTextOption.FromByteArray(data,0) },
                { (Byte)DHCPv4OptionTypes.NetworkInformationServers, (data) => DHCPv4PacketAddressListOption.FromByteArray(data,0)  },
                { (Byte)DHCPv4OptionTypes.NetworkTimeProtocolServers, (data) => DHCPv4PacketAddressListOption.FromByteArray(data,0)  },
                { (Byte)DHCPv4OptionTypes.VendorSpecificInformation, (data) => DHCPv4PacketRawByteOption.FromByteArray(data,0) },
                { (Byte)DHCPv4OptionTypes.NetBIOSoverTCP_IPNameServer, (data) => DHCPv4PacketAddressListOption.FromByteArray(data,0)  },
                { (Byte)DHCPv4OptionTypes.NetBIOSoverTCP_IPDatagramDistributionServer, (data) => DHCPv4PacketAddressListOption.FromByteArray(data,0) },
                { (Byte)DHCPv4OptionTypes.NetBIOSoverTCP_IPNodeType, (data) => DHCPv4PacketByteOption.FromByteArray(data,0) },
                { (Byte)DHCPv4OptionTypes.NetBIOSoverTCP_IPScope, (data) => DHCPv4PacketRawByteOption.FromByteArray(data,0) },
                { (Byte)DHCPv4OptionTypes.XWindowSystemFontServer, (data) => DHCPv4PacketAddressListOption.FromByteArray(data,0)  },
                { (Byte)DHCPv4OptionTypes.XWindowSystemDisplayManager, (data) => DHCPv4PacketAddressListOption.FromByteArray(data,0)  },
                { (Byte)DHCPv4OptionTypes.NetworkInformationServicePlusDomain, (data) =>  DHCPv4PacketTextOption.FromByteArray(data,0) },
                { (Byte)DHCPv4OptionTypes.NetworkInformationServicePlusServers, (data) => DHCPv4PacketAddressListOption.FromByteArray(data,0) },
                { (Byte)DHCPv4OptionTypes.MobileIPHomeAgent, (data) => DHCPv4PacketAddressListOption.FromByteArray(data,0) },
                { (Byte)DHCPv4OptionTypes.SMTPServers, (data) => DHCPv4PacketAddressListOption.FromByteArray(data,0)  },
                { (Byte)DHCPv4OptionTypes.POP3Servers, (data) => DHCPv4PacketAddressListOption.FromByteArray(data,0)  },
                { (Byte)DHCPv4OptionTypes.NetworkNewsTransportProtocolServer, (data) => DHCPv4PacketAddressListOption.FromByteArray(data,0)  },
                { (Byte)DHCPv4OptionTypes.WWWServers, (data) =>DHCPv4PacketAddressListOption.FromByteArray(data,0)  },
                { (Byte)DHCPv4OptionTypes.DefaultFingerServer, (data) => DHCPv4PacketAddressListOption.FromByteArray(data,0)  },
                { (Byte)DHCPv4OptionTypes.IRCServer, (data) => DHCPv4PacketAddressListOption.FromByteArray(data,0)  },
                { (Byte)DHCPv4OptionTypes.StreetTalkServer, (data) => DHCPv4PacketAddressListOption.FromByteArray(data,0)  },
                { (Byte)DHCPv4OptionTypes.StreetTalkDirectoryAssistance, (data) => DHCPv4PacketAddressListOption.FromByteArray(data,0)  },
                { (Byte)DHCPv4OptionTypes.RequestedIPAddress, (data) => DHCPv4PacketAddressOption.FromByteArray(data,0) },
                { (Byte)DHCPv4OptionTypes.IPAddressLeaseTime, (data) => DHCPv4PacketTimeSpanOption.FromByteArray(data,0,false) },
                { (Byte)DHCPv4OptionTypes.OptionOverload, (data) => DHCPv4PacketByteOption.FromByteArray(data,0) },
                { (Byte)DHCPv4OptionTypes.TFTPServername, (data) => DHCPv4PacketTextOption.FromByteArray(data,0) },
                { (Byte)DHCPv4OptionTypes.BootfileName, (data) => DHCPv4PacketTextOption.FromByteArray(data,0) },
                { (Byte)DHCPv4OptionTypes.MessageType, (data) => DHCPv4PacketMessageTypeOption.FromByteArray(data,0) },
                { (Byte)DHCPv4OptionTypes.ServerIdentifier, (data) => DHCPv4PacketAddressOption.FromByteArray(data,0) },
                { (Byte)DHCPv4OptionTypes.ParamterRequestList, (data) =>DHCPv4PacketParameterRequestListOption.FromByteArray(data,0)},
                { (Byte)DHCPv4OptionTypes.Message, (data) => DHCPv4PacketTextOption.FromByteArray(data,0) },
                { (Byte)DHCPv4OptionTypes.MaximumDHCPMessageSize, (data) => DHCPv4PacketUInt16Option.FromByteArray(data,0) },
                { (Byte)DHCPv4OptionTypes.RenewalTimeValue, (data) => DHCPv4PacketTimeSpanOption.FromByteArray(data,0,false) },
                { (Byte)DHCPv4OptionTypes.RebindingTimeValue, (data) => DHCPv4PacketTimeSpanOption.FromByteArray(data,0,false) },
                { (Byte)DHCPv4OptionTypes.VendorClassIdentifier, (data) =>  DHCPv4PacketRawByteOption.FromByteArray(data,0)  },
                { (Byte)DHCPv4OptionTypes.ClientIdentifier, (data) =>  DHCPv4PacketRawByteOption.FromByteArray(data,0) },
                { (Byte)DHCPv4OptionTypes.Option82, (data) => DHCPv4PacketRawByteOption.FromByteArray(data,0) },
            };
        }

        #endregion

        #region Methods

        public static void AddOptionType(Byte code, Func<Byte[], DHCPv4PacketOption> func, Boolean replace)
        {
            if (_constructorDict.ContainsKey(code) == true)
            {
                if (replace == false)
                {
                    throw new InvalidOperationException("option code exists, try replace instead");
                }

                _constructorDict[code] = func;
            }
            else
            {
                _constructorDict.Add(code, func);
            }
        }

        public static DHCPv4PacketOption GetOption(Byte[] data)
        {
            Byte code = data[0];
            return GetOption(code, data);
        }

        public static DHCPv4PacketOption GetOption(Byte code, Byte[] data)
        {
            if (_constructorDict.ContainsKey(code) == true)
            {
                return _constructorDict[code].Invoke(data);
            }
            else
            {
                return DHCPv4PacketRawByteOption.FromByteArray(data, 0);
            }
        }

        #endregion
    }
}
