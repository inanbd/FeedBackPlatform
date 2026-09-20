namespace FeedbackPlatform.Application.Features.Feedbacks.Queries;

public sealed record FeedbackDto(
    Guid Id,
    Guid FeedbackAppId,
    string AppVersion,
    string Title,
    string Comment,
    int StarRating,
    string? ReporterName,
    string? ReporterContact,
    string IpAddress,
    string? CustomFieldsJson,
    DateTimeOffset CreatedAtUtc);
