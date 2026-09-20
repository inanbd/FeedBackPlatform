namespace FeedbackPlatform.Application.Common.Interfaces;

public sealed record FeedbackNotificationMessage(
    string OwnerEmail,
    string AppName,
    string FeedbackTitle,
    string FeedbackComment,
    int StarRating,
    string AppVersion,
    DateTimeOffset CreatedAtUtc);

/// <summary>Enqueues feedback notification emails for asynchronous, non-blocking delivery.</summary>
public interface IEmailNotificationQueue
{
    void QueueFeedbackNotification(FeedbackNotificationMessage message);
}
