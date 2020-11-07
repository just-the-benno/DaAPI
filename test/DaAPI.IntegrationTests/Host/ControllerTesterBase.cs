using DaAPI.Infrastructure.StorageEngine;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DaAPI.IntegrationTests.Host
{
    public abstract class ControllerTesterBase : WebApplicationFactoryBase
    {
        protected static async Task IsEmptyResult(HttpResponseMessage responseMessage)
        {
            Assert.True(responseMessage.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.NoContent, responseMessage.StatusCode);

            String content = await responseMessage.Content.ReadAsStringAsync();
            Assert.True(String.IsNullOrEmpty(content));
        }

        protected static async Task<T> IsObjectResult<T>(HttpResponseMessage responseMessage)
        {
            Assert.True(responseMessage.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.OK, responseMessage.StatusCode);

            String rawContent = await responseMessage.Content.ReadAsStringAsync();
            T result = JsonConvert.DeserializeObject<T>(rawContent);

            return result;
        }

        protected static async Task<T> IsObjectResult<T>(HttpResponseMessage responseMessage, T expected)
        {
            T result = await IsObjectResult<T>(responseMessage);

            Assert.Equal(expected, result);
            return result;
        }

        protected static void AddFakeAuthentication(IServiceCollection services, String authenticaionShema)
        {
            services.AddAuthentication(authenticaionShema)
                .AddScheme<FakeAuthenticationSchemeOptions, FakeAuthenticationHandler>(
                    authenticaionShema, options =>
                    {
                        options.ShouldBeAuthenticated = true;
                    });
        }
    }
}
