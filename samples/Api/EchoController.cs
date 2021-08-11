// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Mvc;
using System;
using Microsoft.AspNetCore.Authorization;

namespace Api
{
    [AllowAnonymous]
    public class EchoController : ControllerBase
    {
        [HttpGet("{**catch-all}")]
        public IActionResult Get()
        {
            string message;
            var sub = User.FindFirst(("sub"));
            
            if (!User.Identity.IsAuthenticated)
            {
                message = "Hello, anonymous caller";
            }
            else if (sub != null)
            {
                var userName = User.FindFirst(("name"));
                var client = User.FindFirst("client_id");
                if (userName == null)
                    message = $"Hello sub {sub.Value} in client {client.Value}";
                else
                    message = $"Hello user, {userName.Value} (sub {sub.Value}) in client {client.Value}";
            }
            else
            {
                var client = User.FindFirst("client_id");
                message = $"Hello client, {client.Value}";
            }
            
            var response = new
            {
                path = Request.Path.Value,
                message = message,
                time = DateTime.UtcNow.ToString(),
                headers = Request.Headers
            };

            return Ok(response);
        }
    }
}
