using FeedbackPlatform.Domain.Entities;

namespace FeedbackPlatform.Application.Common.Interfaces;

public interface IFeedbackAppRepository
{
    Task<FeedbackApp?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<FeedbackApp>> ListByOwnerAsync(Guid ownerUserId, CancellationToken ct = default);
    Task<IReadOnlyList<FeedbackApp>> ListAllAsync(CancellationToken ct = default);
    Task CreateAsync(FeedbackApp app, CancellationToken ct = default);
    Task SetRateLimitOverrideAsync(Guid id, int? requestsPerMinute, CancellationToken ct = default);
}
