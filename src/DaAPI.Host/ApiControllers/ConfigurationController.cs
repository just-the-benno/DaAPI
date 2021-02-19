using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using DaAPI.Host.Application.Commands;
using DaAPI.Host.Infrastrucutre;
using DaAPI.Infrastructure.StorageEngine.DHCPv6;
using DaAPI.Shared.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static DaAPI.Shared.Responses.ServerControllerResponses;
using static DaAPI.Shared.Responses.ServerControllerResponses.V1;

namespace DaAPI.Host.ApiControllers
{
    public class OpenIdConnectionConfigurationJsonConverter : JsonConverter<OpenIdConnectionConfiguration>
    {
        public override OpenIdConnectionConfiguration Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            throw new NotImplementedException();

        public override void Write(Utf8JsonWriter writer, OpenIdConnectionConfiguration value, JsonSerializerOptions options)
        {
            var uniqueScopes = new HashSet<String>(value.DefaultScopes.Union(value.Scopes));

            String scopes = String.Empty;
            foreach (var item in uniqueScopes)
            {
                scopes += $"{item} ";
            }

            var realObject = new
            {
                authority = value.Authority,
                metadataUrl = value.MetadataUrl,
                client_id = value.ClientId,
                redirect_uri = value.RedirectUri,
                post_logout_redirect_uri = value.PostLogoutRedirectUri,
                response_type = value.ResponseType,
                response_mode = value.ResponseMode,
                scope = scopes.Substring(0,scopes.Length-1),
            };

            writer.WriteStartObject();
            foreach (var item in realObject.GetType().GetProperties())
            {
                String propertyName = item.Name;
                String propertyValue = item.GetValue(realObject) as String;
                if(String.IsNullOrEmpty(propertyValue) == true)
                {
                    continue;
                }

                writer.WriteString(propertyName, propertyValue);
            }

            writer.WriteEndObject();
        }
    }

    [Authorize(AuthenticationSchemes = "Bearer", Policy = "DaAPI-API")]
    [ApiController]
    public class ConfigurationController : ControllerBase
    {
        private readonly OpenIdConnectionConfiguration _openIdConnectOptions;

        public ConfigurationController(OpenIdConnectionConfiguration openIdConnectOptions)
           
        {
            _openIdConnectOptions = openIdConnectOptions ?? throw new ArgumentNullException(nameof(openIdConnectOptions));
        }

        [AllowAnonymous]
        [HttpGet("/Configuration/OidcClientConfig")]
        public IActionResult CheckIfServerIsInitialized()
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };

            options.Converters.Add(new OpenIdConnectionConfigurationJsonConverter());

            return base.Ok(JsonSerializer.Serialize(_openIdConnectOptions, options));
        }
    }
}
