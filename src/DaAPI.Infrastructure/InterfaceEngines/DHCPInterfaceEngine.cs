using DaAPI.Core.Common;
using DaAPI.Core.Listeners;
using DaAPI.Core.Packets;
using DaAPI.Infrastructure.ServiceBus.Messages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using DaAPI.Infrastructure.Helper;

namespace DaAPI.Infrastructure.InterfaceEngines
{
    public abstract class DHCPInterfaceEngine<TEngine, TListeners, TAddress, TServer, TPacket, TPacketArrivedMessage, TInvalidPacketArrivedMessage>
        where TListeners : DHCPListener<TAddress>
        where TAddress : IPAddress<TAddress>
        where TServer : DHCPServer<TServer, TAddress, TPacket, TPacketArrivedMessage, TInvalidPacketArrivedMessage>
        where TPacket : DHCPPacket<TPacket, TAddress>
        where TPacketArrivedMessage : IMessage
        where TInvalidPacketArrivedMessage : IMessage
    {
        private readonly ILogger<TEngine> _logger;
        private readonly Dictionary<TListeners, TServer> _activeSockets = new Dictionary<TListeners, TServer>();
        private readonly Dictionary<TAddress, TServer> _addressSocketMapper = new Dictionary<TAddress, TServer>();
        private readonly Func<TListeners, TServer> _serverFactory;

        public DHCPInterfaceEngine(
            ILogger<TEngine> logger,
            Func<TListeners, TServer> serverFactory
            )
        {
            _logger = logger;
            _serverFactory = serverFactory;
        }

        protected abstract Boolean IsValidAddress(UnicastIPAddressInformation addressInfo);

        public IEnumerable<TListeners> GetPossibleListeners(Func<NetworkInterface, IPAddress, TListeners> listnerFactory)
        {
            List<TListeners> result = new();

            foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                var properites = nic.GetIPProperties();
                if (properites == null) { continue; }

                foreach (var ipAddress in properites.UnicastAddresses)
                {
                    if (IsValidAddress(ipAddress) == false)
                    {
                        continue;
                    }

                    TListeners listener = listnerFactory(nic, ipAddress.Address);
                    result.Add(listener);
                }
            }

            return result;
        }

        public Boolean OpenListener(TListeners listener)
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
                _logger.LogError("the nic {nic} is not configured for ip", nic);
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

            TServer server = _serverFactory(listener);
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
            RestartSever((TServer)sender);
        }

        private void RestartSever(TServer notWorkingServer)
        {
            _logger.LogInformation("server {address} requested a restart", notWorkingServer.Endpoint.Address);

            var listener = _activeSockets.Where(x => x.Value == notWorkingServer).Select(x => x.Key).FirstOrDefault();
            if (listener != null)
            {
                CloseListener(listener);
                OpenListener(listener);
            }
        }

        public Boolean CloseListener(TListeners listener)
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

        public Boolean SendPacket(TPacket packet)
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
                        item.Value.SocketErrorOccured -= Server_SocketErrorOccured;
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
