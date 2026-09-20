using FeedbackPlatform.Application.Common;
using FeedbackPlatform.Application.Common.Exceptions;
using FeedbackPlatform.Application.Common.Interfaces;
using FeedbackPlatform.Domain.Entities;
using FluentValidation;
using MediatR;

namespace FeedbackPlatform.Application.Features.Settings.Commands;

/// <summary>Admin-only: overrides the rate limit for one application, or clears it (null) to fall back to the global default.</summary>
public sealed record SetAppRateLimitOverrideCommand(Guid FeedbackAppId, int? RequestsPerMinute) : IRequest;

public sealed class SetAppRateLimitOverrideCommandValidator : AbstractValidator<SetAppRateLimitOverrideCommand>
{
    public SetAppRateLimitOverrideCommandValidator()
    {
        RuleFor(x => x.RequestsPerMinute).InclusiveBetween(1, 100_000).When(x => x.RequestsPerMinute is not null);
    }
}

public sealed class SetAppRateLimitOverrideCommandHandler(
    IFeedbackAppRepository feedbackAppRepository, ICurrentUserService currentUser)
    : IRequestHandler<SetAppRateLimitOverrideCommand>
{
    public async Task Handle(SetAppRateLimitOverrideCommand request, CancellationToken cancellationToken)
    {
        AuthorizationGuard.EnsureIsAdmin(currentUser.IsAdmin);

        _ = await feedbackAppRepository.GetByIdAsync(request.FeedbackAppId, cancellationToken)
            ?? throw new NotFoundException(nameof(FeedbackApp), request.FeedbackAppId);

        await feedbackAppRepository.SetRateLimitOverrideAsync(request.FeedbackAppId, request.RequestsPerMinute, cancellationToken);
    }
}
