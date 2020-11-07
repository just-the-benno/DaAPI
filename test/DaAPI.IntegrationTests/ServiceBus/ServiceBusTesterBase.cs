using DaAPI.Host;
using DaAPI.Infrastructure.ServiceBus;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace DaAPI.IntegrationTests.ServiceBus
{
    public abstract class ServiceBusTesterBase : WebApplicationFactoryBase, IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> _factory;

        protected ServiceBusTesterBase(WebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        protected (HttpClient client, IServiceBus serviceBus) GetTestClient(String dbfileName)
        {
            IServiceBus serviceBus = null;
            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    AddSqliteDatabase(services, dbfileName);

                    services.AddSingleton<IServiceBus, MediaRBasedServiceBus>(sp =>
                    {
                        var bus = new MediaRBasedServiceBus(sp.GetService);
                        serviceBus = bus;

                        return bus;
                    }); ;
                });

            }).CreateClient();

            return (client, serviceBus);
        }
    }
}
