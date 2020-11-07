using DaAPI.Core.Clients;
using DaAPI.Core.Common;
using DaAPI.Core.Packets.DHCPv4;
using DaAPI.Core.Scopes.DHCPv4;
using DaAPI.Infrastructure.AggregateStore;
using DaAPI.Infrastructure.Notifcations;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DaAPI.Infrastructure.LeaseEngines
{
    public class DHCPv4LeaseEngine : INotificationHandler<DHCPv4PacketReadyToProcessNotification>
    {
        private readonly IMediator _mediator;
        private readonly IDHCPv4AggregateStore _store;
        private readonly DHCPv4RootScope _rootScope;

        public DHCPv4LeaseEngine(
                  IMediator mediator,
                  IDHCPv4AggregateStore store,
                  DHCPv4RootScope rootScope)
        {
            this._mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            this._store = store ?? throw new ArgumentNullException(nameof(store));
            this._rootScope = rootScope ?? throw new ArgumentNullException(nameof(rootScope));
        }

        public async Task Handle(DHCPv4PacketReadyToProcessNotification notification, CancellationToken cancellationToken)
        {
            await HandlePacket(notification.Packet);
        }

        public async Task<Boolean> HandlePacket(DHCPv4Packet packet)
        {
            if (packet == null || packet.IsValid == false)
            {
                return false;
            }

            DHCPv4ClientIdentifier clientIdentifier = packet.GetClientIdentifier();
          
            DHCPv4Client client = await _store.GetClientByClientIdentifier(clientIdentifier);
            if(client == DHCPv4Client.Unknow)
            {
                client = DHCPv4Client.Create(Guid.NewGuid(), clientIdentifier);
            }

            DHCPv4Packet result;
            if (packet.MessageType == DHCPv4Packet.DHCPv4MessagesTypes.DHCPDISCOVER)
            {
                client.AddTransaction(packet);
                result = _rootScope.HandleDiscover(packet);
                
            }
            else if(packet.MessageType == DHCPv4Packet.DHCPv4MessagesTypes.Request)
            {
                client.AddOrUpdateTransaction(packet);
                result = _rootScope.HandleRequest(packet);
            }
            else if(packet.MessageType == DHCPv4Packet.DHCPv4MessagesTypes.Decline)
            {
                client.UpdateTransaction(packet);
                result = _rootScope.HandleDecline(packet);
            }
            else if (packet.MessageType == DHCPv4Packet.DHCPv4MessagesTypes.DHCPINFORM)
            {
                client.AddTransaction(packet);
                result = _rootScope.HandleInform(packet);
            }
            else if (packet.MessageType == DHCPv4Packet.DHCPv4MessagesTypes.DHCPRELEASE)
            {
                client.AddOrUpdateTransaction(packet);
                result = _rootScope.HandleRelease(packet);
            }
            else
            {
                return false;
            }

            client.AddTransactionEntry(packet, result);

            await _store.Save(_rootScope);
            await _store.Save(client);

            if (result != DHCPv4Packet.Empty)
            {
                await _mediator.Publish(new NewPacketReadyToSendNotification(result));
            }

            //await _mediator.Publish(new DHCPv4PacketHandledNotifaction(changesAtScope,changesAtClient));

            return true;
        }
    }
}
