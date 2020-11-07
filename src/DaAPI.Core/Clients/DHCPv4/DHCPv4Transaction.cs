using DaAPI.Core.Common;
using DaAPI.Core.Packets.DHCPv4;
using System;
using System.Collections.Generic;
using System.Text;
using static DaAPI.Core.Clients.DHCPv4.DHCPv4TransactionEntryEvents;
using static DaAPI.Core.Clients.DHCPv4.DHCPv4TranscationEvents;

namespace DaAPI.Core.Clients.DHCPv4
{
    public class DHCPv4Transaction : AggregateRoot
    {
        private readonly List<DHCPv4TransactionEntry> _entries;

        public enum DHCPv4TransactionStates
        {
            DiscoverReceived = 1,
            OfferSend = 2,
            RequestAsAnswerToReceived = 3,
            AcknowledgeSend = 4,
            NotAcknowledgeSend = 5,
            RequestReceived = 6,
            InformReceived = 10,
            Incomplete = 8,
            DeclineReceived = 11,
            ReleaseReceived = 12,
        }

        public UInt32 TransactionNumber { get; private set; }
        public Boolean IsActive { get; private set; }
        public DHCPv4TransactionStates State { get; private set; }

        public DHCPv4Transaction(Guid id, Action<DomainEvent> applier) : base(id, applier)
        {
            _entries = new List<DHCPv4TransactionEntry>();
        }

        public static DHCPv4Transaction NotFound => null;

        protected override void When(DomainEvent domainEvent)
        {
            switch (domainEvent)
            {
                case DHCPv4TransactionCreatedEvent e:
                    TransactionNumber = e.TransactionId;
                    IsActive = true;
                    switch (e.MessageType)
                    {
                        case DHCPv4Packet.DHCPv4MessagesTypes.DHCPDISCOVER:
                            State = DHCPv4TransactionStates.DiscoverReceived;
                            break;
                        case DHCPv4Packet.DHCPv4MessagesTypes.Request:
                            State = DHCPv4TransactionStates.RequestReceived;
                            break;
                        case DHCPv4Packet.DHCPv4MessagesTypes.DHCPINFORM:
                            State = DHCPv4TransactionStates.InformReceived;
                            break;
                        case DHCPv4Packet.DHCPv4MessagesTypes.DHCPRELEASE:
                            State = DHCPv4TransactionStates.ReleaseReceived;
                            break;
                        default:
                            break;
                    }
                    break;
                case DHCPv4TransactionStateChangedEvent e:
                    State = e.State;
                    break;
                case DHCPv4TransactionEntryCreatedEvent e:
                    DHCPv4TransactionEntry entry = new DHCPv4TransactionEntry(e.EntityId, Apply);
                    ApplyToEnity(entry, e);
                    _entries.Add(entry);
                    break;
                default:
                    break;
            }
        }

        internal void AddTransactionDiscoverEntry(DHCPv4Packet packet, DHCPv4Packet result)
        {
            if (State != DHCPv4TransactionStates.DiscoverReceived)
            {
                Apply(new DHCPv4SuspiciousTransactionDiscoverdEvent(Id, packet,DHCPv4SuspiciousTransactionDiscoverdEvent.SuspiciousReasons.NonExpectedState));
            }

            Apply(new DHCPv4TransactionStateChangedEvent(Id, DHCPv4TransactionStates.OfferSend));
            Apply(new DHCPv4TransactionEntryCreatedEvent(Guid.NewGuid(), Id, packet, result));
        }

        internal void AddTransactionRequestEntry(DHCPv4Packet request, DHCPv4Packet response)
        {
            var requestType = request.GetRequestType();

            if (requestType == DHCPv4Packet.DHCPv4PacketRequestType.AnswerToOffer)
            {
                HandleResponse(request, response, DHCPv4TransactionStates.OfferSend);
            }
            else
            {
                HandleResponse(request, response, DHCPv4TransactionStates.RequestReceived);
            }
        }

        private void HandleResponse(DHCPv4Packet request, DHCPv4Packet response, DHCPv4TransactionStates expectedState )
        {
            if (State != expectedState)
            {
                Apply(new DHCPv4SuspiciousTransactionDiscoverdEvent(Id, request, DHCPv4SuspiciousTransactionDiscoverdEvent.SuspiciousReasons.NonExpectedState));
            }

            DHCPv4TransactionStates nextState = DHCPv4TransactionStates.Incomplete;
            Boolean finisehdSuccessfully = false;
            if (response != DHCPv4Packet.Empty)
            {
                nextState = GetStateFromPacketType(response);
                finisehdSuccessfully = true;
            }

            Apply(new DHCPv4TransactionStateChangedEvent(Id, nextState));
            Apply(new DHCPv4TransactionCompletedEvent(Id, finisehdSuccessfully));
        }

        internal void AddTransactionInformEntry(DHCPv4Packet request)
        {
            if (State != DHCPv4TransactionStates.InformReceived)
            {
                Apply(new DHCPv4SuspiciousTransactionDiscoverdEvent(Id, request, DHCPv4SuspiciousTransactionDiscoverdEvent.SuspiciousReasons.NonExpectedState));
            }

            throw new NotImplementedException();
        }

        internal void AddTransactionReleaseEntry(DHCPv4Packet request, DHCPv4Packet response)
        {
            HandleResponse(request, response, DHCPv4TransactionStates.InformReceived);
        }

        internal void AddTransactionDeclineEntry(DHCPv4Packet request)
        {
            if (State != DHCPv4TransactionStates.AcknowledgeSend)
            {
                Apply(new DHCPv4SuspiciousTransactionDiscoverdEvent(Id, request, DHCPv4SuspiciousTransactionDiscoverdEvent.SuspiciousReasons.NonExpectedState));
            }

            Apply(new DHCPv4TransactionStateChangedEvent(Id, DHCPv4TransactionStates.DeclineReceived));
        }

        private DHCPv4TransactionStates GetStateFromPacketType(DHCPv4Packet response)
        {
            DHCPv4TransactionStates nexState = DHCPv4TransactionStates.AcknowledgeSend;
            switch (response.MessageType)
            {
                case DHCPv4Packet.DHCPv4MessagesTypes.DHCPOFFER:
                    nexState = DHCPv4TransactionStates.OfferSend;
                    break;
                case DHCPv4Packet.DHCPv4MessagesTypes.Acknowledge:
                    nexState = DHCPv4TransactionStates.AcknowledgeSend;
                    break;
                case DHCPv4Packet.DHCPv4MessagesTypes.NotAcknowledge:
                    nexState = DHCPv4TransactionStates.NotAcknowledgeSend;
                    break;
                default:
                    break;
            }

            return nexState;
        }
    }
}
