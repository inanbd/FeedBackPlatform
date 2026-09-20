using FeedbackPlatform.Application.Common.Interfaces;
using MediatR;

namespace FeedbackPlatform.Application.Features.FeedbackApps.Queries;

public sealed record ListMyFeedbackAppsQuery : IRequest<IReadOnlyList<FeedbackAppDto>>;

public sealed class ListMyFeedbackAppsQueryHandler(
    IFeedbackAppRepository feedbackAppRepository, ICurrentUserService currentUser)
    : IRequestHandler<ListMyFeedbackAppsQuery, IReadOnlyList<FeedbackAppDto>>
{
    public async Task<IReadOnlyList<FeedbackAppDto>> Handle(ListMyFeedbackAppsQuery request, CancellationToken cancellationToken)
    {
        var apps = await feedbackAppRepository.ListByOwnerAsync(currentUser.UserId, cancellationToken);
        return apps
            .Select(a => new FeedbackAppDto(a.Id, a.Name, a.Description, a.CreatedAtUtc, a.RateLimitPerMinuteOverride))
            .ToList();
    }
}
