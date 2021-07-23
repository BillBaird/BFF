// // Copyright (c) Duende Software. All rights reserved.
// // See LICENSE in the project root for license information.

using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Api
{
    [Authorize("RequireInteractiveUser")]       // Must have "sub" claim (meaning you are logged in)
    public class UserInfoController : ControllerBase
    {
        private readonly ILogger<UserInfoController> _logger;

        public UserInfoController(ILogger<UserInfoController> logger)
        {
            _logger = logger;
            _logger.LogDebug("UserInfoController created");
        }

        [HttpGet("foo/userinfo")]
        public async Task<IActionResult> GetAsync()
        {
            _logger.LogDebug("GetAsync called");
            // discover IdentityServer endpoints from metadata (this is part of the 
            var client = new HttpClient();
            var authority = "https://identityserver.sweetbridge.com:19101";
            var disco = await client.GetDiscoveryDocumentAsync(authority);
            if (disco.IsError)
            {
                _logger.LogError("disco Error {@Error}", disco.Error);
                // If IdentityServer is not running
                //    Error contains: Error connecting to https://localhost:19101/.well-known/openid-configuration. Connection refused (localhost:19101).
                //    Exception contains:  System.Net.Http.HttpRequestException: Connection refused (localhost:19101)
                return new JsonResult(disco.Error);
            }

            var accessToken = await HttpContext.GetTokenAsync("access_token");
            _logger.LogInformation("Access token: {accessToken}", accessToken);
            
            // request token
            var tokenResponse = await client.GetUserInfoAsync(new UserInfoRequest
            {
                Address = disco.UserInfoEndpoint,
                Token = accessToken
            });

            var count = tokenResponse.Claims.Count();
            _logger.LogInformation("TokenResponse contained {count} claims", count);
            
            var response = new
            {
                claims = tokenResponse.Claims
            };

            return Ok(response);
        }
    }
}