using FeedbackPlatform.Application.Common.Models;
using FeedbackPlatform.Domain.Entities;

namespace FeedbackPlatform.Application.Common.Interfaces;

public interface IFeedbackRepository
{
    Task<Guid> CreateAsync(Feedback feedback, CancellationToken ct = default);

    Task<(IReadOnlyList<Feedback> Items, int TotalCount)> SearchAsync(
        FeedbackSearchQuery query, CancellationToken ct = default);

    Task<IReadOnlyList<FeedbackDailyCount>> GetDailyCountsAsync(
        IReadOnlyCollection<Guid>? appIds, DateOnly fromDateUtc, DateOnly toDateUtc, CancellationToken ct = default);

    Task<IReadOnlyList<FeedbackRatingBucket>> GetRatingDistributionAsync(
        IReadOnlyCollection<Guid>? appIds, CancellationToken ct = default);

    Task<int> GetTotalCountAsync(IReadOnlyCollection<Guid>? appIds, CancellationToken ct = default);
}
