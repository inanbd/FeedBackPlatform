namespace FeedbackPlatform.Application.Common.Interfaces;

public interface IRateLimitSettingsProvider
{
    /// <summary>Effective requests-per-minute limit for a given application (its own override, or the global default).</summary>
    Task<int> GetEffectiveLimitAsync(Guid feedbackAppId, CancellationToken ct = default);

    Task<int> GetGlobalDefaultAsync(CancellationToken ct = default);
    Task SetGlobalDefaultAsync(int requestsPerMinute, CancellationToken ct = default);
}
