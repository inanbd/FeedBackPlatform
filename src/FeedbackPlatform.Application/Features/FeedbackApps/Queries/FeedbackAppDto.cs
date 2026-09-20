namespace FeedbackPlatform.Application.Features.FeedbackApps.Queries;

public sealed record FeedbackAppDto(
    Guid Id, string Name, string? Description, DateTimeOffset CreatedAtUtc, int? RateLimitPerMinuteOverride);

public sealed record FeedbackAppAdminDto(
    Guid Id, string Name, string? Description, DateTimeOffset CreatedAtUtc, int? RateLimitPerMinuteOverride,
    Guid OwnerUserId, string OwnerEmail, string OwnerDisplayName);
