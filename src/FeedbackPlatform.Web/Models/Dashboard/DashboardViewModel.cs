using FeedbackPlatform.Application.Features.Feedbacks.Queries;
using FeedbackPlatform.Web.Models.Feedback;

namespace FeedbackPlatform.Web.Models.Dashboard;

public sealed class DashboardViewModel
{
    public required FeedbackStatsDto Stats { get; init; }
    public required IReadOnlyList<FeedbackAppOption> AppOptions { get; init; }
    public Guid? AppIdFilter { get; init; }
    public DateOnly FromDate { get; init; }
    public DateOnly ToDate { get; init; }

    public double AverageRating
    {
        get
        {
            var totalVotes = Stats.RatingDistribution.Sum(r => r.Count);
            if (totalVotes == 0)
            {
                return 0;
            }

            var weightedSum = Stats.RatingDistribution.Sum(r => r.StarRating * r.Count);
            return Math.Round(weightedSum / (double)totalVotes, 2);
        }
    }
}
