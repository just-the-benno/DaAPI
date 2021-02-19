using DaAPI.Core.Common;
using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Listeners;
using DaAPI.Core.Packets;
using DaAPI.Core.Packets.DHCPv6;
using DaAPI.Infrastructure.ServiceBus;
using DaAPI.Infrastructure.ServiceBus.Messages;
using DaAPI.Infrastructure.StorageEngine;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using DaAPI.Infrastructure.Helper;
using DaAPI.Infrastructure.StorageEngine.DHCPv6;
using Newtonsoft.Json.Serialization;

namespace DaAPI.Infrastructure.InterfaceEngines
{
    public class DHCPv6InterfaceEngine : DHCPInterfaceEngine<DHCPv6InterfaceEngine, DHCPv6Listener, IPv6Address, DHCPv6Server, DHCPv6Packet, DHCPv6PacketArrivedMessage, InvalidDHCPv6PacketArrivedMessage>, IDHCPv6InterfaceEngine
    {
        private readonly IDHCPv6StorageEngine _storage;

        public DHCPv6InterfaceEngine(
            IServiceBus serviceBus,
            IDHCPv6StorageEngine storage,
            ILoggerFactory loggerFactory
            ) : base(loggerFactory.CreateLogger<DHCPv6InterfaceEngine>(),
                (listener) => new DHCPv6Server(listener.Address, serviceBus, loggerFactory.CreateLogger<DHCPv6Server>()))
        {
            this._storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        public async Task<IEnumerable<DHCPv6Listener>> GetActiveListeners() => await _storage.GetDHCPv6Listener();

        protected override Boolean IsValidAddress(UnicastIPAddressInformation addressInfo)
        {
            if (addressInfo.Address.AddressFamily != System.Net.Sockets.AddressFamily.InterNetworkV6)
            {
                return false;
            }

            if (addressInfo.Address.IsIPv6LinkLocal == true)
            {
                return false;
            }

            return true;
        }

        public IEnumerable<DHCPv6Listener> GetPossibleListeners() => GetPossibleListeners((nic, ipAddress) => DHCPv6Listener.FromNIC(nic, ipAddress));
    }
}
