using FeedbackPlatform.Application.Common.Exceptions;
using FeedbackPlatform.Application.Common.Interfaces;
using FeedbackPlatform.Domain.Entities;

namespace FeedbackPlatform.Application.Common;

/// <summary>
/// Resolves which application ids a feedback listing/stats query is allowed to see:
/// a single app (if the caller may access it), the caller's own apps, or every app (admin, unfiltered).
/// </summary>
public sealed class FeedbackScopeResolver(IFeedbackAppRepository feedbackAppRepository)
{
    public async Task<IReadOnlyCollection<Guid>?> ResolveAsync(
        Guid? appIdFilter, Guid requestingUserId, bool isAdmin, CancellationToken ct)
    {
        if (appIdFilter is { } appId)
        {
            var app = await feedbackAppRepository.GetByIdAsync(appId, ct)
                ?? throw new NotFoundException(nameof(FeedbackApp), appId);

            AuthorizationGuard.EnsureCanAccessApp(app, requestingUserId, isAdmin);
            return [appId];
        }

        if (isAdmin)
        {
            return null;
        }

        var ownedApps = await feedbackAppRepository.ListByOwnerAsync(requestingUserId, ct);
        return ownedApps.Select(a => a.Id).ToList();
    }
}
