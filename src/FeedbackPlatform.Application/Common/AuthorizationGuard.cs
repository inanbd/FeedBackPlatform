using FeedbackPlatform.Application.Common.Exceptions;
using FeedbackPlatform.Domain.Entities;

namespace FeedbackPlatform.Application.Common;

internal static class AuthorizationGuard
{
    public static void EnsureCanAccessApp(FeedbackApp app, Guid requestingUserId, bool isAdmin)
    {
        if (!isAdmin && app.OwnerUserId != requestingUserId)
        {
            throw new ForbiddenAccessException();
        }
    }

    public static void EnsureIsAdmin(bool isAdmin)
    {
        if (!isAdmin)
        {
            throw new ForbiddenAccessException("This action is restricted to administrators.");
        }
    }
}
