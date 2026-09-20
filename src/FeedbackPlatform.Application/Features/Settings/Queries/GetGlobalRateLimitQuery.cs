using FeedbackPlatform.Application.Common;
using FeedbackPlatform.Application.Common.Interfaces;
using MediatR;

namespace FeedbackPlatform.Application.Features.Settings.Queries;

/// <summary>Admin-only: the platform-wide default requests-per-minute limit applied to API keys.</summary>
public sealed record GetGlobalRateLimitQuery : IRequest<int>;

public sealed class GetGlobalRateLimitQueryHandler(
    IRateLimitSettingsProvider rateLimitSettingsProvider, ICurrentUserService currentUser)
    : IRequestHandler<GetGlobalRateLimitQuery, int>
{
    public Task<int> Handle(GetGlobalRateLimitQuery request, CancellationToken cancellationToken)
    {
        AuthorizationGuard.EnsureIsAdmin(currentUser.IsAdmin);
        return rateLimitSettingsProvider.GetGlobalDefaultAsync(cancellationToken);
    }
}
