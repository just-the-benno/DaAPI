using Microsoft.AspNetCore.Authentication;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace DaAPI.IntegrationTests.Host
{
    public class FakeAuthenticationSchemeOptions : AuthenticationSchemeOptions
    {
        public Boolean ShouldBeAuthenticated { get; set; } = true;
        public ICollection<Claim> Claims { get; } = new List<Claim>();

        public void AddClaim(String type, String value) =>
            Claims.Add(new Claim(type, value));

        public void AddClaims(ICollection<Claim> claims)
        {
            foreach (var item in claims)
            {
                Claims.Add(item);
            }
        }
    }
}
