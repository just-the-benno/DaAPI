using DaAPI.Core.Clients.DHCPv4;
using DaAPI.Core.Common;
using DaAPI.Core.Exceptions;
using DaAPI.Core.Packets.DHCPv4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static DaAPI.Core.Clients.DHCPv4.DHCPv4ClientEvents;
using static DaAPI.Core.Clients.DHCPv4.DHCPv4TranscationEvents;

namespace DaAPI.Core.Clients
{
    public class DHCPv4Client : AggregateRootWithEvents
    {
        private readonly List<DHCPv4Transaction> _transactions = new List<DHCPv4Transaction>();

        #region Properties

        public DHCPv4ClientIdentifier Identifier { get; private set; }

        #endregion

        #region constructor and factories

        private DHCPv4Client(Guid id) : base(id)
        {

        }
        public DHCPv4Client() : base(Guid.Empty)
        {

        }

        public static DHCPv4Client Create(Guid id, DHCPv4ClientIdentifier clientIdentifier)
        {
            DHCPv4Client client = new DHCPv4Client(id);

            client.Apply(new DHCPv4ClientCreatedEvent(
                id,
                clientIdentifier
                ));

            return client;
        }

        public static DHCPv4Client Unknow => null;

        #endregion

        protected override void When(DomainEvent domainEvent)
        {
            switch (domainEvent)
            {
                case DHCPv4ClientCreatedEvent e:
                    Identifier = e.Identifier;
                    break;
                case DHCPv4TransactionCreatedEvent e:
                    DHCPv4Transaction transaction = new DHCPv4Transaction(e.EntityId, Apply);
                    ApplyToEnity(transaction, e);
                    _transactions.Add(transaction);
                    break;
                default:
                    break;
            }
        }

        private DHCPv4Transaction GetTransactionById(UInt32 id)
        {
            DHCPv4Transaction transaction = _transactions.FirstOrDefault(x => x.TransactionNumber == id);
            return transaction;
        }

        public void AddTransaction(DHCPv4Packet packet)
        {
            DHCPv4Transaction transaction = GetTransactionById(packet.TransactionId);
            if (transaction == DHCPv4Transaction.NotFound)
            {
                if (
                    packet.MessageType != DHCPv4Packet.DHCPv4MessagesTypes.DHCPDISCOVER ||
                    packet.MessageType != DHCPv4Packet.DHCPv4MessagesTypes.Request ||
                    packet.MessageType != DHCPv4Packet.DHCPv4MessagesTypes.DHCPINFORM ||
                    packet.MessageType != DHCPv4Packet.DHCPv4MessagesTypes.DHCPRELEASE
                    )
                {
                    Apply(new DHCPv4SuspiciousTransactionByClientDiscoverdEvent(
                        Id, packet,
                        DHCPv4SuspiciousTransactionByClientDiscoverdEvent.SuspiciousReasons.MessageTypeSholdNotStartANewTransaction));
                }

                Apply(new DHCPv4TransactionCreatedEvent(Guid.NewGuid(), packet.MessageType, packet.TransactionId));
            }
            else
            {
                Apply(new DHCPv4SuspiciousTransactionByClientDiscoverdEvent(Id, packet, DHCPv4SuspiciousTransactionByClientDiscoverdEvent.SuspiciousReasons.TransactionFoundByExpectedANew));
            }
        }

        private void UpdateTransaction(DHCPv4Packet packet, DHCPv4Transaction transaction)
        {
            if (transaction == DHCPv4Transaction.NotFound)
            {
                Apply(new DHCPv4SuspiciousTransactionByClientDiscoverdEvent(Id, packet, DHCPv4SuspiciousTransactionByClientDiscoverdEvent.SuspiciousReasons.TransactionNotFound));
            }
            else if (transaction.IsActive == false)
            {
                Apply(new DHCPv4SuspiciousTransactionByClientDiscoverdEvent(Id, packet, DHCPv4SuspiciousTransactionByClientDiscoverdEvent.SuspiciousReasons.TransactionAlreadyClosed));
            }
        }
        public void UpdateTransaction(DHCPv4Packet packet)
        {
            DHCPv4Transaction transaction = GetTransactionById(packet.TransactionId);
            UpdateTransaction(packet, transaction);
        }

        public void AddOrUpdateTransaction(DHCPv4Packet packet)
        {
            DHCPv4Transaction transaction = GetTransactionById(packet.TransactionId);
            if (transaction == DHCPv4Transaction.NotFound)
            {
                AddTransaction(packet);
            }
            else
            {
                UpdateTransaction(packet, transaction);
            }
        }
        public void AddTransactionEntry(DHCPv4Packet request, DHCPv4Packet response)
        {
            DHCPv4Transaction transaction = GetTransactionById(request.TransactionId);
            if (transaction == DHCPv4Transaction.NotFound)
            {
                Apply(new DHCPv4SuspiciousTransactionByClientDiscoverdEvent(Id, request, DHCPv4SuspiciousTransactionByClientDiscoverdEvent.SuspiciousReasons.TransactionNotFound));
                return;
            }

            switch (request.MessageType)
            {
                case DHCPv4Packet.DHCPv4MessagesTypes.DHCPDISCOVER:
                    transaction.AddTransactionDiscoverEntry(request, response);
                    break;
                case DHCPv4Packet.DHCPv4MessagesTypes.Request:
                    transaction.AddTransactionRequestEntry(request, response);
                    break;
                case DHCPv4Packet.DHCPv4MessagesTypes.Decline:
                    transaction.AddTransactionDeclineEntry(request);
                    break;
                case DHCPv4Packet.DHCPv4MessagesTypes.DHCPRELEASE:
                    transaction.AddTransactionReleaseEntry(request, response);
                    break;
                case DHCPv4Packet.DHCPv4MessagesTypes.DHCPINFORM:
                    transaction.AddTransactionInformEntry(request);
                    break;
                default:
                    break;
            }
        }
    }
}
