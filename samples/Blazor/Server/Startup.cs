// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using Duende.Bff;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Http;
using Serilog;

namespace Blazor.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddBff()
                .AddServerSideSessions();
            
            services.AddControllers();
            services.AddRazorPages();
            
            services.AddAuthentication(options =>
                {
                    options.DefaultScheme = "cookie";
                    options.DefaultChallengeScheme = "oidc";
                    options.DefaultSignOutScheme = "oidc";
                })
                .AddCookie("cookie", options =>
                {
                    options.Cookie.Name = "__Host-blazor";
                    options.Cookie.SameSite = SameSiteMode.Strict;
                })
                .AddOpenIdConnect("oidc", options =>
                {
                    options.Authority = "https://identityserver.sweetbridge.com:19101";
                    
                    // confidential client using code flow + PKCE
                    options.ClientId = "duende.bff.fork.blazor.server";
                    options.ClientSecret = "bffSecret";
                    options.ResponseType = "code";
                    options.ResponseMode = "query";

                    options.MapInboundClaims = false;
                    options.GetClaimsFromUserInfoEndpoint = true;
                    options.SaveTokens = true;

                    // request scopes + refresh tokens
                    options.Scope.Clear();
                    options.Scope.Add("openid");
                    options.Scope.Add("profile");
                    options.Scope.Add("email");     // Scopes need to be configured here as well as in IdentityServer ClientScopes.
                                                        // If here, but not in ClientScopes, an "invalid_scope" error is returned.
                                                        // The IdentityServer log will indicate that the client is not allowed to access the scope.
                                                        
                    options.Scope.Add("api");      // Need this to call either API (Fetch data - EchoController or UserInfo - UserInfoController)
                    options.Scope.Add("kyc");
                    options.ClaimActions.MapUniqueJsonKey("kyc_level (remapped as an example)", "kyc_status");      // Needed to see the kys_status claim
                                                        // Note the the first parameter can be any name you want, such as kycLevel, key_status, etc.
                                                        // If you don't map a value, it will not show up.
                    options.Scope.Add("doc_server");
                    options.ClaimActions.MapUniqueJsonKey("doc_role", "doc_role");      // Needed to see the kys_status claim
                                                        
                    options.Scope.Add("offline_access");
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseSerilogRequestLogging();
            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseHttpsRedirection();
            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();
            app.UseBff();
            app.UseAuthorization();
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBffManagementEndpoints();
                
                endpoints.MapRemoteBffApiEndpoint("/api", "https://localhost:5010")
                    .RequireAccessToken(TokenType.UserOrClient);
                
                endpoints.MapRazorPages();
                
                endpoints.MapControllers()
                    .RequireAuthorization()
                    .AsBffApiEndpoint();
                
                endpoints.MapFallbackToFile("index.html");
            });
        }
    }
}
