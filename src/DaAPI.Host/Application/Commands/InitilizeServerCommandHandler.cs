using DaAPI.Core.Common;
using DaAPI.Host.Infrastrucutre;
using DaAPI.Infrastructure.StorageEngine.DHCPv6;
using MediatR;
using Microsoft.AspNetCore.Identity.UI.V4.Pages.Internal.Account;
using Microsoft.Extensions.DependencyInjection;
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

        private readonly IServiceProvider provider;

        public InitilizeServerCommandHandler(
            IServiceProvider provider
            )
        {
            this.provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        public async Task<Boolean> Handle(InitilizeServerCommand request, CancellationToken cancellationToken)
        {
            using (var scope = provider.CreateScope())
            {
                IDHCPv6ReadStore readStore = scope.ServiceProvider.GetRequiredService<IDHCPv6ReadStore>();
                IDHCPv6EventStore eventStore = scope.ServiceProvider.GetRequiredService<IDHCPv6EventStore>();
                OpenIdConnectOptions openIdConnectOptions = scope.ServiceProvider.GetRequiredService<OpenIdConnectOptions>();
                ILocalUserService userService = scope.ServiceProvider.GetRequiredService<ILocalUserService>();
                ILogger<InitilizeServerCommandHandler> logger = scope.ServiceProvider.GetRequiredService<ILogger<InitilizeServerCommandHandler>>();

                var properties = await readStore.GetServerProperties();
                if (properties.IsInitilized == true)
                {
                    logger.LogWarning("server is already intilzied");
                    return false;
                }

                Boolean userCreated = false;
                if (openIdConnectOptions.IsSelfHost == true)
                {
                    if (await userService.GetUserAmount() == 0)
                    {
                        Guid? userId = await userService.CreateUser(request.UserName, request.Password);
                        userCreated = userId.HasValue;
                    }
                }
                else
                {
                    userCreated = true;
                }

                if (userCreated == false)
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
}
