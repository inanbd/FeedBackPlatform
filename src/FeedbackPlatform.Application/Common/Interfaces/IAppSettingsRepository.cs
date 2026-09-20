namespace FeedbackPlatform.Application.Common.Interfaces;

public interface IAppSettingsRepository
{
    Task<string?> GetAsync(string key, CancellationToken ct = default);
    Task SetAsync(string key, string value, CancellationToken ct = default);
}
