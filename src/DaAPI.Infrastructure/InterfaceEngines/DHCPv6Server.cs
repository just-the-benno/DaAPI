using DaAPI.Core.Common;
using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Packets.DHCPv6;
using DaAPI.Infrastructure.ServiceBus;
using DaAPI.Infrastructure.ServiceBus.Messages;
using Microsoft.Extensions.Logging;
using NetCoreServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DaAPI.Infrastructure.InterfaceEngines
{
    public class DHCPv6Server : UdpServer
    {
        private const UInt16 _dhcpServerPort = 547;
        private const UInt16 _dhcpRelayPort = 547;
        private const UInt16 _dhcpClientPort = 546;

        private readonly IServiceBus _serviceBus;
        private readonly ILogger<DHCPv6Server> _logger;

        public DHCPv6Server(
            IPv6Address address,
            IServiceBus serviceBus,
            ILogger<DHCPv6Server> logger) : base(new IPAddress(address.GetBytes()), _dhcpServerPort)
        {
            this._serviceBus = serviceBus;
            this._logger = logger;
        }

        protected override void OnStarted()
        {
            _logger.LogInformation("Listening on port {address}:{_serverPort} for dhdpv6 packets", base.Endpoint.Address, _dhcpServerPort);
            ReceiveAsync();
        }

        protected async override void OnReceived(EndPoint endpoint, byte[] buffer, long offset, long size)
        {
            IPEndPoint iPEndPoint = (IPEndPoint)endpoint;

            _logger.LogDebug("received incoming bytes ({size}) from [{endpoint}]:{endpointPort} on [{listener}]:{listenerPort}",
                size, iPEndPoint.Address, iPEndPoint.Port, base.Endpoint.Address, base.Endpoint.Port);
            _logger.LogDebug("NEXT STEP: check if it is a valid dhcpv6 packet");

            DHCPv6Packet packet = DHCPv6Packet.FromByteArray(ByteHelper.CopyData(buffer, (Int32)offset, (Int32)size),
                new IPv6HeaderInformation(
                    IPv6Address.FromByteArray(((IPEndPoint)endpoint).Address.GetAddressBytes()),
                    IPv6Address.FromByteArray(base.Endpoint.Address.GetAddressBytes()))
                );

            if (packet.IsValid == false)
            {
                _logger.LogInformation("received an invalid DHCPv6 packet from [{endpoint}]:{endpointPort} on [{listener}]:{listenerPort}",
                    iPEndPoint.Address, iPEndPoint.Port, base.Endpoint.Address, base.Endpoint.Port);
                _logger.LogInformation("NEXT STEP: drop packet");

                await _serviceBus.Publish(new InvalidDHCPv6PacketArrivedMessage(packet));
            }
            else
            {
                _logger.LogInformation("received a valid dhcpv6 pakcet from [{endpoint}]:{endpointPort} on [{listener}]:{listenerPort}",
                    iPEndPoint.Address, iPEndPoint.Port, base.Endpoint.Address, base.Endpoint.Port);
                _logger.LogDebug("NEXT STEP: check if packet needs to be filtered");

                await _serviceBus.Publish(new DHCPv6PacketArrivedMessage(packet));
            }

            ReceiveAsync();
        }

        public bool SendAsync(DHCPv6Packet packet)
        {
            UInt16 port = _dhcpClientPort;
            if (packet is DHCPv6RelayPacket)
            {
                port = _dhcpRelayPort;
            }

            Byte[] packetAsStream = packet.GetAsStream();


            IPEndPoint remoteEndpoint = new IPEndPoint(
                new IPAddress(packet.Header.Destionation.GetBytes()),
                port);

            _logger.LogDebug("sending DHCPv6Packet with {length} bytes to [{destinationAddress}]:{destionationPort} from [{listenerAddress}:{listenerPort}]",
                packetAsStream.Length, packet.Header.Destionation, port,
                base.Endpoint.Address, base.Endpoint.Port);

            if (_logger.IsEnabled(LogLevel.Debug) == true)
            {

            }

            return SendAsync(remoteEndpoint, packetAsStream);
        }

        protected override void OnSent(EndPoint endpoint, long sent)
        {
            // Continue receive datagrams
            ReceiveAsync();
        }

        public event EventHandler<EventArgs> SocketErrorOccured;

        protected override void OnError(SocketError error)
        {
            _logger.LogError("an socket error occured", error);

            SocketErrorOccured?.Invoke(this, EventArgs.Empty);
        }
    }
}
