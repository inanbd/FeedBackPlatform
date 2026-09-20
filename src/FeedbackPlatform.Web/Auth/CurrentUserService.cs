using System.Security.Claims;
using FeedbackPlatform.Application.Common.Interfaces;
using FeedbackPlatform.Domain.Enums;

namespace FeedbackPlatform.Web.Auth;

public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public Guid UserId =>
        Guid.TryParse(httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
            ? id
            : Guid.Empty;

    public UserRole Role =>
        Enum.TryParse<UserRole>(httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Role), out var role)
            ? role
            : UserRole.User;
}
