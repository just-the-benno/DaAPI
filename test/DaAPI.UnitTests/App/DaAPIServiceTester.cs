using Castle.Core.Logging;
using DaAPI.App.Pages;
using DaAPI.App.Pages.DHCPv6Scopes;
using DaAPI.App.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using static DaAPI.Shared.Requests.DHCPv6ScopeRequests.V1;

namespace DaAPI.UnitTests.App
{
    public class DaAPIServiceTester
    {
        private Boolean IsEqualSerializiedContent<T>(HttpRequestMessage message, T otherObject)
        {
            String content = message.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            String otherContent = Newtonsoft.Json.JsonConvert.SerializeObject(otherObject, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
            });

            return content == otherContent;
        }

        [Fact]
        public async Task CreateScope()
        {
            CreateOrUpdateDHCPv6ScopeRequest request = new CreateOrUpdateDHCPv6ScopeRequest
            {
                Name = "name",
                Resolver = new CreateScopeResolverRequest
                {
                    Typename = "test",
                    PropertiesAndValues = new Dictionary<String, String>
                    {
                        { "SomeKey", "\"SomeValue\"" },
                        { "SomeKey2", "null" },
                        { "SomeKey3", "45897" },
                        { "SomeKey4", "123456" },
                        { "SomeKey5", "[\"1\",\"2\"]" },
                    }
                },
                AddressProperties = new DHCPv6ScopeAddressPropertyReqest
                {
                    AcceptDecline = true,
                    AddressAllocationStrategy = DHCPv6ScopeAddressPropertyReqest.AddressAllocationStrategies.Next,
                    End = "fe80::FF",
                    Start = "fe80::1",
                    InformsAreAllowd = true,
                    ExcludedAddresses = new[] { "fe80::5" },
                    RapitCommitEnabled = true,
                    ReuseAddressIfPossible = true,
                    SupportDirectUnicast = true,
                    PreferredLifeTime = TimeSpan.FromMinutes(20),
                    ValidLifeTime = TimeSpan.FromMinutes(40),
                    T1 = 0.5,
                    T2 = 0.75,
                    PrefixDelgationInfo = new DHCPv6PrefixDelgationInfoRequest
                    {
                        AssingedPrefixLength = 42,
                        Prefix = "2a30::",
                        PrefixLength = 38,
                    }
                },
                Properties = new List<DHCPv6ScopePropertyRequest>
                {
                   new DHCPv6NumericScopePropertyRequest
                   {
                       MarkAsRemovedInInheritance = false,
                       NumericType = DaAPI.Core.Scopes.NumericScopePropertiesValueTypes.Byte,
                       OptionCode = 7,
                       Value = 60,
                       Type  = DaAPI.Core.Scopes.DHCPv6.ScopeProperties.DHCPv6ScopePropertyType.Byte
                   }
                }
            };

            String serviceUrl = "http://localhost:7000";
            String expectedUrl = $"{serviceUrl}/api/scopes/dhcpv6/";

            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.Is<HttpRequestMessage>(x =>
                    x.Method == HttpMethod.Post &&
                    x.RequestUri.ToString() == expectedUrl &&
                    IsEqualSerializiedContent(x, request) == true
                    ),
                  ItExpr.IsAny<CancellationToken>()
               )
               .ReturnsAsync(new HttpResponseMessage()
               {
                   StatusCode = HttpStatusCode.OK,
                   Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(true)),
               })
               .Verifiable();


            HttpClient client = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri(serviceUrl),
            };

            DaAPIService service = new DaAPIService(client, Mock.Of<ILogger<DaAPIService>>());
            CreateDHCPv6ScopeViewModel viewModel = new CreateDHCPv6ScopeViewModel
            {
                Name = "name",
                End = "fe80::FF",
                Start = "fe80::1",
                ResolverTypeName = "test",
            };

            viewModel.RootAddressProperties.AcceptDecline = true;
            viewModel.RootAddressProperties.AddressAllocationStrategy = DHCPv6ScopeAddressPropertyReqest.AddressAllocationStrategies.Next;
            viewModel.RootAddressProperties.InformsAreAllowd = true;
            viewModel.RootAddressProperties.RapitCommitEnabled = true;
            viewModel.RootAddressProperties.ReuseAddressIfPossible = true;
            viewModel.RootAddressProperties.SupportDirectUnicast = true;
            viewModel.RootAddressProperties.PreferredLifetime = TimeSpan.FromMinutes(20);
            viewModel.RootAddressProperties.ValidLifetime = TimeSpan.FromMinutes(40);
            viewModel.RootAddressProperties.T1 = 0.5;
            viewModel.RootAddressProperties.T2 = 0.75;
            viewModel.HasPrefixInfo = true;
            viewModel.PrefixDelgationInfo = new DHCPv6PrefixDelgationViewModel
            {
                AssingedPrefixLength = 42,
                Prefix = "2a30::",
                PrefixLength = 38,
            };

            viewModel.AddEmptyExcludedAddress();
            viewModel.ExcludedAddresses[0].Value = "fe80::5";

            viewModel.AddScopeResolverProperty("SomeKey", DaAPI.Core.Scopes.ScopeResolverPropertyDescription.ScopeResolverPropertyValueTypes.String);
            viewModel.ResolverProperties[0].SingleValue = "SomeValue";

            viewModel.AddScopeResolverProperty("SomeKey2", DaAPI.Core.Scopes.ScopeResolverPropertyDescription.ScopeResolverPropertyValueTypes.NullableUInt32);
            viewModel.ResolverProperties[1].NullableNumericValue = null;

            viewModel.AddScopeResolverProperty("SomeKey3", DaAPI.Core.Scopes.ScopeResolverPropertyDescription.ScopeResolverPropertyValueTypes.NullableUInt32);
            viewModel.ResolverProperties[2].NullableNumericValue = 45897;

            viewModel.AddScopeResolverProperty("SomeKey4", DaAPI.Core.Scopes.ScopeResolverPropertyDescription.ScopeResolverPropertyValueTypes.Numeric);
            viewModel.ResolverProperties[3].NumericValue = 123456;

            viewModel.AddScopeResolverProperty("SomeKey5", DaAPI.Core.Scopes.ScopeResolverPropertyDescription.ScopeResolverPropertyValueTypes.IPv4AddressList);
            viewModel.ResolverProperties[4].MultipleValues = new[] { "1", "2" };

            Boolean result = await service.CreateDHCPv6Scope(viewModel.GetRequest());
            Assert.True(result);
        }
    }
}
