using FeedbackPlatform.Application.Common;
using FeedbackPlatform.Application.Common.Exceptions;
using FeedbackPlatform.Application.Common.Interfaces;
using FeedbackPlatform.Domain.Entities;
using MediatR;

namespace FeedbackPlatform.Application.Features.ApiKeys.Commands;

public sealed record RevokeApiKeyCommand(Guid ApiKeyId) : IRequest;

public sealed class RevokeApiKeyCommandHandler(
    IApiKeyRepository apiKeyRepository, IFeedbackAppRepository feedbackAppRepository, ICurrentUserService currentUser)
    : IRequestHandler<RevokeApiKeyCommand>
{
    public async Task Handle(RevokeApiKeyCommand request, CancellationToken cancellationToken)
    {
        var apiKey = await apiKeyRepository.GetByIdAsync(request.ApiKeyId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.ApiKey), request.ApiKeyId);

        var app = await feedbackAppRepository.GetByIdAsync(apiKey.FeedbackAppId, cancellationToken)
            ?? throw new NotFoundException(nameof(FeedbackApp), apiKey.FeedbackAppId);

        AuthorizationGuard.EnsureCanAccessApp(app, currentUser.UserId, currentUser.IsAdmin);

        await apiKeyRepository.RevokeAsync(apiKey.Id, cancellationToken);
    }
}
