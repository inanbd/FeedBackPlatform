using FeedbackPlatform.Application.Common;
using FeedbackPlatform.Application.Common.Exceptions;
using FeedbackPlatform.Application.Common.Interfaces;
using FeedbackPlatform.Domain.Entities;
using MediatR;

namespace FeedbackPlatform.Application.Features.ApiKeys.Commands;

/// <summary>RawKey is only ever returned here, at creation time — it is never stored or shown again.</summary>
public sealed record GenerateApiKeyResult(Guid Id, string RawKey, string KeyPrefix, DateTimeOffset CreatedAtUtc);

public sealed record GenerateApiKeyCommand(Guid FeedbackAppId) : IRequest<GenerateApiKeyResult>;

public sealed class GenerateApiKeyCommandHandler(
    IFeedbackAppRepository feedbackAppRepository,
    IApiKeyRepository apiKeyRepository,
    IApiKeyGenerator apiKeyGenerator,
    ICurrentUserService currentUser)
    : IRequestHandler<GenerateApiKeyCommand, GenerateApiKeyResult>
{
    public async Task<GenerateApiKeyResult> Handle(GenerateApiKeyCommand request, CancellationToken cancellationToken)
    {
        var app = await feedbackAppRepository.GetByIdAsync(request.FeedbackAppId, cancellationToken)
            ?? throw new NotFoundException(nameof(FeedbackApp), request.FeedbackAppId);

        AuthorizationGuard.EnsureCanAccessApp(app, currentUser.UserId, currentUser.IsAdmin);

        var generated = apiKeyGenerator.Generate();
        var apiKey = new ApiKey
        {
            Id = Guid.NewGuid(),
            FeedbackAppId = app.Id,
            KeyPrefix = generated.KeyPrefix,
            KeyHash = generated.KeyHash,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        await apiKeyRepository.CreateAsync(apiKey, cancellationToken);

        return new GenerateApiKeyResult(apiKey.Id, generated.RawKey, apiKey.KeyPrefix, apiKey.CreatedAtUtc);
    }
}
