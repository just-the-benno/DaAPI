using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DaAPI.Host.Infrastrucutre
{
    public class OpenIdConnectOptions
    {
        public String AuthorityUrl { get; set; }
        public Boolean IsSelfHost { get; set; }
    }

    public class AppSettings
    {
        public OpenIdConnectOptions OpenIdConnectOptions { get; set; }
    }
}
