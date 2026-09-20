using FeedbackPlatform.Application.Common.Models;
using FeedbackPlatform.Application.Features.Feedbacks.Queries;

namespace FeedbackPlatform.Web.Models.Feedback;

public sealed record FeedbackAppOption(Guid Id, string Name);

public sealed class FeedbackIndexViewModel
{
    public required PagedResult<FeedbackDto> Feedback { get; init; }
    public required IReadOnlyList<FeedbackAppOption> AppOptions { get; init; }
    public Guid? AppIdFilter { get; init; }
    public int? MinStarRating { get; init; }

    public string AppName(Guid appId) => AppOptions.FirstOrDefault(a => a.Id == appId)?.Name ?? "(unknown)";
}
