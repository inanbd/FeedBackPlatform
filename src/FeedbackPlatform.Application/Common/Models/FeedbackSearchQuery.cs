namespace FeedbackPlatform.Application.Common.Models;

/// <summary>
/// Describes a feedback listing request. <see cref="AppIds"/> null means "no restriction"
/// (used by admins browsing every application); a non-null collection scopes the search
/// to those applications (used for a regular user's own apps, or a single-app filter).
/// </summary>
public sealed record FeedbackSearchQuery(
    IReadOnlyCollection<Guid>? AppIds,
    int? MinStarRating,
    int Page,
    int PageSize);
