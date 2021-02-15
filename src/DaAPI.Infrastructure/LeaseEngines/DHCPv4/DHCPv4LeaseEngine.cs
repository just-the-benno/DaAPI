using DaAPI.Core.Packets.DHCPv4;
using DaAPI.Core.Scopes.DHCPv4;
using DaAPI.Infrastructure.ServiceBus;
using DaAPI.Infrastructure.StorageEngine.DHCPv4;
using Microsoft.Extensions.Logging;

namespace DaAPI.Infrastructure.LeaseEngines.DHCPv4
{
    public class DHCPv4LeaseEngine : DHCPLeaseEngine<DHCPv4LeaseEngine, DHCPv4Packet, DHCPv4RootScope>, IDHCPv4LeaseEngine
    {
        public DHCPv4LeaseEngine(
                  IDHCPv4EventStore store,
                  DHCPv4RootScope rootScope,
                  IServiceBus serviceBus,
            ILogger<DHCPv4LeaseEngine> logger) : base(rootScope, serviceBus, store, logger)
        {
        }

        protected override DHCPv4Packet GetResponse(DHCPv4Packet input)
        {
            if (input == null || input.IsValid == false)
            {
                Logger.LogError("handling of empty packet is forbidden");
                return DHCPv4Packet.Empty;
            }

            DHCPv4Packet response = DHCPv4Packet.Empty;

            switch (input.MessageType)
            {
                case DHCPv4MessagesTypes.Discover:
                    response = RootScope.HandleDiscover(input);
                    break;
                case DHCPv4MessagesTypes.Request:
                    response = RootScope.HandleRequest(input);
                    break;
                case DHCPv4MessagesTypes.Decline:
                    response = RootScope.HandleDecline(input);
                    break;
                case DHCPv4MessagesTypes.Release:
                    response = RootScope.HandleRelease(input);
                    break;
                case DHCPv4MessagesTypes.Inform:
                    response = RootScope.HandleInform(input);
                    break;
                default:
                    break;
            }

            return response;
        }

    }
}
