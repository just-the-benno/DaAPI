using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.Notifications;
using DaAPI.Core.Notifications.Actors;
using DaAPI.Core.Notifications.Triggers;
using DaAPI.Core.Services;
using DaAPI.TestHelper;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DaAPI.UnitTests.Core.Notifications.Actors
{
    public class NxOsStaticRouteUpdaterNotificationActorTester
    {
        [Fact]
        public void ApplyValues_Cycle()
        {
            Random random = new Random();

            String uri = "https://" + random.GetIPv4Address().ToString() + ":8081";
            String username = random.GetAlphanumericString();
            String password = random.GetAlphanumericString();

            var actor = new NxOsStaticRouteUpdaterNotificationActor(
                Mock.Of<INxOsDeviceConfigurationService>(MockBehavior.Strict),
                Mock.Of<ILogger<NxOsStaticRouteUpdaterNotificationActor>>());

            Dictionary<string, string> propertiesAndValues = new Dictionary<String, String>
            {
                { "Url", "\"" + uri+ "\"" },
                { "Password","\"" + password + "\""},
                { "Username","\"" + username + "\"" },
            };
            Boolean applyResult = actor.ApplyValues(propertiesAndValues);

            Assert.True(applyResult);

            Assert.Equal(uri, actor.Url);
            Assert.Equal(username, actor.Username);
            Assert.Equal(password, actor.Password);

            var createModel = actor.ToCreateModel();

            Assert.Equal("NxOsStaticRouteUpdaterNotificationActor", createModel.Typename);
            Assert.Equal(propertiesAndValues, createModel.PropertiesAndValues, new NonStrictDictionaryComparer<String, String>());
        }

        [Fact]
        public void ApplyValues_Failed_InvalidUris()
        {
            Random random = new Random();

            String username = random.GetAlphanumericString();
            String password = random.GetAlphanumericString();

            String[] wrongUris = new[]
            {
                "10.10.10.10",
                "10.10.10.10:80",
                "rtp://10.10.10.10:80",
            };

            foreach (var item in wrongUris)
            {
                var actor = new NxOsStaticRouteUpdaterNotificationActor(
            Mock.Of<INxOsDeviceConfigurationService>(MockBehavior.Strict),
            Mock.Of<ILogger<NxOsStaticRouteUpdaterNotificationActor>>());

                Dictionary<string, string> propertiesAndValues = new Dictionary<String, String>
            {
                { "Url", "\"" + item+ "\"" },
                { "Password2","\"" + password + "\""},
                { "Username","\"" + username + "\"" },
            };
                Boolean applyResult = actor.ApplyValues(propertiesAndValues);

                Assert.False(applyResult);
            }
        }

        [Fact]
        public void ApplyValues_Failed()
        {
            Random random = new Random();

            String address = random.GetAlphanumericString();
            String username = random.GetAlphanumericString();
            String password = random.GetAlphanumericString();

            var actor = new NxOsStaticRouteUpdaterNotificationActor(
                Mock.Of<INxOsDeviceConfigurationService>(MockBehavior.Strict),
                Mock.Of<ILogger<NxOsStaticRouteUpdaterNotificationActor>>());

            Dictionary<string, string> propertiesAndValues = new Dictionary<String, String>
            {
                { "IPAddress", "\"" + address+ "\"" },
                { "Password2","\"" + password + "\""},
                { "Username","\"" + username + "\"" },
            };
            Boolean applyResult = actor.ApplyValues(propertiesAndValues);

            Assert.False(applyResult);
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public async Task Handle(Boolean oldPrefixHasValue, Boolean newPrefixHasValue)
        {
            Random random = new Random();

            String address = "https://" + random.GetIPv4Address().ToString() + ":8081";
            String username = random.GetAlphanumericString();
            String password = random.GetAlphanumericString();

            PrefixBinding oldPrefix = new PrefixBinding(IPv6Address.FromString("2cff:34::0"), new IPv6SubnetMask(new IPv6SubnetMaskIdentifier(38)), IPv6Address.FromString("fe80::2"));
            PrefixBinding newPrefix = new PrefixBinding(IPv6Address.FromString("1cff:50::0"), new IPv6SubnetMask(new IPv6SubnetMaskIdentifier(38)), IPv6Address.FromString("fe80::CC"));
            Guid scopeId = random.NextGuid();

            PrefixEdgeRouterBindingUpdatedTrigger trigger;
            if (newPrefixHasValue == true && oldPrefixHasValue == true)
            {
                trigger = PrefixEdgeRouterBindingUpdatedTrigger.WithOldAndNewBinding(scopeId, oldPrefix, newPrefix);
            }
            else if (newPrefixHasValue == true && oldPrefixHasValue == false)
            {
                trigger = PrefixEdgeRouterBindingUpdatedTrigger.WithNewBinding(scopeId, newPrefix);
            }
            else if (newPrefixHasValue == false && oldPrefixHasValue == true)
            {
                trigger = PrefixEdgeRouterBindingUpdatedTrigger.WithOldBinding(scopeId, oldPrefix);
            }
            else
            {
                trigger = PrefixEdgeRouterBindingUpdatedTrigger.NoChanges(scopeId);
            }

            Mock<INxOsDeviceConfigurationService> deviceServiceMock = new Mock<INxOsDeviceConfigurationService>(MockBehavior.Strict);
            deviceServiceMock.Setup(x => x.Connect(address, username, password)).ReturnsAsync(true).Verifiable();

            if (oldPrefixHasValue == true)
            {
                deviceServiceMock.Setup(x => x.RemoveIPv6StaticRoute(oldPrefix.Prefix, oldPrefix.Mask.Identifier, oldPrefix.Host)).ReturnsAsync(true).Verifiable();
            }

            if (newPrefixHasValue == true)
            {
                deviceServiceMock.Setup(x => x.AddIPv6StaticRoute(newPrefix.Prefix, newPrefix.Mask.Identifier, newPrefix.Host)).ReturnsAsync(true).Verifiable();

            }
            var actor = new NxOsStaticRouteUpdaterNotificationActor(deviceServiceMock.Object,
    Mock.Of<ILogger<NxOsStaticRouteUpdaterNotificationActor>>());

            actor.ApplyValues(new Dictionary<String, String>
            {
                { "Url", "\"" + address+ "\"" },
                { "Password","\"" + password + "\""},
                { "Username","\"" + username + "\"" },
            });

            var pipeline = NotificationPipelineTester.CreatePipleline(random, trigger.GetTypeIdentifier(), NotificationCondition.True, actor);

            var pipelineResult = await pipeline.Execute(trigger);

            Assert.Equal(NotifactionPipelineExecutionResults.Success, pipelineResult);

            deviceServiceMock.Verify();
        }
    }
}
