using FeedbackPlatform.Domain.Enums;

namespace FeedbackPlatform.Application.Common.Interfaces;

public interface ICurrentUserService
{
    Guid UserId { get; }
    UserRole Role { get; }
    bool IsAdmin => Role == UserRole.Admin;
}
