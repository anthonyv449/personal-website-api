using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace personal_website_api.Auth
{
    public class AzureAdTokenValidator : ITokenValidator
    {
        private readonly IConfigurationManager<OpenIdConnectConfiguration> _configManager;
        private readonly string _audience;

        public AzureAdTokenValidator(IConfiguration configuration)
        {
            var tenant = configuration["AzureAd:TenantId"] ?? configuration["AzureAd:Tenant"];
            var policy = configuration["AzureAd:Policy"];
            _audience = configuration["AzureAd:ClientId"] ?? string.Empty;

            if (string.IsNullOrEmpty(tenant) || string.IsNullOrEmpty(policy))
            {
                throw new InvalidOperationException("AzureAd configuration is missing TenantId or Policy");
            }
            var authority = $"https://{tenant}.b2clogin.com/{tenant}.onmicrosoft.com/{policy}/v2.0";
            var documentRetriever = new HttpDocumentRetriever { RequireHttps = true };
            _configManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                authority + "/.well-known/openid-configuration",
                new OpenIdConnectConfigurationRetriever(),
                documentRetriever);
        }

        public async Task<AuthTokenPayload> ValidateAsync(string token)
        {
            var config = await _configManager.GetConfigurationAsync(CancellationToken.None);
            var handler = new JwtSecurityTokenHandler();
            var result = handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidAudience = _audience,
                ValidIssuer = config.Issuer,
                IssuerSigningKeys = config.SigningKeys,
                ValidateLifetime = true,
                ValidateIssuer = true,
                ValidateAudience = true
            }, out var _);

            var name = result.FindFirst(ClaimTypes.Name)?.Value ?? result.Identity?.Name;
            var email = result.FindFirst(ClaimTypes.Email)?.Value ?? result.FindFirst("emails")?.Value;
            return new AuthTokenPayload { Name = name, Email = email };
        }
    }
}
