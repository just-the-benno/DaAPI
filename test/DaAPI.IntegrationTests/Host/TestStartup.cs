using DaAPI.Host;
using DaAPI.Host.Infrastrucutre;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace DaAPI.IntegrationTests.Host
{
    public class TestStartup : Startup
    {
        public TestStartup(IWebHostEnvironment environment, IConfiguration configuration) : base(environment, configuration)
        {
        }

        protected override void ConfigureAuthentication(IServiceCollection services, AppSettings appSettings)
        {

        }
    }
}
