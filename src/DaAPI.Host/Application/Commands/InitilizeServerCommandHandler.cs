using DaAPI.Core.Common;
using DaAPI.Host.Infrastrucutre;
using DaAPI.Infrastructure.StorageEngine.DHCPv6;
using MediatR;
using Microsoft.AspNetCore.Identity.UI.V4.Pages.Internal.Account;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DaAPI.Host.Application.Commands
{
    public class InitilizeServerCommandHandler : IRequestHandler<InitilizeServerCommand, Boolean>
    {
        private readonly IDHCPv6ReadStore readStore;
        private readonly IDHCPv6EventStore eventStore;
        private readonly OpenIdConnectOptions openIdConnectOptions;
        private readonly ILocalUserService userService;
        private readonly ILogger<InitilizeServerCommandHandler> logger;

        public InitilizeServerCommandHandler(
            IDHCPv6ReadStore readStore, 
            IDHCPv6EventStore eventStore,
            OpenIdConnectOptions openIdConnectOptions,
            ILocalUserService userService,
            ILogger<InitilizeServerCommandHandler> logger)
        {
            this.readStore = readStore ?? throw new ArgumentNullException(nameof(readStore));
            this.eventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));
            this.openIdConnectOptions = openIdConnectOptions;
            this.userService = userService;
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Boolean> Handle(InitilizeServerCommand request, CancellationToken cancellationToken)
        {
            var properties = await readStore.GetServerProperties();
            if(properties.IsInitilized == true)
            {
                logger.LogWarning("server is already intilzied");
                return false;
            }

            Boolean userCreated = false;
            if(openIdConnectOptions.IsSelfHost == true)
            {
                if(await userService.GetUserAmount() == 0)
                {
                    Guid? userId = await userService.CreateUser(request.UserName, request.Password);
                    userCreated = userId.HasValue;
                }
            }
            else
            {
                userCreated = true;
            }

            if(userCreated == false)
            {
                return false;
            }

            properties.IsInitilized = true;
            properties.ServerDuid = new UUIDDUID(Guid.NewGuid());

            Boolean saveResult = await eventStore.SaveInitialServerConfiguration(properties);
            return saveResult;
        }
    }
}
