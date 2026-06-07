using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BagoScout.Services
{
    public class JwtAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public JwtAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder) : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            string token = null;

            // 1. Try to extract from Authorization Header (Mobile Bearer Token)
            var authHeader = Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                token = authHeader.Substring("Bearer ".Length).Trim();
            }

            // 2. Fallback to Cookie (Web App Cookie-Stored JWT)
            if (string.IsNullOrEmpty(token) && Request.Cookies.TryGetValue("jwt_token", out var cookieToken))
            {
                token = cookieToken;
            }

            if (string.IsNullOrEmpty(token))
            {
                return Task.FromResult(AuthenticateResult.Fail("No authentication token found."));
            }

            try
            {
                if (!JwtHelper.ValidateToken(token, out var claims) || claims == null)
                {
                    return Task.FromResult(AuthenticateResult.Fail("Invalid or expired authentication token."));
                }

                // Create Claims list
                var claimsList = new List<Claim>();

                if (claims.TryGetValue("sub", out var sub))
                {
                    claimsList.Add(new Claim(ClaimTypes.NameIdentifier, sub));
                }
                if (claims.TryGetValue("email", out var email))
                {
                    claimsList.Add(new Claim(ClaimTypes.Email, email));
                }
                if (claims.TryGetValue("userType", out var userType))
                {
                    claimsList.Add(new Claim(ClaimTypes.Role, userType));
                }
                if (claims.TryGetValue("name", out var name))
                {
                    claimsList.Add(new Claim(ClaimTypes.Name, name));
                }

                // Add any other claims directly
                foreach (var claim in claims)
                {
                    if (claim.Key != "sub" && claim.Key != "email" && claim.Key != "userType" && claim.Key != "name")
                    {
                        claimsList.Add(new Claim(claim.Key, claim.Value));
                    }
                }

                // Populate Session dynamically to maintain backward compatibility
                try
                {
                    if (Context.Session != null)
                    {
                        if (claims.TryGetValue("sub", out var subVal) && int.TryParse(subVal, out var userId))
                        {
                            if (!Context.Session.GetInt32("UserId").HasValue)
                            {
                                Context.Session.SetInt32("UserId", userId);
                            }
                        }
                        if (claims.TryGetValue("email", out var emailVal) && string.IsNullOrEmpty(Context.Session.GetString("UserEmail")))
                        {
                            Context.Session.SetString("UserEmail", emailVal);
                        }
                        if (claims.TryGetValue("userType", out var typeVal) && string.IsNullOrEmpty(Context.Session.GetString("UserType")))
                        {
                            Context.Session.SetString("UserType", typeVal);
                        }
                        if (claims.TryGetValue("name", out var nameVal) && string.IsNullOrEmpty(Context.Session.GetString("UserName")))
                        {
                            Context.Session.SetString("UserName", nameVal);
                        }
                    }
                }
                catch (InvalidOperationException)
                {
                    // Session might not be initialized or accessible (e.g. some background API requests)
                }

                var identity = new ClaimsIdentity(claimsList, Scheme.Name);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, Scheme.Name);

                return Task.FromResult(AuthenticateResult.Success(ticket));
            }
            catch (Exception ex)
            {
                return Task.FromResult(AuthenticateResult.Fail($"Authentication failed: {ex.Message}"));
            }
        }

        protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            if (Request.Path.StartsWithSegments("/api"))
            {
                Response.StatusCode = 401;
                Response.ContentType = "application/json";
                await Response.WriteAsync("{\"success\":false,\"message\":\"Unauthorized. Please log in.\"}");
            }
            else
            {
                Response.Redirect("/");
            }
        }
    }
}
