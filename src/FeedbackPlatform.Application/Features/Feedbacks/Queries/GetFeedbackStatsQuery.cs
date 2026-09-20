using FeedbackPlatform.Application.Common;
using FeedbackPlatform.Application.Common.Interfaces;
using FeedbackPlatform.Application.Common.Models;
using MediatR;

namespace FeedbackPlatform.Application.Features.Feedbacks.Queries;

public sealed record FeedbackStatsDto(
    IReadOnlyList<FeedbackDailyCount> DailyCounts,
    IReadOnlyList<FeedbackRatingBucket> RatingDistribution,
    int TotalCount);

public sealed record GetFeedbackStatsQuery(Guid? AppIdFilter, DateOnly FromDateUtc, DateOnly ToDateUtc)
    : IRequest<FeedbackStatsDto>;

public sealed class GetFeedbackStatsQueryHandler(
    IFeedbackRepository feedbackRepository, FeedbackScopeResolver scopeResolver, ICurrentUserService currentUser)
    : IRequestHandler<GetFeedbackStatsQuery, FeedbackStatsDto>
{
    public async Task<FeedbackStatsDto> Handle(GetFeedbackStatsQuery request, CancellationToken cancellationToken)
    {
        var scope = await scopeResolver.ResolveAsync(
            request.AppIdFilter, currentUser.UserId, currentUser.IsAdmin, cancellationToken);

        var dailyCountsTask = feedbackRepository.GetDailyCountsAsync(
            scope, request.FromDateUtc, request.ToDateUtc, cancellationToken);
        var ratingDistributionTask = feedbackRepository.GetRatingDistributionAsync(scope, cancellationToken);
        var totalCountTask = feedbackRepository.GetTotalCountAsync(scope, cancellationToken);

        await Task.WhenAll(dailyCountsTask, ratingDistributionTask, totalCountTask);

        return new FeedbackStatsDto(await dailyCountsTask, await ratingDistributionTask, await totalCountTask);
    }
}
