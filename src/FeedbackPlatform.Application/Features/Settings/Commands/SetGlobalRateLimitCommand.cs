using FeedbackPlatform.Application.Common;
using FeedbackPlatform.Application.Common.Interfaces;
using FluentValidation;
using MediatR;

namespace FeedbackPlatform.Application.Features.Settings.Commands;

/// <summary>Admin-only: sets the platform-wide default requests-per-minute limit (see requirement #9).</summary>
public sealed record SetGlobalRateLimitCommand(int RequestsPerMinute) : IRequest;

public sealed class SetGlobalRateLimitCommandValidator : AbstractValidator<SetGlobalRateLimitCommand>
{
    public SetGlobalRateLimitCommandValidator()
    {
        RuleFor(x => x.RequestsPerMinute).InclusiveBetween(1, 100_000);
    }
}

public sealed class SetGlobalRateLimitCommandHandler(
    IRateLimitSettingsProvider rateLimitSettingsProvider, ICurrentUserService currentUser)
    : IRequestHandler<SetGlobalRateLimitCommand>
{
    public Task Handle(SetGlobalRateLimitCommand request, CancellationToken cancellationToken)
    {
        AuthorizationGuard.EnsureIsAdmin(currentUser.IsAdmin);
        return rateLimitSettingsProvider.SetGlobalDefaultAsync(request.RequestsPerMinute, cancellationToken);
    }
}
