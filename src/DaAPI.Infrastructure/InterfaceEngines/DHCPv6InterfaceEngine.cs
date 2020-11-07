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
    public class DHCPv6InterfaceEngine : IDHCPv6InterfaceEngine
    {
        private readonly ILogger<DHCPv6InterfaceEngine> _logger;
        private readonly IDHCPv6StorageEngine _storage;
        private readonly IServiceBus _serviceBus;
        private readonly ILoggerFactory _loggerFactory;
        private readonly Dictionary<DHCPv6Listener, DHCPv6Server> _activeSockets = new Dictionary<DHCPv6Listener, DHCPv6Server>();
        private readonly Dictionary<IPv6Address, DHCPv6Server> _addressSocketMapper = new Dictionary<IPv6Address, DHCPv6Server>();


        public DHCPv6InterfaceEngine(
            IServiceBus serviceBus,
            IDHCPv6StorageEngine storage,
            ILoggerFactory loggerFactory
            )
        {
            _serviceBus = serviceBus ?? throw new ArgumentNullException(nameof(serviceBus));
            this._storage = storage ?? throw new ArgumentNullException(nameof(storage));
            this._loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            this._logger = loggerFactory.CreateLogger<DHCPv6InterfaceEngine>();
        }

        public async Task<IEnumerable<DHCPv6Listener>> GetActiveListeners() => await _storage.GetDHCPv6Listener();

        public IEnumerable<DHCPv6Listener> GetPossibleListeners()
        {
            List<DHCPv6Listener> result = new List<DHCPv6Listener>();

            foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                var properites = nic.GetIPProperties();
                if (properites == null) { continue; }

                foreach (var ipAddress in properites.UnicastAddresses)
                {
                    if (ipAddress.Address.AddressFamily != System.Net.Sockets.AddressFamily.InterNetworkV6)
                    {
                        continue;
                    }

                    if (ipAddress.Address.IsIPv6LinkLocal == true)
                    {
                        continue;
                    }

                    DHCPv6Listener listener = DHCPv6Listener.FromNIC(nic, ipAddress.Address);
                    result.Add(listener);
                }
            }

            return result;
        }

        public Boolean OpenListener(DHCPv6Listener listener)
        {
            if (_activeSockets.ContainsKey(listener) == true)
            {
                _logger.LogInformation("a socket to {address} is already listening", listener);
                return false;
            }

            NetworkInterface nic = NetworkInterface.GetAllNetworkInterfaces()
                .FirstOrDefault(x => x.Id == listener.PhysicalInterfaceId);

            if (nic == null)
            {
                _logger.LogError("no network interface card with {address} found", listener);
                return false;
            }

            var properties = nic.GetIPProperties();
            if (properties == null)
            {
                _logger.LogError("the nic {nic} is not configured for ipv6", nic);
                return false;
            }

            var unicastAddress = properties.UnicastAddresses.FirstOrDefault(x =>
            ByteHelper.AreEqual(x.Address.GetAddressBytes(), listener.Address.GetBytes()));

            if (unicastAddress == null)
            {
                _logger.LogError("the nic {nic} is has no {address} configured", nic, listener);
                return false;
            }

            _logger.LogDebug("initilaizing server for {addresss}", listener);

            DHCPv6Server server = new DHCPv6Server(listener.Address, _serviceBus, _loggerFactory.CreateLogger<DHCPv6Server>());
            Boolean result = server.Start();
            if (result == true)
            {
                _activeSockets.Add(listener, server);
                _addressSocketMapper.Add(listener.Address, server);
                server.SocketErrorOccured += Server_SocketErrorOccured;

                _logger.LogDebug("server is now listing on {address}", listener);
            }
            else
            {
                _logger.LogError("unable to start the dhcpv6 server. No packets will be received");
            }

            return result;
        }

        private void Server_SocketErrorOccured(object sender, EventArgs e)
        {
            RestartSever((DHCPv6Server)sender);
        }

        private void RestartSever(DHCPv6Server notWorkingServer)
        {
            _logger.LogInformation("server {address} requested a restart", notWorkingServer.Endpoint.Address);

            var listener = _activeSockets.Where(x => x.Value == notWorkingServer).Select(x => x.Key).FirstOrDefault();
            if (listener != null)
            {
                CloseListener(listener);
                OpenListener(listener);
            }
        }

        public Boolean CloseListener(DHCPv6Listener listener)
        {
            _logger.LogDebug("closing listener {address}...", listener.Address);
            if (_activeSockets.ContainsKey(listener) == false)
            {
                _logger.LogError("unable to find a active socket for {address}", listener);
                return false;
            }

            var server = _activeSockets[listener];
            Boolean stopped = server.Stop();
            server.Dispose();

            server.SocketErrorOccured += Server_SocketErrorOccured;

            _addressSocketMapper.Remove(server);
            _activeSockets.Remove(listener);

            _logger.LogDebug("server with {address} stopped and disposed", listener);

            return stopped;
        }

        public Boolean SendPacket(DHCPv6Packet packet)
        {
            if (_addressSocketMapper.ContainsKey(packet.Header.ListenerAddress) == false)
            {
                _logger.LogError("unbale to find a socket for {address}", packet.Header.Source);
                return false;
            }

            var server = _addressSocketMapper[packet.Header.Source];
            Boolean result = server.SendAsync(packet);
            if (result == false)
            {
                _logger.LogError("unable to send packet to {receiver}", packet.Header.Destionation);
            }

            return result;
        }

        protected virtual void Dispose(bool disposing)
        {
            _logger.LogDebug("Dispose");
            if (disposing == true)
            {
                foreach (var item in _activeSockets)
                {
                    try
                    {
                        item.Value.Stop();
                        item.Value.Dispose();
                        item.Value.SocketErrorOccured += Server_SocketErrorOccured;
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
            }
        }

        public void Dispose()
        {
            // Dispose of unmanaged resources.
            Dispose(true);
            // Suppress finalization.
            GC.SuppressFinalize(this);
        }
    }
}
