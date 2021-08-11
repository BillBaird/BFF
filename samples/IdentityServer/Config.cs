// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.


using Duende.IdentityServer.Models;
using System.Collections.Generic;

namespace IdentityServerHost
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> IdentityResources =>
            new IdentityResource[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
            };

        public static IEnumerable<ApiScope> ApiScopes =>
            new ApiScope[]
            {
                new ApiScope("api", new[] { "name" }),
            };

        public static IEnumerable<Client> Clients =>
            new Client[]
            {
                new Client
                {
                    ClientId = "spa",
                    ClientSecrets = { new Secret("secret".Sha256()) },
                    
                    AllowedGrantTypes = GrantTypes.CodeAndClientCredentials,

                    RedirectUris = { "https://wlbaird.synology.me:5008/signin-oidc" },
                    
                    FrontChannelLogoutUri = "https://wlbaird.synology.me:5008/signout-oidc",
                    BackChannelLogoutUri = "https://wlbaird.synology.me:5008/bff/backchannel",
                    
                    PostLogoutRedirectUris = { "https://wlbaird.synology.me:5008/signout-callback-oidc" },

                    AllowOfflineAccess = true,
                    AllowedScopes = { "openid", "profile", "api" }
                },
            };
    }
}