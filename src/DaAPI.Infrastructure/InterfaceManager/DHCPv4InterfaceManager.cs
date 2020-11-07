using DaAPI.Core.Common;
using DaAPI.Core.Packets.DHCPv4;
using DaAPI.Infrastructure.Notifcations;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace DaAPI.Infrastructure.InterfaceManager
{
    public class DHCPv4InterfaceManager : IDHCPv4InterfaceManager
    {
        #region Fields

        private const UInt16 _dhcpServerPort = 67;
        private readonly IMediator mediator;
        private readonly ILogger<DHCPv4InterfaceManager> _logger;
        private readonly Dictionary<String, List<Socket>> _openSockets = new Dictionary<string, List<Socket>>();

        #endregion

        #region Constructor

        public DHCPv4InterfaceManager(
            IMediator mediator,
            ILogger<DHCPv4InterfaceManager> logger)
        {
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        public IEnumerable<IPv4Address> GetIPv4Address(String interfaceId)
        {
            NetworkInterface nic = NetworkInterface.GetAllNetworkInterfaces().FirstOrDefault(x => x.Id == interfaceId);
            if (nic == null)
            {
                return new List<IPv4Address>();
            }

            IPInterfaceProperties ipProperties = nic.GetIPProperties();
            List<IPv4Address> linkLocals = new List<IPv4Address>();

            foreach (var unicast in ipProperties.UnicastAddresses)
            {
                if (unicast.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    linkLocals.Add(IPv4Address.FromByteArray(unicast.Address.GetAddressBytes()));
                }
            }

            return linkLocals;
        }

        public void OpenSocket(String interfaceId)
        {
            if (_openSockets.ContainsKey(interfaceId) == true) { return; }

            IEnumerable<IPv4Address> addresses = GetIPv4Address(interfaceId);
            if (addresses.Any() == false) { return; }

            List<Socket> socketsOpenend = new List<Socket>();
            foreach (IPv4Address addressModel in addresses)
            {
                IPAddress address = new IPAddress(addressModel.GetBytes());
                IPEndPoint localEndPoint = new IPEndPoint(address, _dhcpServerPort);
                Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                listener.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.PacketInformation, true);
                listener.EnableBroadcast = true;

                listener.Bind(localEndPoint);

                _logger.LogInformation("unicast ipv4 socket {address}:{port} is now in listing state", localEndPoint.Address.ToString(), localEndPoint.Port);

                SocketAsyncEventArgs eventArgs = new SocketAsyncEventArgs();
                Byte[] buffer = new Byte[1500];
                eventArgs.SetBuffer(buffer, 0, buffer.Length);
                eventArgs.UserToken = listener;
                eventArgs.RemoteEndPoint = new IPEndPoint(IPAddress.Any, 68);

                eventArgs.Completed += HandleIncome;
                listener.ReceiveFromAsync(eventArgs);

                socketsOpenend.Add(listener);
            }


            _openSockets.Add(interfaceId, socketsOpenend);
        }

        private void HandleIncome(Object e, SocketAsyncEventArgs args)
        {
            _logger.LogTrace("packet income with {byteAmount}", args.BytesTransferred);
            Byte[] data = ByteHelper.CopyData(args.Buffer, 0, args.BytesTransferred);

            Socket socket = ((Socket)args.UserToken);

            IPv4Address source = IPv4Address.FromByteArray(((IPEndPoint)socket.LocalEndPoint).Address.GetAddressBytes());
            IPv4Address destionation = IPv4Address.FromByteArray(((IPEndPoint)args.RemoteEndPoint).Address.GetAddressBytes());

            DHCPv4Packet packet = DHCPv4Packet.FromByteArray(data, source,destionation); 
            if(packet.IsValid == false)
            {
                mediator.Publish(new NewInvalidPacketArrivedNotification(packet));
            }
            else
            {
                 mediator.Publish(new NewValidPacketArrivedNotification(packet));
            }

            args.SetBuffer(0, args.Buffer.Length);
            socket.ReceiveFromAsync(args);
        }

        public void CloseSocket(String interfaceId)
        {
            if (_openSockets.ContainsKey(interfaceId) == false) { return; }

            List<Socket> socketsToClose = _openSockets[interfaceId];

            foreach (var item in socketsToClose)
            {
                item.Close();
                item.Dispose();
            }

            _openSockets.Remove(interfaceId);
        }

        public void CloseAllSockets()
        {
            foreach (var item in _openSockets.Keys)
            {
                CloseSocket(item);
            }
        }
    }
}
