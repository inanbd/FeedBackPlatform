using System.Threading.Channels;
using FeedbackPlatform.Application.Common.Interfaces;

namespace FeedbackPlatform.Infrastructure.Email;

/// <summary>
/// In-process, in-memory queue: feedback submission stays fast because the API request
/// never waits on an SMTP round-trip. <see cref="EmailNotificationBackgroundService"/> drains it.
/// </summary>
public sealed class EmailNotificationQueue : IEmailNotificationQueue
{
    private readonly Channel<FeedbackNotificationMessage> _channel =
        Channel.CreateUnbounded<FeedbackNotificationMessage>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });

    public ChannelReader<FeedbackNotificationMessage> Reader => _channel.Reader;

    public void QueueFeedbackNotification(FeedbackNotificationMessage message) =>
        _channel.Writer.TryWrite(message);
}
