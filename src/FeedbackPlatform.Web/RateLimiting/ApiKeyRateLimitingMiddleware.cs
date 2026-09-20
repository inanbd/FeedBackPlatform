using FeedbackPlatform.Application.Common.Interfaces;
using FeedbackPlatform.Web.Auth;
using Microsoft.Extensions.Caching.Memory;

namespace FeedbackPlatform.Web.RateLimiting;

/// <summary>
/// Fixed-window (1 minute) rate limiter keyed per FeedbackApp, with the limit itself
/// resolved per-request from <see cref="IRateLimitSettingsProvider"/> (admin-configurable,
/// see requirement #9). Runs only for requests already authenticated via the API key scheme.
/// </summary>
public sealed class ApiKeyRateLimitingMiddleware(RequestDelegate next)
{
    private sealed class Counter
    {
        public int Value;
    }

    public async Task InvokeAsync(HttpContext context, IRateLimitSettingsProvider rateLimitSettingsProvider, IMemoryCache cache)
    {
        var appIdClaim = context.User.FindFirst(ApiKeyAuthenticationHandler.FeedbackAppIdClaimType)?.Value;
        if (appIdClaim is null || !Guid.TryParse(appIdClaim, out var feedbackAppId))
        {
            await next(context);
            return;
        }

        var limit = await rateLimitSettingsProvider.GetEffectiveLimitAsync(feedbackAppId, context.RequestAborted);

        var window = DateTime.UtcNow.ToString("yyyyMMddHHmm");
        var counterKey = $"apikey-rate-limit:{feedbackAppId}:{window}";

        var counter = cache.GetOrCreate(counterKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
            return new Counter();
        })!;

        var currentCount = Interlocked.Increment(ref counter.Value);

        if (currentCount > limit)
        {
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.Headers.RetryAfter = "60";
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Rate limit exceeded.",
                limitPerMinute = limit
            });
            return;
        }

        await next(context);
    }
}
