using System.Security.Claims;
using System.Text.Encodings.Web;
using FeedbackPlatform.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FeedbackPlatform.Web.Auth;

/// <summary>
/// Authenticates external applications calling the feedback submission API via the
/// "X-Api-Key" header. On success the resolved FeedbackApp id is exposed as a claim.
/// </summary>
public sealed class ApiKeyAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IApiKeyRepository apiKeyRepository,
    IApiKeyGenerator apiKeyGenerator)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "ApiKey";
    public const string HeaderName = "X-Api-Key";
    public const string FeedbackAppIdClaimType = "feedback_app_id";

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(HeaderName, out var headerValues))
        {
            return AuthenticateResult.Fail($"Missing '{HeaderName}' header.");
        }

        var rawKey = headerValues.ToString();
        if (string.IsNullOrWhiteSpace(rawKey))
        {
            return AuthenticateResult.Fail($"Missing '{HeaderName}' header.");
        }

        var hash = apiKeyGenerator.Hash(rawKey);
        var apiKey = await apiKeyRepository.GetByHashAsync(hash);
        if (apiKey is null || !apiKey.IsActive)
        {
            return AuthenticateResult.Fail("Invalid or revoked API key.");
        }

        await apiKeyRepository.UpdateLastUsedAsync(apiKey.Id, DateTimeOffset.UtcNow);

        var claims = new[]
        {
            new Claim(FeedbackAppIdClaimType, apiKey.FeedbackAppId.ToString()),
            new Claim(ClaimTypes.NameIdentifier, apiKey.Id.ToString())
        };
        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return AuthenticateResult.Success(ticket);
    }
}
