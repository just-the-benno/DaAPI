using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace DaAPI.IntegrationTests.Host
{
    public class FakeAuthenticationHandler : AuthenticationHandler<FakeAuthenticationSchemeOptions>
    {
        public FakeAuthenticationHandler(IOptionsMonitor<FakeAuthenticationSchemeOptions> options,
            ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (Options.ShouldBeAuthenticated == false)
            {
                return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(new ClaimsIdentity()), "")));
            }

            List<Claim> claims = new List<Claim> { 
                new Claim(ClaimTypes.Name, "Test user"),
                new Claim("scope","daapi")
            };

            claims.AddRange(Options.Claims);

            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "Test");

            var result = AuthenticateResult.Success(ticket);

            return Task.FromResult(result);
        }
    }
}
