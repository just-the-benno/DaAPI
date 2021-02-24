// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using DaAPI.Host.Infrastrucutre;
using IdentityServer4.Models;
using System.Collections.Generic;

namespace DaAPI.Host
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> Ids =>
            new IdentityResource[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
            };


        public static IEnumerable<ApiScope> Apis =>
            new ApiScope[]
            {
                new ApiScope("daapi", "The API to access DaAPI")
            };


        public static IEnumerable<Client> Clients(OpenIdConnectionConfiguration config) =>
            new Client[]
            {
                // client credentials flow client
                // SPA client using code flow + pkce
                new Client
                {
                    ClientId = "daapi-app",
                    ClientName = "DaAPI SPA Client",

                    AllowedGrantTypes = GrantTypes.Code,
                    RequirePkce = true,
                    RequireClientSecret = false,
                    RequireConsent = false,
                    AllowOfflineAccess = true,

                    RedirectUris =
                    {
                        config.RedirectUri
                    },

                    PostLogoutRedirectUris = { config.PostLogoutRedirectUri },
                    AllowedCorsOrigins = { config.Authority },
                    AllowedScopes = { "openid", "profile", "daapi", "offline-access" }
                }
            };
    }
}