using DaAPI.Core.Packets.DHCPv6;
using DaAPI.Core.Scopes.DHCPv6;
using DaAPI.Core.Services;
using DaAPI.Infrastructure.ServiceBus;
using DaAPI.Infrastructure.ServiceBus.Messages;
using DaAPI.Infrastructure.StorageEngine.DHCPv6;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DaAPI.Infrastructure.LeaseEngines.DHCPv6
{
    public class DHCPv6LeaseEngine : DHCPLeaseEngine<DHCPv6LeaseEngine, DHCPv6Packet, DHCPv6RootScope>, IDHCPv6LeaseEngine
    {
        private readonly IDHCPv6ServerPropertiesResolver _serverPropertyResolver;

        public DHCPv6LeaseEngine(
                  IDHCPv6EventStore store,
                  DHCPv6RootScope rootScope,
                  IDHCPv6ServerPropertiesResolver serverPropertyResolver,
                  IServiceBus serviceBus,
            ILogger<DHCPv6LeaseEngine> logger) : base(rootScope, serviceBus, store, logger)
        {
            this._serverPropertyResolver = serverPropertyResolver;
        }

        protected override DHCPv6Packet GetResponse(DHCPv6Packet input)
        {
            if (input == null || input.IsValid == false)
            {
                Logger.LogError("handling of empty packet is forbidden");
                return DHCPv6Packet.Empty;
            }

            DHCPv6Packet response = DHCPv6Packet.Empty;
            DHCPv6Packet innerPacket = input.GetInnerPacket();

            switch (innerPacket.PacketType)
            {
                case DHCPv6PacketTypes.Solicit:
                    Logger.LogDebug("packet {packet} is a solicit packet. Start handling of Hhndling solicit packet", input.PacketType);
                    response = RootScope.HandleSolicit(input, _serverPropertyResolver);
                    break;
                case DHCPv6PacketTypes.REQUEST:
                    Logger.LogDebug("packet {packet} is a request packet. Start handling of request packet", input.PacketType);
                    response = RootScope.HandleRequest(input, _serverPropertyResolver);
                    break;
                case DHCPv6PacketTypes.RENEW:
                    Logger.LogDebug("packet {packet} is a renew packet. Start handling of renew packet", input.PacketType);
                    response = RootScope.HandleRenew(input, _serverPropertyResolver);
                    break;
                case DHCPv6PacketTypes.REBIND:
                    Logger.LogDebug("packet {packet} is a rebind packet. Start handling of rebind packet", input.PacketType);

                    response = RootScope.HandleRebind(input, _serverPropertyResolver);
                    break;
                case DHCPv6PacketTypes.RELEASE:
                    Logger.LogDebug("packet {packet} is a release packet. Start handling of rebind packet", input.PacketType);
                    response = RootScope.HandleRelease(input, _serverPropertyResolver);
                    break;

                case DHCPv6PacketTypes.CONFIRM:
                    Logger.LogDebug("packet {packet} is a confirm packet. Start handling of confirmpacket", input.PacketType);
                    response = RootScope.HandleConfirm(input, _serverPropertyResolver);

                    break;
                default:
                    break;
            }

            return response;

        }
    }
}
