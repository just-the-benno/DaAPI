using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Notifications.Triggers;
using DaAPI.Core.Services;
using DaAPI.Host;
using DaAPI.Infrastructure.ServiceBus;
using DaAPI.Infrastructure.ServiceBus.Messages;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static DaAPI.Infrastructure.NotificationEngine.NotifciationsReadModels.V1;

namespace DaAPI.IntegrationTests.Host.APIControllers
{
    public class NotificationsControllerTester : ControllerTesterBase,
          IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private readonly CustomWebApplicationFactory<Startup> _factory;

        public NotificationsControllerTester(CustomWebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        private StringContent GetContent<T>(T input) => new StringContent(JsonConvert.SerializeObject(input), Encoding.UTF8, "application/json");

        private (HttpClient Client, IServiceBus ServiceBus, Mock<INxOsDeviceConfigurationService> ActorServiceMock) GetTestClient(String dbfileName)
        {

            Mock<INxOsDeviceConfigurationService> actorServiceMock = new Mock<INxOsDeviceConfigurationService>(MockBehavior.Strict);
            IServiceBus serviceBus = null;
            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.UseStartup<TestStartup>();
                builder.ConfigureTestServices(services =>
                {
                    AddFakeAuthentication(services, "Bearer");
                    AddSqliteDatabase(services, dbfileName);
                    ReplaceService(services, actorServiceMock.Object);

                    services.AddSingleton<IServiceBus, MediaRBasedServiceBus>(sp =>
                    {
                        var bus = new MediaRBasedServiceBus(sp.GetService);
                        serviceBus = bus;

                        return bus;
                    }); ;
                });

            }).CreateClient();

            return (client, serviceBus, actorServiceMock);
        }

        [Fact]
        public async Task FullCycle()
        {
            Random random = new Random();

            String sqlLiteDbFileName = $"{random.Next()}.db";

            var serviceInteractions = GetTestClient(sqlLiteDbFileName);

            Guid scopeId = Guid.Parse("7ec8da2e-73a8-4205-9dd8-9bde4be5434a");

            try
            {
                var firstResponse = await serviceInteractions.Client.PostAsync("/api/notifications/pipelines/", GetContent(new
                {
                    name = "my first pipeline",
                    description = "my first pipeline description",
                    triggerName = "PrefixEdgeRouterBindingUpdatedTrigger",
                    condtionName = "DHCPv6ScopeIdNotificationCondition",
                    conditionProperties = new Dictionary<String, String> { { "includesChildren", JsonConvert.SerializeObject(true) }, { "scopeIds", JsonConvert.SerializeObject(new[] { scopeId }) } },
                    actorName = "NxOsStaticRouteUpdaterNotificationActor",
                    actorProperties = new Dictionary<String, String> { { "url", "https://192.168.1.1" }, { "username", "5353535" }, { "password", "36363636" } },
                }));

                Guid firstId = await IsObjectResult<Guid>(firstResponse);
                Assert.NotEqual(Guid.Empty, firstId);

                var secondResponse = await serviceInteractions.Client.PostAsync("/api/notifications/pipelines/", GetContent(new
                {
                    name = "my second pipeline",
                    description = "my second pipeline description",
                    triggerName = "PrefixEdgeRouterBindingUpdatedTrigger",
                    condtionName = "DHCPv6ScopeIdNotificationCondition",
                    conditionProperties = new Dictionary<String, String> { { "includesChildren", JsonConvert.SerializeObject(true) }, { "scopeIds", JsonConvert.SerializeObject(new[] { scopeId }) } },
                    actorName = "NxOsStaticRouteUpdaterNotificationActor",
                    actorProperties = new Dictionary<String, String> { { "url", "https://192.168.1.1" }, { "username", "5353535" }, { "password", "36363636" } },
                }));

                Guid secondId = await IsObjectResult<Guid>(secondResponse);
                Assert.NotEqual(Guid.Empty, secondId);

                var thirdresponse = await serviceInteractions.Client.DeleteAsync($"/api/notifications/pipelines/{firstId}");
                await IsEmptyResult(thirdresponse);

                serviceInteractions.Client.Dispose();
                serviceInteractions = GetTestClient(sqlLiteDbFileName);

                var resultResponse = await serviceInteractions.Client.GetAsync("/api/notifications/pipelines/");
                var result = await IsObjectResult<IEnumerable<NotificationPipelineReadModel>>(resultResponse);
                Assert.Single(result);
                Assert.Equal(secondId, result.First().Id);

                var newPrefix = new PrefixBinding(IPv6Address.FromString("fe80:1000::"), new IPv6SubnetMaskIdentifier(32), IPv6Address.FromString("fe80::2"));

                var actorServiceMock = serviceInteractions.ActorServiceMock;

                Int32 actorFired = 0; ;
                actorServiceMock.Setup(x => x.Connect("https://192.168.1.1", "5353535", "36363636")).ReturnsAsync(true).Verifiable();
                actorServiceMock.Setup(x => x.AddIPv6StaticRoute(newPrefix.Prefix, newPrefix.Mask.Identifier, newPrefix.Host)).ReturnsAsync(true).Callback( () => actorFired++).Verifiable();

                await serviceInteractions.ServiceBus.Publish(new NewTriggerHappendMessage(new[] { PrefixEdgeRouterBindingUpdatedTrigger.WithNewBinding(scopeId, newPrefix) }));

                Int32 triesLeft = 10;
                while (triesLeft-- > 0)
                {
                    await Task.Delay(1000);

                    if (actorFired > 0)
                    {
                        break;
                    }
                }

                actorServiceMock.Verify();
            }
            finally
            {
                File.Delete(sqlLiteDbFileName);
            }
        }
    }
}

