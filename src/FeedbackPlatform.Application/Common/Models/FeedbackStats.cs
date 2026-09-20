namespace FeedbackPlatform.Application.Common.Models;

public sealed record FeedbackDailyCount(DateOnly Date, int Count);

public sealed record FeedbackRatingBucket(int StarRating, int Count);
