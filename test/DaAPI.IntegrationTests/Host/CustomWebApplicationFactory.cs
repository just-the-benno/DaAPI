using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DaAPI.Host;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;

namespace DaAPI.IntegrationTests.Host
{
    public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup>
        where TStartup : class
    {
        protected override IHostBuilder CreateHostBuilder()
        {
            return Program.CreateHostBuilder(new[] { "--use-startup=false" });
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {

        }
    }
}

