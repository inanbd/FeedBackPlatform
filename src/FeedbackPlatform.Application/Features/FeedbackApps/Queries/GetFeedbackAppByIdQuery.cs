using FeedbackPlatform.Application.Common;
using FeedbackPlatform.Application.Common.Exceptions;
using FeedbackPlatform.Application.Common.Interfaces;
using MediatR;

namespace FeedbackPlatform.Application.Features.FeedbackApps.Queries;

public sealed record GetFeedbackAppByIdQuery(Guid Id) : IRequest<FeedbackAppDto>;

public sealed class GetFeedbackAppByIdQueryHandler(
    IFeedbackAppRepository feedbackAppRepository, ICurrentUserService currentUser)
    : IRequestHandler<GetFeedbackAppByIdQuery, FeedbackAppDto>
{
    public async Task<FeedbackAppDto> Handle(GetFeedbackAppByIdQuery request, CancellationToken cancellationToken)
    {
        var app = await feedbackAppRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.FeedbackApp), request.Id);

        AuthorizationGuard.EnsureCanAccessApp(app, currentUser.UserId, currentUser.IsAdmin);

        return new FeedbackAppDto(app.Id, app.Name, app.Description, app.CreatedAtUtc, app.RateLimitPerMinuteOverride);
    }
}
