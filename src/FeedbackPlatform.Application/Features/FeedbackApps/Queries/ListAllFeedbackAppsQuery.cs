using FeedbackPlatform.Application.Common;
using FeedbackPlatform.Application.Common.Interfaces;
using MediatR;

namespace FeedbackPlatform.Application.Features.FeedbackApps.Queries;

/// <summary>Admin-only: every application from every user on the platform.</summary>
public sealed record ListAllFeedbackAppsQuery : IRequest<IReadOnlyList<FeedbackAppAdminDto>>;

public sealed class ListAllFeedbackAppsQueryHandler(
    IFeedbackAppRepository feedbackAppRepository, IUserRepository userRepository, ICurrentUserService currentUser)
    : IRequestHandler<ListAllFeedbackAppsQuery, IReadOnlyList<FeedbackAppAdminDto>>
{
    public async Task<IReadOnlyList<FeedbackAppAdminDto>> Handle(ListAllFeedbackAppsQuery request, CancellationToken cancellationToken)
    {
        AuthorizationGuard.EnsureIsAdmin(currentUser.IsAdmin);

        var apps = await feedbackAppRepository.ListAllAsync(cancellationToken);
        var ownerCache = new Dictionary<Guid, (string Email, string DisplayName)>();
        var result = new List<FeedbackAppAdminDto>(apps.Count);

        foreach (var app in apps)
        {
            if (!ownerCache.TryGetValue(app.OwnerUserId, out var owner))
            {
                var user = await userRepository.GetByIdAsync(app.OwnerUserId, cancellationToken);
                owner = (user?.Email ?? "(unknown)", user?.DisplayName ?? "(unknown)");
                ownerCache[app.OwnerUserId] = owner;
            }

            result.Add(new FeedbackAppAdminDto(
                app.Id, app.Name, app.Description, app.CreatedAtUtc, app.RateLimitPerMinuteOverride,
                app.OwnerUserId, owner.Email, owner.DisplayName));
        }

        return result;
    }
}
