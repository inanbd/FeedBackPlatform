using FeedbackPlatform.Domain.Entities;

namespace FeedbackPlatform.Application.Common.Interfaces;

public interface IApiKeyRepository
{
    Task<ApiKey?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ApiKey?> GetByHashAsync(string keyHash, CancellationToken ct = default);
    Task<IReadOnlyList<ApiKey>> ListByAppAsync(Guid feedbackAppId, CancellationToken ct = default);
    Task CreateAsync(ApiKey apiKey, CancellationToken ct = default);
    Task RevokeAsync(Guid id, CancellationToken ct = default);
    Task UpdateLastUsedAsync(Guid id, DateTimeOffset whenUtc, CancellationToken ct = default);
}
