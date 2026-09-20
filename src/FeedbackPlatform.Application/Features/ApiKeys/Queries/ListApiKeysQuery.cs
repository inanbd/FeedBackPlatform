using FeedbackPlatform.Application.Common;
using FeedbackPlatform.Application.Common.Exceptions;
using FeedbackPlatform.Application.Common.Interfaces;
using FeedbackPlatform.Domain.Entities;
using MediatR;

namespace FeedbackPlatform.Application.Features.ApiKeys.Queries;

public sealed record ApiKeyDto(
    Guid Id, string KeyPrefix, DateTimeOffset CreatedAtUtc, DateTimeOffset? RevokedAtUtc, DateTimeOffset? LastUsedAtUtc)
{
    public bool IsActive => RevokedAtUtc is null;
}

public sealed record ListApiKeysQuery(Guid FeedbackAppId) : IRequest<IReadOnlyList<ApiKeyDto>>;

public sealed class ListApiKeysQueryHandler(
    IApiKeyRepository apiKeyRepository, IFeedbackAppRepository feedbackAppRepository, ICurrentUserService currentUser)
    : IRequestHandler<ListApiKeysQuery, IReadOnlyList<ApiKeyDto>>
{
    public async Task<IReadOnlyList<ApiKeyDto>> Handle(ListApiKeysQuery request, CancellationToken cancellationToken)
    {
        var app = await feedbackAppRepository.GetByIdAsync(request.FeedbackAppId, cancellationToken)
            ?? throw new NotFoundException(nameof(FeedbackApp), request.FeedbackAppId);

        AuthorizationGuard.EnsureCanAccessApp(app, currentUser.UserId, currentUser.IsAdmin);

        var keys = await apiKeyRepository.ListByAppAsync(app.Id, cancellationToken);
        return keys
            .Select(k => new ApiKeyDto(k.Id, k.KeyPrefix, k.CreatedAtUtc, k.RevokedAtUtc, k.LastUsedAtUtc))
            .ToList();
    }
}
