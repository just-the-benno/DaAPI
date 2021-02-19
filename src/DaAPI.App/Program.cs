using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using DaAPI.App.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Blazored.Modal;
using Blazored.Toast;
using BlazorStrap;
using DaAPI.App.Helper;

namespace DaAPI.App
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<DaAPI.App.App>("app");
            builder.Services.AddBlazoredModal();
            builder.Services.AddBlazoredToast();

            builder.Services.AddHttpClient<DaAPIService>(client =>
                client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
                 .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

            builder.Services.AddHttpClient<AnnomynousDaAPIService>(client =>
                client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));

            builder.Services.AddSingleton<DHCPv6PacketOptionCodeToNameConverter>();
            builder.Services.AddSingleton<DHCPv6PacketResponseCodeHelper>();

            builder.Services.AddSingleton<DHCPv4PacketOptionCodeToNameConverter>();
            builder.Services.AddSingleton<DHCPv4PacketResponseCodeHelper>();

            builder.Services.AddApiAuthorization((opt) =>
           {
               opt.ProviderOptions.ConfigurationEndpoint = "/Configuration/OidcClientConfig";
           });

            builder.Services.AddSingleton(new LayoutService());
            builder.Services.AddScoped<SignOutService>();
            builder.Services.AddBootstrapCss();

            builder.Services.AddLocalization((opt) =>
           {
               opt.ResourcesPath = "Resources";
           });

            //builder.Services.AddTransient(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            await builder.Build().RunAsync();
        }
    }
}
