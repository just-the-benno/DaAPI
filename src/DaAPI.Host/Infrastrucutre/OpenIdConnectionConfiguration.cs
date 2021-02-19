using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DaAPI.Host.Infrastrucutre
{
    public class OpenIdConnectionConfiguration : OidcProviderOptions
    {
        public IList<String> Scopes { get; set; }
    }
}
