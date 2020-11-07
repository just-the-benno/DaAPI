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
    public class DHCPv6LeaseEngine : IDHCPv6LeaseEngine
    {
        private readonly IDHCPv6StorageEngine _store;
        private readonly DHCPv6RootScope _rootScope;
        private readonly IDHCPv6ServerPropertiesResolver _serverPropertyResolver;
        private readonly IServiceBus _serviceBus;
        private readonly ILogger<DHCPv6LeaseEngine> _logger;

        public DHCPv6LeaseEngine(
                  IDHCPv6StorageEngine store,
                  DHCPv6RootScope rootScope,
                  IDHCPv6ServerPropertiesResolver serverPropertyResolver,
                  IServiceBus serviceBus,
            ILogger<DHCPv6LeaseEngine> logger)
        {
            this._store = store ?? throw new ArgumentNullException(nameof(store));
            this._rootScope = rootScope ?? throw new ArgumentNullException(nameof(rootScope));
            this._serverPropertyResolver = serverPropertyResolver;
            this._serviceBus = serviceBus;
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));

        }

        public async Task<DHCPv6Packet> HandlePacket(DHCPv6Packet packet)
        {
            if (packet == null || packet.IsValid == false)
            {
                _logger.LogError("handling of empty packet is forbidden");
                return DHCPv6Packet.Empty;
            }

            DHCPv6Packet response = DHCPv6Packet.Empty;
            DHCPv6Packet innerPacket = packet.GetInnerPacket();

            //_logger.LogDebug("waiting for eninge to be ready");

            //await _semaphoreSlim.WaitAsync();
           // _logger.LogDebug("interface engine is ready to process");

            try
            {
                switch (innerPacket.PacketType)
                {
                    case DHCPv6PacketTypes.Solicit:
                        _logger.LogDebug("packet {packet} is a solicit packet. Start handling of Hhndling solicit packet", packet.PacketType);
                        response = _rootScope.HandleSolicit(packet, _serverPropertyResolver);
                        break;
                    case DHCPv6PacketTypes.REQUEST:
                        _logger.LogDebug("packet {packet} is a request packet. Start handling of request packet", packet.PacketType);
                        response = _rootScope.HandleRequest(packet, _serverPropertyResolver);
                        break;
                    case DHCPv6PacketTypes.RENEW:
                        _logger.LogDebug("packet {packet} is a renew packet. Start handling of renew packet", packet.PacketType);
                        response = _rootScope.HandleRenew(packet, _serverPropertyResolver);
                        break;
                    case DHCPv6PacketTypes.REBIND:
                        _logger.LogDebug("packet {packet} is a rebind packet. Start handling of rebind packet", packet.PacketType);

                        response = _rootScope.HandleRebind(packet, _serverPropertyResolver);
                        break;
                    case DHCPv6PacketTypes.RELEASE:
                        _logger.LogDebug("packet {packet} is a release packet. Start handling of rebind packet", packet.PacketType);
                        response = _rootScope.HandleRelease(packet, _serverPropertyResolver);
                        break;

                    case DHCPv6PacketTypes.CONFIRM:
                        _logger.LogDebug("packet {packet} is a confirm packet. Start handling of confirmpacket", packet.PacketType);
                        response = _rootScope.HandleConfirm(packet, _serverPropertyResolver);

                        break;
                    default:
                        break;
                }

                var changes = _rootScope.GetChanges();
                _logger.LogDebug("rootscope has {changeAmount} changes", changes.Count());
                if (changes.Any() == true)
                {
                    await _store.Save(_rootScope);
                    _logger.LogDebug("changes saved");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("unable to handel packet", ex);
            }
            finally
            {
               // _logger.LogDebug("releasing semaphore. Ready for next process");
               // _semaphoreSlim.Release();
            }

            if (response == null)
            {
                _logger.LogDebug("unable to get a response for {packet}", packet.PacketType);
            }

            var triggers = _rootScope.GetTriggers();
            if(triggers.Any() == true)
            {
                _rootScope.ClearTriggers();
                await _serviceBus.Publish(new NewTriggerHappendMessage(triggers));
            }

            return response;
        }
    }
}
