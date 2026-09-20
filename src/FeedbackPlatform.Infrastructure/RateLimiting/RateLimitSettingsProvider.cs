using FeedbackPlatform.Application.Common.Interfaces;
using FeedbackPlatform.Domain.Entities;
using Microsoft.Extensions.Caching.Memory;

namespace FeedbackPlatform.Infrastructure.RateLimiting;

/// <summary>
/// Admin-configurable rate limiting: a global default (persisted in AppSettings) that any
/// application can override per-key via <see cref="FeedbackApp.RateLimitPerMinuteOverride"/>.
/// The global default is cached briefly since it's read on every inbound API request.
/// </summary>
public sealed class RateLimitSettingsProvider(
    IAppSettingsRepository appSettingsRepository,
    IFeedbackAppRepository feedbackAppRepository,
    IMemoryCache cache) : IRateLimitSettingsProvider
{
    private const string CacheKey = "rate-limit:global-default";
    private const int FallbackDefault = 60;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(30);

    public async Task<int> GetEffectiveLimitAsync(Guid feedbackAppId, CancellationToken ct = default)
    {
        var app = await feedbackAppRepository.GetByIdAsync(feedbackAppId, ct);
        if (app?.RateLimitPerMinuteOverride is { } overrideValue)
        {
            return overrideValue;
        }

        return await GetGlobalDefaultAsync(ct);
    }

    public async Task<int> GetGlobalDefaultAsync(CancellationToken ct = default)
    {
        if (cache.TryGetValue(CacheKey, out int cached))
        {
            return cached;
        }

        var stored = await appSettingsRepository.GetAsync(AppSettingKeys.DefaultRateLimitPerMinute, ct);
        var value = int.TryParse(stored, out var parsed) ? parsed : FallbackDefault;

        cache.Set(CacheKey, value, CacheDuration);
        return value;
    }

    public async Task SetGlobalDefaultAsync(int requestsPerMinute, CancellationToken ct = default)
    {
        await appSettingsRepository.SetAsync(
            AppSettingKeys.DefaultRateLimitPerMinute, requestsPerMinute.ToString(), ct);
        cache.Set(CacheKey, requestsPerMinute, CacheDuration);
    }
}
