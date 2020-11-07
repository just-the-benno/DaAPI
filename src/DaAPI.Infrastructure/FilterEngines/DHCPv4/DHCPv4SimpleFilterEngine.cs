using DaAPI.Core.Common;
using DaAPI.Core.Packets.DHCPv4;
using DaAPI.Infrastructure.Notifcations;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static DaAPI.Core.Packets.DHCPv4.DHCPv4Packet;

namespace DaAPI.Infrastructure.FilterEngines.DHCPv4
{
    public class DHCPv4SimpleFilterEngine : IDHCPv4FilterEngine, INotificationHandler<NewValidPacketArrivedNotification>
    {
        #region Fields

        private readonly IDHCPv4TransactionIdBasedFilter _transactionFilter;
        private readonly IDHCPv4ClientFilter _clientFilter;
        private readonly IDHCPv4RateLimitBasedFilter _rateLimiter;
        private readonly IMediator mediator;

        #endregion

        #region Properties

        #endregion

        #region Constructor

        public DHCPv4SimpleFilterEngine(
            IDHCPv4TransactionIdBasedFilter transactionFilter,
            IDHCPv4ClientFilter clientFilter,
            IDHCPv4RateLimitBasedFilter rateLimiter,
            IMediator mediator
            )
        {
            this._transactionFilter = transactionFilter ?? throw new ArgumentNullException(nameof(transactionFilter));
            this._clientFilter = clientFilter ?? throw new ArgumentNullException(nameof(clientFilter));
            this._rateLimiter = rateLimiter ?? throw new ArgumentNullException(nameof(rateLimiter));
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task Handle(NewValidPacketArrivedNotification notification, CancellationToken cancellationToken)
        {
            Boolean result = await ShouldPacketBeFilterd(notification.Packet);
            if(result == false)
            {
                await mediator.Publish(new DHCPv4PacketReadyToProcessNotification(notification.Packet));
            }
        }

        #endregion

        #region Methods
        //DHCPv4PacketReadyToProcessNotification


        public async Task<Boolean> ShouldPacketBeFilterd(DHCPv4Packet packet)
        {
            Boolean rateLimitResult = _rateLimiter.FilterByRateLimit(packet);
            if (rateLimitResult == true)
            {
                return true;
            }

            Boolean isNewTransactionIdExpected = false;
            if(packet.MessageType == DHCPv4MessagesTypes.DHCPDISCOVER ||  packet.MessageType == DHCPv4MessagesTypes.DHCPINFORM ||
                packet.MessageType == DHCPv4MessagesTypes.DHCPRELEASE ||
                (packet.MessageType == DHCPv4MessagesTypes.Request && packet.ClientIPAdress != IPv4Address.Empty) )
            {
                isNewTransactionIdExpected = true;
            }

            if (isNewTransactionIdExpected == false)
            {
                Boolean transactionIdResult = await _transactionFilter.FilterByTranscationId(packet.TransactionId);
                if (transactionIdResult == true)
                {
                    return true;
                }
            }

            Boolean hwAddressResult = await _clientFilter.FilterClientByHardwareAddress(packet.ClientHardwareAddress);
            if (hwAddressResult == true)
            {
                return true;
            }
            DHCPv4PacketOption identifierOption = packet.Options.FirstOrDefault(x => x.OptionType == (Byte)DHCPv4OptionTypes.ClientIdentifier);
            if (identifierOption != null)
            {
                Boolean clientIdentifierResult = await _clientFilter.FilterClientByClientIdentifier(identifierOption.OptionData);
                if (clientIdentifierResult == true)
                {
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}
