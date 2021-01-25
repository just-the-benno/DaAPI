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
using DaAPI.Core.Packets;

namespace DaAPI.Infrastructure.InterfaceEngines
{
    public abstract class DHCPServer<TServer, TAddress, TPacket, TPacketArrivedMessage, TInvalidPacketArrivedMessage> : UdpServer
        where TAddress : IPAddress<TAddress>
        where TPacket : DHCPPacket<TPacket, TAddress>
        where TPacketArrivedMessage : IMessage
        where TInvalidPacketArrivedMessage : IMessage

    {
        readonly protected UInt16 _port;

        private readonly IServiceBus _serviceBus;
        private readonly ILogger<TServer> _logger;

        private readonly Func<TPacket, TPacketArrivedMessage> _validPacketarrivedMessageFactory;
        private readonly Func<TPacket, TInvalidPacketArrivedMessage> _invalidPacketarrivedMessageFactory;

        public DHCPServer(
            TAddress address,
            UInt16 port,
            Func<TPacket, TPacketArrivedMessage> validPacketarrivedMessageFactory,
            Func<TPacket, TInvalidPacketArrivedMessage> invalidPacketarrivedMessageFactory,
            IServiceBus serviceBus,

            ILogger<TServer> logger) : base(new IPAddress(address.GetBytes()), port)
        {
            this._serviceBus = serviceBus;
            this._logger = logger;
            _port = port;
            _validPacketarrivedMessageFactory = validPacketarrivedMessageFactory;
            _invalidPacketarrivedMessageFactory = invalidPacketarrivedMessageFactory;
        }

        protected override void OnStarted()
        {
            _logger.LogInformation("Listening on port {address}:{_serverPort} for packets", base.Endpoint.Address, _port);
            ReceiveAsync();
        }

        protected abstract TPacket ParsePacket(Byte[] sourceAddress, byte[] buffer, Int32 offset, Int32 size);

        protected async override void OnReceived(EndPoint endpoint, byte[] buffer, long offset, long size)
        {
            IPEndPoint iPEndPoint = (IPEndPoint)endpoint;

            _logger.LogDebug("received incoming bytes ({size}) from [{endpoint}]:{endpointPort} on [{listener}]:{listenerPort}",
                size, iPEndPoint.Address, iPEndPoint.Port, base.Endpoint.Address, base.Endpoint.Port);
            _logger.LogDebug("NEXT STEP: check if it is a valid packet");

            TPacket packet = ParsePacket(((IPEndPoint)endpoint).Address.GetAddressBytes(), buffer, (Int32)offset, (Int32)size);

            if (packet.IsValid == false)
            {
                _logger.LogInformation("received an invalid DHCPv6 packet from [{endpoint}]:{endpointPort} on [{listener}]:{listenerPort}",
                    iPEndPoint.Address, iPEndPoint.Port, base.Endpoint.Address, base.Endpoint.Port);
                _logger.LogInformation("NEXT STEP: drop packet");

                await _serviceBus.Publish(_invalidPacketarrivedMessageFactory(packet));
            }
            else
            {
                _logger.LogInformation("received a valid dhcpv6 pakcet from [{endpoint}]:{endpointPort} on [{listener}]:{listenerPort}",
                    iPEndPoint.Address, iPEndPoint.Port, base.Endpoint.Address, base.Endpoint.Port);
                _logger.LogDebug("NEXT STEP: check if packet needs to be filtered");

                await _serviceBus.Publish(_validPacketarrivedMessageFactory(packet));
            }

            ReceiveAsync();
        }

        protected abstract UInt16 GetResponsePort(TPacket response);

        public bool SendAsync(TPacket packet)
        {
            UInt16 port = GetResponsePort(packet);

            Byte[] packetAsStream = packet.GetAsStream();

            IPEndPoint remoteEndpoint = new IPEndPoint(
                new IPAddress(packet.Header.Destionation.GetBytes()),
                port);

            _logger.LogDebug("sending packet with {length} bytes to [{destinationAddress}]:{destionationPort} from [{listenerAddress}:{listenerPort}]",
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
